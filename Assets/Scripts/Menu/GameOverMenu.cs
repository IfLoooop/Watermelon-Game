using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Menu
{
    internal sealed class GameOverMenu : MenuBase
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
                this.stats.SetText1(this.scoreText, this.score.ToString());
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
        
        public override MenuBase Open_Close(MenuBase _PreviousMenu)
        {
            // About to close
            if (this.gameObject.activeSelf)
            {
                this.SetMultiplier();
                this.AddFruits();
                this.AddSkills();
                this.Reset();
                GameController.StartGame();
            }
            // About to open
            else
            {
                var _currentGameDuration = Time.time - GameController.Instance.currentGameTimeStamp;
                this.Duration = this.Duration.Add(TimeSpan.FromSeconds(_currentGameDuration));
            }
            
            return base.Open_Close(_PreviousMenu);
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
            
            this.stats.SetText1(this.durationText, _duration);
        }
        
        public void AddFruitCount(Fruit.Fruit _Fruit)
        {
            switch (_Fruit)
            {
                case Fruit.Fruit.Grape:
                    this.stats.GrapeEvolvedCount++;
                    break;
                case Fruit.Fruit.Cherry:
                    this.stats.CherryEvolvedCount++;
                    break;
                case Fruit.Fruit.Strawberry:
                    this.stats.StrawberryEvolvedCount++;
                    break;
                case Fruit.Fruit.Lemon:
                    this.stats.LemonEvolvedCount++;
                    break;
                case Fruit.Fruit.Orange:
                    this.stats.OrangeEvolvedCount++;
                    break;
                case Fruit.Fruit.Apple:
                    this.stats.AppleEvolvedCount++;
                    break;
                case Fruit.Fruit.Pear:
                    this.stats.PearEvolvedCount++;
                    break;
                case Fruit.Fruit.Pineapple:
                    this.stats.PineappleEvolvedCount++;
                    break;
                case Fruit.Fruit.HoneyMelon:
                    this.stats.HoneyMelonEvolvedCount++;
                    break;
                case Fruit.Fruit.Melon:
                    this.stats.MelonEvolvedCount++;
                    break;
            }
        }

        public void AddSkillCount(Skill _Skill)
        {
            switch (_Skill)
            {
                case Skill.Power:
                    this.stats.PowerSkillUsedCount++;
                    break;
                case Skill.Evolve:
                    this.stats.EvolveSkillUsedCount++;
                    break;
                case Skill.Destroy:
                    this.stats.DestroySkillUsedCount++;
                    break;
            }
        }

        private void SetMultiplier()
        {
            if (StatsMenu.Instance.Stats.HighestMultiplier < this.stats.HighestMultiplier)
            {
                StatsMenu.Instance.Stats.HighestMultiplier = this.stats.HighestMultiplier;
            }
        }
        
        private void AddFruits()
        {
            StatsMenu.Instance.AddFruitCount(Fruit.Fruit.Grape, this.stats.GrapeEvolvedCount);
            StatsMenu.Instance.AddFruitCount(Fruit.Fruit.Cherry, this.stats.CherryEvolvedCount);
            StatsMenu.Instance.AddFruitCount(Fruit.Fruit.Strawberry, this.stats.StrawberryEvolvedCount);
            StatsMenu.Instance.AddFruitCount(Fruit.Fruit.Lemon, this.stats.LemonEvolvedCount);
            StatsMenu.Instance.AddFruitCount(Fruit.Fruit.Orange, this.stats.OrangeEvolvedCount);
            StatsMenu.Instance.AddFruitCount(Fruit.Fruit.Apple, this.stats.AppleEvolvedCount);
            StatsMenu.Instance.AddFruitCount(Fruit.Fruit.Pear, this.stats.PearEvolvedCount);
            StatsMenu.Instance.AddFruitCount(Fruit.Fruit.Pineapple, this.stats.PineappleEvolvedCount);
            StatsMenu.Instance.AddFruitCount(Fruit.Fruit.HoneyMelon, this.stats.HoneyMelonEvolvedCount);
            StatsMenu.Instance.AddFruitCount(Fruit.Fruit.Melon, this.stats.MelonEvolvedCount);
        }

        private void AddSkills()
        {
            StatsMenu.Instance.AddSkillCount(Skill.Power, this.stats.PowerSkillUsedCount);
            StatsMenu.Instance.AddSkillCount(Skill.Evolve, this.stats.EvolveSkillUsedCount);
            StatsMenu.Instance.AddSkillCount(Skill.Destroy, this.stats.DestroySkillUsedCount);
        }

        private void Reset()
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
            this.stats.PowerSkillUsedCount = 0;
            this.stats.EvolveSkillUsedCount = 0;
            this.stats.DestroySkillUsedCount = 0;
            this.duration = new TimeSpan();
        }
        #endregion
    }
}