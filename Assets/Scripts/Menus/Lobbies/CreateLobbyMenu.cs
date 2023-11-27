using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Steamworks.NET;

namespace Watermelon_Game.Menus.Lobbies
{
    /// <summary>
    /// Menu to create a lobby
    /// </summary>
    internal sealed class CreateLobbyMenu : MenuBase
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the button to start the lobby")]
        [SerializeField] private Button startLobbyButton;
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
            this.startLobbyButton.interactable = false;
            SteamLobby.HostLobby(this.requiresPassword, this.isFriendsOnly);
        }

        /// <summary>
        /// <see cref="SteamLobby.OnLobbyCreateAttempt"/>
        /// </summary>
        /// <param name="_Failure">True when the attempt was a failure, otherwise false</param>
        /// <param name="_Reason">The reason (if failure)</param>
        private void LobbyCreateAttempt(bool _Failure, EResult _Reason)
        {
            this.startLobbyButton.interactable = true;

            if (!_Failure)
            {
                MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.LobbyHostMenu);
            }
            else
            {
                // TODO: Feedback on failure (maybe print reason)
            }
        }
        #endregion
    }
}