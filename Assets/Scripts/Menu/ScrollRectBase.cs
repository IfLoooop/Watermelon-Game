using UnityEngine;
using UnityEngine.UI;

namespace Watermelon_Game.Menu
{
    internal abstract class ScrollRectBase : MenuBase
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] protected ScrollRect scrollRect;
        #endregion

        #region Fields
        private float currentScrollPosition;
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
            // TODO: Call this method in "OnValueChanged" of the scrollbar
            if (base.Menu == Menu.GameOver)
            {
                this.scrollRect.verticalScrollbar.value = 1;   
            }
            else
            {
                this.scrollRect.verticalScrollbar.value = this.currentScrollPosition;   
            }
        }
        #endregion
    }
}