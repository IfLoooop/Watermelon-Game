using UnityEngine;
using UnityEngine.UI;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Base class for every <see cref="Menu"/> that contains a <see cref="ScrollRect"/>
    /// </summary>
    internal abstract class ScrollRectBase : MenuBase
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("The ScrollRect component of the menu")]
        [SerializeField] protected ScrollRect scrollRect;
        #endregion

        #region Fields
        /// <summary>
        /// The last position of the scrollbar before the menu was closed
        /// </summary>
        private float lastScrollPosition;
        /// <summary>
        /// Indicates whether the <see cref="lastScrollPosition"/> should be set or not
        /// </summary>
        private bool setLastScrollPosition = true;
        #endregion
        
        #region Methods
        protected void OnDisable()
        {
            this.lastScrollPosition = this.scrollRect.verticalScrollbar.value;
            setLastScrollPosition = true;
        }
        
        /// <summary>
        /// Is called at the end of the "MenuPopUp"-Animation
        /// </summary>
        public void DisableSetScrollPosition()
        {
            setLastScrollPosition = false;
        }
        
        /// <summary>
        /// Is called by <see cref="ScrollRect.onValueChanged"/> of the <see cref="scrollRect"/>
        /// </summary>
        public void SetScrollPosition()
        {
            if (setLastScrollPosition)
            {
                if (base.Menu == Menu.GameOver)
                {
                    this.scrollRect.verticalScrollbar.value = 1;   
                }
                else
                {
                    this.scrollRect.verticalScrollbar.value = this.lastScrollPosition; 
                }
            }
        }
        #endregion
    }
}