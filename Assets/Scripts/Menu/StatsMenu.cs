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
        [SerializeField] private Stats stats;
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
                this.stats.SetText1(this.bestScoreText, this.bestScore.ToString());
            } 
        }
        public Stats Stats => this.stats;
        public uint GamesPlayed
        {
            get => this.gamesPlayed;
            set
            {
                this.gamesPlayed = value;
                this.stats.SetText1(this.gamesPlayedText, this.gamesPlayed.ToString());
            } 
        }
        public TimeSpan TimeSpendInGame
        {
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
        
        private void SetTimeSpendText(float _Seconds = 0.0f)
        {
            var _timeSpendInGame = new TimeSpan().Add(this.timeSpendInGame).Add(TimeSpan.FromSeconds(_Seconds));

            if (_timeSpendInGame.Hours > 0)
            {
                this.stats.SetText1(this.timeSpendInGameText, string.Concat(_timeSpendInGame.Hours, "h"));
            }
            else if (_timeSpendInGame.Minutes > 0)
            {
                this.stats.SetText1(this.timeSpendInGameText, string.Concat(_timeSpendInGame.Minutes, "min"));
            }
            else
            {
                this.stats.SetText1(this.timeSpendInGameText, string.Concat(_timeSpendInGame.Seconds, "sec"));
            }
        }
        
        private void Load()
        {
            this.BestScore = (uint)PlayerPrefs.GetInt(BEST_SCORE_KEY);
            this.stats.HighestMultiplier = (uint)PlayerPrefs.GetInt(HIGHEST_MULTIPLIER_KEY);
            this.stats.GrapeEvolvedCount = (uint)PlayerPrefs.GetInt(GRAPE_KEY);
            this.stats.CherryEvolvedCount = (uint)PlayerPrefs.GetInt(CHERRY_KEY);
            this.stats.StrawberryEvolvedCount = (uint)PlayerPrefs.GetInt(STRAWBERRY_KEY);
            this.stats.LemonEvolvedCount = (uint)PlayerPrefs.GetInt(LEMON_KEY);
            this.stats.OrangeEvolvedCount = (uint)PlayerPrefs.GetInt(ORANGE_KEY);
            this.stats.AppleEvolvedCount = (uint)PlayerPrefs.GetInt(APPLE_KEY); 
            this.stats.PearEvolvedCount = (uint)PlayerPrefs.GetInt(PEAR_KEY);
            this.stats.PineappleEvolvedCount = (uint)PlayerPrefs.GetInt(PINEAPPLE_KEY);
            this.stats.HoneyMelonEvolvedCount = (uint)PlayerPrefs.GetInt(HONEY_MELON_KEY);
            this.stats.MelonEvolvedCount = (uint)PlayerPrefs.GetInt(MELON_KEY);
            this.stats.GoldenFruitCount = (uint)PlayerPrefs.GetInt(GOLDEN_FRUIT_KEY);
            this.stats.PowerSkillUsedCount = (uint)PlayerPrefs.GetInt(POWER_KEY);
            this.stats.EvolveSkillUsedCount = (uint)PlayerPrefs.GetInt(EVOLVE_KEY);
            this.stats.DestroySkillUsedCount = (uint)PlayerPrefs.GetInt(DESTROY_KEY);
            this.GamesPlayed = (uint)PlayerPrefs.GetInt(GAMES_PLAYED_KEY);
            TimeSpan.TryParse(PlayerPrefs.GetString(TIME_SPEND_KEY), out var _timeSPendInGame);
            this.TimeSpendInGame = _timeSPendInGame;
        }
        
        public void Save()
        {
            PlayerPrefs.SetInt(BEST_SCORE_KEY, (int)this.bestScore);
            PlayerPrefs.SetInt(HIGHEST_MULTIPLIER_KEY, (int)this.stats.HighestMultiplier);
            PlayerPrefs.SetInt(GRAPE_KEY, (int)this.stats.GrapeEvolvedCount);
            PlayerPrefs.SetInt(CHERRY_KEY, (int)this.stats.CherryEvolvedCount);
            PlayerPrefs.SetInt(STRAWBERRY_KEY, (int)this.stats.StrawberryEvolvedCount);
            PlayerPrefs.SetInt(LEMON_KEY, (int)this.stats.LemonEvolvedCount);
            PlayerPrefs.SetInt(ORANGE_KEY, (int)this.stats.OrangeEvolvedCount);
            PlayerPrefs.SetInt(APPLE_KEY, (int)this.stats.AppleEvolvedCount);
            PlayerPrefs.SetInt(PEAR_KEY, (int)this.stats.PearEvolvedCount);
            PlayerPrefs.SetInt(PINEAPPLE_KEY, (int)this.stats.PineappleEvolvedCount);
            PlayerPrefs.SetInt(HONEY_MELON_KEY, (int)this.stats.HoneyMelonEvolvedCount);
            PlayerPrefs.SetInt(MELON_KEY, (int)this.stats.MelonEvolvedCount);
            PlayerPrefs.SetInt(GOLDEN_FRUIT_KEY, (int)this.stats.GoldenFruitCount);
            PlayerPrefs.SetInt(POWER_KEY, (int)this.stats.PowerSkillUsedCount);
            PlayerPrefs.SetInt(EVOLVE_KEY, (int)this.stats.EvolveSkillUsedCount);
            PlayerPrefs.SetInt(DESTROY_KEY, (int)this.stats.DestroySkillUsedCount);
            PlayerPrefs.SetInt(GAMES_PLAYED_KEY, (int)this.gamesPlayed);
            PlayerPrefs.SetString(TIME_SPEND_KEY, this.timeSpendInGame.Add(TimeSpan.FromSeconds(Time.time)).ToString());
        }

        public void AddFruitCount(Fruit.Fruit _Fruit, uint _Amount)
        {
            switch (_Fruit)
            {
                case Fruit.Fruit.Grape:
                    this.stats.GrapeEvolvedCount += _Amount;
                    break;
                case Fruit.Fruit.Cherry:
                    this.stats.CherryEvolvedCount += _Amount;
                    break;
                case Fruit.Fruit.Strawberry:
                    this.stats.StrawberryEvolvedCount += _Amount;
                    break;
                case Fruit.Fruit.Lemon:
                    this.stats.LemonEvolvedCount += _Amount;
                    break;
                case Fruit.Fruit.Orange:
                    this.stats.OrangeEvolvedCount += _Amount;
                    break;
                case Fruit.Fruit.Apple:
                    this.stats.AppleEvolvedCount += _Amount;
                    break;
                case Fruit.Fruit.Pear:
                    this.stats.PearEvolvedCount += _Amount;
                    break;
                case Fruit.Fruit.Pineapple:
                    this.stats.PineappleEvolvedCount += _Amount;
                    break;
                case Fruit.Fruit.HoneyMelon:
                    this.stats.HoneyMelonEvolvedCount += _Amount;
                    break;
                case Fruit.Fruit.Melon:
                    this.stats.MelonEvolvedCount += _Amount;
                    break;
            }
        }
        
        public void AddSkillCount(Skill _Skill, uint _Amount)
        {
            switch (_Skill)
            {
                case Skill.Power:
                    this.stats.PowerSkillUsedCount += _Amount;
                    break;
                case Skill.Evolve:
                    this.stats.EvolveSkillUsedCount += _Amount;
                    break;
                case Skill.Destroy:
                    this.stats.DestroySkillUsedCount += _Amount;
                    break;
            }
        }
        #endregion
    }
}