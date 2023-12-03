using System;

namespace Watermelon_Game.Menus.InfoMenus
{
    /// <summary>
    /// Menu to display various text to the user
    /// </summary>
    internal sealed class InfoMenu : InfoMenuBase
    {
        #region Events
        /// <summary>
        /// Is called when <see cref="InfoMenu"/> closes
        /// </summary>
        public static event Action OnInfoMenuClose;
        #endregion

        #region Methods
        public override InfoMenuBase Close(bool _PlaySound)
        {
            OnInfoMenuClose?.Invoke();
            return base.Close(_PlaySound);
        }
        
        /// <summary>
        /// Closes this menu<br/>
        /// <i>Used on the "Close Menu" button (for mouse)</i>
        /// </summary>
        public void Close()
        {
            MenuController.CloseMenuPopup();
            this.Close(true);
        }
        #endregion
    }
}