using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Steamworks.NET;

namespace Watermelon_Game.Menus.Lobbies
{
    /// <summary>
    /// Menu while a game is being hosted
    /// </summary>
    internal sealed class LobbyHostMenu : MenuBase
    {
        #region Inspector Fields
        [Tooltip("Parent transform for all lobby member prefabs")]
        [PropertyOrder(1)][SerializeField] private Transform playerList;
        [Tooltip("Prefab to instantiate for each lobby member")]
        [PropertyOrder(1)][SerializeField] private LobbyMember lobbyMemberPrefab;
        [Tooltip("Displays the password")]
        [PropertyOrder(1)][SerializeField] private TextMeshProUGUI password;
        [Tooltip("Reference to the refresh button")]
        [PropertyOrder(1)][SerializeField] private Button refreshButton;
        [Tooltip("Reference to the button that shows/hides the password")]
        [PropertyOrder(1)][SerializeField] private Button hideButton;
        [Tooltip("Hide Password Sprite")]
        [PropertyOrder(1)][SerializeField] private Sprite hideSprite;
        [Tooltip("Show Password Sprite")]
        [PropertyOrder(1)][SerializeField] private Sprite showSprite;
        #endregion

        #region Fields
        /// <summary>
        /// Singleton of <see cref="LobbyHostMenu"/>
        /// </summary>
        private static LobbyHostMenu instance;
        /// <summary>
        /// Indicates whether a password change has been requested and is being waited for
        /// </summary>
        private bool passwordChangeRequested;
        /// <summary>
        /// Indicates if the password is currently being replaced with '*' characters
        /// </summary>
        private bool isPasswordHidden = true;
        /// <summary>
        /// Content for <see cref="password"/> while it is hidden
        /// </summary>
        private const string HIDDEN_PASSWORD = "****";

        /// <summary>
        /// Contains all members of this lobby
        /// </summary>
        private readonly List<LobbyMember> lobbyMembers = new();
        #endregion

        #region Events
        /// <summary>
        /// Is called when a host leaves their own lobby <br/>
        /// <i>Dirty fix for ContainerBounds loosing reference to the FruitSpawner, after a client joins another hosts lobby and host leaves the lobby</i>
        /// </summary>
        public static event Action OnHostLeaveLobby;
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            SteamLobby.OnLobbyChatUpdated += AdjustLobbyMembers;
            SteamLobby.OnLobbyDataUpdated += OnPasswordUpdate;
        }

        private void OnDisable()
        {
            SteamLobby.OnLobbyChatUpdated -= AdjustLobbyMembers;
            SteamLobby.OnLobbyDataUpdated -= OnPasswordUpdate;
        }

        public override MenuBase Open(MenuBase _CurrentActiveMenu)
        {
            if (string.IsNullOrWhiteSpace(SteamLobby.HostPassword))
            {
                this.hideButton.interactable = false;
                this.refreshButton.interactable = false;
            }
            else
            {
                this.HidePassword(true);
                this.hideButton.interactable = true;
                this.refreshButton.interactable = true;
            }
            
            return base.Open(_CurrentActiveMenu);
        }

        /// <summary>
        /// Adds the host of the lobby to <see cref="lobbyMembers"/>
        /// </summary>
        public static void AddHost()
        {
            instance.lobbyMembers.Add(Instantiate(instance.lobbyMemberPrefab, instance.playerList).SetMemberData(SteamManager.SteamID.m_SteamID, SteamFriends.GetPersonaName()));
            foreach (var _lobbyMember in instance.lobbyMembers)
            {
                Debug.LogError(_lobbyMember.ToString());
            }
            instance.lobbyMembers[0].gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Adjusts the entries of <see cref="lobbyMembers"/> depending on the given values
        /// </summary>
        /// <param name="_SteamId">The steam id of the player</param>
        /// <param name="_Username">The username of the player</param>
        /// <param name="_StateChange">The state change of the client e.g. joined, left, kicked, etc.</param>
        private void AdjustLobbyMembers(ulong _SteamId, string _Username, EChatMemberStateChange? _StateChange)
        {
            if (_StateChange is {} _stateChange)
            {
                switch (_stateChange)
                {
                    case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                        if (this.lobbyMembers.All(_LobbyMembers => _LobbyMembers.SteamId != _SteamId))
                        {
                            this.lobbyMembers.Add(Instantiate(this.lobbyMemberPrefab, this.playerList).SetMemberData(_SteamId, _Username));
                        }
                        break;
                    default:
                        if (this.lobbyMembers.FirstOrDefault(_LobbyMember => _LobbyMember.SteamId == _SteamId) is {} _lobbyMember)
                        {
                            Destroy(_lobbyMember.gameObject);
                            this.lobbyMembers.Remove(_lobbyMember);
                        }
                        break;
                }
            }
            else
            {
                if (this.lobbyMembers.FirstOrDefault(_LobbyMember => _LobbyMember.SteamId == _SteamId) is {} _lobbyMember)
                {
                    _lobbyMember.SetMemberData(_SteamId, _Username);
                }
            }
        }
        
        /// <summary>
        /// Leaves and closes the current lobby
        /// </summary>
        public void LeaveLobby()
        {
            base.Close(true);
            this.lobbyMembers.ForEach(_LobbyMember => _LobbyMember.KickLobbyMember());
            // To destroy the LobbyMember GameObject of the host, "AdjustLobbyMembers()" won't be called for the host
            this.lobbyMembers.ForEach(_LobbyMember => Destroy(_LobbyMember.gameObject));
            this.lobbyMembers.Clear();
            SteamLobby.LeaveLobby();
            OnHostLeaveLobby?.Invoke();
        }

        /// <summary>
        /// Hides/shows the text input
        /// </summary>
        public void Hide()
        {
            this.HidePassword(!isPasswordHidden);
        }
        
        /// <summary>
        /// Hides/shows the password based on the given value
        /// </summary>
        /// <param name="_Hide">Indicates if the password should be replaces with '*' characters</param>
        private void HidePassword(bool _Hide)
        {
            if (_Hide)
            {
                this.isPasswordHidden = true;
                this.password.text = HIDDEN_PASSWORD;
                this.hideButton.image.sprite = this.showSprite;
            }
            else
            {
                this.isPasswordHidden = false;
                this.password.text = SteamLobby.HostPassword;
                this.hideButton.image.sprite = this.hideSprite;
            }
        }
        
        /// <summary>
        /// Refreshes the <see cref="SteamLobby.HostPassword"/>
        /// </summary>
        public void RefreshPassword()
        {
            this.refreshButton.interactable = false;
            if (SteamLobby.RefreshPassword())
            {
                this.passwordChangeRequested = true;
            }
            else
            {
                this.refreshButton.interactable = true;
            }
        }
        
        /// <summary>
        /// Makes the <see cref="refreshButton"/> <see cref="Button.interactable"/> again
        /// </summary>
        private void OnPasswordUpdate()
        {
            if (passwordChangeRequested)
            {
                // TODO: Visual feedback
                this.HidePassword(this.isPasswordHidden);
                this.refreshButton.interactable = true;
            }
        }
        #endregion
    }
}