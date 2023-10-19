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
        [SerializeField] private TextMeshProUGUI highestMultiplierText;
        [SerializeField] private TextMeshProUGUI grapeText;
        [SerializeField] private TextMeshProUGUI cherryText;
        [SerializeField] private TextMeshProUGUI strawberryText;
        [SerializeField] private TextMeshProUGUI lemonText;
        [SerializeField] private TextMeshProUGUI orangeText;
        [SerializeField] private TextMeshProUGUI appleText;
        [SerializeField] private TextMeshProUGUI pearText;
        [SerializeField] private TextMeshProUGUI pineappleText;
        [SerializeField] private TextMeshProUGUI honeMelonText;
        [SerializeField] private TextMeshProUGUI melonText;
        [SerializeField] private TextMeshProUGUI goldenFruitText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI evolveText;
        [SerializeField] private TextMeshProUGUI destroyText;
        #endregion

        #region Fields
        private uint highestMultiplier;
        private uint grapeEvolvedCount;
        private uint cherryEvolvedCount;
        private uint strawberryEvolvedCount;
        private uint lemonEvolvedCount;
        private uint orangeEvolvedCount;
        private uint appleEvolvedCount;
        private uint pearEvolvedCount;
        private uint pineappleEvolvedCount;
        private uint honeyMelonEvolvedCount;
        private uint melonEvolvedCount;
        private uint goldenFruitCount;
        private uint powerSkillUsedCount;
        private uint evolveSkillUsedCount;
        private uint destroySkillUsedCount;
        #endregion

        #region Properties
        public uint HighestMultiplier
        {
            get => this.highestMultiplier;
            set
            {
                this.highestMultiplier = value;
                this.SetForText(this.highestMultiplierText, this.highestMultiplier);
            } 
        }
        public uint GrapeEvolvedCount
        {
            get => this.grapeEvolvedCount;
            set
            {
                this.grapeEvolvedCount = value;
                this.SetForImage(this.grapeText, this.grapeEvolvedCount);
            } 
        }
        public uint CherryEvolvedCount
        {
            get => this.cherryEvolvedCount;
            set
            {
                this.cherryEvolvedCount = value;
                this.SetForImage(this.cherryText, this.cherryEvolvedCount);
            } 
        }
        public uint StrawberryEvolvedCount
        {
            get => this.strawberryEvolvedCount;
            set
            {
                this.strawberryEvolvedCount = value;
                this.SetForImage(this.strawberryText, this.strawberryEvolvedCount);
            }
        }
        public uint LemonEvolvedCount
        {
            get => this.lemonEvolvedCount;
            set
            {
                this.lemonEvolvedCount = value;
                this.SetForImage(this.lemonText, this.lemonEvolvedCount);
            } 
        }
        public uint OrangeEvolvedCount
        {
            get => this.orangeEvolvedCount;
            set
            {
                this.orangeEvolvedCount = value;
                this.SetForImage(this.orangeText, this.orangeEvolvedCount);
            } 
        }
        public uint AppleEvolvedCount
        {
            get => this.appleEvolvedCount;
            set
            {
                this.appleEvolvedCount = value;
                this.SetForImage(this.appleText, this.appleEvolvedCount);
            } 
        }
        public uint PearEvolvedCount
        {
            get => this.pearEvolvedCount;
            set
            {
                this.pearEvolvedCount = value;
                this.SetForImage(this.pearText, this.pearEvolvedCount);
            } 
        }
        public uint PineappleEvolvedCount
        {
            get => this.pineappleEvolvedCount;
            set
            {
                this.pineappleEvolvedCount = value;
                this.SetForImage(this.pineappleText, this.pineappleEvolvedCount);
            } 
        }
        public uint HoneyMelonEvolvedCount
        {
            get => this.honeyMelonEvolvedCount;
            set
            {
                this.honeyMelonEvolvedCount = value;
                this.SetForImage(this.honeMelonText, this.honeyMelonEvolvedCount);
            } 
        }
        public uint MelonEvolvedCount
        {
            get => this.melonEvolvedCount;
            set
            {
                this.melonEvolvedCount = value;
                this.SetForImage(this.melonText, this.melonEvolvedCount);
            } 
        }
        public uint GoldenFruitCount
        {
            get => this.goldenFruitCount;
            set
            {
                this.goldenFruitCount = value;
                this.SetForText(this.goldenFruitText, this.goldenFruitCount);
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
            _Text.text = string.Concat(_Text.gameObject.name, $" {_Value}");
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