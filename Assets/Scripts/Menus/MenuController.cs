using System;
using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.Fruits;
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
        [Tooltip("Reference to the StatsMenu component")]
        [SerializeField] private StatsMenu statsMenu;
        [Tooltip("Reference to the GameOverMenu component")]
        [SerializeField] private GameOverMenu gameOverMenu;
        [Tooltip("Reference to the ExitMenu component")]
        [SerializeField] private ExitMenu exitMenu;
        #endregion

        #region Fields
        /// <summary>
        /// The currently active menu
        /// </summary>
        [CanBeNull] private MenuBase currentActiveMenu;
        
        /// <summary>
        /// Whether the <see cref="MenuController"/> currently takes input or not
        /// </summary>
        private bool allowInput = true;
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
            this.InitializeMenu(this.statsMenu);
            this.InitializeMenu(this.gameOverMenu);
            this.InitializeMenu(this.exitMenu);
        }

        private void OnEnable()
        {
            GameController.OnGameStart += this.GameStarted;
            GameController.OnResetGameStarted += this.ResetGameStarted;
            GameController.OnRestartGame += this.GameOver;
            GameController.OnResetGameFinished += this.FlipAllowInput;
            FruitController.OnEvolve += this.AddFruit;
            FruitBehaviour.OnGoldenFruitSpawn += AddGoldenFruit;
            FruitBehaviour.OnSkillUsed += AddSkill;
            Multiplier.OnMultiplierActivated += MultiplierActivated;
        }

        private void OnDisable()
        {
            GameController.OnGameStart -= this.GameStarted;
            GameController.OnResetGameStarted -= this.ResetGameStarted;
            GameController.OnRestartGame -= this.GameOver;
            GameController.OnResetGameFinished -= this.FlipAllowInput;
            FruitController.OnEvolve -= this.AddFruit;
            FruitBehaviour.OnGoldenFruitSpawn -= AddGoldenFruit;
            FruitBehaviour.OnSkillUsed -= AddSkill;
            Multiplier.OnMultiplierActivated -= MultiplierActivated;
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
            else if (Input.GetKeyDown(KeyCode.P))
            {
                this.Open_CloseMenu(this.statsMenu);
            }
            else if (Input.GetKeyDown(KeyCode.Return))
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
        /// Needed to set the "Instance" properties in the Menu class
        /// </summary>
        /// <param name="_Menu">The menu to initialize</param>
        private void InitializeMenu(MenuBase _Menu)
        {
            _Menu.gameObject.SetActive(true);
            _Menu.gameObject.SetActive(false);
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
        /// Flips the value of <see cref="allowInput"/>
        /// </summary>
        private void FlipAllowInput() // TODO: Remove the flip
        {
            this.allowInput = !this.allowInput;
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
            var _points = PointsController.CurrentPoints;
            this.gameOverMenu.Points = _points;
            this.CheckForNewBestScore(_points);
            this.CloseCurrentlyActiveMenu();
            this.FlipAllowInput();
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
                this.gameOverMenu.Stats.BestMultiplier = _CurrentMultiplier;
            }
            if (_CurrentMultiplier > this.statsMenu.Stats.BestMultiplier)
            {
                this.statsMenu.Stats.BestMultiplier = _CurrentMultiplier;
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
        private void AddGoldenFruit()
        {
            this.gameOverMenu.AddGoldenFruit();
            this.statsMenu.AddGoldenFruit();
        }

        /// <summary>
        /// Adds the given <see cref="Skill"/> to <see cref="Stats"/> -> <see cref="FruitController."/>
        /// </summary>
        /// <param name="_Skill">The <see cref="Skill"/> to add to <see cref="Stats"/></param>
        private void AddSkill(Skill? _Skill)
        {
            this.gameOverMenu.AddSkillCount(_Skill);
            this.statsMenu.AddSkillCount(_Skill);
        }
        #endregion
    }
}