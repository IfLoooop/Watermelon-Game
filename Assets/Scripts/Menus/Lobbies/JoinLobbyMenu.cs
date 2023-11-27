using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Networking;
using Watermelon_Game.Steamworks.NET;

namespace Watermelon_Game.Menus.Lobbies
{
    /// <summary>
    /// Menu that lists all available lobbies to join
    /// </summary>
    internal sealed class JoinLobbyMenu : MenuBase, IEnhancedScrollerDelegate, IScrollBase
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("EnhancedScroller component")]
        [SerializeField] private EnhancedScroller scroller;
        [Tooltip("Displays one row in the leaderboard")]
        [SerializeField] private LobbyEntry lobbyEntryPrefab;
        [Tooltip("Password menu, for when trying to enter a password protected lobby")]
        [SerializeField] private LobbyPasswordMenu lobbyPasswordMenu;
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="JoinLobbyMenu"/>
        /// </summary>
        private static JoinLobbyMenu instance;
        /// <summary>
        /// Reference to <see cref="IScrollBase"/>
        /// </summary>
        private IScrollBase scrollBase;
        /// <summary>
        /// Indicates whether this <see cref="JoinLobbyMenu"/> is currently open or not
        /// </summary>
        private bool isOpen;
        /// <summary>
        /// The heit of one <see cref="lobbyEntryPrefab"/>
        /// </summary>
        private float lobbyEntryHeight;
        /// <summary>
        /// Contains all available lobbies
        /// </summary>
        private List<LobbyData> lobbyList = new();
        /// <summary>
        /// <see cref="EnhancedScroller"/>.<see cref="EnhancedScroller.NormalizedScrollPosition"/>
        /// </summary>
        private float normalizedScrollPosition;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="lobbyList"/>
        /// </summary>
        public static List<LobbyData> LobbyList => instance.lobbyList;
        public float LastScrollPosition { get; set; } = 0;
        public bool ScrollPositionLocked { get; set; }
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
            this.scrollBase = this;
            this.lobbyEntryHeight = (this.lobbyEntryPrefab.transform as RectTransform)!.sizeDelta.y;
            this.scroller.Delegate = this;
        }

        private void OnEnable()
        {
            SteamLobby.OnLobbyEntriesProcessed += this.ReloadLobbyList;
            CustomNetworkManager.OnSteamLobbyEnterAttempt += this.SteamLobbyEnterAttempt;
        }

        private void OnDisable()
        {
            SteamLobby.OnLobbyEntriesProcessed -= this.ReloadLobbyList;
            CustomNetworkManager.OnSteamLobbyEnterAttempt -= this.SteamLobbyEnterAttempt;
        }

        /// <summary>
        /// Opens the <see cref="LobbyPasswordMenu"/>
        /// </summary>
        /// <param name="_LobbyId">The id of the lobby to enter the password for</param>
        public static void OpenPasswordMenu(ulong _LobbyId)
        { 
            instance.lobbyPasswordMenu.Open(_LobbyId);
        }
        
        public override MenuBase Open(MenuBase _CurrentActiveMenu)
        {
            this.isOpen = true;
            this.scrollBase.LockScrollPosition(true);
            SteamLobby.GetLobbies();
            
            return base.Open(_CurrentActiveMenu);
        }

        public override void Close(bool _PlaySound)
        {
            if (this.lobbyPasswordMenu.IsOpen)
            {
                this.lobbyPasswordMenu.Close(_PlaySound);
            }
            else
            {
                this.Close();
                MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.MultiplayerMenu);
            }
        }

        public override void ForceClose(bool _PlaySound)
        {
            this.lobbyPasswordMenu.Close(_PlaySound);
            this.Close();
        }

        /// <summary>
        /// Closes this menu and sets <see cref="isOpen"/> to false <br/>
        /// <b>Use this method for closing, instead of the override or base method</b>
        /// </summary>
        private void Close()
        {
            this.isOpen = false;
            this.normalizedScrollPosition = this.scroller.NormalizedScrollPosition;
            this.scrollBase.SetLastScrollPosition(this.scroller.ScrollRect.verticalScrollbar);
            base.Close(false);
        }
        
        /// <summary>
        /// Overwrites <see cref="lobbyList"/> with <see cref="SteamLobby"/>.<see cref="SteamLobby.Lobbies"/>
        /// </summary>
        private void ReloadLobbyList()
        {
            if (CustomNetworkManager.AttemptingToJoinALobby)
            {
                return;
            }
            
            this.lobbyList = new List<LobbyData>(SteamLobby.Lobbies);
            this.scroller.ReloadData(this.normalizedScrollPosition);
        }
        
        /// <summary>
        /// <see cref="CustomNetworkManager.OnSteamLobbyEnterAttempt"/>
        /// </summary>
        /// <param name="_Failure">Indicates if the attempt failed</param>
        private void SteamLobbyEnterAttempt(bool _Failure)
        {
            if (!isOpen)
            {
                return;
            }
            
            if (_Failure)
            {
                if (this.lobbyPasswordMenu.IsOpen)
                {
                    this.lobbyPasswordMenu.EnterAttemptFailed();   
                }
                else
                {
                    // TODO: Feedback on failure
                }
            }
            else
            {
                this.lobbyPasswordMenu.Close(true);
                this.Close();
            }
            
            AllowJoinButtonInteraction(true);
        }
        
        /// <summary>
        /// Sets the <see cref="Button.interactable"/> state of all elements in <see cref="lobbyList"/> to the given value
        /// </summary>
        /// <param name="_Interactable">The value to set the <see cref="Button.interactable"/> state of all elements in <see cref="lobbyList"/> to</param>
        public static void AllowJoinButtonInteraction(bool _Interactable)
        {
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < instance.lobbyList.Count; i++)
            {
                instance.lobbyList[i] = new LobbyData(instance.lobbyList[i], _Interactable);
            }
            
            instance.scroller.RefreshActiveCellViews();
        }
        
        public int GetNumberOfCells(EnhancedScroller _Scroller)
        {
            return this.lobbyList.Count;
        }

        public float GetCellViewSize(EnhancedScroller _Scroller, int _DataIndex)
        {
            return this.lobbyEntryHeight;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller _Scroller, int _DataIndex, int _CellIndex)
        {
            var _lobbyEntry = (_Scroller.GetCellView(this.lobbyEntryPrefab) as LobbyEntry)!;

#if UNITY_EDITOR // Helpful for debugging
            _lobbyEntry.name = string.Concat("Cell", _DataIndex);
#endif
            _lobbyEntry.SetData(_DataIndex);
            
            return _lobbyEntry;
        }

        public override void OnAnimationFinished()
        {
            this.scrollBase.LockScrollPosition(false);
        }

        public void OnScrollPositionChanged()
        {
            this.scrollBase.SetScrollPosition(this.scroller.ScrollRect.verticalScrollbar);
        }
        #endregion
    }
}