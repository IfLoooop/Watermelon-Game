using System;
using TMPro;
using UnityEngine;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Contains various statistics
    /// </summary>
    [Serializable]
    internal sealed class Stats
    {
        #region Inspector Fields
        [Tooltip("TMP component that displays the best multiplier")]
        [SerializeField] private TextMeshProUGUI bestMultiplierText;
        [Tooltip("TMP component that displays the total evolved grape count")]
        [SerializeField] private TextMeshProUGUI grapeText;
        [Tooltip("TMP component that displays the total evolved cherry count")]
        [SerializeField] private TextMeshProUGUI cherryText;
        [Tooltip("TMP component that displays the total evolved strawberry count")]
        [SerializeField] private TextMeshProUGUI strawberryText;
        [Tooltip("TMP component that displays the total evolved lemon count")]
        [SerializeField] private TextMeshProUGUI lemonText;
        [Tooltip("TMP component that displays the total evolved orange count")]
        [SerializeField] private TextMeshProUGUI orangeText;
        [Tooltip("TMP component that displays the total evolved apple count")]
        [SerializeField] private TextMeshProUGUI appleText;
        [Tooltip("TMP component that displays the total evolved pear count")]
        [SerializeField] private TextMeshProUGUI pearText;
        [Tooltip("TMP component that displays the total evolved pineapple count")]
        [SerializeField] private TextMeshProUGUI pineappleText;
        [Tooltip("TMP component that displays the total evolved honeymelon count")]
        [SerializeField] private TextMeshProUGUI honeymelonText;
        [Tooltip("TMP component that displays the total evolved watermelon count")]
        [SerializeField] private TextMeshProUGUI watermelonText;
        [Tooltip("TMP component that displays the total golden fruit count")]
        [SerializeField] private TextMeshProUGUI goldenFruitsText;
        [Tooltip("TMP component that displays the total amount of power skills used")]
        [SerializeField] private TextMeshProUGUI powerText;
        [Tooltip("TMP component that displays the total amount of evolve skills used")]
        [SerializeField] private TextMeshProUGUI evolveText;
        [Tooltip("TMP component that displays the total amount of destroy skills used")]
        [SerializeField] private TextMeshProUGUI destroyText;
        #endregion

        #region Fields
        /// <summary>
        /// <see cref="StatsValues"/>
        /// </summary>
        private StatsValues statsValues;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="statsValues"/>
        /// </summary>
        public StatsValues StatsValues => this.statsValues;
        
        /// <summary>
        /// <see cref="StatsValues.BestScore"/>
        /// </summary>
        public int BestScore
        {
            get => this.statsValues.BestScore;
            set => this.statsValues.BestScore = value;
        }
        /// <summary>
        /// <see cref="StatsValues.GamesPlayed"/>
        /// </summary>
        public int GamesPlayed
        {
            get => this.statsValues.GamesPlayed;
            set => this.statsValues.GamesPlayed = value;
        }
        /// <summary>
        /// <see cref="StatsValues.TimeSpendInGame"/>
        /// </summary>
        public TimeSpan TimeSpendInGame
        {
            get => this.statsValues.TimeSpendInGame;
            set => this.statsValues.TimeSpendInGame = value;
        }
        
        /// <summary>
        /// <see cref="StatsValues.BestMultiplier"/>
        /// </summary>
        public int BestMultiplier
        {
            get => this.statsValues.BestMultiplier;
            set
            {
                this.statsValues.BestMultiplier = value;
                this.SetForText(this.bestMultiplierText, this.statsValues.BestMultiplier);
            } 
        }
        /// <summary>
        /// <see cref="Menus.StatsValues.GrapeEvolvedCount"/>
        /// </summary>
        public int GrapeEvolvedCount
        {
            get => this.statsValues.GrapeEvolvedCount;
            set
            {
                this.statsValues.GrapeEvolvedCount = value;
                this.SetForImage(this.grapeText, this.statsValues.GrapeEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="Menus.StatsValues.CherryEvolvedCount"/>
        /// </summary>
        public int CherryEvolvedCount
        {
            get => this.statsValues.CherryEvolvedCount;
            set
            {
                this.statsValues.CherryEvolvedCount = value;
                this.SetForImage(this.cherryText, this.statsValues.CherryEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="Menus.StatsValues.StrawberryEvolvedCount"/>
        /// </summary>
        public int StrawberryEvolvedCount
        {
            get => this.statsValues.StrawberryEvolvedCount;
            set
            {
                this.statsValues.StrawberryEvolvedCount = value;
                this.SetForImage(this.strawberryText, this.statsValues.StrawberryEvolvedCount);
            }
        }
        /// <summary>
        /// <see cref="Menus.StatsValues.LemonEvolvedCount"/>
        /// </summary>
        public int LemonEvolvedCount
        {
            get => this.statsValues.LemonEvolvedCount;
            set
            {
                this.statsValues.LemonEvolvedCount = value;
                this.SetForImage(this.lemonText, this.statsValues.LemonEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="Menus.StatsValues.OrangeEvolvedCount"/>
        /// </summary>
        public int OrangeEvolvedCount
        {
            get => this.statsValues.OrangeEvolvedCount;
            set
            {
                this.statsValues.OrangeEvolvedCount = value;
                this.SetForImage(this.orangeText, this.statsValues.OrangeEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="Menus.StatsValues.AppleEvolvedCount"/>
        /// </summary>
        public int AppleEvolvedCount
        {
            get => this.statsValues.AppleEvolvedCount;
            set
            {
                this.statsValues.AppleEvolvedCount = value;
                this.SetForImage(this.appleText, this.statsValues.AppleEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="Menus.StatsValues.PearEvolvedCount"/>
        /// </summary>
        public int PearEvolvedCount
        {
            get => this.statsValues.PearEvolvedCount;
            set
            {
                this.statsValues.PearEvolvedCount = value;
                this.SetForImage(this.pearText, this.statsValues.PearEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="Menus.StatsValues.PineappleEvolvedCount"/>
        /// </summary>
        public int PineappleEvolvedCount
        {
            get => this.statsValues.PineappleEvolvedCount;
            set
            {
                this.statsValues.PineappleEvolvedCount = value;
                this.SetForImage(this.pineappleText, this.statsValues.PineappleEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="Menus.StatsValues.HoneymelonEvolvedCount"/>
        /// </summary>
        public int HoneymelonEvolvedCount
        {
            get => this.statsValues.HoneymelonEvolvedCount;
            set
            {
                this.statsValues.HoneymelonEvolvedCount = value;
                this.SetForImage(this.honeymelonText, this.statsValues.HoneymelonEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="Menus.StatsValues.WatermelonEvolvedCount"/>
        /// </summary>
        public int WatermelonEvolvedCount
        {
            get => this.statsValues.WatermelonEvolvedCount;
            set
            {
                this.statsValues.WatermelonEvolvedCount = value;
                this.SetForImage(this.watermelonText, this.statsValues.WatermelonEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="Menus.StatsValues.GoldenFruitCount"/>
        /// </summary>
        public int GoldenFruitCount
        {
            get => this.statsValues.GoldenFruitCount;
            set
            {
                this.statsValues.GoldenFruitCount = value;
                this.SetForText(this.goldenFruitsText, this.statsValues.GoldenFruitCount);
            } 
        }
        /// <summary>
        /// <see cref="StatsValues.PowerSkillUsedCount"/>
        /// </summary>
        public int PowerSkillUsedCount
        {
            get => this.statsValues.PowerSkillUsedCount;
            set
            {
                this.statsValues.PowerSkillUsedCount = value;
                this.SetForImage(this.powerText, this.statsValues.PowerSkillUsedCount);
            } 
        }
        /// <summary>
        /// <see cref="StatsValues.EvolveSkillUsedCount"/>
        /// </summary>
        public int EvolveSkillUsedCount
        {
            get => this.statsValues.EvolveSkillUsedCount;
            set
            {
                this.statsValues.EvolveSkillUsedCount = value;
                this.SetForImage(this.evolveText, this.statsValues.EvolveSkillUsedCount);
            } 
        }
        /// <summary>
        /// <see cref="StatsValues.DestroySkillUsedCount"/>
        /// </summary>
        public int DestroySkillUsedCount
        {
            get => this.statsValues.DestroySkillUsedCount;
            set
            {
                this.statsValues.DestroySkillUsedCount = value;
                this.SetForImage(this.destroyText, this.statsValues.DestroySkillUsedCount);
            } 
        }
        #endregion

        #region Methods
        /// <summary>
        /// Use for text labels
        /// </summary>
        /// <param name="_Text">The <see cref="TextMeshProUGUI"/> to set the <see cref="TextMeshProUGUI.text"/> of</param>
        /// <param name="_Value">The value to set into the <see cref="TextMeshProUGUI.text"/></param>
        public void SetForText(TextMeshProUGUI _Text, int _Value)
        {
            this.SetForText(_Text, _Value.ToString());
        }

        /// <summary>
        /// Use for text labels
        /// </summary>
        /// <param name="_Text">The <see cref="TextMeshProUGUI"/> to set the <see cref="TextMeshProUGUI.text"/> of</param>
        /// <param name="_Value">The value to set into the <see cref="TextMeshProUGUI.text"/></param>
        public void SetForText(TextMeshProUGUI _Text, string _Value)
        {
            _Text.text = string.Concat(_Text.gameObject.name, $": {_Value}");
        }
        
        /// <summary>
        /// Use for image labels
        /// </summary>
        /// <param name="_Text">The <see cref="TextMeshProUGUI"/> to set the <see cref="TextMeshProUGUI.text"/> of</param>
        /// <param name="_Value">The value to set into the <see cref="TextMeshProUGUI.text"/></param>
        public void SetForImage(TextMeshProUGUI _Text, int _Value)
        {
            _Text.text = string.Concat($": {_Value}");
        }
        
        /// <summary>
        /// Adds the given <see cref="Fruits.Fruit"/> to <see cref="Stats"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruits.Fruit"/> to add to <see cref="Stats"/></param>
        public void AddFruitCount(Fruits.Fruit _Fruit)
        {
            switch (_Fruit)
            {
                case Fruits.Fruit.Grape:
                    this.GrapeEvolvedCount++;
                    break;
                case Fruits.Fruit.Cherry:
                    this.CherryEvolvedCount++;
                    break;
                case Fruits.Fruit.Strawberry:
                    this.StrawberryEvolvedCount++;
                    break;
                case Fruits.Fruit.Lemon:
                    this.LemonEvolvedCount++;
                    break;
                case Fruits.Fruit.Orange:
                    this.OrangeEvolvedCount++;
                    break;
                case Fruits.Fruit.Apple:
                    this.AppleEvolvedCount++;
                    break;
                case Fruits.Fruit.Pear:
                    this.PearEvolvedCount++;
                    break;
                case Fruits.Fruit.Pineapple:
                    this.PineappleEvolvedCount++;
                    break;
                case Fruits.Fruit.Honeymelon:
                    this.HoneymelonEvolvedCount++;
                    break;
                case Fruits.Fruit.Watermelon:
                    this.WatermelonEvolvedCount++;
                    break;
            }
        }
        
        /// <summary>
        /// Adds the given <see cref="Skills.Skill"/> to <see cref="Stats"/>
        /// </summary>
        /// <param name="_Skill">The <see cref="Skills.Skill"/> to add to <see cref="Stats"/></param>
        public void AddSkillCount(Skill? _Skill)
        {
            switch (_Skill)
            {
                case Skill.Power:
                    this.PowerSkillUsedCount++;
                    break;
                case Skill.Evolve:
                    this.EvolveSkillUsedCount++;
                    break;
                case Skill.Destroy:
                    this.DestroySkillUsedCount ++;
                    break;
            }
        }
        #endregion
    }
}