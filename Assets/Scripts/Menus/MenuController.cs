using System;
using System.Collections;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Controls all menu logic
    /// </summary>
    internal sealed class MenuController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Container for all menus")]
        [SerializeField] private MenuContainer menuContainer;
        [Tooltip("Reference to the ExitMenu component")]
        [SerializeField] private ExitMenu exitMenu;

        [Header("Settings")]
        [Tooltip("Delay in seconds, between opening and closing the menu, on language change")]
        [SerializeField] private float menuReopenDelay = .25f;
        
        [Header("Debug")]
        [Tooltip("The currently active menu")]
        [SerializeField][ReadOnly][CanBeNull] private MenuBase currentActiveMenu;
        #endregion

        #region Fields
        /// <summary>
        /// Singleton of <see cref="MenuController"/>
        /// </summary>
        private static MenuController instance;
        /// <summary>
        /// Whether the <see cref="MenuController"/> currently takes input or not
        /// </summary>
        private bool allowInput = true;
        /// <summary>
        /// Restarts the game if true and the <see cref="CurrentStats"/>-Menu is closed -> <see cref="RestartGame"/>
        /// </summary>
        private bool readyToRestart;
        #endregion
        
        #region Properties
        /// <summary>
        /// Indicates whether any of the menus is currently opened
        /// </summary>
        public static bool IsAnyMenuOpen => instance.currentActiveMenu != null;
        /// <summary>
        /// The <see cref="UnityEngine.Canvas"/> of this <see cref="MenuController"/>
        /// </summary>
        public static Canvas Canvas { get; private set; }
        #endregion
        
        #region Events
        /// <summary>
        /// Is called when the player manually restarts the game through the <see cref="ExitMenu"/>
        /// </summary>
        public static event Action OnManualRestart;
        /// <summary>
        /// Is called when the <see cref="CurrentStats"/> is closed
        /// </summary>
        public static event Action OnRestartGame; 
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
            Canvas = base.GetComponentInChildren<Canvas>();
        }
        
        private void OnEnable()
        {
            GameController.OnResetGameStarted += this.ResetGameStarted;
            GameController.OnResetGameFinished += this.EnableInput;
            GameController.OnRestartGame += this.GameOver;
            LanguageController.OnLanguageChanged += this.ReopenMenu;
        }
        
        private void OnDisable()
        {
            GameController.OnResetGameStarted -= this.ResetGameStarted;
            GameController.OnResetGameFinished -= this.EnableInput;
            GameController.OnRestartGame -= this.GameOver;
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
                if (this.currentActiveMenu != null)
                {
                    this.CloseCurrentMenu();
                }
                else
                {
                    this.Open_Close(this.exitMenu);
                }
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                this.Open_Close(this.menuContainer);
            }
#if DEBUG || DEVELOPMENT_BUILD
            else if (this.currentActiveMenu is { Menu: Menu.Exit })
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    this.exitMenu.ExitGame();
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    this.Restart();
                }
            }
#endif
        }

        /// <summary>
        /// Opens the given <see cref="MenuBase"/> if it's not <see cref="currentActiveMenu"/>, otherwise closes it
        /// </summary>
        /// <param name="_Menu">The menu to open/close</param>
        public void Open_Close(MenuBase _Menu)
        {
            if (this.currentActiveMenu != null && this.currentActiveMenu.Menu == _Menu.Menu)
            {
                this.CloseCurrentMenu();
            }
            else
            {
                this.Open(_Menu);
            }
        }
        
        /// <summary>
        /// Opens the given <see cref="MenuBase"/>
        /// </summary>
        /// <param name="_Menu">The menu to open</param>
        public void Open(MenuBase _Menu)
        {
            this.currentActiveMenu = _Menu.Open(this.currentActiveMenu);
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
        private void CloseCurrentMenu()
        {
            if (this.currentActiveMenu != null)
            {
                this.currentActiveMenu = this.currentActiveMenu.Close();
                this.RestartGame();
            }
        }

        /// <summary>
        /// Manual game restart <br/>
        /// <i>Is called from the <see cref="ExitMenu"/>-button</i>
        /// </summary>
        public void Restart()
        {
            this.CloseCurrentMenu();
            OnManualRestart?.Invoke();
        }
        
        /// <summary>
        /// <see cref="GameController.OnResetGameStarted"/>
        /// </summary>
        private void ResetGameStarted()
        {
            this.DisableInput();
            this.CloseCurrentMenu();
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
        /// Opens the <see cref="CurrentStats"/> at the end of a game -> <see cref="GameController.OnRestartGame"/>
        /// </summary>
        /// <param name="_Timestamp">Timestamp in seconds, when the game was over</param>
        private void GameOver(float _Timestamp)
        {
            this.menuContainer.CurrentStats.GameOverTimestamp = _Timestamp;
            this.Open(ContainerMenu.CurrentStats);
            this.readyToRestart = true;
        }

        /// <summary> // TODO: Needs better solution
        /// Restarts the game when <see cref="GameController.ActiveGame"/> is false
        /// </summary>
        private void RestartGame()
        {
            if (this.readyToRestart)
            {
                this.readyToRestart = false;
                OnRestartGame?.Invoke();
            }
        }
        
        /// <summary>
        /// Reopens the <see cref="currentActiveMenu"/> if any was open
        /// </summary>
        private void ReopenMenu()
        {
            if (this.currentActiveMenu != null)
            {
                this.StartCoroutine(this.ReOpen());   
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
            this.CloseCurrentMenu();
            
            yield return new WaitForSeconds(this.menuReopenDelay);
            this.Open(_menu);
            this.EnableInput();
        }
        #endregion
    }
}