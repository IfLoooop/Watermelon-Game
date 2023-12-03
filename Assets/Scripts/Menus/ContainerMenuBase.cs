using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Menus.MenuContainers;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Base class for every <see cref="Menu"/> that contains a <see cref="ScrollRect"/>
    /// </summary>
    internal abstract class ContainerMenuBase : MonoBehaviour, IScrollBase
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("The menu tab associated with this menu")]
        [PropertyOrder(1)][SerializeField] protected Button tab;
        [Tooltip("The Scrollbar component of the menu")]
        [PropertyOrder(1)][SerializeField] protected Scrollbar scrollBar;
        
        [Header("Settings")]
        [Tooltip("The type of the menu")]
        [PropertyOrder(2)][SerializeField] private ContainerMenu menu;
        #endregion

        #region Fields
        /// <summary>
        /// The default <see cref="ColorBlock.normalColor"/> of <see cref="tab"/>
        /// </summary>
        private Color defaultNormalColor;
        /// <summary>
        /// The default <see cref="ColorBlock.highlightedColor"/> of <see cref="tab"/>
        /// </summary>
        private Color defaultHighlightedColor;
        /// <summary>
        /// The default <see cref="ColorBlock.selectedColor"/> of <see cref="tab"/>
        /// </summary>
        private Color defaultSelectedColor;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="menu"/>
        /// </summary>
        public ContainerMenu Menu => this.menu;
        /// <summary>
        /// Reference to <see cref="IScrollBase"/>
        /// </summary>
        public IScrollBase ScrollBase { get; private set; }
        public float LastScrollPosition { get; set; } = 1;
        public bool ScrollPositionLocked { get; set; }
        #endregion
        
        #region Methods
        protected virtual void Awake()
        {
            this.ScrollBase = this;
            this.defaultNormalColor = this.tab.colors.normalColor;
            this.defaultHighlightedColor = this.tab.colors.highlightedColor;
            this.defaultSelectedColor = this.tab.colors.selectedColor;
        }
        
        public void OnScrollPositionChanged()
        {
            this.ScrollBase.SetScrollPosition(this.scrollBar);
        }
        
        /// <summary>
        /// Disables the given <see cref="ContainerMenuBase"/> <see cref="GameObject"/> and enables this one
        /// </summary>
        /// <param name="_CurrentActiveMenu">The <see cref="ContainerMenuBase"/> to disable the <see cref="GameObject"/> of</param>
        /// <returns>The currently active menu</returns>
        public virtual ContainerMenuBase SetActive([CanBeNull] ContainerMenuBase _CurrentActiveMenu)
        {
            if (_CurrentActiveMenu != null)
            {
                _CurrentActiveMenu.DeselectTab();
                _CurrentActiveMenu.gameObject.SetActive(false);   
            }
            
            this.SelectTab();
            this.gameObject.SetActive(true);

            return this;
        }

        /// <summary>
        /// Sets the <see cref="Button.colors"/> of this <see cref="tab"/> their selected state
        /// </summary>
        private void SelectTab()
        {
            var _colors = this.tab.colors;
            _colors.normalColor = this.tab.colors.pressedColor;
            _colors.highlightedColor = this.tab.colors.pressedColor;
            _colors.selectedColor = this.tab.colors.pressedColor;
            this.tab.colors = _colors;
        }

        /// <summary>
        /// Sets the <see cref="Button.colors"/> of this <see cref="tab"/> their default state
        /// </summary>
        private void DeselectTab()
        {
            var _colors = this.tab.colors;
            _colors.normalColor = this.defaultNormalColor;
            _colors.highlightedColor = this.defaultHighlightedColor;
            _colors.selectedColor = this.defaultSelectedColor;
            this.tab.colors = _colors;
        }
        
        /// <summary>
        /// Disables this <see cref="ContainerMenuBase"/>
        /// </summary>
        public void SetInactive()
        {
            this.LastScrollPosition = this.scrollBar.value;
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}