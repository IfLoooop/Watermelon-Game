using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace Watermelon_Game.Menus.ScrollViewTemplates
{
    /// <summary>
    /// Template for how to use the <see cref="EnhancedScroller"/> <br/>
    /// <i>Replace this class on the prefab</i>
    /// </summary>
    internal sealed class ScrollViewTemplate : MonoBehaviour, IEnhancedScrollerDelegate
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("EnhancedScroller component")]
        [SerializeField] private EnhancedScroller scroller;
        [Tooltip("The prefab that is used for every entry")]
        [SerializeField] private ScrollViewEntry prefab;
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="ScrollViewTemplate"/>
        /// </summary>
        private static ScrollViewTemplate instance;
        /// <summary>
        /// The height of one <see cref="prefab"/> <br/>
        /// <i>Can be set to any value, doesn't have to be a value from the <see cref="prefab"/></i>
        /// </summary>
        private float entryHeight;
        /// <summary>
        /// Contains the data that can currently be shown in the <see cref="scroller"/> <br/>
        /// <i>If the content of this list changes, <see cref="scroller"/>.<see cref="EnhancedScroller.ReloadData"/> has to be called</i>
        /// </summary>
        private readonly List<ScrollViewData> dataList = new();
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="dataList"/>
        /// </summary>
        public static List<ScrollViewData> DataList => instance.dataList;
        #endregion
        
        #region Members
        private void Awake()
        {
            instance = this;
            this.scroller.Delegate = this;
            this.entryHeight = (this.prefab.transform as RectTransform)!.sizeDelta.y;
        }

        /// <summary>
        /// Call this to display the values of <see cref="dataList"/> inside the <see cref="scroller"/> <br/>
        /// <i>Has to be called again, when the size of <see cref="dataList"/> changes</i>
        /// </summary>
        /// <param name="_ScrollPosition">
        /// Normalized scroll position (0 - 1) <br/>
        /// <b>0:</b> Top <br/>
        /// <b>1:</b> Bottom
        /// </param>
        // ReSharper disable once UnusedMember.Global
        public void ReloadData(float _ScrollPosition = 0)
        {
            this.scroller.ReloadData(_ScrollPosition);
        }

        /// <summary>
        /// Can be called instead of <see cref="ReloadData"/>, when the values of the <see cref="ScrollViewData"/> in <see cref="dataList"/> change (Less expensive) <br/>
        /// <i>If the size of <see cref="dataList"/> changes, you have to call <see cref="ReloadData"/></i>
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void RefreshActiveCellViews()
        {
            this.scroller.RefreshActiveCellViews();
        }
        
        public int GetNumberOfCells(EnhancedScroller _Scroller)
        {
            return this.dataList.Count;
        }

        public float GetCellViewSize(EnhancedScroller _Scroller, int _DataIndex)
        { 
            return this.entryHeight;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller _Scroller, int _DataIndex, int _CellIndex)
        {
            var _scrollViewEntry = (_Scroller.GetCellView(this.prefab) as ScrollViewEntry)!; // TODO: Change to the Component type of your prefab

#if UNITY_EDITOR // Helpful for debugging
            _scrollViewEntry.name = string.Concat("Cell", _DataIndex);
#endif
            _scrollViewEntry.SetData(_DataIndex);
            
            return _scrollViewEntry;
        }
        #endregion
    }
}