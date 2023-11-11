using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Steamworks.NET;

namespace Watermelon_Game.Menus.Leaderboards
{
    // TODO:
    // Add sorting options (ascending/descending)
    
    /// <summary>
    /// Leaderboard menu
    /// </summary>
    internal sealed class Leaderboard : ScrollRectBase, IEnhancedScrollerDelegate
    {
        #region Inspector Fields
        [Tooltip("Displays the current/max page")]
        [SerializeField] private TextMeshProUGUI paging;
        [Tooltip("EnhancedScroller component")]
        [SerializeField] private EnhancedScroller scroller;
        [Tooltip("Displays one row in the leaderboard")]
        [SerializeField] private LeaderboardEntry leaderboardEntryPrefab;

        [Header("Settings")]
        [Tooltip("The maximum number of entries per page")]
        [SerializeField] private uint maxEntriesPerPage = 100;
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="Leaderboard"/>
        /// </summary>
        private static Leaderboard instance;
        /// <summary>
        /// The heit of one <see cref="leaderboardEntryPrefab"/>
        /// </summary>
        private float leaderboardEntryHeight;
        /// <summary>
        /// Indicates whether this <see cref="Leaderboard"/> has already been initialized by the <see cref="MenuController"/>
        /// </summary>
        private bool hasBeenInitialized;
        /// <summary>
        /// The currently active page in the leaderboard menu <br/>
        /// <i>First page starts at 0</i>
        /// </summary>
        private int activePage;
        /// <summary>
        /// Indicates whether the <see cref="Toggle"/> to only display friends in the leaderboard is on or not
        /// </summary>
        private bool onlyFriendsToggleIsOn;
        /// <summary>
        /// Contains all friends
        /// </summary>
        private List<LeaderboardUserData> friends = new();
        /// <summary>
        /// Hold the data for the currently active page in the leaderboard menu
        /// </summary>
        private readonly List<LeaderboardUserData> steamUsers = new();
        #endregion

        #region Properties
        /// <summary>
        /// The last page number
        /// </summary>
        private int LastPage => (int)(this.UserList.Count / this.maxEntriesPerPage);
        /// <summary>
        /// <see cref="steamUsers"/>
        /// </summary>
        public static List<LeaderboardUserData> SteamUsers => instance.steamUsers;
        /// <summary>
        /// Returns <see cref="friends"/> if <see cref="onlyFriendsToggleIsOn"/> it true, otherwise <see cref="SteamLeaderboard"/>.<see cref="SteamLeaderboard.SteamUsers"/>
        /// </summary>
        private List<LeaderboardUserData> UserList => this.onlyFriendsToggleIsOn ? this.friends : SteamLeaderboard.SteamUsers;
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
            this.leaderboardEntryHeight = (this.leaderboardEntryPrefab.transform as RectTransform)!.sizeDelta.y;
            this.scroller.Delegate = this;
        }

        private void OnEnable()
        {
            SteamLeaderboard.OnLeaderboardScoresDownloaded += LeaderboardScoresDownloaded;
            SteamLeaderboard.OnUsernameFound += this.RefreshLeaderboardEntries;
            
            this.GetLeaderboardEntries();
            // Needs to be set after "GetLeaderboardEntries()", otherwise the "this.scroller" will have a null ref. (Will be set first time during "MenuController.cs" initialization)
            this.hasBeenInitialized = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            SteamLeaderboard.OnLeaderboardScoresDownloaded -= LeaderboardScoresDownloaded;
            SteamLeaderboard.OnUsernameFound -= this.RefreshLeaderboardEntries;
        }
        
        private void Start()
        {
            // Needs to be called twice the first time, ("OnEnable()" and "Start()"), otherwise the menu won't have any entries (only in build)
            // Shitty solution but it works
            this.GetLeaderboardEntries();
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
            // TODO:
            // Set the jump position to be the middle of the leaderboard (if possible, e.g. enough entries are in it)
            // Maybe smooth scroll towards the position, instead of jumping
            var _index = this.UserList.FindIndex(_SteamUser => _SteamUser.SteamId == SteamManager.SteamID.m_SteamID);
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
        /// Only displays friends (and the local user), if the toggle is on
        /// </summary>
        /// <param name="_Toggle">The <see cref="Toggle"/> that called this method</param>
        public void ToggleFriends(Toggle _Toggle)
        {
            this.activePage = 0;
            this.onlyFriendsToggleIsOn = _Toggle.isOn;
            
            if (_Toggle.isOn)
            {
                this.friends.Clear();
                const EFriendFlags FRIEND_FLAGS = EFriendFlags.k_EFriendFlagAll;
                var _friendCount = SteamFriends.GetFriendCount(FRIEND_FLAGS);
                
                var _userIndex = SteamLeaderboard.SteamUsers.FindIndex(_SteamUser => _SteamUser.SteamId == SteamManager.SteamID.m_SteamID);
                if (_userIndex != -1)
                {
                    this.friends.Add(SteamLeaderboard.SteamUsers[_userIndex]);
                }
                
                // ReSharper disable once InconsistentNaming
                for (var i = 0; i < _friendCount; i++)
                {
                    var _friendID = SteamFriends.GetFriendByIndex(i, FRIEND_FLAGS).m_SteamID;
                    var _friendIndex = SteamLeaderboard.SteamUsers.FindIndex(_SteamUser => _SteamUser.SteamId == _friendID);
                    
                    if (_friendIndex != -1)
                    {
                        this.friends.Add(SteamLeaderboard.SteamUsers[_friendIndex]);
                    }
                }

                this.friends = this.friends.OrderBy(_Friend => _Friend.GlobalRank).ToList();
                
                this.steamUsers.Clear();
                
                // ReSharper disable once InconsistentNaming
                foreach (var _friend in this.friends)
                {
                    this.steamUsers.Add(_friend);
                    
                    if (this.steamUsers.Count == this.maxEntriesPerPage)
                    {
                        break;
                    }
                }
                
                this.scroller.ReloadData();
                this.SetPaging();
            }
            else
            {
                this.GetLeaderboardEntries();
            }
        }
        
        /// <summary>
        /// Fills <see cref="steamUsers"/> with entries from <see cref="SteamLeaderboard"/>.<see cref="SteamLeaderboard.SteamUsers"/>, depending on the current <see cref="activePage"/>
        /// </summary>
        private void GetLeaderboardEntries()
        {
            if (!this.hasBeenInitialized)
            {
                return;
            }
            
            this.steamUsers.Clear();
            
            // ReSharper disable once InconsistentNaming
            for (var i = (int)(this.activePage * this.maxEntriesPerPage); i < this.UserList.Count; i++)
            {
                this.steamUsers.Add(this.UserList[i]);
                
                if (this.steamUsers.Count == this.maxEntriesPerPage)
                {
                    break;
                }
            }
            
            this.scroller.ReloadData();
            this.SetPaging();
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
        /// Updates the names of the <see cref="activePage"/> -> <see cref="SteamLeaderboard.OnUsernameFound"/>
        /// </summary>
        /// <param name="_Index">The index in <see cref="SteamLeaderboard.steamUsers"/>, for whom the username was found</param>
        private void RefreshLeaderboardEntries(int _Index)
        {
            var _dataIndex = (int)(_Index - this.activePage * this.maxEntriesPerPage);
            if (_dataIndex > this.steamUsers.Count - 1)
            {
                return;
            }
            if (_dataIndex < 0)
            {
                return;
            }
            
            var _currentUser = this.steamUsers[_dataIndex];
            var _incomingUser = SteamLeaderboard.SteamUsers[_Index];
            
            if (_currentUser.SteamId == _incomingUser.SteamId)
            {
                this.steamUsers[_dataIndex] = new LeaderboardUserData(_currentUser, _incomingUser.Username);
            }
            
            this.scroller.RefreshActiveCellViews();
        }
        
        public int GetNumberOfCells(EnhancedScroller _Scroller)
        {
            return this.steamUsers.Count;
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