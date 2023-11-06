namespace Watermelon_Game.Editor.Steam
{
    /// <summary>
    /// Contains identifier for markup languages
    /// </summary>
    internal readonly struct Markup
    {
        #region Properties
        /// <summary>
        /// Symbols the markup code starts with
        /// </summary>
        public string StartsWith { get; }
        /// <summary>
        /// Symbols the markup code ends with
        /// </summary>
        public string EndsWith { get; }
        /// <summary>
        /// Symbols that are in between <see cref="StartsWith"/> and <see cref="EndsWith"/>
        /// </summary>
        public string[] Contains { get; }
        /// <summary>
        /// Indicates whether the characters between <see cref="StartsWith"/> and <see cref="EndsWith"/> should be removed
        /// </summary>
        public bool RemoveInBetween { get; }
        /// <summary>
        /// Indicates whether this <see cref="Markup"/> allows white spaces between <see cref="StartsWith"/> and <see cref="EndsWith"/>
        /// </summary>
        public bool AllowSpacesBetween { get; }
        #endregion

        #region Constructor
        /// <param name="_StartsWith"><see cref="StartsWith"/></param>
        /// <param name="_EndsWith"><see cref="EndsWith"/></param>
        /// <param name="_RemoveInBetween"><see cref="RemoveInBetween"/></param>
        /// <param name="_AllowSpacesBetween"><see cref="AllowSpacesBetween"/></param>
        /// <param name="_Contains"><see cref="Contains"/></param>
        public Markup(string _StartsWith, string _EndsWith, bool _RemoveInBetween = false, bool _AllowSpacesBetween = true, params string[] _Contains)
        {
            this.StartsWith = _StartsWith;
            this.EndsWith = _EndsWith;
            this.Contains = _Contains;
            this.AllowSpacesBetween = _AllowSpacesBetween;
            this.RemoveInBetween = _RemoveInBetween;
        }
        #endregion
    }
}