namespace Watermelon_Game.Menus.ScrollViewTemplates
{
    /// <summary>
    /// Holds the data for one  <see cref="ScrollViewEntry"/>
    /// </summary>
    internal readonly struct ScrollViewData
    {
        #region Properties
        /// <summary>
        /// Doesn't need to be a string, can be anything
        /// </summary>
        public string Text { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// <see cref="ScrollViewData"/>
        /// </summary>
        /// <param name="_Text"><see cref="Text"/></param>
        // ReSharper disable once UnusedMember.Global
        public ScrollViewData(string _Text)
        {
            this.Text = _Text;
        }
        #endregion
    }
}