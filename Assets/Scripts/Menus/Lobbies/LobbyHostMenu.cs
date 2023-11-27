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
        [Tooltip("Displays the password")]
        [SerializeField] private TextMeshProUGUI password;
        [Tooltip("Reference to the refresh button")]
        [SerializeField] private Button refreshButton;
        [Tooltip("Reference to the button that shows/hides the password")]
        [SerializeField] private Button hideButton;
        [Tooltip("Hide Password Sprite")]
        [SerializeField] private Sprite hideSprite;
        [Tooltip("Show Password Sprite")]
        [SerializeField] private Sprite showSprite;
        #endregion

        #region Fields
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
        #endregion
        
        #region Methods
        private void OnEnable()
        {
            SteamLobby.OnLobbyDataUpdated += OnPasswordUpdate;
        }

        private void OnDisable()
        {
            SteamLobby.OnLobbyDataUpdated += OnPasswordUpdate;
        }

        /// <summary>
        /// Leaves the current lobby
        /// </summary>
        public void LeaveLobby()
        {
            SteamLobby.LeaveLobby();
            base.Close(true);
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
        /// Makes the <see cref="refreshButton"/> <see cref="Button.interactable"/> again
        /// </summary>
        /// <param name="_Success">
        /// Indicates whether the change was successful or not <br/>
        /// <i>0 = Failure</i> <br/>
        /// <i>1 = Success</i>
        /// </param>
        private void OnPasswordUpdate(byte _Success)
        {
            if (passwordChangeRequested)
            {
                // TODO: Visual feedback
                
                this.refreshButton.interactable = true;
            }
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
        #endregion
    }
}