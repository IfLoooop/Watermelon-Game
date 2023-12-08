using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Menus.InfoMenus;
using Watermelon_Game.Menus.MainMenus;
using Watermelon_Game.Networking;
using Watermelon_Game.Steamworks.NET;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Menus.Lobbies
{
    /// <summary>
    /// Menu that lists all available lobbies to join
    /// </summary>
    internal sealed class LobbyJoinMenu : MenuBase, IEnhancedScrollerDelegate, IScrollBase
    {
        #region Inspector Fields
        [Tooltip("EnhancedScroller component")]
        [PropertyOrder(1)][SerializeField] private EnhancedScroller scroller;
        [Tooltip("Displays one row in the leaderboard")]
        [PropertyOrder(1)][SerializeField] private LobbyEntry lobbyEntryPrefab;
        [Tooltip("Password menu, for when trying to enter a password protected lobby")]
        [PropertyOrder(1)][SerializeField] private LobbyPasswordMenu lobbyPasswordMenu;
        [Tooltip("Menu for joining or canceling the lobby enter attempt")]
        [PropertyOrder(1)][SerializeField] private LobbyConnectMenu lobbyConnectMenu;
        [Tooltip("Button to directly join through a lobby id")]
        [PropertyOrder(1)][SerializeField] private Button directJoinButton;
        [Tooltip("Inputfield to enter the lobby id")]
        [PropertyOrder(1)][SerializeField] private TMP_InputField inputfield;
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="LobbyJoinMenu"/>
        /// </summary>
        private static LobbyJoinMenu instance;
        /// <summary>
        /// Reference to <see cref="IScrollBase"/>
        /// </summary>
        private IScrollBase scrollBase;
        /// <summary>
        /// Indicates whether this <see cref="LobbyJoinMenu"/> is currently open or not
        /// </summary>
        private ProtectedBool isOpen;
        /// <summary>
        /// WIll be true while a direct join attempt is being processed
        /// </summary>
        private ProtectedBool directJoin;
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
            if (instance != null)
            {
                return;
            }
            
            instance = this;
            this.scrollBase = this;
            this.lobbyEntryHeight = (this.lobbyEntryPrefab.transform as RectTransform)!.sizeDelta.y;
            this.scroller.Delegate = this;
        }

        private void Start()
        {
            SteamLobby.GetLobbies();
        }

        private void OnEnable()
        {
            InfoMenu.OnInfoMenuClose += this.InfoMenuClosed;
            SteamLobby.OnLobbyEntriesProcessed += this.ReloadLobbyList;
            SteamLobby.OnLobbyDataUpdated += this.DirectJoinAttempt;
            CustomNetworkManager.OnSteamLobbyEnterAttempt += this.SteamLobbyEnterAttempt;
        }

        private void OnDisable()
        {
            InfoMenu.OnInfoMenuClose -= this.InfoMenuClosed;
            SteamLobby.OnLobbyEntriesProcessed -= this.ReloadLobbyList;
            SteamLobby.OnLobbyDataUpdated -= this.DirectJoinAttempt;
            CustomNetworkManager.OnSteamLobbyEnterAttempt -= this.SteamLobbyEnterAttempt;
        }
        
        public override MenuBase Open(MenuBase _CurrentActiveMenu)
        {
            this.isOpen = true;
            this.scrollBase.LockScrollPosition(true);
            this.inputfield.text = string.Empty;
            SteamLobby.GetLobbies();
            
            return base.Open(_CurrentActiveMenu);
        }
        
        public override void Close(bool _PlaySound)
        {
            if (this.lobbyPasswordMenu.IsOpen)
            {
                this.lobbyPasswordMenu.Close(_PlaySound);
                AllowJoinButtonInteraction(true);
            }
            else if (this.lobbyConnectMenu.IsOpen)
            {
                this.lobbyConnectMenu.Close(_PlaySound);
                AllowJoinButtonInteraction(true);
            }
            else
            {
                this.isOpen = false;
                this.SetScrollPosition();
                base.Close(false);
                MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.MultiplayerMenu);
            }
        }

        public override void ForceClose(bool _PlaySound)
        {
            this.isOpen = false;
            this.lobbyPasswordMenu.Close(false);
            this.lobbyConnectMenu.Close(false);
            this.SetScrollPosition();
            base.Close(_PlaySound);
            AllowJoinButtonInteraction(true);
        }
        
        /// <summary>
        /// Closes this menu and opens the <see cref="MultiplayerMenu"/> <br/>
        /// <i>Used on the "Close Menu" button (for mouse)</i>
        /// </summary>
        public void CloseButton()
        {
            this.ForceClose(false);
            MenuController.Open(_MenuControllerMenu => _MenuControllerMenu.MultiplayerMenu);
        }
        
        /// <summary>
        /// Try joining the lobby with the id in the given <see cref="TMP_InputField"/>
        /// </summary>
        /// <param name="_ButtonPress">Set to true when Method is called from clicking on the button</param>
        public async void DirectJoin(bool _ButtonPress)
        {
            // When the inputfield is selected and the button is pressed with the mouse, this method is called twice (because of the OnEndEdit-Event in the inspector) and this error is printed:
            // Attempting to select while already selecting an object.
            if (UnityEngine.EventSystems.EventSystem.current.alreadySelecting)
            {
                return;
            }
            if (!string.IsNullOrWhiteSpace(inputfield.text) && (_ButtonPress || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && this.directJoinButton.interactable)
            {
                if (CustomNetworkManager.AttemptingToJoinALobby)
                {
                    return;
                }
                
                this.directJoin = true;
                AllowJoinButtonInteraction(false);
                this.KeepOpen(true);
            
                if (ulong.TryParse(inputfield.text.Trim(), out var _steamId))
                {
                    if (await SteamLobby.RequestLobbyDataAsync(new CSteamID(_steamId), new WaitCondition(), false))
                    {
                        return;
                    }
                }
                
                MenuController.OpenPopup(_MenuControllerMenu => _MenuControllerMenu.InfoMenu.SetMessage(InfoMessage.LobbyIdNotValid));
            }
        }

        /// <summary>
        /// Tries joining the given steam lobby after receiving the <see cref="SteamLobby.OnLobbyDataUpdate"/> callback
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private void DirectJoinAttempt(LobbyDataUpdate_t _Callback)
        {
            if (!this.directJoin)
            {
                return;
            }
            
            this.directJoin = false;
            
            if (_Callback.m_bSuccess == 1)
            {
                var _lobbyId = new CSteamID(_Callback.m_ulSteamIDLobby);
                if (bool.Parse(SteamMatchmaking.GetLobbyData(_lobbyId, SteamLobby.FRIENDS_ONLY))) // Check if friends only
                {
                    if (ulong.TryParse(SteamMatchmaking.GetLobbyData(_lobbyId, SteamLobby.NETWORK_ADDRESS), out var _steamId)) // Get steam id of lobby host
                    {
                        if (!SteamFriends.HasFriend(new CSteamID(_steamId), EFriendFlags.k_EFriendFlagImmediate)) // Check if friends
                        {
                            MenuController.OpenPopup(_MenuControllerMenu => _MenuControllerMenu.InfoMenu.SetMessage(InfoMessage.FriendsOnly));
                            return;
                        }
                    }
                }
                
                var _password = SteamMatchmaking.GetLobbyData(new CSteamID(_Callback.m_ulSteamIDLobby), SteamLobby.PASSWORD);
                
                if (!string.IsNullOrWhiteSpace(_password))
                {
                    OpenPasswordMenu(_Callback.m_ulSteamIDLobby);
                }
                else
                {
                    SteamLobby.JoinLobbyAsync(_Callback.m_ulSteamIDLobby); // TODO: Force join if friend (maybe)
                }
            }
            // Lobby id doesn't exist
            else
            {
                MenuController.OpenPopup(_MenuControllerMenu => _MenuControllerMenu.InfoMenu.SetMessage(InfoMessage.LobbyDoesNotExist));
            }
        }

        /// <summary>
        /// Sets the button interactable again after the <see cref="InfoMenu"/> closes
        /// </summary>
        private void InfoMenuClosed()
        {
            if (!this.directJoinButton.interactable)
            {
                AllowJoinButtonInteraction(true);
                this.KeepOpen(false);
            }
        }
        
        /// <summary>
        /// Sets the scroll position when closing this menu
        /// </summary>
        private void SetScrollPosition()
        {
            this.normalizedScrollPosition = this.scroller.NormalizedScrollPosition;
            this.scrollBase.SetLastScrollPosition(this.scroller.ScrollRect.verticalScrollbar);
        }
        
        /// <summary>
        /// Sets <see cref="MenuBase.KeepOpen"/> to the given value
        /// </summary>
        /// <param name="_Value">Sets the value of <see cref="MenuBase.KeepOpen"/></param>
        public new void KeepOpen(bool _Value)
        {
            base.KeepOpen = _Value;
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
        /// Opens the <see cref="LobbyPasswordMenu"/>
        /// </summary>
        /// <param name="_LobbyId">The id of the lobby to enter the password for</param>
        public static void OpenPasswordMenu(ulong _LobbyId)
        {
            AllowJoinButtonInteraction(false);
            instance.lobbyPasswordMenu.Open(_LobbyId);
        }
        
        /// <summary>
        /// <see cref="CustomNetworkManager.OnSteamLobbyEnterAttempt"/>
        /// </summary>
        /// <param name="_Failure">Indicates if the attempt failed</param>
        private void SteamLobbyEnterAttempt(bool _Failure)
        {
            if (!this.isOpen)
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
                    AllowJoinButtonInteraction(true);
                }
            }
            else
            {
                this.lobbyPasswordMenu.Close(true);
                this.lobbyConnectMenu.Open(null);
                AllowJoinButtonInteraction(false);
            }
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

            instance.directJoinButton.interactable = _Interactable;
            instance.inputfield.interactable = _Interactable;
            
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