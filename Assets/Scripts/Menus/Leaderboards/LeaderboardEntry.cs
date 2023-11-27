using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Steamworks.NET;

namespace Watermelon_Game.Menus.Leaderboards
{
    /// <summary>
    /// Holds the <see cref="TextMeshProUGUI"/> objects for one line in the leaderboard
    /// </summary>
    internal sealed class LeaderboardEntry : EnhancedScrollerCellView
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Image component on of this GameObject")]
        [SerializeField] private Image background;
        [Tooltip("TMP that displays the placement of the players")]
        [SerializeField] private TextMeshProUGUI placement;
        [Tooltip("TMP that displays the username of the players")]
        [SerializeField] private TextMeshProUGUI username;
        [Tooltip("TMP that displays the score of the players")]
        [SerializeField] private TextMeshProUGUI score;
        #endregion
        
        #region Methods
        /// <summary>
        /// Sets the data of this <see cref="LeaderboardEntry"/>
        /// </summary>
        /// <param name="_SteamUsersDataIndex">Index in <see cref="Leaderboard.steamUsers"/>, this object holds the data of</param>
        public void SetData(int _SteamUsersDataIndex)
        {
            base.dataIndex = _SteamUsersDataIndex;
            this.RefreshCellView();
        }

        public override void RefreshCellView()
        {
            base.RefreshCellView();

            var _steamUser = Leaderboard.SteamUsers[base.dataIndex];
            var _isLocalUser = _steamUser.SteamId == SteamManager.SteamID.m_SteamID;
            
            this.background.color = _isLocalUser ? this.background.color.WithAlpha(.5f) : this.background.color.WithAlpha(0);
            this.placement.text = _steamUser.GlobalRank.ToString();
            this.username.text = _steamUser.Username;
            this.score.text = _steamUser.Score.ToString();
        }
        #endregion
    }
}