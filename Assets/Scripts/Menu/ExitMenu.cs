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

        public override MenuBase Open_Close(MenuBase _CurrentActiveMenu, bool _ForceClose = false)
        {
            if (_CurrentActiveMenu is { Menu: Menu.Exit })
            {
                GameController.Restart();
            }
            
            return base.Open_Close(_CurrentActiveMenu, _ForceClose);
        }
        
        /// <summary>
        /// TODO: Use different animation
        /// Needed for the "Popup"-Animation
        /// </summary>
        private void SetScrollPosition() { }
        #endregion
    }
}