using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.ExtensionMethods;

namespace Watermelon_Game.Menu
{
    internal abstract class MenuBase : MonoBehaviour
    {
        #region Inspector Fieds
        [SerializeField] private Menu menu;
        [SerializeField] private bool canBeClosedByDifferentMenu;
        [SerializeField] protected ScrollRect scrollRect;
        #endregion

        #region Fields
        private float currentScrollPosition;
        #endregion
        
        #region Properties
        public Menu Menu => this.menu;
        #endregion

        #region Methods
        private void OnDisable()
        {
            this.currentScrollPosition = this.scrollRect.verticalScrollbar.value;
        }
        
        /// <summary>
        /// Is called at the end of the "MenuPopUp"-Animation
        /// </summary>
        private void SetScrollPosition()
        {
            if (this.menu == Menu.GameOver)
            {
                this.scrollRect.verticalScrollbar.value = 1;   
            }
            else
            {
                this.scrollRect.verticalScrollbar.value = this.currentScrollPosition;   
            }
        }
        
        [CanBeNull]
        public virtual MenuBase Open_Close([CanBeNull] MenuBase _PreviousMenu)
        {
            if (_PreviousMenu != null && _PreviousMenu.menu != this.menu)
            {
                if (!_PreviousMenu.canBeClosedByDifferentMenu)
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