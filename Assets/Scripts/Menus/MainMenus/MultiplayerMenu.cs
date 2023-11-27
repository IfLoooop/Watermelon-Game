using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Steamworks.NET;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Menus.MainMenus
{
    /// <summary>
    /// Main menu while in multiplayer mode
    /// </summary>
    internal sealed class MultiplayerMenu : MainMenuBase
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the button to restart the game")]
        [SerializeField] private Button restartButton;
        [Tooltip("Reference to the button to change to singleplayer")]
        [SerializeField] private Button singleplayerButton;
        [Tooltip("Reference to the button to join a lobby")]
        [SerializeField] private Button joinLobbyButton;
        [Tooltip("Reference to the button to leave the lobby")]
        [SerializeField] private Button leaveLobbyButton;
        [Tooltip("Reference to the button to create a lobby")]
        [SerializeField] private Button createLobbyButton;
        #endregion
        
        #region Methods
        /// <summary>
        /// Restarts the game
        /// </summary>
        public void Restart()
        {
            MenuController.Restart();
        }
        
        /// <summary>
        /// Changes the <see cref="GameMode"/> to <see cref="GameMode.SinglePlayer"/>
        /// </summary>
        public void SwitchToSingleplayer()
        {
            CurrentGameMode = GameMode.SinglePlayer;
            GameModeTransition(CurrentGameMode);
        }

        /// <summary>
        /// Opens the lobby browser menu
        /// </summary>
        public void JoinLobby()
        {
            MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.JoinLobbyMenu);
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
        /// Hosts a lobby
        /// </summary>
        public void CreateLobby()
        {
            MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.CreateLobbyMenu);
        }

        public override MenuBase Open(MenuBase _CurrentActiveMenu)
        {
            if (SteamLobby.CurrentLobbyId == null)
            {
                this.joinLobbyButton.gameObject.SetActive(true);
                this.leaveLobbyButton.gameObject.SetActive(false);
                this.restartButton.interactable = true;
                this.singleplayerButton.interactable = true;
                this.createLobbyButton.interactable = true;
            }
            else
            {
                if (SteamLobby.IsHost)
                {
                    return MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.LobbyHostMenu);
                }
                
                this.leaveLobbyButton.gameObject.SetActive(true);
                this.joinLobbyButton.gameObject.SetActive(false);
                this.restartButton.interactable = false; // TODO: Maybe allow while alone in lobby
                this.singleplayerButton.interactable = false;
                this.createLobbyButton.interactable = false;
            }
            
            return base.Open(_CurrentActiveMenu);
        }

        #endregion
    }
}