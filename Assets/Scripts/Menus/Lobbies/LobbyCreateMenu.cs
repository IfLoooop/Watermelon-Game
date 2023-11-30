using Sirenix.OdinInspector;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Steamworks.NET;

namespace Watermelon_Game.Menus.Lobbies
{
    /// <summary>
    /// Menu to create a lobby
    /// </summary>
    internal sealed class LobbyCreateMenu : MenuBase
    {
        #region Inspector Fields
        [Tooltip("Reference to the button to start the lobby")]
        [PropertyOrder(1)][SerializeField] private Button startLobbyButton;
        [Tooltip("Reference to the password toggle")]
        [PropertyOrder(1)][SerializeField] private Toggle passwordToggle;
        [Tooltip("Reference to the friends only toggle")]
        [PropertyOrder(1)][SerializeField] private Toggle friendsOnlyToggle;
        [Tooltip("Displays the reason why the host could not start")]
        [PropertyOrder(1)][SerializeField] private TextMeshProUGUI failureReason;
        [Tooltip("Loading indicator")]
        [PropertyOrder(1)][SerializeField] private GameObject loadingIndicator;
        #endregion
        
        #region Fields
        /// <summary>
        /// Indicates whether this lobby will require a password
        /// </summary>
        private bool requiresPassword;
        /// <summary>
        /// Indicates whether only friends are allowed to join
        /// </summary>
        private bool isFriendsOnly;

        /// <summary>
        /// Will be true when the <see cref="LobbyCreateMenu"/> is closed, while waiting for a response from the steam backend to host a lobby
        /// </summary>
        private bool cancelHostingAttempt;
        /// <summary>
        /// Will be true when <see cref="StartLobby"/> is called, is reset after the callback from the steam backend in <see cref="LobbyCreateAttempt"/>
        /// </summary>
        private bool waitingForCallback;
        #endregion
        
        #region Methods
        private void OnEnable()
        {
            SteamLobby.OnLobbyCreateAttempt += this.LobbyCreateAttempt;
        }

        private void OnDisable()
        {
            SteamLobby.OnLobbyCreateAttempt -= this.LobbyCreateAttempt;
        }

        public override void Close(bool _PlaySound)
        {
            base.Close(false);
            MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.MultiplayerMenu);

            if (this.waitingForCallback)
            {
                this.cancelHostingAttempt = true;
            }
        }

        public override void ForceClose(bool _PlaySound)
        {
            base.Close(_PlaySound);
        }

        /// <summary>
        /// Sets <see cref="requiresPassword"/> to the value of <see cref="Toggle"/>.<see cref="Toggle.isOn"/>
        /// </summary>
        /// <param name="_Toggle">The <see cref="Toggle"/> to get the value from</param>
        public void SetPassword(Toggle _Toggle)
        {
            this.requiresPassword = _Toggle.isOn;
        }

        /// <summary>
        /// Sets <see cref="isFriendsOnly"/> to the value of <see cref="Toggle"/>.<see cref="Toggle.isOn"/>
        /// </summary>
        /// <param name="_Toggle">The <see cref="Toggle"/> to get the value from</param>
        public void SetFriendsOnly(Toggle _Toggle)
        {
            this.isFriendsOnly = _Toggle.isOn;
        }

        /// <summary>
        /// Start the lobby as a host
        /// </summary>
        public void StartLobby()
        {
            this.EnableUI(false);
            base.KeepOpen = true;
            this.cancelHostingAttempt = false;
            this.waitingForCallback = true;
            this.failureReason.text = string.Empty;
            SteamLobby.HostLobby(this.requiresPassword, this.isFriendsOnly);
        }

        /// <summary>
        /// <see cref="SteamLobby.OnLobbyCreateAttempt"/>
        /// </summary>
        /// <param name="_Failure">True when the attempt was a failure, otherwise false</param>
        /// <param name="_Reason">The reason (if failure)</param>
        private void LobbyCreateAttempt(bool _Failure, EResult _Reason)
        {
            Debug.LogError($"[LobbyCreateMenu].LobbyCreateAttempt: Failure:{_Failure} | Reason:{_Reason} | CancelHostingAttempt:{this.cancelHostingAttempt}");
            this.waitingForCallback = false;
            base.KeepOpen = false;

            if (this.cancelHostingAttempt)
            {
                SteamLobby.LeaveLobby();
            }
            else
            {
                if (!_Failure)
                {
                    LobbyHostMenu.AddHost();
                    MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.LobbyHostMenu);
                }
                else
                {
                    this.failureReason.text = _Reason.ToString();
                }   
            }
            this.EnableUI(true);
        }

        /// <summary>
        /// Sets the interactable state of the UI to the given value
        /// </summary>
        /// <param name="_Value">True = enables the UI, False = disables it</param>
        private void EnableUI(bool _Value)
        {
            this.startLobbyButton.interactable = _Value;
            this.passwordToggle.interactable = _Value;
            this.friendsOnlyToggle.interactable = _Value;
            this.loadingIndicator.SetActive(!_Value);
        }
        #endregion
    }
}