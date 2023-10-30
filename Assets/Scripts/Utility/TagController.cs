namespace Watermelon_Game.Utility
{
    /// <summary>
    /// Caches all needed Tags
    /// </summary>
    internal static class TagController
    {
        #region Constants
        /// <summary>
        /// Tag of the bottom wall
        /// </summary>
        private const string WALL_BOTTOM = "Wall Bottom";
        #endregion

        #region Properties
        /// <summary>
        /// Returns the Tag of the bottom wall
        /// </summary>
        public static string WallBottom => WALL_BOTTOM;
        #endregion
    }
}