using UnityEditor;
using UnityEngine;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Menus.MainMenus
{
    /// <summary>
    /// Main menu while in singleplayer mode
    /// </summary>
    internal sealed class SingleplayerMenu : MainMenuBase
    {
        #region Methods
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