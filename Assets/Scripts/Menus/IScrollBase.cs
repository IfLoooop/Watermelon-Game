using UnityEngine.UI;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Contains logic to lock the scroll position of a <see cref="Scrollbar"/>
    /// </summary>
    internal interface IScrollBase
    {
        #region Properties
        /// <summary>
        /// The scroll position to lock the <see cref="Scrollbar"/> in <see cref="SetScrollPosition"/> to
        /// </summary>
        public float LastScrollPosition { get; set; }
        /// <summary>
        /// Locks the scroll position to <see cref="LastScrollPosition"/> for the given <see cref="Scrollbar"/> in <see cref="SetScrollPosition"/>, while true
        /// </summary>
        public bool ScrollPositionLocked { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the value of <see cref="ScrollPositionLocked"/> <br/>
        /// <i>Set to true, when a menu is opened and false on the end of the "popup"-animation</i>
        /// </summary>
        public void LockScrollPosition(bool _Value)
        {
            this.ScrollPositionLocked = _Value;
        }

        /// <summary>
        /// Sets <see cref="LastScrollPosition"/> to the scroll position of the given <see cref="Scrollbar"/> <br/>
        /// <i>Use when a menu is closed</i>
        /// </summary>
        /// <param name="_Scrollbar">The <see cref="Scrollbar"/> to get the scroll position of</param>
        public void SetLastScrollPosition(Scrollbar _Scrollbar)
        {
            this.LastScrollPosition = _Scrollbar.value;
        }
        
        /// <summary>
        /// Call this method in an "OnValueChanged"-callback from a <see cref="Scrollbar"/>, and use <see cref="SetScrollPosition"/>
        /// </summary>
        // ReSharper disable once UnusedMemberInSuper.Global
        public void OnScrollPositionChanged();
        
        /// <summary>
        /// Sets the value of the given <see cref="Scrollbar"/> to <see cref="LastScrollPosition"/> when <see cref="ScrollPositionLocked"/> is true <br/>
        /// <i>Use in <see cref="OnScrollPositionChanged"/></i>
        /// </summary>
        /// <param name="_ScrollBar"></param>
        public void SetScrollPosition(Scrollbar _ScrollBar)
        {
            if (this.ScrollPositionLocked)
            {
                _ScrollBar.value = this.LastScrollPosition;
            }
        }
        #endregion
    }
}