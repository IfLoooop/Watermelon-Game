using System;
using TMPro;
using UnityEngine;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Menu
{
    internal sealed class StatsMenu : MenuBase
    {
        #region Inspector Fields
        [SerializeField] private TextMeshProUGUI bestScoreText;
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
        [SerializeField] private TextMeshProUGUI gamesPlayedText;
        [SerializeField] private TextMeshProUGUI timeSpendInGameText;
        #endregion

        #region Constants
        public const string BEST_SCORE_KEY = "Highscore";
        private const string HIGHEST_MULTIPLIER_KEY = "Multiplier";
        private const string GRAPE_KEY = "Grape";
        private const string CHERRY_KEY = "Cherry";
        private const string STRAWBERRY_KEY = "Strawberry";
        private const string LEMON_KEY = "Lemon";
        private const string ORANGE_KEY = "Orange";
        private const string APPLE_KEY = "Apple";
        private const string PEAR_KEY = "Pear";
        private const string PINEAPPLE_KEY = "Pineapple";
        private const string HONEY_MELON_KEY = "Honemelon";
        private const string MELON_KEY = "Melon";
        private const string GOLDEN_FRUIT_KEY = "GoldenFruit";
        private const string POWER_KEY = "Power";
        private const string EVOLVE_KEY = "Evolve";
        private const string DESTROY_KEY = "Destroy";
        private const string GAMES_PLAYED_KEY = "GamesPlayed";
        private const string TIME_SPEND_KEY = "TimeSpend";
        #endregion
        
        #region Fields
        private uint bestScore;
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
        private uint gamesPlayed;
        private TimeSpan timeSpendInGame;
        #endregion

        #region Properties
        public static StatsMenu Instance { get; private set; }

        public uint BestScore
        {
            get => this.bestScore;
            set
            {
                this.bestScore = value;
                this.SetText1(this.bestScoreText, this.bestScore.ToString());
            } 
        }
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
        public uint GamesPlayed
        {
            get => this.gamesPlayed;
            set
            {
                this.gamesPlayed = value;
                this.SetText1(this.gamesPlayedText, this.gamesPlayed.ToString());
            } 
        }
        public TimeSpan TimeSpendInGame
        {
            get => this.timeSpendInGame;
            set
            {
                this.timeSpendInGame = value;
                SetTimeSpendText();
            } 
        }
        #endregion

        #region Methods
        private void Awake()
        {
            Instance = this;
            
            this.Load();
        }

        private void OnEnable()
        {
            this.SetTimeSpendText(Time.time);
        }
        
        private void SetText1(TextMeshProUGUI _Text, string _Value)
        {
            _Text.text = string.Concat(_Text.gameObject.name, $" {_Value}");
        }

        private void SetText2(TextMeshProUGUI _Text, string _Value)
        {
            _Text.text = string.Concat($": {_Value}");
        }

        private void SetTimeSpendText(float _Seconds = 0.0f)
        {
            var _timeSpendInGame = new TimeSpan().Add(this.timeSpendInGame).Add(TimeSpan.FromSeconds(_Seconds));

            if (this.timeSpendInGame.Hours > 0)
            {
                this.SetText1(this.timeSpendInGameText, string.Concat(_timeSpendInGame.Hours, "h"));
            }
            else if (this.timeSpendInGame.Minutes > 0)
            {
                this.SetText1(this.timeSpendInGameText, string.Concat(_timeSpendInGame.Minutes, "min"));
            }
            else
            {
                this.SetText1(this.timeSpendInGameText, string.Concat(_timeSpendInGame.Seconds, "sec"));
            }
        }
        
        private void Load()
        {
            this.BestScore = (uint)PlayerPrefs.GetInt(BEST_SCORE_KEY);
            this.HighestMultiplier = (uint)PlayerPrefs.GetInt(HIGHEST_MULTIPLIER_KEY);
            this.GrapeEvolvedCount = (uint)PlayerPrefs.GetInt(GRAPE_KEY);
            this.CherryEvolvedCount = (uint)PlayerPrefs.GetInt(CHERRY_KEY);
            this.StrawberryEvolvedCount = (uint)PlayerPrefs.GetInt(STRAWBERRY_KEY);
            this.LemonEvolvedCount = (uint)PlayerPrefs.GetInt(LEMON_KEY);
            this.OrangeEvolvedCount = (uint)PlayerPrefs.GetInt(ORANGE_KEY);
            this.AppleEvolvedCount = (uint)PlayerPrefs.GetInt(APPLE_KEY); 
            this.PearEvolvedCount = (uint)PlayerPrefs.GetInt(PEAR_KEY);
            this.PineappleEvolvedCount = (uint)PlayerPrefs.GetInt(PINEAPPLE_KEY);
            this.HoneyMelonEvolvedCount = (uint)PlayerPrefs.GetInt(HONEY_MELON_KEY);
            this.MelonEvolvedCount = (uint)PlayerPrefs.GetInt(MELON_KEY);
            this.GoldenFruitCount = (uint)PlayerPrefs.GetInt(GOLDEN_FRUIT_KEY);
            this.PowerSkillUsedCount = (uint)PlayerPrefs.GetInt(POWER_KEY);
            this.EvolveSkillUsedCount = (uint)PlayerPrefs.GetInt(EVOLVE_KEY);
            this.DestroySkillUsedCount = (uint)PlayerPrefs.GetInt(DESTROY_KEY);
            this.GamesPlayed = (uint)PlayerPrefs.GetInt(GAMES_PLAYED_KEY);
            TimeSpan.TryParse(PlayerPrefs.GetString(TIME_SPEND_KEY), out var _timeSPendInGame);
            this.TimeSpendInGame = _timeSPendInGame;
        }
        
        public void Save()
        {
            PlayerPrefs.SetInt(BEST_SCORE_KEY, (int)this.bestScore);
            PlayerPrefs.SetInt(HIGHEST_MULTIPLIER_KEY, (int)this.highestMultiplier);
            PlayerPrefs.SetInt(GRAPE_KEY, (int)this.grapeEvolvedCount);
            PlayerPrefs.SetInt(CHERRY_KEY, (int)this.cherryEvolvedCount);
            PlayerPrefs.SetInt(STRAWBERRY_KEY, (int)this.strawberryEvolvedCount);
            PlayerPrefs.SetInt(LEMON_KEY, (int)this.lemonEvolvedCount);
            PlayerPrefs.SetInt(ORANGE_KEY, (int)this.orangeEvolvedCount);
            PlayerPrefs.SetInt(APPLE_KEY, (int)this.appleEvolvedCount);
            PlayerPrefs.SetInt(PEAR_KEY, (int)this.pearEvolvedCount);
            PlayerPrefs.SetInt(PINEAPPLE_KEY, (int)this.pineappleEvolvedCount);
            PlayerPrefs.SetInt(HONEY_MELON_KEY, (int)this.honeyMelonEvolvedCount);
            PlayerPrefs.SetInt(MELON_KEY, (int)this.melonEvolvedCount);
            PlayerPrefs.SetInt(GOLDEN_FRUIT_KEY, (int)this.goldenFruitCount);
            PlayerPrefs.SetInt(POWER_KEY, (int)this.powerSkillUsedCount);
            PlayerPrefs.SetInt(EVOLVE_KEY, (int)this.evolveSkillUsedCount);
            PlayerPrefs.SetInt(DESTROY_KEY, (int)this.destroySkillUsedCount);
            PlayerPrefs.SetInt(GAMES_PLAYED_KEY, (int)this.gamesPlayed);
            PlayerPrefs.SetString(TIME_SPEND_KEY, this.timeSpendInGame.Add(TimeSpan.FromSeconds(Time.time)).ToString());
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
                    this.DestroySkillUsedCount++;
                    break;
            }
        }
        #endregion
    }
}