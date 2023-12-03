using System;
using System.Collections;
using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Watermelon_Game.Menus.InfoMenus;
using Watermelon_Game.Menus.Languages;
using Watermelon_Game.Menus.Lobbies;
using Watermelon_Game.Menus.MainMenus;
using Watermelon_Game.Menus.MenuContainers;
using Watermelon_Game.Singletons;
using Watermelon_Game.Steamworks.NET;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Controls all menu logic
    /// </summary>
    internal sealed class MenuController : PersistantMonoBehaviour<MenuController>
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Container for submenus")]
        [SerializeField] private MenuContainer menuContainer;
        [Tooltip("Reference to the Singleplayer")]
        [SerializeField] private SingleplayerMenu singleplayerMenu;
        [Tooltip("Reference to the Multiplayer")]
        [SerializeField] private MultiplayerMenu multiplayerMenu;
        [Tooltip("Reference to the JoinLobbyMenu")]
        [SerializeField] private LobbyJoinMenu lobbyJoinMenu;
        [Tooltip("Reference to the CreateLobbyMenu")]
        [SerializeField] private LobbyCreateMenu lobbyCreateMenu;
        [Tooltip("Reference to the LobbyHostMenu")]
        [SerializeField] private LobbyHostMenu lobbyHostMenu;
        [Tooltip("Reference to the ConnectionErrorMenu")]
        [SerializeField] private ConnectionErrorMenu connectionErrorMenu;
        [Tooltip("Reference to the InfoMenu")]
        [SerializeField] private InfoMenu infoMenu;

        [Tooltip("Win message")]
        [SerializeField] private TextMeshProUGUI win;
        [Tooltip("Loose message")]
        [SerializeField] private TextMeshProUGUI loose;

        [Header("Settings")]
        [Tooltip("Delay in seconds, between opening and closing the menu, on language change")]
        [SerializeField] private float menuReopenDelay = .25f;
        
        [Header("Debug")]
        [Tooltip("The currently active menu")]
        [SerializeField][ReadOnly][CanBeNull] private MenuBase currentActiveMenu;
        [Tooltip("The currently active popup menu")]
        [SerializeField][ReadOnly][CanBeNull] private InfoMenuBase currentActiveMenuPopup;
#if UNITY_EDITOR
        [Tooltip("Forces all menu options to be always enabled")]
        [ShowInInspector] public static bool DebugMultiplayerMenu;
#endif
        #endregion

        #region Fields
        /// <summary>
        /// Whether the <see cref="MenuController"/> currently takes input or not
        /// </summary>
        private ProtectedBool allowInput = true;
        /// <summary>
        /// Restarts the game if true and the <see cref="CurrentStats"/>-Menu is closed -> <see cref="RestartGame"/>
        /// </summary>
        private ProtectedBool readyToRestart;
        #endregion
        
        #region Properties
        // ReSharper disable UnusedMember.Global
        /// <summary>
        /// <see cref="menuContainer"/>
        /// </summary>
        public MenuBase MenuContainer => this.menuContainer;
        /// <summary>
        /// <see cref="singleplayerMenu"/>
        /// </summary>
        public MenuBase SingleplayerMenu => this.singleplayerMenu;
        /// <summary>
        /// <see cref="multiplayerMenu"/>
        /// </summary>
        public MenuBase MultiplayerMenu => this.multiplayerMenu;
        /// <summary>
        /// <see cref="lobbyJoinMenu"/>
        /// </summary>
        public MenuBase LobbyJoinMenu => this.lobbyJoinMenu;
        /// <summary>
        /// <see cref="lobbyCreateMenu"/>
        /// </summary>
        public MenuBase LobbyCreateMenu => this.lobbyCreateMenu;
        /// <summary>
        /// <see cref="lobbyHostMenu"/>
        /// </summary>
        public MenuBase LobbyHostMenu => this.lobbyHostMenu;
        /// <summary>
        /// <see cref="connectionErrorMenu"/>
        /// </summary>
        public MenuBase ConnectionErrorMenu => this.connectionErrorMenu;
        /// <summary>
        /// <see cref="infoMenu"/>
        /// </summary>
        public InfoMenu InfoMenu => this.infoMenu;
        // ReSharper restore UnusedMember.Global
        
        /// <summary>
        /// Indicates whether any of the menus is currently opened
        /// </summary>
        public static ProtectedBool IsAnyMenuOpen => Instance.currentActiveMenu != null;
        #endregion
        
        #region Events
        /// <summary>
        /// Is called when the player manually restarts the game through the <see cref="SingleplayerMenu"/>
        /// </summary>
        public static event Action OnManualRestart;
        #endregion
        
        #region Methods
        private void OnEnable()
        {
            MenuBase.OnMenuClose += MenuClosed;
            GameController.OnResetGameStarted += this.ResetGameStarted;
            GameController.OnResetGameFinished += this.ResetGameFinished;
            GameController.OnGameModeTransition += OpenMenuForGameMode;
            LanguageController.OnLanguageChanged += this.ReopenMenu;
        }
        
        private void OnDisable()
        {
            MenuBase.OnMenuClose -= MenuClosed;
            GameController.OnResetGameStarted -= this.ResetGameStarted;
            GameController.OnResetGameFinished -= this.ResetGameFinished;
            GameController.OnGameModeTransition -= OpenMenuForGameMode;
            LanguageController.OnLanguageChanged -= this.ReopenMenu;
        }
        
        private void Update()
        {
            if (!this.allowInput)
            {
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (this.currentActiveMenuPopup != null)
                {
                    CloseMenuPopup();
                }
                else if (this.currentActiveMenu != null)
                {
                    CloseCurrentMenu();
                }
                else
                {
                    this.OpenMenuForGameMode(GameController.CurrentGameMode, false);
                }
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                this.Open_Close(this.menuContainer);
            }
#if DEBUG || DEVELOPMENT_BUILD
            else if (this.currentActiveMenu is { Menu: Menu.Singleplayer })
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    this.singleplayerMenu.ExitGame();
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    Restart();
                }
            }
#endif
        }

        /// <summary>
        /// Sets <see cref="currentActiveMenu"/> to null, if the given <see cref="MenuBase"/> == <see cref="currentActiveMenu"/>
        /// </summary>
        /// <param name="_Menu">The menu that was closed</param>
        private void MenuClosed(MenuBase _Menu)
        {
            if (this.currentActiveMenu == _Menu)
            {
                this.currentActiveMenu = null;
            }
        }

        /// <summary>
        /// Opens the main menu depending on the <see cref="GameController.CurrentGameMode"/>
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void ESCButtonMenu()
        {
            this.OpenMenuForGameMode(GameController.CurrentGameMode, false);
        }

        /// <summary>
        /// Opens/closes the <see cref="Watermelon_Game.Menus.MenuContainers.MenuContainer"/>
        /// </summary>
        public void MenuButtonMenu()
        {
            this.Open_Close(this.menuContainer);
        }
        
        /// <summary>
        /// Opens a menu from the <see cref="MenuController"/> or closes it if it is already opened
        /// </summary>
        /// <param name="_MenuControllerMenu">Can be any <see cref="MenuBase"/> in <see cref="MenuController"/></param>
        /// <typeparam name="T">Must inherit from <see cref="MenuBase"/></typeparam>
        /// <returns>The <see cref="MenuBase"/> that was opened or null if the menu was closed</returns>
        public static MenuBase Open_Close<T>(Func<MenuController, T> _MenuControllerMenu) where T : MenuBase
        {
            return Instance.Open_Close(_MenuControllerMenu.Invoke(Instance));
        }
        
        /// <summary>
        /// Opens the given <see cref="MenuBase"/> if it's not <see cref="currentActiveMenu"/>, otherwise closes it
        /// </summary>
        /// <param name="_Menu">The menu to open/close</param>
        /// <returns>The currently open menu or null if no menu is currently open</returns>
        [CanBeNull]
        private MenuBase Open_Close(MenuBase _Menu)
        {
            if (this.currentActiveMenu != null && this.currentActiveMenu.Menu == _Menu.Menu)
            {
                CloseCurrentMenu();
            }
            else
            {
                return this.Open(_Menu);
            }

            return null;
        }
        
        /// <summary>
        /// Opens a <see cref="InfoMenuBase"/> from the <see cref="MenuController"/>
        /// </summary>
        /// <param name="_MenuControllerMenu">Can be any <see cref="MenuBase"/> in <see cref="MenuController"/></param>
        /// <typeparam name="T">Must inherit from <see cref="InfoMenuBase"/></typeparam>
        /// <returns>The <see cref="MenuBase"/> that was opened</returns>
        public static InfoMenuBase OpenPopup<T>(Func<MenuController, T> _MenuControllerMenu) where T : InfoMenuBase
        {
            return Instance.currentActiveMenuPopup = _MenuControllerMenu.Invoke(Instance).Open(Instance.currentActiveMenuPopup).As<InfoMenuBase>();
        }

        /// <summary>
        /// Closes <see cref="currentActiveMenuPopup"/>
        /// </summary>
        public static void CloseMenuPopup()
        {
            if (Instance.currentActiveMenuPopup != null)
            {
                Instance.currentActiveMenuPopup = Instance.currentActiveMenuPopup.Close(true);
            }
        }
        
        /// <summary>
        /// Opens a <see cref="MenuBase"/> from the <see cref="MenuController"/>
        /// </summary>
        /// <param name="_MenuControllerMenu">Can be any <see cref="MenuBase"/> in <see cref="MenuController"/></param>
        /// <typeparam name="T">Must inherit from <see cref="MenuBase"/></typeparam>
        /// <returns>The <see cref="MenuBase"/> that was opened</returns>
        public static MenuBase Open<T>(Func<MenuController, T> _MenuControllerMenu) where T : MenuBase
        {
            return Instance.Open(_MenuControllerMenu.Invoke(Instance));
        }
        
        /// <summary>
        /// Opens the given <see cref="MenuBase"/>
        /// </summary>
        /// <param name="_Menu">The menu to open</param>
        /// <returns>The <see cref="MenuBase"/> that was opened</returns>
        public MenuBase Open(MenuBase _Menu)
        {
            return this.currentActiveMenu = _Menu.Open(this.currentActiveMenu);
        }

        /// <summary>
        /// Opens the given submenu in <see cref="menuContainer"/>
        /// </summary>
        /// <param name="_ContainerMenu">The <see cref="ContainerMenu"/> to open</param>
        private void Open(ContainerMenu _ContainerMenu)
        {
            this.currentActiveMenu = this.menuContainer.Open(this.currentActiveMenu, _ContainerMenu);
        }
        
        /// <summary>
        /// Closes the <see cref="currentActiveMenu"/> <br/>
        /// <i>Won't do anything if no menu is currently open</i>
        /// </summary>
        /// <param name="_ForceClose">Also closes all submenus</param>
        public static void CloseCurrentMenu(bool _ForceClose = false)
        {
            if (Instance.currentActiveMenu != null)
            {
                if (_ForceClose)
                {
                    Instance.currentActiveMenu.ForceClose(true);   
                }
                else
                {
                    Instance.currentActiveMenu.Close(true);
                }
                Instance.RestartGame(); // TODO: Neéds better solution
            }
        }

        /// <summary>
        /// Manual game restart <br/>
        /// <i>Is called from the <see cref="SingleplayerMenu"/>-button</i>
        /// </summary>
        public static void Restart()
        {
            CloseCurrentMenu();
            OnManualRestart?.Invoke();
        }
        
        /// <summary>
        /// <see cref="GameController.OnResetGameStarted"/>
        /// </summary>
        private void ResetGameStarted()
        {
            this.DisableInput();
            CloseCurrentMenu();
        }
        
        /// <summary>
        /// <see cref="GameController.OnResetGameFinished"/>
        /// </summary>
        /// <param name="_ResetReason"><see cref="ResetReason"/></param>
        private void ResetGameFinished(ResetReason _ResetReason)
        {
            this.EnableInput();
            
            if (_ResetReason == ResetReason.GameOver)
            {
                this.GameOver(Time.time);
            }
        }
        
        /// <summary>
        /// Enables <see cref="allowInput"/> and sets it to true
        /// </summary>
        private void EnableInput()
        {
            this.allowInput = true;   
        }
        
        /// <summary>
        /// Disables <see cref="allowInput"/> and sets it to false
        /// </summary>
        private void DisableInput()
        {
            this.allowInput = false;
        }
        
        /// <summary>
        /// Opens the <see cref="CurrentStats"/> at the end of a game -> <see cref="GameController.OnResetGameFinished"/>
        /// </summary>
        /// <param name="_Timestamp">Timestamp in seconds, when the game was over</param>
        private void GameOver(float _Timestamp)
        {
            this.SetWinner();
            this.menuContainer.CurrentStats.GameOverTimestamp = _Timestamp;
            this.Open(ContainerMenu.CurrentStats);
            this.readyToRestart = true;
        }

        /// <summary>
        /// Sets the win/loose message
        /// </summary>
        private void SetWinner()
        {
            if (SteamManager.Initialized)
            {
                if (SteamLobby.CurrentLobbyId != null)
                {
                    if (SteamManager.SteamID.m_SteamID != GameController.LoosingPlayer)
                    {
                        this.win.gameObject.SetActive(true);
                    }
                    else
                    {
                        this.loose.gameObject.SetActive(true);
                    }
                }   
            }
        }
        
        /// <summary> // TODO: Needs better solution
        /// Restarts the game when <see cref="GameController.ActiveGame"/> is false
        /// </summary>
        private void RestartGame()
        {
            if (this.readyToRestart)
            {
                this.win.gameObject.SetActive(false);
                this.loose.gameObject.SetActive(false);
                this.readyToRestart = false;
                NetworkGameController.RestartGame();
            }
        }
        
        /// <summary>
        /// Opens the menu for the given <see cref="GameMode"/>
        /// </summary>
        /// <param name="_GameMode">The <see cref="GameMode"/> to open the menu of</param>
        /// <param name="_ForceSwitch"><see cref="GameController.gameModeWasForced"/></param>
        private void OpenMenuForGameMode(GameMode _GameMode, bool _ForceSwitch)
        {
            if (!_ForceSwitch)
            {
                switch (_GameMode)
                {
                    case GameMode.SinglePlayer:
                        this.Open_Close(this.singleplayerMenu);
                        break;
                    case GameMode.MultiPlayer:
                        this.Open_Close(this.multiplayerMenu);
                        break;
                }   
            }
        }
        
        /// <summary>
        /// Reopens the <see cref="currentActiveMenu"/> if any was open
        /// </summary>
        private void ReopenMenu()
        {
            if (this.currentActiveMenu != null)
            {
                if (!this.currentActiveMenu.KeepOpen)
                {
                    this.StartCoroutine(this.ReOpen());   
                }
            }
        }

        /// <summary>
        /// Closes the <see cref="currentActiveMenu"/> and reopens it after a certain time
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReOpen()
        {
            this.DisableInput();
            var _menu = this.currentActiveMenu;
            CloseCurrentMenu(true);
            
            yield return new WaitForSeconds(this.menuReopenDelay);
            this.Open(_menu);
            this.EnableInput();
        }
        #endregion
    }
}