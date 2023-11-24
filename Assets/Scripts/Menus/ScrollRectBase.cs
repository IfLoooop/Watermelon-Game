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
        /// Locks the scroll position to <see cref="lastScrollPosition"/> while true
        /// </summary>
        private bool lockScrollPosition;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="menu"/>
        /// </summary>
        public ContainerMenu Menu => this.menu;
        #endregion
        
        #region Methods
        /// <summary>
        /// Sets the value of <see cref="lockScrollPosition"/>
        /// </summary>
        public void LockScrollPosition(bool _Value)
        {
            this.lockScrollPosition = _Value;
        }
        
        /// <summary>
        /// Is called by <see cref="ScrollRect.onValueChanged"/> of the <see cref="scrollBar"/>
        /// </summary>
        /// <param name="_Scrollbar">The <see cref="Scrollbar"/> whose <see cref="Scrollbar.value"/> was changed</param>
        public virtual void OnScrollPositionChanged(Scrollbar _Scrollbar)
        {
            if (this.lockScrollPosition)
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
        
        /// <summary>
        /// Disables this <see cref="ScrollRectBase"/>
        /// </summary>
        public void SetInactive()
        {
            this.lastScrollPosition = this.scrollBar.value;
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}