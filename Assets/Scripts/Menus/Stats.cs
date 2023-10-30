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
        /// Best multiplier
        /// </summary>
        private uint bestMultiplier;
        /// <summary>
        /// Total evolved grape count
        /// </summary>
        private uint grapesEvolvedCount;
        /// <summary>
        /// Total evolved cherry count
        /// </summary>
        private uint cherriesEvolvedCount;
        /// <summary>
        /// Total evolved strawberry count
        /// </summary>
        private uint strawberriesEvolvedCount;
        /// <summary>
        /// Total evolved lemon count
        /// </summary>
        private uint lemonsEvolvedCount;
        /// <summary>
        /// Total evolved orange count
        /// </summary>
        private uint orangesEvolvedCount;
        /// <summary>
        /// Total evolved apple count
        /// </summary>
        private uint applesEvolvedCount;
        /// <summary>
        /// Total evolved pear count
        /// </summary>
        private uint pearsEvolvedCount;
        /// <summary>
        /// Total evolved pineapple count
        /// </summary>
        private uint pineapplesEvolvedCount;
        /// <summary>
        /// Total evolved honeymelon count
        /// </summary>
        private uint honeymelonsEvolvedCount;
        /// <summary>
        /// Total evolved watermelon count
        /// </summary>
        private uint watermelonsEvolvedCount;
        /// <summary>
        /// Total golden fruit count
        /// </summary>
        private uint goldenFruitsCount;
        /// <summary>
        /// Total amount of power skills used
        /// </summary>
        private uint powerSkillUsedCount;
        /// <summary>
        /// Total amount of evolve skills used
        /// </summary>
        private uint evolveSkillUsedCount;
        /// <summary>
        /// Total amount of destroy skills used
        /// </summary>
        private uint destroySkillUsedCount;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="bestMultiplier"/>
        /// </summary>
        public uint BestMultiplier
        {
            get => this.bestMultiplier;
            set
            {
                this.bestMultiplier = value;
                this.SetForText(this.bestMultiplierText, this.bestMultiplier);
            } 
        }
        /// <summary>
        /// <see cref="grapesEvolvedCount"/>
        /// </summary>
        public uint GrapeEvolvedCount
        {
            get => this.grapesEvolvedCount;
            set
            {
                this.grapesEvolvedCount = value;
                this.SetForImage(this.grapeText, this.grapesEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="cherriesEvolvedCount"/>
        /// </summary>
        public uint CherryEvolvedCount
        {
            get => this.cherriesEvolvedCount;
            set
            {
                this.cherriesEvolvedCount = value;
                this.SetForImage(this.cherryText, this.cherriesEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="strawberriesEvolvedCount"/>
        /// </summary>
        public uint StrawberryEvolvedCount
        {
            get => this.strawberriesEvolvedCount;
            set
            {
                this.strawberriesEvolvedCount = value;
                this.SetForImage(this.strawberryText, this.strawberriesEvolvedCount);
            }
        }
        /// <summary>
        /// <see cref="lemonsEvolvedCount"/>
        /// </summary>
        public uint LemonEvolvedCount
        {
            get => this.lemonsEvolvedCount;
            set
            {
                this.lemonsEvolvedCount = value;
                this.SetForImage(this.lemonText, this.lemonsEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="orangesEvolvedCount"/>
        /// </summary>
        public uint OrangeEvolvedCount
        {
            get => this.orangesEvolvedCount;
            set
            {
                this.orangesEvolvedCount = value;
                this.SetForImage(this.orangeText, this.orangesEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="applesEvolvedCount"/>
        /// </summary>
        public uint AppleEvolvedCount
        {
            get => this.applesEvolvedCount;
            set
            {
                this.applesEvolvedCount = value;
                this.SetForImage(this.appleText, this.applesEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="pearsEvolvedCount"/>
        /// </summary>
        public uint PearEvolvedCount
        {
            get => this.pearsEvolvedCount;
            set
            {
                this.pearsEvolvedCount = value;
                this.SetForImage(this.pearText, this.pearsEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="pineapplesEvolvedCount"/>
        /// </summary>
        public uint PineappleEvolvedCount
        {
            get => this.pineapplesEvolvedCount;
            set
            {
                this.pineapplesEvolvedCount = value;
                this.SetForImage(this.pineappleText, this.pineapplesEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="honeymelonsEvolvedCount"/>
        /// </summary>
        public uint HoneymelonEvolvedCount
        {
            get => this.honeymelonsEvolvedCount;
            set
            {
                this.honeymelonsEvolvedCount = value;
                this.SetForImage(this.honeymelonText, this.honeymelonsEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="watermelonsEvolvedCount"/>
        /// </summary>
        public uint WatermelonEvolvedCount
        {
            get => this.watermelonsEvolvedCount;
            set
            {
                this.watermelonsEvolvedCount = value;
                this.SetForImage(this.watermelonText, this.watermelonsEvolvedCount);
            } 
        }
        /// <summary>
        /// <see cref="goldenFruitsCount"/>
        /// </summary>
        public uint GoldenFruitCount
        {
            get => this.goldenFruitsCount;
            set
            {
                this.goldenFruitsCount = value;
                this.SetForText(this.goldenFruitsText, this.goldenFruitsCount);
            } 
        }
        /// <summary>
        /// <see cref="powerSkillUsedCount"/>
        /// </summary>
        public uint PowerSkillUsedCount
        {
            get => this.powerSkillUsedCount;
            set
            {
                this.powerSkillUsedCount = value;
                this.SetForImage(this.powerText, this.powerSkillUsedCount);
            } 
        }
        /// <summary>
        /// <see cref="evolveSkillUsedCount"/>
        /// </summary>
        public uint EvolveSkillUsedCount
        {
            get => this.evolveSkillUsedCount;
            set
            {
                this.evolveSkillUsedCount = value;
                this.SetForImage(this.evolveText, this.evolveSkillUsedCount);
            } 
        }
        /// <summary>
        /// <see cref="destroySkillUsedCount"/>
        /// </summary>
        public uint DestroySkillUsedCount
        {
            get => this.destroySkillUsedCount;
            set
            {
                this.destroySkillUsedCount = value;
                this.SetForImage(this.destroyText, this.destroySkillUsedCount);
            } 
        }
        #endregion

        #region Methods
        /// <summary>
        /// Use for text labels
        /// </summary>
        /// <param name="_Text">The <see cref="TextMeshProUGUI"/> to set the <see cref="TextMeshProUGUI.text"/> of</param>
        /// <param name="_Value">The value to set into the <see cref="TextMeshProUGUI.text"/></param>
        public void SetForText(TextMeshProUGUI _Text, uint _Value)
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
        public void SetForImage(TextMeshProUGUI _Text, uint _Value)
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