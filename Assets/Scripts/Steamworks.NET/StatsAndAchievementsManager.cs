#if !DISABLESTEAMWORKS
using System;
using Steamworks;
using UnityEngine;
using Watermelon_Game.Container;
using Watermelon_Game.Fruits;
using Watermelon_Game.Menus;
using Watermelon_Game.Points;
using Watermelon_Game.Skills;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Steamworks.NET
{
    /// <summary>
    /// Contains logic for Steam Stats and Achievements
    /// </summary>
    internal sealed class StatsAndAchievementsManager : MonoBehaviour
    {
        #region Achievement API Names
        private const string MULTIPLIER_10 = "MULTIPLIER_10";
        private const string MULTIPLIER_20 = "MULTIPLIER_20";
        private const string FIRST_WATERMELON = "FIRST_WATERMELON";
        private const string POWER_SKILL = "POWER_SKILL";
        private const string EVOLVE_SKILL = "EVOLVE_SKILL";
        private const string DESTROY_SKILL = "DESTROY_SKILL";
        private const string MATCH_1_H = "MATCH_1_H";
        public const string PLAYTIME_10_H = "PLAYTIME_10_H";
        public const string PLAYTIME_100_H = "PLAYTIME_100_H";
        public const string PLAYTIME_1000_H = "PLAYTIME_1000_H";
        #endregion
        
        #region Fields
        /// <summary>
        /// Indicates whether the <see cref="SteamUserStats.RequestCurrentStats"/> was successful or not
        /// </summary>
        private bool successfulStatsRequest;
        /// <summary>
        /// <see cref="Watermelon_Game.Steamworks.NET.Stats"/>
        /// </summary>
        private Stats stats;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.Init();
        }
        
        /// <summary>
        /// Is called at the end of <see cref="SteamManager"/>.<see cref="Awake"/>
        /// </summary>
        private void Init()
        {
            this.successfulStatsRequest = SteamUserStats.RequestCurrentStats();
            this.stats = Stats.LoadAllStats();
        }

        private void OnEnable()
        {
            MaxHeight.OnGameOver += this.GameFinished;
            Multiplier.OnMultiplierActivated += MultiplierActivated;
            FruitController.OnEvolve += FruitEvolved;
            FruitBehaviour.OnGoldenFruitSpawn += GoldenFruitSpawned;
            FruitBehaviour.OnSkillUsed += SkillUsed;
            
            Application.quitting += this.ApplicationIsQuitting;
        }
        
        /// <summary>
        /// <see cref="Application.quitting"/>
        /// </summary>
        private void ApplicationIsQuitting()
        {
            this.AddPlaytime();
        }
        
        private void OnDisable()
        {
            SteamUserStats.StoreStats();
            
            MaxHeight.OnGameOver -= this.GameFinished;
            Multiplier.OnMultiplierActivated -= MultiplierActivated;
            FruitController.OnEvolve -= FruitEvolved;
            FruitBehaviour.OnGoldenFruitSpawn -= GoldenFruitSpawned;
            FruitBehaviour.OnSkillUsed -= SkillUsed;
        }

        private void Start()
        {
            this.CheckCurrentPlaytime();
        }
        
        /// <summary>
        /// Is called on <see cref="MaxHeight"/>.<see cref="MaxHeight.OnGameOver"/>
        /// </summary>
        private void GameFinished()
        {
            this.stats.SetStat(_Stats => _Stats.GamesFinished, 1, Operation.Add);

            var _newHighscore = PointsController.CurrentPoints > this.stats.GetStat(_Stats => _Stats.Highscore);
            if (_newHighscore)
            {
                this.stats.SetStat(_Stats => _Stats.Highscore, (int)PointsController.CurrentPoints, Operation.Set);
            }
            
            var _currentGameDuration = Time.time - GameController.CurrentGameTimeStamp;
            var _isAtLeastAnHour = TimeSpan.FromSeconds(_currentGameDuration).Hours >= 1;
            
            if (_isAtLeastAnHour)
            {
                SteamUserStats.SetAchievement(MATCH_1_H);
            }
            
            SteamUserStats.StoreStats();
        }

        /// <summary>
        /// Is called on <see cref="Multiplier"/>.<see cref="Multiplier.OnMultiplierActivated"/>
        /// </summary>
        /// <param name="_CurrentMultiplier"><see cref="Multiplier.CurrentMultiplier"/></param>
        private void MultiplierActivated(uint _CurrentMultiplier)
        {
            var _newBestMultiplier = _CurrentMultiplier > StatsMenu.Instance.Stats.BestMultiplier;
            if (!_newBestMultiplier)
            {
                return;
            }
            
            switch (_CurrentMultiplier)
            {
                case 10:
                    SteamUserStats.SetAchievement(MULTIPLIER_10);
                    SteamUserStats.StoreStats();
                    break;
                case 20:
                    SteamUserStats.SetAchievement(MULTIPLIER_20);
                    SteamUserStats.StoreStats();
                    break;
            }
        }

        /// <summary>
        /// Is called on <see cref="FruitController"/>.<see cref="FruitController.OnEvolve"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> that has been combined</param>
        private void FruitEvolved(Fruit _Fruit)
        {
            var _evolveCount = 0;
            
            switch (_Fruit)
            {
                case Fruit.Grape:
                    _evolveCount = this.stats.SetStat(_Stats => _Stats.Grapes, 1, Operation.Add);
                    break;
                case Fruit.Cherry:
                    _evolveCount = this.stats.SetStat(_Stats => _Stats.Cherries, 1, Operation.Add);
                    break;
                case Fruit.Strawberry:
                    _evolveCount = this.stats.SetStat(_Stats => _Stats.Strawberries, 1, Operation.Add);
                    break;
                case Fruit.Lemon:
                    _evolveCount = this.stats.SetStat(_Stats => _Stats.Lemons, 1, Operation.Add);
                    break;
                case Fruit.Orange:
                    _evolveCount = this.stats.SetStat(_Stats => _Stats.Oranges, 1, Operation.Add);
                    break;
                case Fruit.Apple:
                    _evolveCount = this.stats.SetStat(_Stats => _Stats.Apples, 1, Operation.Add);
                    break;
                case Fruit.Pear:
                    _evolveCount = this.stats.SetStat(_Stats => _Stats.Pears, 1, Operation.Add);
                    break;
                case Fruit.Pineapple:
                    _evolveCount =  this.stats.SetStat(_Stats => _Stats.Pineapples, 1, Operation.Add);
                    break;
                case Fruit.Honeymelon:
                    _evolveCount = this.stats.SetStat(_Stats => _Stats.Honeymelons, 1, Operation.Add);
                    break;
                case Fruit.Watermelon:
                    _evolveCount = this.stats.SetStat(_Stats => _Stats.Watermelons, 1, Operation.Add);
                    break;
            }

            var _evolvedFruitIsWatermelon = (int)_Fruit + 1 == (int)Fruit.Watermelon;
            var _isFirstWatermelon = StatsMenu.Instance.Stats.WatermelonEvolvedCount == 0;

            if (_evolvedFruitIsWatermelon && _isFirstWatermelon)
            {
                SteamUserStats.SetAchievement(FIRST_WATERMELON);
            }
            
            if (_evolveCount is 1000 or 10000 || (_evolvedFruitIsWatermelon && _isFirstWatermelon))
            {
                SteamUserStats.StoreStats();
            }
        }

        /// <summary>
        /// Is called on <see cref="FruitBehaviour"/>.<see cref="FruitBehaviour.OnGoldenFruitSpawn"/>
        /// </summary>
        /// <param name="_IsUpgradedGoldenFruit">Indicates whether the golden fruit is anj upgraded golden fruit or not</param>
        private void GoldenFruitSpawned(bool _IsUpgradedGoldenFruit)
        {
            var _goldenFruitCount = _IsUpgradedGoldenFruit 
                ? this.stats.SetStat(_Stats => _Stats.UpgradedGoldenFruits, 1, Operation.Add) 
                : this.stats.SetStat(_Stats => _Stats.GoldenFruits, 1, Operation.Add);

            if (_goldenFruitCount is 1 or 10 or 100 || (_IsUpgradedGoldenFruit && _goldenFruitCount is 1000))
            {
                SteamUserStats.StoreStats();
            }
        }

        /// <summary>
        /// Is called on <see cref="SkillController"/>.<see cref="SkillController.OnSkillUsed"/>
        /// </summary>
        /// <param name="_Skill">The <see cref="Skill"/> that was used</param>
        private void SkillUsed(Skill? _Skill)
        {
            var _firstPowerSkillUsed = _Skill is Skill.Power && StatsMenu.Instance.Stats.PowerSkillUsedCount is 0;
            var _firstEvolveSkillUsed = _Skill is Skill.Evolve && StatsMenu.Instance.Stats.EvolveSkillUsedCount is 0;
            var _firstDestroySkillUsed = _Skill is Skill.Destroy && StatsMenu.Instance.Stats.DestroySkillUsedCount is 0;

            if (_firstPowerSkillUsed)
            {
                SteamUserStats.SetAchievement(POWER_SKILL);
            }
            else if (_firstEvolveSkillUsed)
            {
                SteamUserStats.SetAchievement(EVOLVE_SKILL);
            }
            else if (_firstDestroySkillUsed)
            {
                SteamUserStats.SetAchievement(DESTROY_SKILL);
            }
            
            var _skillsUsedCount = this.stats.SetStat(_Stats => _Stats.SkillCount, 1, Operation.Add);

            if (_firstPowerSkillUsed || _firstEvolveSkillUsed || _firstDestroySkillUsed || _skillsUsedCount is 100 or 1000 or 10000)
            {
                SteamUserStats.StoreStats();
            }
        }

        /// <summary>
        /// Adds the <see cref="Time.time"/> (converted to hours), to <see cref="Stats.Playtime"/>
        /// </summary>
        private void AddPlaytime()
        {
            this.stats.SetStat(_Stats => _Stats.Playtime, Time.time / 3600, Operation.Add);
        }

        /// <summary>
        /// Checks the current playtime of the player and sets the achievements, if the threshold is reached
        /// </summary>
        private void CheckCurrentPlaytime()
        {
            var _currentPlaytime = this.stats.GetStat(_Stats => _Stats.Playtime);
            var _is10H = _currentPlaytime >= 10;
            var _is100H = _currentPlaytime >= 100;
            var _is1000H = _currentPlaytime >= 1000;
            
            if (_is1000H)
            {
                SteamUserStats.SetAchievement(PLAYTIME_1000_H);
            }
            else if (_is100H)
            {
                SteamUserStats.SetAchievement(PLAYTIME_100_H);
            }
            else if (_is10H)
            {
                SteamUserStats.SetAchievement(PLAYTIME_10_H);
            }
            
            if (_is10H || _is100H || _is1000H)
            {
                SteamUserStats.StoreStats();
            }
        }
        #endregion
    }
}
#endif