using System;
using TMPro;
using UnityEngine;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Menu
{
    internal sealed class GameOverMenu : ScrollRectBase
    {
        #region Inspector Fields
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Stats stats;
        [SerializeField] private TextMeshProUGUI durationText;
        #endregion

        #region Fields
        private uint score;
        private TimeSpan duration;
        #endregion
        
        #region Properties
        public static GameOverMenu Instance { get; private set; }

        public uint Score
        {
            set
            {
                this.score = value;
                this.stats.SetForText(this.scoreText, this.score);
            } 
        }
        public Stats Stats => this.stats;
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

        #region Methods
        private void Awake()
        {
            Instance = this;
        }
        
        public override MenuBase Open_Close(MenuBase _PreviousMenu, bool _ForceClose = false)
        {
            // About to close
            if (this.gameObject.activeSelf)
            {
                GameController.StartGame();
            }
            // About to open
            else
            {
                var _currentGameDuration = Time.time - GameController.Instance.CurrentGameTimeStamp;
                this.Duration = this.Duration.Add(TimeSpan.FromSeconds(_currentGameDuration));
            }
            
            return base.Open_Close(_PreviousMenu, _ForceClose);
        }
        
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
        
        public void AddFruitCount(Fruit.Fruit _Fruit)
        {
            this.stats.AddFruitCount(_Fruit);
        }

        public void AddSkillCount(Skill _Skill)
        {
            this.stats.AddSkillCount(_Skill);
        }
        
        public void Reset()
        {
            this.Score = 0;
            this.stats.HighestMultiplier = 0;
            this.stats.HighestMultiplier = 0;
            this.stats.GrapeEvolvedCount = 0;
            this.stats.CherryEvolvedCount = 0;
            this.stats.StrawberryEvolvedCount = 0;
            this.stats.LemonEvolvedCount = 0;
            this.stats.OrangeEvolvedCount = 0;
            this.stats.AppleEvolvedCount = 0;
            this.stats.PearEvolvedCount = 0;
            this.stats.PineappleEvolvedCount = 0;
            this.stats.HoneyMelonEvolvedCount = 0;
            this.stats.MelonEvolvedCount = 0;
            this.Stats.GoldenFruitCount = 0;
            this.stats.PowerSkillUsedCount = 0;
            this.stats.EvolveSkillUsedCount = 0;
            this.stats.DestroySkillUsedCount = 0;
            this.duration = new TimeSpan();
        }
        #endregion
    }
}