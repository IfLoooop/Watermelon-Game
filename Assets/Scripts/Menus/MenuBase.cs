using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.Audio;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Base class for every <see cref="Menus.Menu"/>
    /// </summary>
    internal abstract class MenuBase : MonoBehaviour
    {
        #region Inspector Fieds
        [Header("Settings")]
        [Tooltip("The type of the menu")]
        [SerializeField] private Menu menu;
        [Tooltip("Other menus will not be able to close this one, if set to true")]
        [SerializeField] private bool canNotBeClosedByDifferentMenu;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="menu"/>
        /// </summary>
        public Menu Menu => this.menu;
        #endregion

        #region Methods
        /// <summary>
        /// Controls the open/close logic of a <see cref="Menus.Menu"/>
        /// </summary>
        /// <param name="_CurrentActiveMenu"><see cref="MenuController.currentActiveMenu"/></param>
        /// <param name="_ForceClose">Forces the active menu to close even if <see cref="canNotBeClosedByDifferentMenu"/> is true</param>
        /// <returns>The new active <see cref="Menus.Menu"/> or null if all menus are closed</returns>
        [CanBeNull]
        public virtual MenuBase Open_Close([CanBeNull] MenuBase _CurrentActiveMenu, bool _ForceClose = false)
        {
            AudioPool.PlayClip(AudioClipName.MenuPopup);
            
            if (_ForceClose && _CurrentActiveMenu != null)
            {
                this.SwitchActiveState(_CurrentActiveMenu, true);
                return null;
            }
            
            if (_CurrentActiveMenu != null && _CurrentActiveMenu.menu != this.menu)
            {
                if (_CurrentActiveMenu.canNotBeClosedByDifferentMenu)
                {
                    return _CurrentActiveMenu;   
                }

                this.SwitchActiveState(_CurrentActiveMenu);
            }
            
            var _active = this.SwitchActiveState(this);

            return _active ? this : null;
        }

        /// <summary>
        /// Switches the active state of the given <see cref="MenuBase"/>
        /// </summary>
        /// <param name="_Menu">The <see cref="MenuBase"/> to switch the active state of</param>
        /// <param name="_ForceClose">Forces the active menu to close even if <see cref="canNotBeClosedByDifferentMenu"/> is true</param>
        /// <returns></returns>
        private bool SwitchActiveState(MenuBase _Menu, bool _ForceClose = false)
        {
            if (!_ForceClose)
            {
                var _activeState = !_Menu.gameObject.activeSelf;
                _Menu.gameObject.SetActive(_activeState);
                return _activeState;   
            }
            
            _Menu.gameObject.SetActive(false);
            return false;
        }
        #endregion
    }
}