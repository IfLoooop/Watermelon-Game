using System;
using TMPro;
using UnityEngine;

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
                this.SetText1(this.highestMultiplierText, this.highestMultiplier.ToString());
            } 
        }
        public uint GrapeEvolvedCount
        {
            get => this.grapeEvolvedCount;
            set
            {
                this.grapeEvolvedCount = value;
                this.SetText2(this.grapeText, this.grapeEvolvedCount.ToString());
            } 
        }
        public uint CherryEvolvedCount
        {
            get => this.cherryEvolvedCount;
            set
            {
                this.cherryEvolvedCount = value;
                this.SetText2(this.cherryText, this.cherryEvolvedCount.ToString());
            } 
        }
        public uint StrawberryEvolvedCount
        {
            get => this.strawberryEvolvedCount;
            set
            {
                this.strawberryEvolvedCount = value;
                this.SetText2(this.strawberryText, this.strawberryEvolvedCount.ToString());
            }
        }
        public uint LemonEvolvedCount
        {
            get => this.lemonEvolvedCount;
            set
            {
                this.lemonEvolvedCount = value;
                this.SetText2(this.lemonText, this.lemonEvolvedCount.ToString());
            } 
        }
        public uint OrangeEvolvedCount
        {
            get => this.orangeEvolvedCount;
            set
            {
                this.orangeEvolvedCount = value;
                this.SetText2(this.orangeText, this.orangeEvolvedCount.ToString());
            } 
        }
        public uint AppleEvolvedCount
        {
            get => this.appleEvolvedCount;
            set
            {
                this.appleEvolvedCount = value;
                this.SetText2(this.appleText, this.appleEvolvedCount.ToString());
            } 
        }
        public uint PearEvolvedCount
        {
            get => this.pearEvolvedCount;
            set
            {
                this.pearEvolvedCount = value;
                this.SetText2(this.pearText, this.pearEvolvedCount.ToString());
            } 
        }
        public uint PineappleEvolvedCount
        {
            get => this.pineappleEvolvedCount;
            set
            {
                this.pineappleEvolvedCount = value;
                this.SetText2(this.pineappleText, this.pineappleEvolvedCount.ToString());
            } 
        }
        public uint HoneyMelonEvolvedCount
        {
            get => this.honeyMelonEvolvedCount;
            set
            {
                this.honeyMelonEvolvedCount = value;
                this.SetText2(this.honeMelonText, this.honeyMelonEvolvedCount.ToString());
            } 
        }
        public uint MelonEvolvedCount
        {
            get => this.melonEvolvedCount;
            set
            {
                this.melonEvolvedCount = value;
                this.SetText2(this.melonText, this.melonEvolvedCount.ToString());
            } 
        }
        public uint GoldenFruitCount
        {
            get => this.goldenFruitCount;
            set
            {
                this.goldenFruitCount = value;
                this.SetText1(this.goldenFruitText, this.goldenFruitCount.ToString());
            } 
        }
        public uint PowerSkillUsedCount
        {
            get => this.powerSkillUsedCount;
            set
            {
                this.powerSkillUsedCount = value;
                this.SetText2(this.powerText, this.powerSkillUsedCount.ToString());
            } 
        }
        public uint EvolveSkillUsedCount
        {
            get => this.evolveSkillUsedCount;
            set
            {
                this.evolveSkillUsedCount = value;
                this.SetText2(this.evolveText, this.evolveSkillUsedCount.ToString());
            } 
        }
        public uint DestroySkillUsedCount
        {
            get => this.destroySkillUsedCount;
            set
            {
                this.destroySkillUsedCount = value;
                this.SetText2(this.destroyText, this.destroySkillUsedCount.ToString());
            } 
        }
        #endregion

        #region Methods
        public void SetText1(TextMeshProUGUI _Text, string _Value)
        {
            _Text.text = string.Concat(_Text.gameObject.name, $" {_Value}");
        }

        public void SetText2(TextMeshProUGUI _Text, string _Value)
        {
            _Text.text = string.Concat($": {_Value}");
        }
        #endregion
    }
}