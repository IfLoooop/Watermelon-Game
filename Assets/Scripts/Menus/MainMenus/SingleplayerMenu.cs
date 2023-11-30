using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Steamworks.NET;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Menus.MainMenus
{
    /// <summary>
    /// Main menu while in singleplayer mode
    /// </summary>
    internal sealed class SingleplayerMenu : MainMenuBase
    {
        #region Inspector Fields
        [Tooltip("Reference to the button to switch to multiplayer")]
        [PropertyOrder(1)][SerializeField] private Button multiplayerButton;
        #endregion
        
        #region Methods
        public override MenuBase Open(MenuBase _CurrentActiveMenu)
        {
            if (!SteamManager.Initialized)
            {
                this.multiplayerButton.interactable = false;
            }
            
            return base.Open(_CurrentActiveMenu);
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
        
        /// <summary>
        /// Changes the <see cref="GameMode"/> to <see cref="GameMode.MultiPlayer"/>
        /// </summary>
        public void SwitchToMultiplayer()
        {
            CurrentGameMode = GameMode.MultiPlayer;
            GameModeTransition(CurrentGameMode);
        }
        #endregion
    }
}