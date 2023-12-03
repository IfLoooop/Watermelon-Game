using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using Watermelon_Game.Container;
using Watermelon_Game.Fruits;
using Watermelon_Game.Menus.Leaderboards;
using Watermelon_Game.Points;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Menus.MenuContainers
{
    /// <summary>
    /// Container for the menus in <see cref="ContainerMenu"/>
    /// </summary>
    internal sealed class MenuContainer : MenuBase
    {
        #region Inspector Fields
        [Tooltip("1: StatsMenu, 2: GameOverMenu: 3: Leaderboard")]
        [OdinSerialize] private Dictionary<ContainerMenu, ContainerMenuBase> menus = new();
        
        [FormerlySerializedAs("currentActiveMenu")]
        [Header("Debug")]
        [Tooltip("The currently active menu")]
        [SerializeField][ReadOnly][CanBeNull] private ContainerMenuBase currentActiveContainerMenu;
        #endregion

        #region Fields
        /// <summary>
        /// This menu will be opened when <see cref="currentActiveContainerMenu"/> is null
        /// </summary>
        private ContainerMenu lastActiveMenu = ContainerMenu.GlobalStats;
        #endregion
        
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedMember.Global
        #region Properties
        /// <summary>
        /// <see cref="GlobalStats"/>
        /// </summary>
        public GlobalStats GlobalStats => (GlobalStats)this.menus[ContainerMenu.GlobalStats];
        /// <summary>
        /// <see cref="CurrentStats"/>
        /// </summary>
        public CurrentStats CurrentStats => (CurrentStats)this.menus[ContainerMenu.CurrentStats];
        /// <summary>
        /// <see cref="Leaderboard"/>
        /// </summary>
        public Leaderboard Leaderboard => (Leaderboard)this.menus[ContainerMenu.Leaderboard];
        /// <summary>
        /// <see cref="Controls"/>
        /// </summary>
        public Controls Controls => (Controls)this.menus[ContainerMenu.Controls];
        #endregion
        // ReSharper restore UnusedMember.Global
        // ReSharper restore MemberCanBePrivate.Global
        
        #region Events
        /// <summary>
        /// Is called when a new best score is reached <br/>
        /// <b>Parameter:</b> The new best score amount
        /// </summary>
        public static event Action<uint> OnNewBestScore;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.Init();
        }

        /// <summary>
        /// Initializes all needed values
        /// </summary>
        private void Init()
        {
            foreach (var (_, _menu) in this.menus)
            {
                _menu.gameObject.SetActive(true);
                _menu.gameObject.SetActive(false);
            }
        }
        
        private void OnEnable()
        {
            MaxHeight.OnGameOver += this.GameOver;
            GameController.OnResetGameStarted += this.ResetGameStarted;
            GameController.OnGameStart += this.GameStarted;
            FruitController.OnEvolve += this.AddFruit;
            FruitBehaviour.OnGoldenFruitSpawn += this.AddGoldenFruit;
            FruitBehaviour.OnSkillUsed += this.AddSkill;
            Multiplier.OnMultiplierActivated += this.MultiplierActivated;
        }
        
        private void OnDisable()
        {
            MaxHeight.OnGameOver -= this.GameOver;
            GameController.OnResetGameStarted -= this.ResetGameStarted;
            GameController.OnGameStart -= this.GameStarted;
            FruitController.OnEvolve -= this.AddFruit;
            FruitBehaviour.OnGoldenFruitSpawn -= this.AddGoldenFruit;
            FruitBehaviour.OnSkillUsed -= this.AddSkill;
            Multiplier.OnMultiplierActivated -= this.MultiplierActivated;
        }
        
        private void OnApplicationQuit()
        {
            this.CheckForNewBestScore(PointsController.CurrentPoints);
            this.GlobalStats.Save();
        }
        
        public override void OnAnimationFinished()
        {
            this.currentActiveContainerMenu!.ScrollBase.LockScrollPosition(false);
        }
        
        /// <summary>
        /// Opens the given sub menu <br/>
        /// <i>Is used by the Tab-Buttons</i>
        /// </summary>
        /// <param name="_Menu">The <see cref="ContainerMenu"/> to open</param>
        public void Open(ContainerMenuBase _Menu)
        {
            this.currentActiveContainerMenu = _Menu.SetActive(this.currentActiveContainerMenu);
        }
        
        /// <summary>
        /// Opens the given <see cref="ContainerMenu"/> in this <see cref="MenuContainer"/>
        /// </summary>
        /// <param name="_CurrentActiveMenu">The <see cref="MenuBase"/> to close</param>
        /// <param name="_ContainerMenu">The <see cref="ContainerMenu"/> to open</param>
        /// <returns>This <see cref="MenuBase"/></returns>
        public MenuBase Open([CanBeNull] MenuBase _CurrentActiveMenu, ContainerMenu _ContainerMenu)
        {
            if (this.menus.TryGetValue(_ContainerMenu, out var _menu))
            {
                this.currentActiveContainerMenu = _menu.SetActive(this.currentActiveContainerMenu);
            }

            this.currentActiveContainerMenu!.ScrollBase.LockScrollPosition(true);
            
            return base.Open(_CurrentActiveMenu);
        }

        public override MenuBase Open(MenuBase _CurrentActiveMenu)
        {
            return this.Open(_CurrentActiveMenu, this.lastActiveMenu);
        }
        
        public override void Close(bool _PlaySound)
        {
            base.Close(_PlaySound);
            this.lastActiveMenu = this.currentActiveContainerMenu!.Menu;
            this.currentActiveContainerMenu!.SetInactive();
        }

        /// <summary>
        /// Closes this menu <br/>
        /// <i>Used on the "Close Menu" button (for mouse)</i>
        /// </summary>
        public void CloseButton()
        {
            this.Close(true);
        }
        
        /// <summary>
        /// <see cref="MaxHeight.OnGameOver"/>
        /// </summary>
        /// <param name="_SteamId">Not needed here</param>
        private void GameOver(ulong _SteamId)
        {
            this.GlobalStats.AddGamesPlayed();
        }
        
        /// <summary>
        /// <see cref="GameController.OnResetGameStarted"/>
        /// </summary>
        private void ResetGameStarted()
        {
            var _points = PointsController.CurrentPoints.Value;
            this.CurrentStats.Points = (int)_points;
            this.CheckForNewBestScore(_points);
        }

        /// <summary>
        /// <see cref="GameController.OnGameStart"/>
        /// </summary>
        private void GameStarted()
        {
            this.CurrentStats.Reset();
        }
        
        /// <summary>
        /// Checks if a new best score was reached
        /// </summary>
        /// <param name="_NewScore">The new score amount to check</param>
        private void CheckForNewBestScore(uint _NewScore)
        {
            var _newBestScore = _NewScore > this.GlobalStats.BestScore;
            if (_newBestScore)
            {
                OnNewBestScore?.Invoke(_NewScore);
                this.GlobalStats.SetBestScore((int)_NewScore);
            }
        }
        
        /// <summary>
        /// Checks if the given multiplier is higher than the multiplier saved in <see cref="CurrentStats"/> and <see cref="GlobalStats"/>
        /// </summary>
        /// <param name="_CurrentMultiplier"></param>
        private void MultiplierActivated(uint _CurrentMultiplier)
        {
            if (_CurrentMultiplier > this.CurrentStats.Stats.BestMultiplier)
            {
                this.CurrentStats.Stats.BestMultiplier = (int)_CurrentMultiplier;
            }
            if (_CurrentMultiplier > this.GlobalStats.Stats.BestMultiplier)
            {
                this.GlobalStats.Stats.BestMultiplier = (int)_CurrentMultiplier;
            }
        }
        
        /// <summary>
        /// Adds the given <see cref="Fruit"/> to <see cref="Stats"/> -> <see cref="FruitController.OnEvolve"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to add to <see cref="Stats"/></param>
        private void AddFruit(Fruit _Fruit)
        {
            this.CurrentStats.AddFruitCount(_Fruit);
            this.GlobalStats.AddFruitCount(_Fruit);
        }

        /// <summary>
        /// Increments <see cref="Stats.GoldenFruitCount"/> in <see cref="CurrentStats"/> and <see cref="GlobalStats"/>
        /// </summary>
        /// <param name="_IsUpgradedGoldenFruit">Indicates whether the golden fruit is an upgraded golden fruit or not</param>
        private void AddGoldenFruit(bool _IsUpgradedGoldenFruit)
        {
            this.CurrentStats.AddGoldenFruit();
            this.GlobalStats.AddGoldenFruit();
        }

        /// <summary>
        /// Adds the given <see cref="Skill"/> to <see cref="Stats"/> -> <see cref="FruitController"/>
        /// </summary>
        /// <param name="_Skill">The <see cref="Skill"/> to add to <see cref="Stats"/></param>
        private void AddSkill(Skill? _Skill)
        {
            this.CurrentStats.AddSkillCount(_Skill);
            this.GlobalStats.AddSkillCount(_Skill);
        }
        #endregion
    }
}