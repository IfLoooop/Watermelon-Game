using UnityEditor;
using UnityEngine;

namespace Watermelon_Game.Menu
{
    internal sealed class ExitMenu : MenuBase
    {
        #region Methods
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
        private void SetScrollPosition() { }
        #endregion
    }
}