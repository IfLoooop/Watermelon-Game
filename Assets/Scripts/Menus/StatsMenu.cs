using System;
using OPS.AntiCheat.Field;
using TMPro;
using UnityEngine;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Contains global stats
    /// </summary>
    internal sealed class StatsMenu : ScrollRectBase
    {
        #region Inspector Fields
        [Tooltip("TMP component that displays the best score")]
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [Tooltip("Contains various statistics")]
        [SerializeField] private Stats stats;
        [Tooltip("TMP component that displays the played games amount")]
        [SerializeField] private TextMeshProUGUI gamesPlayedText;
        [Tooltip("TMP component that displays the total time spend in game")]
        [SerializeField] private TextMeshProUGUI timeSpendInGameText;
        #endregion

        // TODO: Use safe-controller
        #region Playerprefs keys
#if DEBUG || DEVELOPMENT_BUILD
        private const string BEST_SCORE_KEY = "Highscore_";
        private const string HIGHEST_MULTIPLIER_KEY = "Multiplier_";
        private const string GRAPE_KEY = "Grape_";
        private const string CHERRY_KEY = "Cherry_";
        private const string STRAWBERRY_KEY = "Strawberry_";
        private const string LEMON_KEY = "Lemon_";
        private const string ORANGE_KEY = "Orange_";
        private const string APPLE_KEY = "Apple_";
        private const string PEAR_KEY = "Pear_";
        private const string PINEAPPLE_KEY = "Pineapple_";
        private const string HONEY_MELON_KEY = "Honemelon_";
        private const string MELON_KEY = "Melon_";
        private const string GOLDEN_FRUIT_KEY = "GoldenFruit_";
        private const string POWER_KEY = "Power_";
        private const string EVOLVE_KEY = "Evolve_";
        private const string DESTROY_KEY = "Destroy_";
        private const string GAMES_PLAYED_KEY = "GamesPlayed_";
        private const string TIME_SPEND_KEY = "TimeSpend_";
#else
        private const string BEST_SCORE_KEY = "Highscore";
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
#endif
        #endregion
        
        #region Properties
        /// <summary>
        /// Singleton of <see cref="StatsMenu"/>
        /// </summary>
        public static StatsMenu Instance { get; private set; }
        
        /// <summary>
        /// <see cref="Menus.Stats"/>
        /// </summary>
        public Stats Stats => this.stats;
        
        /// <summary>
        /// <see cref="StatsValues.BestScore"/>
        /// </summary>
        public int BestScore
        {
            get => this.stats.StatsValues.BestScore;
            private set
            {
                this.stats.BestScore = value;
                this.stats.SetForText(this.bestScoreText, this.stats.BestScore);
            } 
        }
        /// <summary>
        /// <see cref="StatsValues.GamesPlayed"/>
        /// </summary>
        private ProtectedInt32 GamesPlayed
        {
            get => this.stats.StatsValues.GamesPlayed;
            set
            {
                this.stats.GamesPlayed = value;
                this.stats.SetForText(this.gamesPlayedText, this.stats.GamesPlayed);
            } 
        }
        /// <summary>
        /// <see cref="StatsValues.TimeSpendInGame"/>
        /// </summary>
        private TimeSpan TimeSpendInGame
        {
            get => this.stats.StatsValues.TimeSpendInGame;
            set
            {
                this.stats.TimeSpendInGame = value;
                SetTimeSpendInGameText();
            } 
        }
        #endregion

        #region Methods
        private void Awake()
        {
            Instance = this;

            this.Load(true);
        }
        
        /// <summary>
        /// <see cref="MenuBase.Open_Close"/>
        /// </summary>
        /// <param name="_CurrentActiveMenu"><see cref="MenuController.currentActiveMenu"/></param>
        /// <param name="_ForceClose">Forces the active menu to close even if <see cref="MenuBase.canNotBeClosedByDifferentMenu"/> is true</param>
        /// <returns>The new active <see cref="Menus.Menu"/> or null if all menus are closed</returns>
        public override MenuBase Open_Close(MenuBase _CurrentActiveMenu, bool _ForceClose = false)
        {
            this.SetTimeSpendInGameText(Time.time);
            
            return base.Open_Close(_CurrentActiveMenu, _ForceClose);
        }

        // TODO: Try to combine with "GameOverMenu.cs" "SetDurationText()"-Method
        /// <summary>
        /// Sets a formatted value of <see cref="StatsValues.TimeSpendInGame"/> in <see cref="timeSpendInGameText"/>
        /// </summary>
        /// <param name="_DurationToAdd">Seconds to add</param>
        private void SetTimeSpendInGameText(float _DurationToAdd = 0)
        {
            var _timeSpendInGame = new TimeSpan().Add(this.TimeSpendInGame).Add(TimeSpan.FromSeconds(_DurationToAdd));

            if (_timeSpendInGame.Hours > 0)
            {
                this.stats.SetForText(this.timeSpendInGameText, string.Concat(_timeSpendInGame.Hours, "h"));
            }
            else if (_timeSpendInGame.Minutes > 0)
            {
                this.stats.SetForText(this.timeSpendInGameText, string.Concat(_timeSpendInGame.Minutes, "min"));
            }
            else
            {
                this.stats.SetForText(this.timeSpendInGameText, string.Concat(_timeSpendInGame.Seconds, "sec"));
            }
        }
        
        /// <summary>
        /// Loads all settings from the <see cref="PlayerPrefs"/>
        /// </summary>
        /// <param name="_Set">If true, sets the loaded values to their respective properties in <see cref="StatsMenu"/></param>
        /// <returns><see cref="StatsValues"/></returns>
        private StatsValues Load(bool _Set)
        {
            var _bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
            var _bestMultiplier = PlayerPrefs.GetInt(HIGHEST_MULTIPLIER_KEY, 0);
            var _grapeEvolvedCount = PlayerPrefs.GetInt(GRAPE_KEY, 0);
            var _cherryEvolvedCount = PlayerPrefs.GetInt(CHERRY_KEY, 0);
            var _strawberryEvolvedCount = PlayerPrefs.GetInt(STRAWBERRY_KEY, 0);
            var _lemonEvolvedCount = PlayerPrefs.GetInt(LEMON_KEY, 0);
            var _orangeEvolvedCount = PlayerPrefs.GetInt(ORANGE_KEY, 0);
            var _appleEvolvedCount = PlayerPrefs.GetInt(APPLE_KEY, 0); 
            var _pearEvolvedCount = PlayerPrefs.GetInt(PEAR_KEY, 0);
            var _pineappleEvolvedCount = PlayerPrefs.GetInt(PINEAPPLE_KEY, 0);
            var _honeymelonEvolvedCount = PlayerPrefs.GetInt(HONEY_MELON_KEY, 0);
            var _watermelonEvolvedCount = PlayerPrefs.GetInt(MELON_KEY, 0);
            var _goldenFruitCount = PlayerPrefs.GetInt(GOLDEN_FRUIT_KEY, 0);
            var _powerSkillUsedCount = PlayerPrefs.GetInt(POWER_KEY, 0);
            var _evolveSkillUsedCount = PlayerPrefs.GetInt(EVOLVE_KEY, 0);
            var _destroySkillUsedCount = PlayerPrefs.GetInt(DESTROY_KEY, 0);
            var _gamesPlayed = PlayerPrefs.GetInt(GAMES_PLAYED_KEY, 0);
            TimeSpan.TryParse(PlayerPrefs.GetString(TIME_SPEND_KEY), out var _timeSPendInGame);

            var _statsValues = new StatsValues
            (
                _bestScore,
                _bestMultiplier,
                _grapeEvolvedCount,
                _cherryEvolvedCount,
                _strawberryEvolvedCount,
                _lemonEvolvedCount,
                _orangeEvolvedCount,
                _appleEvolvedCount,
                _pearEvolvedCount,
                _pineappleEvolvedCount,
                _honeymelonEvolvedCount,
                _watermelonEvolvedCount,
                _goldenFruitCount,
                _powerSkillUsedCount,
                _evolveSkillUsedCount,
                _destroySkillUsedCount,
                _gamesPlayed,
                _timeSPendInGame
            );

            if (_Set)
            {
                this.BestScore = _bestScore;
                this.stats.BestMultiplier = _bestMultiplier;
                this.stats.GrapeEvolvedCount = _grapeEvolvedCount;
                this.stats.CherryEvolvedCount = _cherryEvolvedCount;
                this.stats.StrawberryEvolvedCount = _strawberryEvolvedCount;
                this.stats.LemonEvolvedCount = _lemonEvolvedCount;
                this.stats.OrangeEvolvedCount = _orangeEvolvedCount;
                this.stats.AppleEvolvedCount = _appleEvolvedCount;
                this.stats.PearEvolvedCount = _pearEvolvedCount;
                this.stats.PineappleEvolvedCount = _pineappleEvolvedCount;
                this.stats.HoneymelonEvolvedCount = _honeymelonEvolvedCount;
                this.stats.WatermelonEvolvedCount = _watermelonEvolvedCount;
                this.stats.GoldenFruitCount = _goldenFruitCount;
                this.stats.PowerSkillUsedCount = _powerSkillUsedCount;
                this.stats.EvolveSkillUsedCount = _evolveSkillUsedCount;
                this.stats.DestroySkillUsedCount = _destroySkillUsedCount;
                this.GamesPlayed = _gamesPlayed;
                this.TimeSpendInGame = _timeSPendInGame;
            }

            return _statsValues;
        }
        
        /// <summary>
        /// Saves all settings with <see cref="PlayerPrefs"/>
        /// </summary>
        public void Save()
        {
            var _loadedStatsValues = this.Load(false);
            var _currentStatsValues = this.GetCurrentStatsValues();
            var _biggerStatsValues = _currentStatsValues.CheckIfBigger(_loadedStatsValues);

            PlayerPrefs.SetInt(BEST_SCORE_KEY, _biggerStatsValues.BestScore);
            PlayerPrefs.SetInt(HIGHEST_MULTIPLIER_KEY, _biggerStatsValues.BestMultiplier);
            PlayerPrefs.SetInt(GRAPE_KEY, _biggerStatsValues.GrapeEvolvedCount);
            PlayerPrefs.SetInt(CHERRY_KEY, _biggerStatsValues.CherryEvolvedCount);
            PlayerPrefs.SetInt(STRAWBERRY_KEY, _biggerStatsValues.StrawberryEvolvedCount);
            PlayerPrefs.SetInt(LEMON_KEY, _biggerStatsValues.LemonEvolvedCount);
            PlayerPrefs.SetInt(ORANGE_KEY, _biggerStatsValues.OrangeEvolvedCount);
            PlayerPrefs.SetInt(APPLE_KEY, _biggerStatsValues.AppleEvolvedCount);
            PlayerPrefs.SetInt(PEAR_KEY, _biggerStatsValues.PearEvolvedCount);
            PlayerPrefs.SetInt(PINEAPPLE_KEY, _biggerStatsValues.PineappleEvolvedCount);
            PlayerPrefs.SetInt(HONEY_MELON_KEY, _biggerStatsValues.HoneymelonEvolvedCount);
            PlayerPrefs.SetInt(MELON_KEY, _biggerStatsValues.WatermelonEvolvedCount);
            PlayerPrefs.SetInt(GOLDEN_FRUIT_KEY, _biggerStatsValues.GoldenFruitCount);
            PlayerPrefs.SetInt(POWER_KEY, _biggerStatsValues.PowerSkillUsedCount);
            PlayerPrefs.SetInt(EVOLVE_KEY, _biggerStatsValues.EvolveSkillUsedCount);
            PlayerPrefs.SetInt(DESTROY_KEY, _biggerStatsValues.DestroySkillUsedCount);
            PlayerPrefs.SetInt(GAMES_PLAYED_KEY, _biggerStatsValues.GamesPlayed);
            PlayerPrefs.SetString(TIME_SPEND_KEY, _biggerStatsValues.TimeSpendInGame.Add(TimeSpan.FromSeconds(Time.time)).ToString());
        }
        
        /// <summary>
        /// Creates a new <see cref="StatsValues"/> object with the values of this <see cref="StatsMenu"/>
        /// </summary>
        /// <returns>A new <see cref="StatsValues"/> object with the values of this <see cref="StatsMenu"/></returns>
        private StatsValues GetCurrentStatsValues()
        {
            return new StatsValues
            (
                this.BestScore,
                this.stats.BestMultiplier,
                this.stats.GrapeEvolvedCount,
                this.stats.CherryEvolvedCount,
                this.stats.StrawberryEvolvedCount,
                this.stats.LemonEvolvedCount,
                this.stats.OrangeEvolvedCount,
                this.stats.AppleEvolvedCount,
                this.stats.PearEvolvedCount,
                this.stats.PineappleEvolvedCount,
                this.stats.HoneymelonEvolvedCount,
                this.stats.WatermelonEvolvedCount,
                this.stats.GoldenFruitCount,
                this.stats.PowerSkillUsedCount,
                this.stats.EvolveSkillUsedCount,
                this.stats.DestroySkillUsedCount,
                this.GamesPlayed,
                this.TimeSpendInGame
            );
        }

        /// <summary>
        /// Sets <see cref="BestScore"/> to the given value
        /// </summary>
        /// <param name="_NewBestScore">The value to set <see cref="BestScore"/> to</param>
        public void SetBestScore(int _NewBestScore)
        {
            this.BestScore = _NewBestScore;
        }
        
        /// <summary>
        /// Increments <see cref="StatsValues.GamesPlayed"/>
        /// </summary>
        public void AddGamesPlayed()
        {
            this.GamesPlayed++;
        }
        
        // TODO: Combine with GameOverMenu
        /// <summary>
        /// <see cref="Stats.AddFruitCount"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruits.Fruit"/> to add to <see cref="stats"/></param>
        public void AddFruitCount(Fruits.Fruit _Fruit)
        {
            this.stats.AddFruitCount(_Fruit);
        }
        
        // TODO: Combine with GameOverMenu
        /// <summary>
        /// <see cref="Stats.AddSkillCount"/>
        /// </summary>
        /// <param name="_Skill">The <see cref="Skills.Skill"/> to add to <see cref="stats"/></param>
        public void AddSkillCount(Skill? _Skill)
        {
            this.stats.AddSkillCount(_Skill);
        }

        // TODO: Combine with GameOverMenu
        /// <summary>
        /// Increments <see cref="Stats.GoldenFruitCount"/>
        /// </summary>
        public void AddGoldenFruit()
        {
            this.stats.GoldenFruitCount++;
        }
        #endregion
    }
}