using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Steamworks.NET;

namespace Watermelon_Game.Menus.Leaderboards
{
    // TODO:
    // Add sorting options (ascending/descending)
    
    /// <summary>
    /// Leaderboard menu
    /// </summary>
    internal sealed class Leaderboard : ContainerMenuBase, IEnhancedScrollerDelegate
    {
        #region Inspector Fields
        [Tooltip("Displays the current/max page")]
        [PropertyOrder(1)][SerializeField] private TextMeshProUGUI paging;
        [Tooltip("Button to refresh all leaderboard entries")]
        [PropertyOrder(1)][SerializeField] private Button refreshButton;
        [Tooltip("Image component of the refresh button")]
        [PropertyOrder(1)][SerializeField] private Image refreshButtonImage;
        [Tooltip("EnhancedScroller component")]
        [PropertyOrder(1)][SerializeField] private EnhancedScroller scroller;
        [Tooltip("Displays one row in the leaderboard")]
        [PropertyOrder(1)][SerializeField] private LeaderboardEntry leaderboardEntryPrefab;
        
        [Tooltip("The maximum number of entries per page")]
        [PropertyOrder(2)][SerializeField] private uint maxEntriesPerPage = 100;
        #endregion
        
        #region Fields
        /// <summary>
        /// The heit of one <see cref="leaderboardEntryPrefab"/>
        /// </summary>
        private float leaderboardEntryHeight;
        /// <summary>
        /// Timestamp in seconds when the refresh button was last pressed
        /// </summary>
        private float refreshTimestamp;
        /// <summary>
        /// Cooldown in seconds, how long the refresh button stays deactivated after having been pressed
        /// </summary>
        private const uint REFRESH_COOLDOWN = 60;
        /// <summary>
        /// The currently active page in the leaderboard menu <br/>
        /// <i>First page starts at 0</i>
        /// </summary>
        private int activePage;
        /// <summary>
        /// The previous active page before the friends toggle was activated
        /// </summary>
        private int previousActivePage;
        /// <summary>
        /// The previous scroll position before the friends toggle was activated
        /// </summary>
        private float previousScrollPosition;
        /// <summary>
        /// Indicates whether the <see cref="Toggle"/> to only display friends in the leaderboard is on or not
        /// </summary>
        private bool friendsToggleIsOn;
        #endregion

        #region Properties
        /// <summary>
        /// The last page number
        /// </summary>
        private int LastPage => (int)(this.UserList.Count / this.maxEntriesPerPage);
        /// <summary>
        /// Hold the data for the currently active page in the leaderboard menu
        /// </summary>
        public static List<LeaderboardUserData> SteamUsers { get; } = new();
        /// <summary>
        /// Returns <see cref="SteamUsers"/> if <see cref="friendsToggleIsOn"/> it true, otherwise <see cref="SteamLeaderboard"/>.<see cref="SteamLeaderboard.SteamUsers"/>
        /// </summary> // TODO: Change "SteamLeaderboard.Friends.ToList()", maybe store "Friends" not as a "ConcurrentBag"
        private List<LeaderboardUserData> UserList => this.friendsToggleIsOn ? SteamLeaderboard.Friends.ToList() : SteamLeaderboard.SteamUsers;
        #endregion
        
        #region Methods
        protected override void Awake()
        {
            base.Awake();
            this.leaderboardEntryHeight = (this.leaderboardEntryPrefab.transform as RectTransform)!.sizeDelta.y;
            this.scroller.Delegate = this;
        }

        private void OnEnable()
        {
            SteamLeaderboard.OnLeaderboardScoresDownloaded += LeaderboardScoresDownloaded;
            SteamLeaderboard.OnUsernameFound += this.RefreshLeaderboardEntries;
        }

        private void OnDisable()
        {
            SteamLeaderboard.OnLeaderboardScoresDownloaded -= LeaderboardScoresDownloaded;
            SteamLeaderboard.OnUsernameFound -= this.RefreshLeaderboardEntries;
        }
        
        private void Start()
        {
            this.RefreshLeaderboard();
            this.GetLeaderboardEntries();
        }

        private void Update()
        {
            this.SetRefreshFill();
        }
        
        /// <summary>
        /// Sets the <see cref="Image.fillAmount"/> of the <see cref="refreshButtonImage"/>
        /// </summary>
        private void SetRefreshFill()
        {
            if (this.refreshButton.interactable)
            {
                return;
            }
            
            this.refreshButtonImage.fillAmount = Mathf.Clamp01((Time.time - this.refreshTimestamp) / REFRESH_COOLDOWN);
            this.EnableRefreshButton();
        }

        /// <summary>
        /// Makes the <see cref="refreshButton"/> <see cref="Button.interactable"/> again
        /// </summary>
        private void EnableRefreshButton()
        {
            if (this.refreshButtonImage.fillAmount >= 1)
            {
                if (!SteamLeaderboard.ProcessingLeaderboardEntries)
                {
                    this.refreshButton.interactable = true;
                }
            }
        }
        
        /// <summary>
        /// Sets the <see cref="TextMeshProUGUI.text"/> of <see cref="paging"/> to <see cref="activePage"/> and <see cref="LastPage"/>
        /// </summary>
        private void SetPaging()
        {
            this.paging.text = $"{this.activePage + 1}/{this.LastPage + 1}";
        }
        
        /// <summary>
        /// Moves <see cref="activePage"/> into the direction of the given value
        /// </summary>
        /// <param name="_Direction">Positive = forward, negative = backwards</param>
        public void MovePage(int _Direction)
        {
            this.activePage += _Direction;
            
            if (this.activePage < 0)
            {
                this.activePage = this.LastPage;
            }
            else if (this.activePage > this.LastPage)
            {
                this.activePage = 0;
            }
            
            this.GetLeaderboardEntries();
        }
        
        /// <summary>
        /// Moves the <see cref="activePage"/> and <see cref="EnhancedScroller.ScrollPosition"/> of the <see cref="scroller"/> to the entry of <see cref="SteamManager.SteamID"/>
        /// </summary>
        public void JumpToSelf()
        {
            var _index = this.UserList.FindIndexParallel(_SteamUser => _SteamUser.SteamId == SteamManager.SteamID.m_SteamID);
            if (_index != -1)
            {
                var _targetPage = (int)(_index / this.maxEntriesPerPage);
                var _dataIndex = (int)(_index - _targetPage * this.maxEntriesPerPage);
                var _direction = 0;
                
                if (_targetPage > this.activePage)
                {
                    _direction += _targetPage - this.activePage;
                }
                else if (this.activePage > _targetPage)
                {
                    _direction -= this.activePage - _targetPage;
                }
                
                this.MovePage(_direction);
                
                var _scrollPosition = this.scroller.GetScrollPositionForDataIndex(_dataIndex, EnhancedScroller.CellViewPositionEnum.Before);
                this.scroller.SetScrollPositionImmediately(_scrollPosition);   
            }
        }

        /// <summary>
        /// Force downloads the leaderboard and refreshes all entries
        /// </summary>
        public void RefreshLeaderboard()
        {
            this.refreshButton.interactable = false;
            this.refreshTimestamp = Time.time;
            this.refreshButtonImage.fillAmount = 0;
            
            SteamUsers.Clear();
            this.scroller.ReloadData();
            SteamLeaderboard.DownloadLeaderboardScores();
        }
        
        /// <summary>
        /// Only displays friends (and the local user), if the toggle is on
        /// </summary>
        /// <param name="_Toggle">The <see cref="Toggle"/> that called this method</param>
        public void ToggleFriends(Toggle _Toggle)
        {
            this.friendsToggleIsOn = _Toggle.isOn;
            
            if (_Toggle.isOn)
            {
                this.previousActivePage = this.activePage;
                this.previousScrollPosition = this.scroller.NormalizedScrollPosition;
                this.activePage = 0;
                
                SteamUsers.Clear();
                SteamUsers.AddRange(SteamLeaderboard.Friends);
                
                this.scroller.ReloadData();
                this.SetPaging();
            }
            else
            {
                this.activePage = this.previousActivePage;
                this.GetLeaderboardEntries(this.previousScrollPosition);
            }
        }
        
        /// <summary>
        /// <see cref="GetLeaderboardEntries"/>
        /// </summary>
        private void LeaderboardScoresDownloaded()
        {
            // Needs to be called twice, otherwise won't display the entries correctly when the menu is opened, while a new entry is added
            // Also shitty solution but it works
            this.GetLeaderboardEntries();
            this.GetLeaderboardEntries();
        }
        
        /// <summary>
        /// Fills <see cref="SteamUsers"/> with entries from <see cref="SteamLeaderboard"/>.<see cref="SteamLeaderboard.SteamUsers"/>, depending on the current <see cref="activePage"/>
        /// </summary>
        /// <param name="_ScrollPosition">The normalized position of the scroller between 0 and 1 (0 = Top, 1 = Bottom)</param>
        private void GetLeaderboardEntries(float _ScrollPosition = 0)
        {
            SteamUsers.Clear();
            
            // ReSharper disable once InconsistentNaming
            for (var i = (int)(this.activePage * this.maxEntriesPerPage); i < this.UserList.Count; i++)
            {
                SteamUsers.Add(this.UserList[i]);
                
                if (SteamUsers.Count == this.maxEntriesPerPage)
                {
                    break;
                }
            }
            
            this.scroller.ReloadData(_ScrollPosition);
            this.SetPaging();
            this.EnableRefreshButton();
        }
        
        /// <summary>
        /// Updates the name of an entry on the <see cref="activePage"/> -> <see cref="SteamLeaderboard.OnUsernameFound"/>
        /// </summary>
        /// <param name="_Index">The index in <see cref="SteamLeaderboard.SteamUsers"/>, for whom the username was found</param>
        private void RefreshLeaderboardEntries(int _Index)
        {
            var _dataIndex = (int)(_Index - this.activePage * this.maxEntriesPerPage);
            if (_dataIndex > SteamUsers.Count - 1)
            {
                return;
            }
            if (_dataIndex < 0)
            {
                return;
            }
            
            var _currentUser = SteamUsers[_dataIndex];
            var _incomingUser = SteamLeaderboard.SteamUsers[_Index];
            
            if (_currentUser.SteamId == _incomingUser.SteamId)
            {
                SteamUsers[_dataIndex] = new LeaderboardUserData(_currentUser, _incomingUser.Username);
            }
            
            this.scroller.RefreshActiveCellViews();
        }
        
        public int GetNumberOfCells(EnhancedScroller _Scroller)
        {
            return SteamUsers.Count;
        }

        public float GetCellViewSize(EnhancedScroller _Scroller, int _DataIndex)
        {
            return this.leaderboardEntryHeight;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller _Scroller, int _DataIndex, int _CellIndex)
        {
            var _leaderboardEntry = (_Scroller.GetCellView(this.leaderboardEntryPrefab) as LeaderboardEntry)!;

#if UNITY_EDITOR // Helpful for debugging
            _leaderboardEntry.name = string.Concat("Cell", _DataIndex);
#endif
            _leaderboardEntry.SetData(_DataIndex);
            
            return _leaderboardEntry;
        }
        #endregion
    }
}