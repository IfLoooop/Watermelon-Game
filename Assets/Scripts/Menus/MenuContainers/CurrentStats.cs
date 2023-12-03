using System;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Watermelon_Game.Points;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Menus.MenuContainers
{
    internal sealed class CurrentStats : ContainerMenuBase
    {
        #region Inspector Fields
        [Tooltip("TMP component that displays the current points")]
        [PropertyOrder(1)][SerializeField] private TextMeshProUGUI pointsText;
        [Tooltip("Contains various statistics")]
        [PropertyOrder(1)][HideLabel]
        [SerializeField] private Stats stats;
        [Tooltip("TMP component that displays the duration of the current game")]
        [PropertyOrder(1)][SerializeField] private TextMeshProUGUI durationText;
        #endregion

        #region Fields
        /// <summary>
        /// Current points amount
        /// </summary>
        private ProtectedInt32 points;
        /// <summary>
        /// Duration of the current game
        /// </summary>
        private TimeSpan duration;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="points"/>
        /// </summary>
        public ProtectedInt32 Points
        {
            set
            {
                this.points = value;
                this.stats.SetForText(this.pointsText, this.points);
            } 
        }
        /// <summary>
        /// <see cref="MenuContainers.Stats"/>
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
        
        /// <summary>
        /// Timestamp in seconds, of the last game over
        /// </summary>
        public ProtectedFloat GameOverTimestamp { get; set; }
        #endregion
        
        #region Methods
        protected override void Awake()
        {
            base.Awake();
            this.Reset();
        }
        
        public override ContainerMenuBase SetActive(ContainerMenuBase _CurrentActiveMenu)
        {
            this.SetDuration();
            if (GameController.ActiveGame)
            {
                this.Points = (int)PointsController.CurrentPoints.Value;
            }
            
            return base.SetActive(_CurrentActiveMenu);
        }

        /// <summary>
        /// Calculates the duration of the current match and sets it to <see cref="Duration"/>
        /// </summary>
        private void SetDuration()
        {
            float _duration;
            if (GameController.ActiveGame)
                _duration = Time.time - GameController.CurrentGameTimeStamp;
            else
                _duration = this.GameOverTimestamp - GameController.CurrentGameTimeStamp;
            
            this.Duration = TimeSpan.FromSeconds(_duration);
        }
        
        // TODO: Try to combine with "GlobalStats.cs" "SetTimeSpendText()"-Method
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
        /// Resets the stats of the <see cref="CurrentStats"/> to their initial values
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
            this.Duration = TimeSpan.Zero;
        }
        #endregion
    }
}