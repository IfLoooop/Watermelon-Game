using System;
using TMPro;
using UnityEngine;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Menu
{
    [Serializable]
    internal sealed class Stats
    {
        #region Inspector Fields
        [SerializeField] private TextMeshProUGUI bestMultiplierText;
        [SerializeField] private TextMeshProUGUI grapeText;
        [SerializeField] private TextMeshProUGUI cherryText;
        [SerializeField] private TextMeshProUGUI strawberryText;
        [SerializeField] private TextMeshProUGUI lemonText;
        [SerializeField] private TextMeshProUGUI orangeText;
        [SerializeField] private TextMeshProUGUI appleText;
        [SerializeField] private TextMeshProUGUI pearText;
        [SerializeField] private TextMeshProUGUI pineappleText;
        [SerializeField] private TextMeshProUGUI honeMelonText;
        [SerializeField] private TextMeshProUGUI watermelonText;
        [SerializeField] private TextMeshProUGUI goldenFruitsText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI evolveText;
        [SerializeField] private TextMeshProUGUI destroyText;
        #endregion

        #region Fields
        private uint bestMultiplier;
        private uint grapesEvolvedCount;
        private uint cherriesEvolvedCount;
        private uint strawberriesEvolvedCount;
        private uint lemonsEvolvedCount;
        private uint orangesEvolvedCount;
        private uint applesEvolvedCount;
        private uint pearsEvolvedCount;
        private uint pineapplesEvolvedCount;
        private uint honeymelonsEvolvedCount;
        private uint watermelonsEvolvedCount;
        private uint goldenFruitsCount;
        private uint powerSkillUsedCount;
        private uint evolveSkillUsedCount;
        private uint destroySkillUsedCount;
        #endregion

        #region Properties
        public uint HighestMultiplier
        {
            get => this.bestMultiplier;
            set
            {
                this.bestMultiplier = value;
                this.SetForText(this.bestMultiplierText, this.bestMultiplier);
            } 
        }
        public uint GrapeEvolvedCount
        {
            get => this.grapesEvolvedCount;
            set
            {
                this.grapesEvolvedCount = value;
                this.SetForImage(this.grapeText, this.grapesEvolvedCount);
            } 
        }
        public uint CherryEvolvedCount
        {
            get => this.cherriesEvolvedCount;
            set
            {
                this.cherriesEvolvedCount = value;
                this.SetForImage(this.cherryText, this.cherriesEvolvedCount);
            } 
        }
        public uint StrawberryEvolvedCount
        {
            get => this.strawberriesEvolvedCount;
            set
            {
                this.strawberriesEvolvedCount = value;
                this.SetForImage(this.strawberryText, this.strawberriesEvolvedCount);
            }
        }
        public uint LemonEvolvedCount
        {
            get => this.lemonsEvolvedCount;
            set
            {
                this.lemonsEvolvedCount = value;
                this.SetForImage(this.lemonText, this.lemonsEvolvedCount);
            } 
        }
        public uint OrangeEvolvedCount
        {
            get => this.orangesEvolvedCount;
            set
            {
                this.orangesEvolvedCount = value;
                this.SetForImage(this.orangeText, this.orangesEvolvedCount);
            } 
        }
        public uint AppleEvolvedCount
        {
            get => this.applesEvolvedCount;
            set
            {
                this.applesEvolvedCount = value;
                this.SetForImage(this.appleText, this.applesEvolvedCount);
            } 
        }
        public uint PearEvolvedCount
        {
            get => this.pearsEvolvedCount;
            set
            {
                this.pearsEvolvedCount = value;
                this.SetForImage(this.pearText, this.pearsEvolvedCount);
            } 
        }
        public uint PineappleEvolvedCount
        {
            get => this.pineapplesEvolvedCount;
            set
            {
                this.pineapplesEvolvedCount = value;
                this.SetForImage(this.pineappleText, this.pineapplesEvolvedCount);
            } 
        }
        public uint HoneyMelonEvolvedCount
        {
            get => this.honeymelonsEvolvedCount;
            set
            {
                this.honeymelonsEvolvedCount = value;
                this.SetForImage(this.honeMelonText, this.honeymelonsEvolvedCount);
            } 
        }
        public uint MelonEvolvedCount
        {
            get => this.watermelonsEvolvedCount;
            set
            {
                this.watermelonsEvolvedCount = value;
                this.SetForImage(this.watermelonText, this.watermelonsEvolvedCount);
            } 
        }
        public uint GoldenFruitCount
        {
            get => this.goldenFruitsCount;
            set
            {
                this.goldenFruitsCount = value;
                this.SetForText(this.goldenFruitsText, this.goldenFruitsCount);
            } 
        }
        public uint PowerSkillUsedCount
        {
            get => this.powerSkillUsedCount;
            set
            {
                this.powerSkillUsedCount = value;
                this.SetForImage(this.powerText, this.powerSkillUsedCount);
            } 
        }
        public uint EvolveSkillUsedCount
        {
            get => this.evolveSkillUsedCount;
            set
            {
                this.evolveSkillUsedCount = value;
                this.SetForImage(this.evolveText, this.evolveSkillUsedCount);
            } 
        }
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
        public void SetForText(TextMeshProUGUI _Text, uint _Value)
        {
            this.SetForText(_Text, _Value.ToString());
        }

        public void SetForText(TextMeshProUGUI _Text, string _Value)
        {
            _Text.text = string.Concat(_Text.gameObject.name, $": {_Value}");
        }
        
        public void SetForImage(TextMeshProUGUI _Text, uint _Value)
        {
            _Text.text = string.Concat($": {_Value}");
        }
        
        public void AddFruitCount(Fruit.Fruit _Fruit)
        {
            switch (_Fruit)
            {
                case Fruit.Fruit.Grape:
                    this.GrapeEvolvedCount++;
                    break;
                case Fruit.Fruit.Cherry:
                    this.CherryEvolvedCount++;
                    break;
                case Fruit.Fruit.Strawberry:
                    this.StrawberryEvolvedCount++;
                    break;
                case Fruit.Fruit.Lemon:
                    this.LemonEvolvedCount++;
                    break;
                case Fruit.Fruit.Orange:
                    this.OrangeEvolvedCount++;
                    break;
                case Fruit.Fruit.Apple:
                    this.AppleEvolvedCount++;
                    break;
                case Fruit.Fruit.Pear:
                    this.PearEvolvedCount++;
                    break;
                case Fruit.Fruit.Pineapple:
                    this.PineappleEvolvedCount++;
                    break;
                case Fruit.Fruit.HoneyMelon:
                    this.HoneyMelonEvolvedCount++;
                    break;
                case Fruit.Fruit.Melon:
                    this.MelonEvolvedCount++;
                    break;
            }
        }
        
        public void AddSkillCount(Skill _Skill)
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