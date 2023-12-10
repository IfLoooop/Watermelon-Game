using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using Steamworks;
using UnityEngine;
using Watermelon_Game.Menus;
using Watermelon_Game.Menus.InfoMenus;
using Watermelon_Game.Menus.Lobbies;
using Watermelon_Game.Networking;
using Watermelon_Game.Utility;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Steamworks.NET
{
    /// <summary>
    /// Handles steam lobby logic
    /// </summary>
    internal sealed class SteamLobby : MonoBehaviour
    {
        #region Constants
        // Keys for LobbyData
        public const string NETWORK_ADDRESS = "networkAddress";
        private const string NAME = "name";
        public const string PASSWORD = "password";
        public const string FRIENDS_ONLY = "friendsOnly";
        /// <summary>
        /// Maximum lenght of the messages for <see cref="SendPlayerKickMessage"/> and <see cref="OnLobbyChatMessage"/>
        /// </summary>
        private const int MAX_CHAT_MESSAGE_LENGHT = 17;
        #endregion

        #region Fields
        /// <summary>
        /// Will be true when a password change has been requested -> <see cref="RefreshPassword"/>, will be set back to false after successful change <see cref="OnLobbyDataUpdate"/>
        /// </summary>
        private static ProtectedBool passwordChangeRequested;
        /// <summary>
        /// Will be true if the first password attempt was wrong and a request has been made to receive the new lobby password
        /// </summary>
        private static readonly WaitCondition waitingForLobbyPassword = new();
        /// <summary>
        /// Indicates how many user information request have been made, e.g. how often the <see cref="GetUserName"/>-Method should be allowed to run. <br/>
        /// <i>
        /// Sometimes the <see cref="onPersonaStateChange"/>-Callback is received without manually requesting it. <br/>
        /// A value less than 1 prevents the <see cref="GetUserName"/>-Method to run
        /// </i>
        /// </summary>
        private static ProtectedInt32 userInformationRequested;
        /// <summary>
        /// Will be set when a user tries to join a lobby from their friends list or from an invite <see cref="OnGameLobbyJoinRequested"/>, otherwise null
        /// </summary>
        private static ProtectedUInt64? gameLobbyJoinRequested;
        #endregion
        
        #region Properties
        /// <summary>
        /// Contains all found lobbies
        /// </summary>
        public static List<LobbyData> Lobbies { get; } = new();
        
        /// <summary>
        /// Id of the lobby, the client is currently in
        /// </summary>
        [Title("Debug", TitleAlignment = TitleAlignments.Centered)]
        [ShowInInspector][CanBeNull][ReadOnly] public static DebugField<ProtectedUInt64> CurrentLobbyId { get; private set; }
        /// <summary>
        /// Indicates whether the local client is currently the host of the lobby or not <br/>
        /// <i>Only valid if <see cref="CurrentLobbyId"/> is not null</i>
        /// </summary>
        [ShowInInspector][ReadOnly] public static DebugField<ProtectedBool> IsHost { get; } = new(false);
        /// <summary>
        /// The password set by the host of a lobby <br/>
        /// <i>Must not be longer than 256 characters</i>
        /// </summary>
        [ShowInInspector][ReadOnly] public static DebugField<ProtectedString> HostPassword { get; } = new(string.Empty);
        /// <summary>
        /// Will be true when a game is started as a host -> <see cref="IsHost"/> and it was started with <see cref="ELobbyType"/>: <see cref="ELobbyType.k_ELobbyTypeFriendsOnly"/>
        /// </summary>
        [ShowInInspector][ReadOnly] public static DebugField<ProtectedBool> FriendsOnly { get; } = new(false);
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

        // ReSharper disable NotAccessedField.Local
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
        /// <b>Will be called on the client that connected to the lobby</b>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyDataUpdate_t</i>
        /// </summary>
        private static Callback<LobbyDataUpdate_t> onLobbyDataUpdate;
        /// <summary>
        /// Called when a client joins/leaves a lobby <br/>
        /// <b>Will be called on the host</b>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyChatUpdate_t</i>
        /// </summary>
        private static Callback<LobbyChatUpdate_t> onLobbyChatUpdate;
        /// <summary>
        /// Is called when information for a user has been requested
        /// </summary>
        private static Callback<PersonaStateChange_t> onPersonaStateChange;
        /// <summary>
        /// Called on all clients (including the host), when a message is being broadcasted <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyChatMsg_t</i>
        /// </summary>
        private static Callback<LobbyChatMsg_t> onLobbyChatMessage;
        #endregion
        // ReSharper restore NotAccessedField.Local

        #region Events
        /// <summary>
        /// Is called after attempting to host a lobby -> <see cref="OnLobbyCreated"/> <br/>
        /// <b>Parameter:</b> True when the attempt was a failure, otherwise false <br/>
        /// <b>Parameter:</b> The reason (if failure)
        /// </summary>
        public static event Action<bool, EResult> OnLobbyCreateAttempt;
        /// <summary>
        /// Is called when <see cref="OnRequestLobbyList"/> has finished processing the lobby entries
        /// </summary>
        public static event Action OnLobbyEntriesProcessed;
        /// <summary>
        /// Is called whenever the data of a lobby is changed/updated <br/>
        /// <b>Parameter:</b> The <see cref="LobbyDataUpdate_t"/> callback
        /// </summary>
        public static event Action<LobbyDataUpdate_t> OnLobbyDataUpdated;
        /// <summary>
        /// Is called on the host, when a client joins/leaves the lobby <br/>
        /// <b>Parameter1:</b> The steam id of the client <br/>
        /// <b>Parameter2:</b> The username of the client <br/>
        /// <b>Parameter3:</b> The state change of the client e.g. joined, left, kicked, etc. <br/>
        /// <i><see cref="EChatMemberStateChange"/> will be null when the data for the user is retrieved from the internet</i>
        /// </summary>
        public static event Action<ulong, string, EChatMemberStateChange?> OnLobbyChatUpdated; 
        #endregion
        
        #region Methods
        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            LobbyPasswordMenu.OnLobbyPasswordMenuClose += PasswordMenuClosed;
            GameController.OnResetGameFinished += LobbyJoinAccepted;
        }

        private void OnDisable()
        {
            LobbyPasswordMenu.OnLobbyPasswordMenuClose += PasswordMenuClosed;
            GameController.OnResetGameFinished += LobbyJoinAccepted;
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
            onPersonaStateChange = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
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
            
            HostPassword.Set(_RequiresPassword ? Random.Range(1000, 9999).ToString() : string.Empty);
            FriendsOnly.Set(_IsFriendsOnly);
            var _lobbyType = FriendsOnly.Value ? ELobbyType.k_ELobbyTypeFriendsOnly : ELobbyType.k_ELobbyTypePublic;
            var _apiCall = SteamMatchmaking.CreateLobby(_lobbyType, CustomNetworkManager.MaxConnections);
            onLobbyCreated.Set(_apiCall, OnLobbyCreated);
        }
        
        /// <summary>
        /// Called when a client requested to host a lobby
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        /// <param name="_Failure">Indicates whether the lobby was successfully created</param>
        private static void OnLobbyCreated(LobbyCreated_t _Callback, bool _Failure)
        {
#if DEBUG || DEVELOPMENT_BUILD
            Debug.LogError($"[{nameof(SteamLobby)}].{nameof(OnLobbyCreated)} Failure:{_Failure} | Result:{_Callback.m_eResult} | LobbyID{_Callback.m_ulSteamIDLobby} | SteamID:{SteamManager.SteamID.m_SteamID}");
#endif
            
            CurrentLobbyId = new DebugField<ProtectedUInt64>(_Callback.m_ulSteamIDLobby);
            var _cSteamID = new CSteamID(CurrentLobbyId.Value.Value);
            
            if (!_Failure)
            {
                IsHost.Set(true);
                SteamMatchmaking.SetLobbyData(_cSteamID, NETWORK_ADDRESS, SteamManager.SteamID.m_SteamID.ToString());
                SteamMatchmaking.SetLobbyData(_cSteamID, NAME, SteamFriends.GetPersonaName());
                SteamMatchmaking.SetLobbyData(_cSteamID, FRIENDS_ONLY, FriendsOnly.Value.Value.ToString());

                if (!string.IsNullOrWhiteSpace(HostPassword.Value.Value))
                {
                    SteamMatchmaking.SetLobbyData(_cSteamID, PASSWORD, HostPassword.Value.Value);
                }
            }
            else
            {
                Debug.LogError($"[{nameof(SteamLobby)}].{nameof(OnLobbyCreated)} Failure:{true} | Result:{_Callback.m_eResult} | LobbyID{_Callback.m_ulSteamIDLobby}");
                LeaveLobby();
            }
            
            OnLobbyCreateAttempt?.Invoke(_Failure, _Callback.m_eResult);
        }
        
        /// <summary>
        /// Sets a new password for <see cref="HostPassword"/>
        /// </summary>
        /// <returns>Indicates whether the password change was a success or not</returns>
        public static bool RefreshPassword()
        {
            if (CurrentLobbyId != null && IsHost.Value.Value && !string.IsNullOrWhiteSpace(HostPassword.Value.Value))
            {
                passwordChangeRequested = true;
                return SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyId.Value.Value), PASSWORD, Random.Range(1000, 9999).ToString());
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
            SteamMatchmaking.AddRequestLobbyListResultCountFilter(int.MaxValue);
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            var _apiCall = SteamMatchmaking.RequestLobbyList();
            onRequestLobbyList.Set(_apiCall, OnRequestLobbyList);
        }

        /// <summary>
        /// <see cref="onRequestLobbyList"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        /// <param name="_Failure">Indicates whether the request was successful</param>
        private static void OnRequestLobbyList(LobbyMatchList_t _Callback, bool _Failure)
        {
#if DEBUG || DEVELOPMENT_BUILD
            Debug.LogError($"[{nameof(SteamLobby)}].{nameof(OnRequestLobbyList)} Failure:{_Failure} | LobbyCount:{_Callback.m_nLobbiesMatching}");
#endif
            
            if (_Failure)
            {
                Debug.LogError($"[{nameof(SteamLobby)}].{nameof(OnRequestLobbyList)} Failure:{true} | LobbyCount:{_Callback.m_nLobbiesMatching}");
                return;
            }
            
            Lobbies.Clear();
                
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < _Callback.m_nLobbiesMatching; i++) // TODO: Game freezes with "_Callback.m_nLobbiesMatching - 1"
            {
                var _lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                if (_lobbyId.IsValid() && _lobbyId.IsLobby())
                {
                    var _lobbyName = SteamMatchmaking.GetLobbyData(_lobbyId, NAME);
                    var _requiresPassword = !string.IsNullOrWhiteSpace(SteamMatchmaking.GetLobbyData(_lobbyId, PASSWORD));
                        
                    Lobbies.Add(new LobbyData(_lobbyId.m_SteamID, _lobbyName, _requiresPassword));
                }
            } 
            
            OnLobbyEntriesProcessed?.Invoke();
        }
        
        /// <summary>
        /// Resets the wait condition of <see cref="waitingForLobbyPassword"/> when the <see cref="LobbyPasswordMenu"/> is closed
        /// </summary>
        private static void PasswordMenuClosed()
        {
             waitingForLobbyPassword.Condition = false;
        }

        /// <summary>
        /// Requests and awaits the lobby data for the given lobby <br/>
        /// <i>Data will be received in a <see cref="LobbyDataUpdate_t"/>-<see cref="Callback{T}"/></i>
        /// </summary>
        /// <param name="_LobbyId">The id of the lobby to request the data of</param>
        /// <param name="_WaitCondition">The condition to wait for until the data arrives</param>
        /// <param name="_Await">Whether to wait to completion or just make the request</param>
        /// <returns>False when not connected to steam</returns>
        public static async Task<bool> RequestLobbyDataAsync(CSteamID _LobbyId, WaitCondition _WaitCondition, bool _Await = true)
        {
            if (SteamMatchmaking.RequestLobbyData(_LobbyId))
            {
                _WaitCondition.Condition = _Await;
                await Task.Run(() =>
                {
                    while (_WaitCondition.Condition) { }
                });

                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Call this to join a lobby <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#JoinLobby</i>
        /// </summary>
        /// <param name="_LobbyId">Id of the lobby to join</param>
        /// <param name="_Password">Optional password for the lobby</param>
        /// <param name="_ForceJoin">Skips the password check if true</param>
        public static async void JoinLobbyAsync(ulong _LobbyId, string _Password = "", bool _ForceJoin = false)
        {
            if (!CustomNetworkManager.AllowSteamLobbyJoinRequest())
            {
                return;
            }
            
            var _lobbyId = new CSteamID(_LobbyId);
            var _validLobby = _lobbyId.IsValid() && _lobbyId.IsLobby();
            var _correctPassword = SteamMatchmaking.GetLobbyData(_lobbyId, PASSWORD) == _Password || _ForceJoin;

            if (!_correctPassword)
            {
                if (await RequestLobbyDataAsync(_lobbyId, waitingForLobbyPassword))
                {
                    _correctPassword = SteamMatchmaking.GetLobbyData(_lobbyId, PASSWORD) == _Password;
                }
            }
            
            // If no password is set for the lobby, "GetLobbyData()" will return string.Empty
            if (_validLobby && _correctPassword)
            {
                var _apiCall = SteamMatchmaking.JoinLobby(_lobbyId);
                onLobbyEnterAttempt.Set(_apiCall, OnLobbyEnterAttempt);
            }
            else
            {
#if DEBUG ||DEVELOPMENT_BUILD
                Debug.LogError($"[{nameof(SteamLobby)}].{nameof(JoinLobbyAsync)} LobbyID:{_LobbyId} | ValidID:{_lobbyId.IsValid()} | IsLobby:{_lobbyId.IsLobby()} | EnteredPassword:{_Password} | HostPassword:{SteamMatchmaking.GetLobbyData(_lobbyId, PASSWORD)}");
#endif
                CustomNetworkManager.SteamLobbyEnterAttempt(true);
            }
        }
        
        /// <summary>
        /// Is received after attempting to join a lobby <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LobbyEnter_t</i>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        /// <param name="_Failure">Indicates whether the attempt was successful</param>
        private static void OnLobbyEnterAttempt(LobbyEnter_t _Callback, bool _Failure)
        {
#if DEBUG || DEVELOPMENT_BUILD
            Debug.LogError($"[{nameof(SteamLobby)}].{nameof(OnLobbyEnterAttempt)} Failure:{_Failure} | ChatRoomEnterResponse:{(EChatRoomEnterResponse)_Callback.m_EChatRoomEnterResponse} | LobbyID:{_Callback.m_ulSteamIDLobby} | Blocked:{_Callback.m_bLocked} | SteamID:{SteamManager.SteamID.m_SteamID}");
#endif
            
            CurrentLobbyId = new DebugField<ProtectedUInt64>(_Callback.m_ulSteamIDLobby);
            var _hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(_Callback.m_ulSteamIDLobby), NETWORK_ADDRESS);
            
            if (_Failure)
            {
                Debug.LogError($"[{nameof(SteamLobby)}].{nameof(OnLobbyEnterAttempt)} Failure:{true} | LobbyID:{_Callback.m_ulSteamIDLobby} | Blocked:{_Callback.m_bLocked}");
                LeaveLobby();
            }
            
            CustomNetworkManager.SteamLobbyEnterAttempt(_Failure, _hostAddress);
        }

        /// <summary>
        /// Laves the currently joined steam lobby <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamMatchmaking#LeaveLobby</i>
        /// </summary>
        public static void LeaveLobby()
        {
            if (CurrentLobbyId != null)
            {
                SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyId.Value.Value));
            }
            
            CurrentLobbyId = null;
            IsHost.Set(false);
            HostPassword.Set(string.Empty);
            FriendsOnly.Set(false);
        }

        /// <summary>
        /// Leaves the currently joined steam lobby -> <see cref="LeaveLobby"/> and disconnects the client/host -> <see cref="CustomNetworkManager.DisconnectFromLobby"/>
        /// </summary>
        public static void DisconnectFromLobby()
        {
            var _isHost = IsHost;
            LeaveLobby();
            CustomNetworkManager.DisconnectFromLobby(_isHost.Value.Value);
        }
        
        /// <summary>
        /// <see cref="onGameLobbyJoinRequested"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t _Callback)
        {
#if DEBUG || DEVELOPMENT_BUILD
            Debug.LogError($"[{nameof(SteamLobby)}].{nameof(OnGameLobbyJoinRequested)} LobbyID:{_Callback.m_steamIDLobby.m_SteamID} | FriendID:{_Callback.m_steamIDFriend.m_SteamID} | SteamID:{SteamManager.SteamID.m_SteamID}");
#endif
            if (gameLobbyJoinRequested == null && !CustomNetworkManager.AttemptingToJoinALobby)
            {
                gameLobbyJoinRequested = _Callback.m_steamIDLobby.m_SteamID;
                GameController.SwitchGameMode(GameMode.MultiPlayer, true);
            }
        }
        
        /// <summary>
        /// <see cref="onGameRichPresenceJoinRequested"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t _Callback)
        {
#if DEBUG || DEVELOPMENT_BUILD
            Debug.LogError($"[{nameof(SteamLobby)}].{nameof(OnGameRichPresenceJoinRequested)} FriendID:{_Callback.m_steamIDFriend.m_SteamID} | ConnectKeyValue:{_Callback.m_rgchConnect} | SteamID:{SteamManager.SteamID.m_SteamID}");
#endif
            if (gameLobbyJoinRequested == null && !CustomNetworkManager.AttemptingToJoinALobby)
            {
                gameLobbyJoinRequested = _Callback.m_steamIDFriend.m_SteamID;
                GameController.SwitchGameMode(GameMode.MultiPlayer, true);
            }
            
            // TODO
            // https://partner.steamgames.com/doc/api/ISteamApps#GetLaunchCommandLine
            // https://partner.steamgames.com/doc/api/ISteamApps#NewUrlLaunchParameters_t
            // https://partner.steamgames.com/doc/api/ISteamApps#GetLaunchQueryParam
            // https://partner.steamgames.com/doc/api/ISteamApps#NewLaunchQueryParameters_t
        }
        
        /// <summary>
        /// Joins the lobby with id <see cref="gameLobbyJoinRequested"/>
        /// </summary>
        /// <param name="_ResetReason"></param>
        private static void LobbyJoinAccepted(ResetReason _ResetReason)
        {
            if (gameLobbyJoinRequested != null)
            {
                MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.LobbyJoinMenu);
                JoinLobbyAsync(gameLobbyJoinRequested.Value, string.Empty);
                gameLobbyJoinRequested = null;
            }
        }
        
        /// <summary>
        /// <see cref="onLobbyDataUpdate"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnLobbyDataUpdate(LobbyDataUpdate_t _Callback)
        {
#if DEBUG || DEVELOPMENT_BUILD
            Debug.LogError($"[{nameof(SteamLobby)}].{nameof(OnLobbyDataUpdate)} Success:{_Callback.m_bSuccess} | LobbyID:{_Callback.m_ulSteamIDLobby} | MemberID:{_Callback.m_ulSteamIDMember} | SteamID:{SteamManager.SteamID.m_SteamID}");
#endif
            
            // If m_ulSteamIDMember == m_ulSteamIDLobby, then lobby meta data has been updated and "SteamMatchmaking.GetLobbyData()" should be used
            if (_Callback.m_ulSteamIDMember == _Callback.m_ulSteamIDLobby)
            {
                // Will be 0/false, when the id for which the data has been requested doesn't exist
                if (_Callback.m_bSuccess == 1)
                {
                    if (passwordChangeRequested)
                    {
                        passwordChangeRequested = false;
                        HostPassword.Set(SteamMatchmaking.GetLobbyData(new CSteamID(_Callback.m_ulSteamIDLobby), PASSWORD));
                    }
                    if (waitingForLobbyPassword.Condition)
                    {
                        waitingForLobbyPassword.Condition = false;
                    }
                }
                
                OnLobbyDataUpdated?.Invoke(_Callback); 
            }
            // Member meta data has been updated an "SteamMatchmaking.GetLobbyMemberData()" should be used
            else
            {
                
            }
        }
        
        /// <summary>
        /// <see cref="onLobbyChatUpdate"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnLobbyChatUpdate(LobbyChatUpdate_t _Callback)
        {
#if DEBUG || DEVELOPMENT_BUILD
            Debug.LogError($"[{nameof(SteamLobby)}].{nameof(OnLobbyChatUpdate)} LobbyID:{_Callback.m_ulSteamIDLobby} | UserID:{_Callback.m_ulSteamIDUserChanged} | CallerID:{_Callback.m_ulSteamIDMakingChange} | StateChange:{(EChatMemberStateChange)_Callback.m_rgfChatMemberStateChange} | SteamID:{SteamManager.SteamID.m_SteamID}");
#endif
            if (IsHost.Value.Value)
            {
                userInformationRequested++;
                var _username = string.Empty;
                // This will only get the username for friends
                // To get the username for unknown players, the "onPersonaStateChange"-Callback needs to be awaited after calling "SteamFriends.RequestUserInformation()"
                var _needsToRetrieveInformationFromInternet = SteamFriends.RequestUserInformation(new CSteamID(_Callback.m_ulSteamIDUserChanged), true);
                if (!_needsToRetrieveInformationFromInternet)
                {
                    GetUserName(_Callback.m_ulSteamIDUserChanged, false, out _username);
                }
                    
                OnLobbyChatUpdated?.Invoke(_Callback.m_ulSteamIDUserChanged, _username, (EChatMemberStateChange)_Callback.m_rgfChatMemberStateChange);

                // if (FriendsOnly) // TODO: Maybe not needed
                // {
                //     if (!SteamFriends.HasFriend(new CSteamID(_Callback.m_ulSteamIDUserChanged), EFriendFlags.k_EFriendFlagImmediate))
                //     {
                //         KickPlayer(new CSteamID(_Callback.m_ulSteamIDUserChanged));
                //     }
                // }
            }
        }

        /// <summary>
        /// <see cref="onPersonaStateChange"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnPersonaStateChange(PersonaStateChange_t _Callback)
        {
            GetUserName(_Callback.m_ulSteamID, true, out _);
        }

        /// <summary>
        /// Gets the username for the given steam id <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamFriends#GetFriendPersonaName</i>
        /// </summary>
        /// <param name="_SteamId">The id of the user to get the username for</param>
        /// <param name="_RetrievedFromInternet">Indicates whether the data for the given steam id was already available, or had to be retrieved from the internet</param>
        /// <param name="_Username">Will contain the username, if <see cref="_RetrievedFromInternet"/> was false</param>
        private static void GetUserName(ulong _SteamId, bool _RetrievedFromInternet, out string _Username)
        {
            _Username = string.Empty;
            if (userInformationRequested <= 0)
            {
                return;
            }
            
            userInformationRequested = Mathf.Clamp(--userInformationRequested, 0, int.MaxValue);

            var _steamId = new CSteamID(_SteamId);
            if (_steamId.IsValid())
            {
                _Username = SteamFriends.GetFriendPersonaName(_steamId);
                if (_RetrievedFromInternet)
                {
                    OnLobbyChatUpdated?.Invoke(_SteamId, _Username, null);
                }
            }
        }

        /// <summary>
        /// Sends the given <see cref="CSteamID"/> as a chat message to all connected clients in the lobby and kicks the player whose steam id matches the given one <br/>
        /// <b>Byte lenght of the given steam id must not exceed <see cref="MAX_CHAT_MESSAGE_LENGHT"/></b>
        /// </summary>
        /// <param name="_Message">The message body to sent</param>
        public static void SendPlayerKickMessage(byte[] _Message)
        {
            if (CurrentLobbyId is null)
            {
                Debug.LogError($"{nameof(CurrentLobbyId)} is null, you need to be in a lobby to send a message");
                return;
            }
            if (_Message.Length > MAX_CHAT_MESSAGE_LENGHT)
            {
                Debug.LogError($"The given message is to long, max message size is: {MAX_CHAT_MESSAGE_LENGHT} byte");
                return;
            }

            if (!SteamMatchmaking.SendLobbyChatMsg(new CSteamID(CurrentLobbyId.Value.Value), _Message, _Message.Length))
            {
                Debug.LogError($"[{nameof(SteamLobby)}].{nameof(SendPlayerKickMessage)}: Message [{System.Text.Encoding.UTF8.GetString(_Message)}] could not be send");
            }

#if DEBUG || DEVELOPMENT_BUILD
            Debug.LogError($"[{nameof(SteamLobby)}].{nameof(SendPlayerKickMessage)} IDToKick:{System.Text.Encoding.UTF8.GetString(_Message)} | SenderID:{SteamManager.SteamID.m_SteamID} MessageSend: {string.Join(string.Empty, _Message)}");
#endif
        }

        /// <summary>
        /// Disconnects a client from the lobby and the connected host, and displays a message
        /// </summary>
        private static void KickPlayer(InfoMessage _InfoMessage)
        {
            DisconnectFromLobby();
            MenuController.OpenPopup(_MenuControllerMenu => _MenuControllerMenu.InfoMenu.SetMessage(_InfoMessage));  
        }
        
        /// <summary>
        /// <see cref="onLobbyChatMessage"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private static void OnLobbyChatMessage(LobbyChatMsg_t _Callback)
        {
            var _body = new byte[MAX_CHAT_MESSAGE_LENGHT];
            var _messageLength = SteamMatchmaking.GetLobbyChatEntry(new CSteamID(_Callback.m_ulSteamIDLobby), (int)_Callback.m_iChatID, out _, _body, MAX_CHAT_MESSAGE_LENGHT, out _);
            var _message = System.Text.Encoding.UTF8.GetString(_body);
            
            if (ulong.TryParse(_message, out var _steamId))
            {
                if (_steamId == SteamManager.SteamID.m_SteamID)
                {
                    KickPlayer(InfoMessage.KickedFromLobby);
                }
            }
            
#if DEBUG || DEVELOPMENT_BUILD
            Debug.LogError($"[{nameof(SteamLobby)}].{nameof(OnLobbyChatMessage)} LobbyID:{_Callback.m_ulSteamIDLobby} | CallerID:{_Callback.m_ulSteamIDUser} | LocalID:{SteamManager.SteamID.m_SteamID} | ReceivedMessage:{string.Join(string.Empty, _body.Select(_Byte => _Byte.ToString()))} | SteamID:{_steamId} | MessageLenght:{_messageLength} | MessageType:{(EChatEntryType)_Callback.m_eChatEntryType} | ChatEntryIndex:{_Callback.m_iChatID}");
#endif
        }
        #endregion
    }
}