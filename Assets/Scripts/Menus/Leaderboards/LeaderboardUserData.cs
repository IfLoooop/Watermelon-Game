namespace Watermelon_Game.Menus.Leaderboards
{
    /// <summary>
    /// Contains leaderboard data for one user
    /// </summary>
    internal struct LeaderboardUserData
    {
        #region Properties
        /// <summary>
        /// Steam ID of the user
        /// </summary>
        public ulong SteamId { get; init; }
        /// <summary>
        /// The users global rank in the leaderboard
        /// </summary>
        public int GlobalRank { get; init; }
        /// <summary>
        /// Username corresponding to the <see cref="SteamId"/>
        /// </summary>
        public string Username { get; init; }
        /// <summary>
        /// Score entry in the leaderboard
        /// </summary>
        public int Score { get; init; }
        #endregion

        #region Constrcutors
        /// <summary>
        /// Copies the values of the given <see cref="LeaderboardUserData"/> into a new one
        /// </summary>
        /// <param name="_LeaderboardUserData">The <see cref="LeaderboardUserData"/> to copy the values from</param>
        /// <param name="_Username"><see cref="Username"/></param>
        public LeaderboardUserData(LeaderboardUserData _LeaderboardUserData, string _Username)
        {
            this.SteamId = _LeaderboardUserData.SteamId;
            this.GlobalRank = _LeaderboardUserData.GlobalRank;
            this.Username = _Username;
            this.Score = _LeaderboardUserData.Score;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Copies the values of the given <see cref="LeaderboardUserData"/> into a new one
        /// </summary>
        /// <param name="_LeaderboardUserData">The <see cref="LeaderboardUserData"/> to copy the values from</param>
        /// <param name="_GlobalRank"><see cref="GlobalRank"/></param>
        public LeaderboardUserData(LeaderboardUserData _LeaderboardUserData, int _GlobalRank)
        {
            this.SteamId = _LeaderboardUserData.SteamId;
            this.GlobalRank = _GlobalRank;
            this.Username = _LeaderboardUserData.Username;
            this.Score = _LeaderboardUserData.Score;
        }
#endif
        #endregion
    }
}