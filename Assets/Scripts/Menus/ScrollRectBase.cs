using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Base class for every <see cref="Menu"/> that contains a <see cref="ScrollRect"/>
    /// </summary>
    internal abstract class ScrollRectBase : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("The ScrollRect component of the menu")]
        [SerializeField] protected Scrollbar scrollBar;
        
        [Header("Settings")]
        [Tooltip("The type of the menu")]
        [SerializeField] private ContainerMenu menu;
        #endregion

        #region Fields
        /// <summary>
        /// The last position of the scrollbar before the menu was closed
        /// </summary>
        private float lastScrollPosition = 1;
        /// <summary>
        /// Indicates whether the <see cref="lastScrollPosition"/> should be set or not
        /// </summary>
        private bool setLastScrollPosition;
        #endregion
        
        #region Methods
        /// <summary>
        /// Used for logic when the <see cref="MenuContainer"/> opens
        /// </summary>
        public virtual void CustomOnEnable()
        {
            this.setLastScrollPosition = true;
        }
        
        /// <summary>
        /// Sets <see cref="lastScrollPosition"/> to the current value of the scrollbar and enables <see cref="setLastScrollPosition"/>
        /// </summary>
        public void SetLastScrollPosition()
        {
            this.lastScrollPosition = this.scrollBar.value;
        }
        
        /// <summary>
        /// Sets <see cref="setLastScrollPosition"/> to false <br/>
        /// <i>Is called at the end of the "MenuPopUp"-Animation</i>
        /// </summary>
        public void DisableSetScrollPosition()
        {
            this.setLastScrollPosition = false;
        }
        
        /// <summary>
        /// Is called by <see cref="ScrollRect.onValueChanged"/> of the <see cref="scrollBar"/>
        /// </summary>
        /// <param name="_Scrollbar">The <see cref="Scrollbar"/> whose <see cref="Scrollbar.value"/> was changed</param>
        public virtual void OnScrollPositionChanged(Scrollbar _Scrollbar)
        {
            if (this.setLastScrollPosition)
            {
                this.scrollBar.value = this.lastScrollPosition;
            }
        }
        
        /// <summary>
        /// Disables the given <see cref="ScrollRectBase"/> <see cref="GameObject"/> and enables this one
        /// </summary>
        /// <param name="_CurrentActiveMenu">The <see cref="ScrollRectBase"/> to disable the <see cref="GameObject"/> of</param>
        /// <returns>The currently active menu</returns>
        public virtual ScrollRectBase SetActive([CanBeNull] ScrollRectBase _CurrentActiveMenu)
        {
            if (_CurrentActiveMenu != null)
            {
                _CurrentActiveMenu.gameObject.SetActive(false);   
            }
            
            this.gameObject.SetActive(true);

            return this;
        }
        #endregion
    }
}