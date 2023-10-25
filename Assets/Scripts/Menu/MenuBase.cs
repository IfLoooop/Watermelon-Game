using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.ExtensionMethods;

namespace Watermelon_Game.Menu
{
    internal abstract class MenuBase : MonoBehaviour
    {
        #region Inspector Fieds
        [Header("Settings")]
        [SerializeField] private Menu menu;
        [SerializeField] private bool canNotBeClosedByDifferentMenu;
        #endregion
        
        #region Properties
        public Menu Menu => this.menu;
        #endregion

        #region Methods
        [CanBeNull]
        public virtual MenuBase Open_Close([CanBeNull] MenuBase _CurrentActiveMenu, bool _ForceClose = false)
        {
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

        private bool SwitchActiveState(MenuBase _Menu, bool _ForceClose = false)
        {
            MenuController.Instance.AudioSource.Play(MenuController.Instance.AudioClipStartTime);
            
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