using System;
using TMPro;
using UnityEngine;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Menus
{
    internal sealed class GameOverMenu : ScrollRectBase
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("TMP component that displays the current points")]
        [SerializeField] private TextMeshProUGUI pointsText;
        [Tooltip("Contains various statistics")]
        [SerializeField] private Stats stats;
        [Tooltip("TMP component that displays the duration of the current game")]
        [SerializeField] private TextMeshProUGUI durationText;
        #endregion

        #region Fields
        /// <summary>
        /// Current points amount
        /// </summary>
        private uint points;
        /// <summary>
        /// Duration of the current game
        /// </summary>
        private TimeSpan duration;
        #endregion
        
        #region Properties
        /// <summary>
        /// Singleton of <see cref="GameOverMenu"/>
        /// </summary>
        public static GameOverMenu Instance { get; private set; }

        /// <summary>
        /// <see cref="points"/>
        /// </summary>
        public uint Points
        {
            set
            {
                this.points = value;
                this.stats.SetForText(this.pointsText, this.points);
            } 
        }
        /// <summary>
        /// <see cref="Menus.Stats"/>
        /// </summary>
        public Stats Stats => this.stats;
        /// <summary>
        /// <see cref="duration"/>
        /// </summary>
        public TimeSpan Duration
        {
            get => this.duration;
            set
            {
                this.duration = value;
                SetDurationText();
            } 
        }
        #endregion

        #region Events
        /// <summary>
        /// Is called when the <see cref="GameOverMenu"/> is closed
        /// </summary>
        public static event Action OnGameOverMenuClosed; 
        #endregion

        #region Methods
        private void Awake()
        {
            Instance = this;
        }
        
        /// <summary>
        /// <see cref="MenuBase.Open_Close"/>
        /// </summary>
        /// <param name="_CurrentActiveMenu"><see cref="MenuController.currentActiveMenu"/></param>
        /// <param name="_ForceClose">Forces the active menu to close even if <see cref="MenuBase.canNotBeClosedByDifferentMenu"/> is true</param>
        /// <returns>The new active <see cref="Menus.Menu"/> or null if all menus are closed</returns>
        public override MenuBase Open_Close(MenuBase _CurrentActiveMenu, bool _ForceClose = false)
        {
            // About to close
            if (this.gameObject.activeSelf)
            {
                OnGameOverMenuClosed?.Invoke();
            }
            // About to open
            else
            {
                var _currentGameDuration = Time.time - GameController.CurrentGameTimeStamp;
                this.Duration = this.Duration.Add(TimeSpan.FromSeconds(_currentGameDuration));
            }
            
            return base.Open_Close(_CurrentActiveMenu, _ForceClose);
        }
        
        // TODO: Try to combine with "StatsMenu.cs" "SetTimeSpendText()"-Method
        /// <summary>
        /// Sets a formatted value of <see cref="duration"/> in <see cref="durationText"/>
        /// </summary>
        private void SetDurationText()
        {
            var _duration = string.Empty;
            
            if (this.duration.Hours > 0)
            {
                _duration = string.Concat(_duration, $"{this.duration.Hours}h ");
            }
            else if (this.duration.Minutes > 0)
            {
                _duration = string.Concat(_duration, $"{this.duration.Minutes}min ");
            }
            else
            {
                _duration = string.Concat(_duration, $"{this.duration.Seconds}sec");
            }
            
            this.stats.SetForText(this.durationText, _duration);
        }
        
        // TODO: Combine with StatsMenu
        /// <summary>
        /// <see cref="Stats.AddFruitCount"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruits.Fruit"/> to add to <see cref="stats"/></param>
        public void AddFruitCount(Fruits.Fruit _Fruit)
        {
            this.stats.AddFruitCount(_Fruit);
        }

        // TODO: Combine with StatsMenu
        /// <summary>
        /// <see cref="Stats.AddSkillCount"/>
        /// </summary>
        /// <param name="_Skill">The <see cref="Skills.Skill"/> to add to <see cref="stats"/></param>
        public void AddSkillCount(Skill? _Skill)
        {
            this.stats.AddSkillCount(_Skill);
        }
        
        // TODO: Combine with StatsMenu
        /// <summary>
        /// Increments <see cref="Stats.GoldenFruitCount"/>
        /// </summary>
        public void AddGoldenFruit()
        {
            this.stats.GoldenFruitCount++;
        }
        
        /// <summary>
        /// Resets the stats of the <see cref="GameOverMenu"/> to their initial values
        /// </summary>
        public void Reset()
        {
            this.Points = 0;
            this.stats.BestMultiplier = 0;
            this.stats.BestMultiplier = 0;
            this.stats.GrapeEvolvedCount = 0;
            this.stats.CherryEvolvedCount = 0;
            this.stats.StrawberryEvolvedCount = 0;
            this.stats.LemonEvolvedCount = 0;
            this.stats.OrangeEvolvedCount = 0;
            this.stats.AppleEvolvedCount = 0;
            this.stats.PearEvolvedCount = 0;
            this.stats.PineappleEvolvedCount = 0;
            this.stats.HoneymelonEvolvedCount = 0;
            this.stats.WatermelonEvolvedCount = 0;
            this.Stats.GoldenFruitCount = 0;
            this.stats.PowerSkillUsedCount = 0;
            this.stats.EvolveSkillUsedCount = 0;
            this.stats.DestroySkillUsedCount = 0;
            this.duration = new TimeSpan();
        }
        #endregion
    }
}