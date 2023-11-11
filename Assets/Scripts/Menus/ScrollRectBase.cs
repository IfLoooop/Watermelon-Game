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
        [SerializeField] protected Scrollbar scrollBar;
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
        protected virtual void OnDisable()
        {
            this.lastScrollPosition = this.scrollBar != null ? this.scrollBar.value : 1;
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
        /// Is called by <see cref="ScrollRect.onValueChanged"/> of the <see cref="scrollBar"/>
        /// </summary>
        /// <param name="_Scrollbar">The <see cref="Scrollbar"/> whose <see cref="Scrollbar.value"/> was changed</param>
        public virtual void OnScrollPositionChanged(Scrollbar _Scrollbar)
        {
            if (setLastScrollPosition)
            {
                if (base.Menu == Menu.GameOver)
                {
                    this.scrollBar.value = 1;   
                }
                else
                {
                    this.scrollBar.value = this.lastScrollPosition; 
                }
            }
        }
        #endregion
    }
}