using System;
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

        #region Constants
        // TODO: Use safe-controller
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
        #endregion
        
        #region Fields
        /// <summary>
        /// All time best score
        /// </summary>
        private uint bestScore;
        /// <summary>
        /// Total amount of games player
        /// </summary>
        private uint gamesPlayed;
        /// <summary>
        /// Total time spend in game
        /// </summary>
        private TimeSpan timeSpendInGame;
        #endregion

        #region Properties
        /// <summary>
        /// Singleton of <see cref="StatsMenu"/>
        /// </summary>
        public static StatsMenu Instance { get; private set; }

        /// <summary>
        /// <see cref="bestScore"/>
        /// </summary>
        public uint BestScore
        {
            get => this.bestScore;
            set
            {
                this.bestScore = value;
                this.stats.SetForText(this.bestScoreText, this.bestScore);
            } 
        }
        /// <summary>
        /// <see cref="Menus.Stats"/>
        /// </summary>
        public Stats Stats => this.stats;
        /// <summary>
        /// <see cref="gamesPlayed"/>
        /// </summary>
        public uint GamesPlayed
        {
            get => this.gamesPlayed;
            set
            {
                this.gamesPlayed = value;
                this.stats.SetForText(this.gamesPlayedText, this.gamesPlayed);
            } 
        }
        /// <summary>
        /// <see cref="timeSpendInGame"/>
        /// </summary>
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
        
        /// <summary>
        /// <see cref="MenuBase.Open_Close"/>
        /// </summary>
        /// <param name="_CurrentActiveMenu"><see cref="MenuController.currentActiveMenu"/></param>
        /// <param name="_ForceClose">Forces the active menu to close even if <see cref="MenuBase.canNotBeClosedByDifferentMenu"/> is true</param>
        /// <returns>The new active <see cref="Menus.Menu"/> or null if all menus are closed</returns>
        public override MenuBase Open_Close(MenuBase _CurrentActiveMenu, bool _ForceClose = false)
        {
            this.SetTimeSpendText(Time.time);
            
            return base.Open_Close(_CurrentActiveMenu, _ForceClose);
        }

        // TODO: Try to combine with "GameOverMenu.cs" "SetDurationText()"-Method
        /// <summary>
        /// Sets a formatted value of <see cref="timeSpendInGame"/> in <see cref="timeSpendInGameText"/>
        /// </summary>
        /// <param name="_DurationToAdd">Seconds to add</param>
        private void SetTimeSpendText(float _DurationToAdd = 0)
        {
            var _timeSpendInGame = new TimeSpan().Add(this.timeSpendInGame).Add(TimeSpan.FromSeconds(_DurationToAdd));

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
        private void Load()
        {
            this.BestScore = (uint)PlayerPrefs.GetInt(BEST_SCORE_KEY);
            this.stats.BestMultiplier = (uint)PlayerPrefs.GetInt(HIGHEST_MULTIPLIER_KEY);
            this.stats.GrapeEvolvedCount = (uint)PlayerPrefs.GetInt(GRAPE_KEY);
            this.stats.CherryEvolvedCount = (uint)PlayerPrefs.GetInt(CHERRY_KEY);
            this.stats.StrawberryEvolvedCount = (uint)PlayerPrefs.GetInt(STRAWBERRY_KEY);
            this.stats.LemonEvolvedCount = (uint)PlayerPrefs.GetInt(LEMON_KEY);
            this.stats.OrangeEvolvedCount = (uint)PlayerPrefs.GetInt(ORANGE_KEY);
            this.stats.AppleEvolvedCount = (uint)PlayerPrefs.GetInt(APPLE_KEY); 
            this.stats.PearEvolvedCount = (uint)PlayerPrefs.GetInt(PEAR_KEY);
            this.stats.PineappleEvolvedCount = (uint)PlayerPrefs.GetInt(PINEAPPLE_KEY);
            this.stats.HoneymelonEvolvedCount = (uint)PlayerPrefs.GetInt(HONEY_MELON_KEY);
            this.stats.WatermelonEvolvedCount = (uint)PlayerPrefs.GetInt(MELON_KEY);
            this.stats.GoldenFruitCount = (uint)PlayerPrefs.GetInt(GOLDEN_FRUIT_KEY);
            this.stats.PowerSkillUsedCount = (uint)PlayerPrefs.GetInt(POWER_KEY);
            this.stats.EvolveSkillUsedCount = (uint)PlayerPrefs.GetInt(EVOLVE_KEY);
            this.stats.DestroySkillUsedCount = (uint)PlayerPrefs.GetInt(DESTROY_KEY);
            this.GamesPlayed = (uint)PlayerPrefs.GetInt(GAMES_PLAYED_KEY);
            TimeSpan.TryParse(PlayerPrefs.GetString(TIME_SPEND_KEY), out var _timeSPendInGame);
            this.TimeSpendInGame = _timeSPendInGame;
        }
        
        /// <summary>
        /// Saves all settings with <see cref="PlayerPrefs"/>
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetInt(BEST_SCORE_KEY, (int)this.bestScore);
            PlayerPrefs.SetInt(HIGHEST_MULTIPLIER_KEY, (int)this.stats.BestMultiplier);
            PlayerPrefs.SetInt(GRAPE_KEY, (int)this.stats.GrapeEvolvedCount);
            PlayerPrefs.SetInt(CHERRY_KEY, (int)this.stats.CherryEvolvedCount);
            PlayerPrefs.SetInt(STRAWBERRY_KEY, (int)this.stats.StrawberryEvolvedCount);
            PlayerPrefs.SetInt(LEMON_KEY, (int)this.stats.LemonEvolvedCount);
            PlayerPrefs.SetInt(ORANGE_KEY, (int)this.stats.OrangeEvolvedCount);
            PlayerPrefs.SetInt(APPLE_KEY, (int)this.stats.AppleEvolvedCount);
            PlayerPrefs.SetInt(PEAR_KEY, (int)this.stats.PearEvolvedCount);
            PlayerPrefs.SetInt(PINEAPPLE_KEY, (int)this.stats.PineappleEvolvedCount);
            PlayerPrefs.SetInt(HONEY_MELON_KEY, (int)this.stats.HoneymelonEvolvedCount);
            PlayerPrefs.SetInt(MELON_KEY, (int)this.stats.WatermelonEvolvedCount);
            PlayerPrefs.SetInt(GOLDEN_FRUIT_KEY, (int)this.stats.GoldenFruitCount);
            PlayerPrefs.SetInt(POWER_KEY, (int)this.stats.PowerSkillUsedCount);
            PlayerPrefs.SetInt(EVOLVE_KEY, (int)this.stats.EvolveSkillUsedCount);
            PlayerPrefs.SetInt(DESTROY_KEY, (int)this.stats.DestroySkillUsedCount);
            PlayerPrefs.SetInt(GAMES_PLAYED_KEY, (int)this.gamesPlayed);
            PlayerPrefs.SetString(TIME_SPEND_KEY, this.timeSpendInGame.Add(TimeSpan.FromSeconds(Time.time)).ToString());
        }

        /// <summary>
        /// Increments <see cref="gamesPlayed"/>
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