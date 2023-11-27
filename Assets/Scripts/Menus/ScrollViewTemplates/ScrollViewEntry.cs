using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;

namespace Watermelon_Game.Menus.ScrollViewTemplates
{
    /// <summary>
    /// Displays the data of one <see cref="ScrollViewData"/>
    /// </summary>
    internal sealed class ScrollViewEntry : EnhancedScrollerCellView
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Doesn't need to be a tmp, can be anything")]
        [SerializeField] private TextMeshProUGUI tmp;
        #endregion
        
        #region Methods
        /// <summary>
        /// Sets the data of this <see cref="ScrollViewEntry"/>
        /// </summary>
        /// <param name="_DataIndex">The index of the element in <see cref="ScrollViewTemplate"/>.<see cref="ScrollViewTemplate.dataList"/> to get the data from</param>
        public void SetData(int _DataIndex)
        {
            base.dataIndex = _DataIndex;
            this.RefreshCellView(); // TODO: If you don't use the override of "RefreshCellView()", you can just move the logic to this method
        }

        /// <summary>
        /// Not necessarily needed, only when you use <see cref="EnhancedScroller"/>.<see cref="EnhancedScroller.RefreshActiveCellViews"/> -> <br/>
        /// <i><see cref="ScrollViewTemplate"/>.<see cref="ScrollViewTemplate.RefreshActiveCellViews"/></i>
        /// </summary>
        public override void RefreshCellView()
        {
            base.RefreshCellView();

            this.tmp.text = ScrollViewTemplate.DataList[base.dataIndex].Text; // TODO: Change to where you get the data from
        }
        #endregion
    }
}