using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Steamworks.NET;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Menus.MainMenus
{
    /// <summary>
    /// Main menu while in multiplayer mode
    /// </summary>
    internal sealed class MultiplayerMenu : MenuBase
    {
        #region Inspector Fields
        [Tooltip("Reference to the button to restart the game")]
        [PropertyOrder(1)][SerializeField] private Button restartButton;
        [Tooltip("Reference to the button to change to singleplayer")]
        [PropertyOrder(1)][SerializeField] private Button singleplayerButton;
        [Tooltip("Reference to the button to join a lobby")]
        [PropertyOrder(1)][SerializeField] private Button joinLobbyButton;
        [Tooltip("Reference to the button to leave the lobby")]
        [PropertyOrder(1)][SerializeField] private Button leaveLobbyButton;
        [Tooltip("Reference to the button to create a lobby")]
        [PropertyOrder(1)][SerializeField] private Button createLobbyButton;
        [Tooltip("Reference to the button to exit the game")]
        [PropertyOrder(1)][SerializeField] private Button exitGameButton;
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
            GameController.SwitchGameMode(GameMode.SinglePlayer);
        }
        
        /// <summary>
        /// Opens the lobby browser menu
        /// </summary>
        public void JoinLobby()
        {
            MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.LobbyJoinMenu);
        }

        /// <summary>
        /// Leaves the current lobby
        /// </summary>
        public void LeaveLobby()
        {
            SteamLobby.DisconnectFromLobby();
            base.Close(true);
        }
        
        /// <summary>
        /// Hosts a lobby
        /// </summary>
        public void CreateLobby()
        {
            MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.LobbyCreateMenu);
        }

        /// <summary>
        /// Exits the game
        /// </summary>
        public void ExitGame()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#endif
            Application.Quit();
        }
        
        public override MenuBase Open(MenuBase _CurrentActiveMenu)
        {
#if UNITY_EDITOR
            if (MenuController.DebugMultiplayerMenu)
            {
                this.joinLobbyButton.gameObject.SetActive(true);
                this.leaveLobbyButton.gameObject.SetActive(true);
                this.restartButton.interactable = true;
                this.singleplayerButton.interactable = true;
                this.createLobbyButton.interactable = true;
                this.exitGameButton.interactable = true;
                
                return base.Open(_CurrentActiveMenu);
            }
#endif
            if (SteamLobby.CurrentLobbyId == null)
            {
                this.joinLobbyButton.gameObject.SetActive(true);
                this.leaveLobbyButton.gameObject.SetActive(false);
                this.restartButton.interactable = true;
                this.singleplayerButton.interactable = true;
                this.createLobbyButton.interactable = true;
                this.exitGameButton.interactable = true;
            }
            else
            {
                if (SteamLobby.IsHost.Value.Value)
                {
                    return MenuController.Open_Close(_MenuControllerMenu => _MenuControllerMenu.LobbyHostMenu);
                }
                
                this.leaveLobbyButton.gameObject.SetActive(true);
                this.joinLobbyButton.gameObject.SetActive(false);
                this.restartButton.interactable = false; // TODO: Maybe allow while alone in lobby
                this.singleplayerButton.interactable = false;
                this.createLobbyButton.interactable = false;
                this.exitGameButton.interactable = false;
            }
            
            return base.Open(_CurrentActiveMenu);
        }

        #endregion
    }
}