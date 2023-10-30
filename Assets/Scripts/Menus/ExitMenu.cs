using UnityEditor;
using UnityEngine;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Contains exit and restart logic
    /// </summary>
    internal sealed class ExitMenu : MenuBase
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
        /// TODO: Use different animation
        /// Needed for the "Popup"-Animation
        /// </summary>
        private void DisableSetScrollPosition() { }
        #endregion
    }
}