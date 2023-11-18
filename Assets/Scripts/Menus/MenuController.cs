using System;
using System.Collections;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon_Game.Fruits;
using Watermelon_Game.Menus.Leaderboards;
using Watermelon_Game.Points;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Controls all menu logic
    /// </summary>
    internal sealed class MenuController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the StatsMenu component")]
        [SerializeField] private StatsMenu statsMenu;
        [Tooltip("Reference to the GameOverMenu component")]
        [SerializeField] private GameOverMenu gameOverMenu;
        [Tooltip("Reference to the Leaderboard component")]
        [SerializeField] private Leaderboard leaderboard;
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
        #endregion
        
        #region Properties
        /// <summary>
        /// Indicates whether any of the menus is currently opened
        /// </summary>
        public static bool IsAnyMenuOpen => instance.currentActiveMenu != null;
        #endregion
        
        #region Events
        /// <summary>
        /// Is called when a new best score is reached <br/>
        /// <b>Parameter:</b> The new best score amount
        /// </summary>
        public static event Action<uint> OnNewBestScore;
        /// <summary>
        /// Is called when the player manually restarts the game through the <see cref="ExitMenu"/>
        /// </summary>
        public static event Action OnManualRestart;
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
            InitializeMenu(this.statsMenu);
            InitializeMenu(this.gameOverMenu);
            InitializeMenu(this.leaderboard);
            InitializeMenu(this.exitMenu);
        }
        
        /// <summary>
        /// Activates and deactivates the given menu, to initialize all needed values <br/>
        /// <i>Because all menu GameObjects start inactive</i>
        /// </summary>
        /// <param name="_Menu">The menu to initialize</param>
        private static void InitializeMenu(MenuBase _Menu)
        {
            _Menu.gameObject.SetActive(true);
            _Menu.gameObject.SetActive(false);
        }
        
        private void OnEnable()
        {
            GameController.OnGameStart += this.GameStarted;
            GameController.OnResetGameStarted += this.ResetGameStarted;
            GameController.OnResetGameFinished += this.EnableInput;
            GameController.OnRestartGame += this.GameOver;
            FruitController.OnEvolve += this.AddFruit;
            FruitBehaviour.OnGoldenFruitSpawn += this.AddGoldenFruit;
            FruitBehaviour.OnSkillUsed += this.AddSkill;
            Multiplier.OnMultiplierActivated += this.MultiplierActivated;
            LanguageController.OnLanguageChanged += this.ReopenMenu;
        }

        private void OnDisable()
        {
            GameController.OnGameStart -= this.GameStarted;
            GameController.OnResetGameStarted -= this.ResetGameStarted;
            GameController.OnResetGameFinished -= this.EnableInput;
            GameController.OnRestartGame -= this.GameOver;
            FruitController.OnEvolve -= this.AddFruit;
            FruitBehaviour.OnGoldenFruitSpawn -= this.AddGoldenFruit;
            FruitBehaviour.OnSkillUsed -= this.AddSkill;
            Multiplier.OnMultiplierActivated -= this.MultiplierActivated;
            LanguageController.OnLanguageChanged -= this.ReopenMenu;
        }
        
        private void OnApplicationQuit()
        {
            this.CheckForNewBestScore(PointsController.CurrentPoints);
            StatsMenu.Instance.Save();
        }
        
        private void Update()
        {
            if (!this.allowInput)
            {
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (this.currentActiveMenu == null)
                {
                    this.Open_CloseMenu(this.exitMenu);
                }
                else
                {
                    this.Open_CloseMenu(this.currentActiveMenu, true);
                }
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                this.Open_CloseMenu(this.statsMenu);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                this.Open_CloseMenu(this.leaderboard);
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (this.currentActiveMenu is { Menu: Menu.Exit })
                {
                    this.exitMenu.ExitGame();
                }
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                if (this.currentActiveMenu is { Menu: Menu.Exit })
                {
                    this.Open_CloseMenu(this.exitMenu);
                    OnManualRestart?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// <see cref="MenuBase.Open_Close"/>
        /// </summary>
        /// <param name="_Menu">The <see cref="Menus.Menu"/> to change the open/closed state of</param>
        /// <param name="_ForceClose">Forces the active menu to close even if <see cref="MenuBase.canNotBeClosedByDifferentMenu"/> is true</param>
        private void Open_CloseMenu(MenuBase _Menu, bool _ForceClose = false)
        {
            this.currentActiveMenu = _Menu.Open_Close(this.currentActiveMenu, _ForceClose);
        }
        
        /// <summary>
        /// <see cref="GameController.OnGameStart"/>
        /// </summary>
        private void GameStarted()
        {
            this.statsMenu.AddGamesPlayed();
            this.gameOverMenu.Reset();
        }
        
        /// <summary>
        /// <see cref="GameController.OnResetGameStarted"/>
        /// </summary>
        private void ResetGameStarted()
        {
            var _points = PointsController.CurrentPoints.Value;
            this.gameOverMenu.Points = (int)_points;
            this.CheckForNewBestScore(_points);
            this.CloseCurrentlyActiveMenu();
            this.DisableInput();
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
        /// Checks if a new best score was reached
        /// </summary>
        /// <param name="_NewScore">The new score amount to check</param>
        private void CheckForNewBestScore(uint _NewScore)
        {
            var _newBestScore = _NewScore > this.statsMenu.BestScore;
            if (_newBestScore)
            {
                OnNewBestScore?.Invoke(_NewScore);
                this.statsMenu.SetBestScore((int)_NewScore);
            }
        }
        
        /// <summary>
        /// Closes <see cref="currentActiveMenu"/>
        /// </summary>
        private void CloseCurrentlyActiveMenu()
        {
            if (this.currentActiveMenu != null)
            {
                this.Open_CloseMenu(this.currentActiveMenu, true);   
            }
        }
        
        /// <summary>
        /// Opens the <see cref="GameOverMenu"/> at the end of a game -> <see cref="GameController.OnRestartGame"/>
        /// </summary>
        private void GameOver()
        {
            this.Open_CloseMenu(this.gameOverMenu);
        }

        /// <summary>
        /// Checks if the given multiplier is higher than the multiplier saved in <see cref="GameOverMenu"/> and <see cref="StatsMenu"/>
        /// </summary>
        /// <param name="_CurrentMultiplier"></param>
        private void MultiplierActivated(uint _CurrentMultiplier)
        {
            if (_CurrentMultiplier > this.gameOverMenu.Stats.BestMultiplier)
            {
                this.gameOverMenu.Stats.BestMultiplier = (int)_CurrentMultiplier;
            }
            if (_CurrentMultiplier > this.statsMenu.Stats.BestMultiplier)
            {
                this.statsMenu.Stats.BestMultiplier = (int)_CurrentMultiplier;
            }
        }
        
        /// <summary>
        /// Adds the given <see cref="Fruit"/> to <see cref="Stats"/> -> <see cref="FruitController.OnEvolve"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to add to <see cref="Stats"/></param>
        private void AddFruit(Fruit _Fruit)
        {
            this.gameOverMenu.AddFruitCount(_Fruit);
            this.statsMenu.AddFruitCount(_Fruit);
        }

        /// <summary>
        /// Increments <see cref="Stats.GoldenFruitCount"/> in <see cref="GameOverMenu"/> and <see cref="StatsMenu"/>
        /// </summary>
        /// <param name="_IsUpgradedGoldenFruit">Indicates whether the golden fruit is an upgraded golden fruit or not</param>
        private void AddGoldenFruit(bool _IsUpgradedGoldenFruit)
        {
            this.gameOverMenu.AddGoldenFruit();
            this.statsMenu.AddGoldenFruit();
        }

        /// <summary>
        /// Adds the given <see cref="Skill"/> to <see cref="Stats"/> -> <see cref="FruitController"/>
        /// </summary>
        /// <param name="_Skill">The <see cref="Skill"/> to add to <see cref="Stats"/></param>
        private void AddSkill(Skill? _Skill)
        {
            this.gameOverMenu.AddSkillCount(_Skill);
            this.statsMenu.AddSkillCount(_Skill);
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
            this.allowInput = false;
            var _menu = this.currentActiveMenu;
            this.Open_CloseMenu(this.currentActiveMenu, true);
            if (_menu!.Menu != Menu.GameOver) // TODO: Not the best solution
            {
                yield return new WaitForSeconds(menuReopenDelay);
                this.Open_CloseMenu(_menu);
            }
            this.allowInput = true;
        }
        #endregion
    }
}