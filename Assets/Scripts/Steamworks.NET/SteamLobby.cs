using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using Watermelon_Game.Menus.Lobbies;
using Watermelon_Game.Networking;
using Random = UnityEngine.Random;
using Task = System.Threading.Tasks.Task;

namespace Watermelon_Game.Steamworks.NET
{
    /// <summary>
    /// Handles steam lobby logic
    /// </summary>
    internal sealed class SteamLobby : MonoBehaviour
    {
        #region Constants
        private const string LOCALHOST = "localhost";
        private const string NETWORK_ADDRESS = "networkAddress";
        private const string NAME = "name";
        private const string PASSWORD = "password";
        #endregion
        
        #region Fields
        /// <summary>
        /// The password entered by the client when trying to join a lobby
        /// </summary>
        private static string clientPassword;
        #endregion

        #region Properties
        /// <summary>
        /// Contains all found lobbies
        /// </summary>
        public static List<LobbyData> Lobbies { get; } = new();
        
        /// <summary>
        /// Id of the lobby, the client is currently in
        /// </summary>
        public static CSteamID? CurrentLobbyId { get; private set; }
        /// <summary>
        /// Indicates whether the local client is currently the host of the lobby or not <br/>
        /// <i>Only valid of <see cref="CurrentLobbyId"/> is not null</i>
        /// </summary>
        public static bool IsHost { get; private set; }
        /// <summary>
        /// The password set by the host of a lobby <br/>
        /// <i>Must not be longer than 256 characters</i>
        /// </summary>
        public static string HostPassword { get; private set; }
        #endregion
        
        #region Callbacks
        /// <summary>
        /// Is called after a request to create a lobby <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyCreated_t</i>
        /// </summary>
        private static readonly CallResult<LobbyCreated_t> onLobbyCreated = new();
        /// <summary>
        /// I called after the lobby list has been requested <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyMatchList_t</i>
        /// </summary>
        private static readonly CallResult<LobbyMatchList_t> onRequestLobbyList = new();
        /// <summary>
        /// Called when attempting to enter a lobby <br/>
        /// <b>Also called on the host</b>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyEnter_t</i>
        /// </summary>
        private static readonly CallResult<LobbyEnter_t> onLobbyEnterAttempt = new();

        /// <summary>
        /// Called when a user tries to join a lobby from their friend list or from an invite <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamFriends#GameLobbyJoinRequested_t</i>
        /// </summary>
        private static Callback<GameLobbyJoinRequested_t> onGameLobbyJoinRequested;
        /// <summary>
        /// Called when the user tries to join a game from their friends list or after a user accepts an invite by a friend <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamFriends#GameRichPresenceJoinRequested_t</i>
        /// </summary>
        private static Callback<GameRichPresenceJoinRequested_t> onGameRichPresenceJoinRequested;
        /// <summary>
        /// Called when the metadata of a lobby has changed <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyDataUpdate_t</i>
        /// </summary>
        private static Callback<LobbyDataUpdate_t> onLobbyDataUpdate;
        /// <summary>
        /// Called when a client joins/leaves a lobby <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyChatUpdate_t</i>
        /// </summary>
        private static Callback<LobbyChatUpdate_t> onLobbyChatUpdate;
        /// <summary>
        /// Called on all clients (including the host), when a message is being broadcasted <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyChatMsg_t</i>
        /// </summary>
        private static Callback<LobbyChatMsg_t> onLobbyChatMessage;
        #endregion

        #region Events
        /// <summary>
        /// Is called after attempting to host a lobby -> <see cref="OnLobbyCreated"/> <br/>
        /// <b>Parameter:</b> True when the attempt was a failure, otherwise false <br/>
        /// <b>Parameter:</b> The reason (if failure)
        /// </summary>
        public static event Action<bool, EResult> OnLobbyCreateAttempt;
        /// <summary>
        /// Is called when the client leaves a lobby
        /// </summary>
        public static event Action OnLobbyLeft;
        /// <summary>
        /// Is called when <see cref="OnRequestLobbyListAsync"/> has finished processing the lobby entries
        /// </summary>
        public static event Action OnLobbyEntriesProcessed;
        /// <summary>
        /// Is called whenever the data of a lobby is changed/updated <br/>
        /// <b>Parameter:</b> Indicates whether the change/update was a success or not <br/>
        /// <i>0 = Failure</i> <br/>
        /// <i>1 = Success</i>
        /// </summary>
        public static event Action<byte> OnLobbyDataUpdated;
        #endregion
        
        #region Methods
        private void Awake()
        {
            Init();
        }

        /// <summary>
        /// Initializes all needed values
        /// </summary>
        private static void Init()
        {
            if (!SteamManager.Initialized)
            {
                return;
            }

            onLobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            onGameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            onGameRichPresenceJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);
            onLobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            onLobbyChatMessage = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMessage);
        }
        
        /// <summary>
        /// Call this to host a lobby <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#CreateLobby</i>
        /// </summary>
        /// <param name="_RequiresPassword">Indicates whether this lobby will require a password</param>
        /// <param name="_IsFriendsOnly">Indicates whether only friends are allowed to join</param>
        public static void HostLobby(bool _RequiresPassword, bool _IsFriendsOnly)
        {
            if (!SteamManager.Initialized)
            {
                return;
            }
            
            HostPassword = _RequiresPassword ? Random.Range(1000, 9999).ToString() : string.Empty;
            var _lobbyType = _IsFriendsOnly ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePublic;
            var _apiCall = SteamMatchmaking.CreateLobby(_lobbyType, CustomNetworkManager.MaxConnection);
            onLobbyCreated.Set(_apiCall, OnLobbyCreated);
        }
        
        /// <summary>
        /// Called when a client requested to host a lobby
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        /// <param name="_Failure">Indicates whether the lobby was successfully created</param>
        private static void OnLobbyCreated(LobbyCreated_t _Callback, bool _Failure)
        {
            if (!_Failure)
            {
                IsHost = true;
                CurrentLobbyId = new CSteamID(_Callback.m_ulSteamIDLobby);
                SteamMatchmaking.SetLobbyData(CurrentLobbyId.Value, NETWORK_ADDRESS, SteamManager.SteamID.m_SteamID.ToString());
                SteamMatchmaking.SetLobbyData(CurrentLobbyId.Value, NAME, SteamFriends.GetPersonaName());

                if (!string.IsNullOrWhiteSpace(HostPassword))
                {
                    SteamMatchmaking.SetLobbyData(CurrentLobbyId.Value, PASSWORD, HostPassword);
                    Debug.Log($"PASSWORD: {SteamMatchmaking.GetLobbyData(CurrentLobbyId.Value, PASSWORD)}");
                }
            }
            
            OnLobbyCreateAttempt?.Invoke(_Failure, _Callback.m_eResult);
        }
        
        /// <summary>
        /// Sets a new password for <see cref="HostPassword"/>
        /// </summary>
        /// <returns>Indicates whether the password change was a success or not</returns>
        public static bool RefreshPassword()
        {
            if (CurrentLobbyId != null && IsHost && !string.IsNullOrWhiteSpace(HostPassword))
            {
                HostPassword = Random.Range(1000, 9999).ToString();
                return SteamMatchmaking.SetLobbyData(CurrentLobbyId.Value, PASSWORD, HostPassword);
            }

            return false;
        }
        
        /// <summary>
        /// Call this to get all currently available lobbies <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#RequestLobbyList</i>
        /// </summary>
        public static void GetLobbies()
        {
            if (!SteamManager.Initialized)
            {
                return;
            }

            //SteamMatchmaking.AddRequestLobbyListNearValueFilter(); // TODO: Add a region/distance filter
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            var _apiCall = SteamMatchmaking.RequestLobbyList();
            onRequestLobbyList.Set(_apiCall, OnRequestLobbyListAsync);
        }

        /// <summary>
        /// <see cref="onRequestLobbyList"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        /// <param name="_Failure">Indicates whether the request was successful</param>
        private static async void OnRequestLobbyListAsync(LobbyMatchList_t _Callback, bool _Failure)
        {
            if (_Failure)
            {
                Debug.LogError("Lobby list couldn't be retrieved");
                return;
            }
            
            await Task.Run(() =>
            {
                Lobbies.Clear();
            
                // ReSharper disable once InconsistentNaming
                for (var i = 0; i < _Callback.m_nLobbiesMatching - 1; i++)
                {
                    var _lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                    if (_lobbyId.IsValid() && _lobbyId.IsLobby())
                    {
                        var _lobbyName = SteamMatchmaking.GetLobbyData(_lobbyId, NAME);
                        var _requiresPassword = !string.IsNullOrWhiteSpace(SteamMatchmaking.GetLobbyData(_lobbyId, PASSWORD));
                        
                        Lobbies.Add(new LobbyData(_lobbyId.m_SteamID, _lobbyName, _requiresPassword));
                    }
                } 
            });
            
            OnLobbyEntriesProcessed?.Invoke();
        }
        
        /// <summary>
        /// Call this to join a lobby <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#JoinLobby</i>
        /// </summary>
        /// <param name="_LobbyDataIndex">Id of the lobby to join</param>
        /// <param name="_Password">Optional password for the lobby</param>
        public static void JoinLobby(ulong _LobbyDataIndex, string _Password = "")
        {
            if (!CustomNetworkManager.AllowSteamLobbyJoinRequest())
            {
                return;
            }

            JoinLobbyMenu.AllowJoinButtonInteraction(false);
            clientPassword = _Password;
            
            var _apiCall = SteamMatchmaking.JoinLobby(new CSteamID(_LobbyDataIndex));
            onLobbyEnterAttempt.Set(_apiCall, OnLobbyEnterAttempt);
        }
        
        /// <summary>
        /// Is received after attempting to join a lobby <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyEnter_t</i>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        /// <param name="_Failure">Indicates whether the attempt was successful</param>
        private static void OnLobbyEnterAttempt(LobbyEnter_t _Callback, bool _Failure)
        {
            IsHost = false;
            var _hostAddress = LOCALHOST;
            if (!_Failure)
            {
                CurrentLobbyId = new CSteamID(_Callback.m_ulSteamIDLobby);
                if (CurrentLobbyId.Value.IsValid() && CurrentLobbyId.Value.IsLobby())
                {
                    // If no password is set for the lobby, "GetLobbyData()" will return string.Empty
                    if (SteamMatchmaking.GetLobbyData(CurrentLobbyId.Value, PASSWORD) != clientPassword)
                    {
                        _Failure = true;
                    }
                    
                    _hostAddress = SteamMatchmaking.GetLobbyData(CurrentLobbyId.Value, NETWORK_ADDRESS);   
                }
                else
                {
                    _Failure = true;
                    Debug.LogError("Not a valid lobby id");
                }
            }
            if (_Failure)
            {
                CurrentLobbyId = null;
            }
            
            Debug.LogError($"[OnLobbyEnterAttempt] m_ulSteamIDLobby:{_Callback.m_ulSteamIDLobby} | m_bLocked:{_Callback.m_bLocked} | m_EChatRoomEnterResponse:{_Callback.m_EChatRoomEnterResponse} | m_rgfChatPermissions:{_Callback.m_rgfChatPermissions}");
            CustomNetworkManager.SteamLobbyEnterAttempt(_Failure, _hostAddress);
        }

        /// <summary>
        /// Laves the currently joined lobby <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LeaveLobby</i>
        /// </summary>
        public static void LeaveLobby()
        {
            if (CurrentLobbyId != null)
            {
                SteamMatchmaking.LeaveLobby(CurrentLobbyId.Value);
                CurrentLobbyId = null;
                IsHost = false;
                
                OnLobbyLeft?.Invoke();
            }
        }

        /// <summary>
        /// <see cref="onGameLobbyJoinRequested"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t _Callback)
        {
            Debug.LogError($"[OnJoinRequested] m_steamIDLobby:{_Callback.m_steamIDLobby.m_SteamID} | m_steamIDFriend:{_Callback.m_steamIDFriend.m_SteamID}");
            SteamMatchmaking.JoinLobby(_Callback.m_steamIDLobby);
            // TODO: Set game to multiplayer mode -> Check if password is needed and open prompt -> Call "JoinLobby();"
        }

        /// <summary>
        /// <see cref="onGameRichPresenceJoinRequested"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t _Callback)
        {
            Debug.LogError($"[OnGameRichPresenceJoinRequested] m_steamIDFriend:{_Callback.m_steamIDFriend.m_SteamID} | m_rgchConnect:{_Callback.m_rgchConnect}");
            SteamMatchmaking.JoinLobby(_Callback.m_steamIDFriend);
            // TODO: Set game to multiplayer mode -> Check if password is needed and open prompt -> Call "JoinLobby();"
        }
        
        /// <summary>
        /// <see cref="onLobbyDataUpdate"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnLobbyDataUpdate(LobbyDataUpdate_t _Callback)
        {
            OnLobbyDataUpdated?.Invoke(_Callback.m_bSuccess);
            Debug.LogError($"[OnLobbyDataUpdate] m_bSuccess:{_Callback.m_bSuccess} | m_ulSteamIDLobby:{_Callback.m_ulSteamIDLobby} | m_ulSteamIDMember:{_Callback.m_ulSteamIDMember}");
        }
        
        /// <summary>
        /// <see cref="onLobbyChatUpdate"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnLobbyChatUpdate(LobbyChatUpdate_t _Callback)
        {
            Debug.LogError($"[OnLobbyChatUpdate] m_ulSteamIDLobby:{_Callback.m_ulSteamIDLobby} | m_rgfChatMemberStateChange:{_Callback.m_rgfChatMemberStateChange} | m_ulSteamIDMakingChange:{_Callback.m_ulSteamIDMakingChange} | m_ulSteamIDUserChanged:{_Callback.m_ulSteamIDUserChanged}");
        }
        
        /// <summary>
        /// <see cref="onLobbyChatMessage"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnLobbyChatMessage(LobbyChatMsg_t _Callback)
        {
            const int DATA_SIZE = 4000;
            var _data = new byte[DATA_SIZE];
            var _dataCount = SteamMatchmaking.GetLobbyChatEntry(new CSteamID(_Callback.m_ulSteamIDLobby), (int)_Callback.m_iChatID, out var _steamIdUser, _data, DATA_SIZE, out var _chatEntryType);
            
            //SteamMatchmaking.SendLobbyChatMsg()
        }
        #endregion
    }
}