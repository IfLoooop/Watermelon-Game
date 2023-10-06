using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.ExtensionMethods;

namespace Watermelon_Game.Menu
{
    internal abstract class MenuBase : MonoBehaviour
    {
        #region Fieds
        [SerializeField] private Menu menu;
        [SerializeField] private bool canBeClosedByDifferentMenu;
        #endregion

        #region Properties
        public Menu Menu => this.menu;
        public bool CanBeClosedByDifferentMenu => this.canBeClosedByDifferentMenu;
        #endregion

        #region Methods
        [CanBeNull]
        public virtual MenuBase Open_Close([CanBeNull] MenuBase _PreviousMenu)
        {
            if (_PreviousMenu != null && _PreviousMenu.menu != this.menu)
            {
                if (!_PreviousMenu.CanBeClosedByDifferentMenu)
                {
                    return _PreviousMenu;   
                }

                this.SetActive(_PreviousMenu);
            }
            
            MenuController.Instance.AudioSource.Play(MenuController.Instance.AudioClipStartTime);
            var _active = this.SetActive(this);

            return _active ? this : null;
        }

        private bool SetActive(MenuBase _Menu)
        {
            var _activeState = !_Menu.gameObject.activeSelf;
            _Menu.gameObject.SetActive(_activeState);

            return _activeState;
        }
        #endregion
    }
}