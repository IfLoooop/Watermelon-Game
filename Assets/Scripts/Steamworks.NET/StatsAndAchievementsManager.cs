#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Steamworks;
using UnityEngine;
using Watermelon_Game.Container;
using Watermelon_Game.Fruits;
using Watermelon_Game.Menus.MenuContainers;
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
        private const string PLAYTIME_10_H = "PLAYTIME_10_H";
        private const string PLAYTIME_100_H = "PLAYTIME_100_H";
        private const string PLAYTIME_1000_H = "PLAYTIME_1000_H";
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="StatsAndAchievementsManager"/>
        /// </summary>
        private static StatsAndAchievementsManager instance;
        /// <summary>
        /// Indicates whether the <see cref="SteamUserStats.RequestCurrentStats"/> was successful or not
        /// </summary>
        private bool successfulStatsRequest; // TODO: Check if this is needed
        /// <summary>
        /// <see cref="Watermelon_Game.Steamworks.NET.Stats"/>
        /// </summary>
        [CanBeNull] private Stats stats;
        /// <summary>
        /// <b>Key:</b> The achievement API name <br/>
        /// <b>Value:</b> Indicates whether this achievement has been unlocked or not
        /// </summary>
        private readonly Dictionary<string, bool> achievementStatus = new()
        {
            { MULTIPLIER_10, false },
            { MULTIPLIER_20, false },
            { FIRST_WATERMELON, false },
            { POWER_SKILL, false },
            { EVOLVE_SKILL, false },
            { DESTROY_SKILL, false },
            { MATCH_1_H, false },
            { PLAYTIME_10_H, false },
            { PLAYTIME_100_H, false },
            { PLAYTIME_1000_H, false }
        };
        #endregion

        #region Callbacks
        /// <summary>
        /// Is called after successfully requesting <see cref="SteamUserStats"/>.<see cref="SteamUserStats.RequestCurrentStats"/> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#RequestCurrentStats</i> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#UserStatsReceived_t</i>
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private Callback<UserStatsReceived_t> onUserStatsReceived;
        /// <summary>
        /// Is called when stats have been successfully stored after calling <see cref="SteamUserStats"/>.<see cref="SteamUserStats.StoreStats"/> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#StoreStats</i> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#UserStatsStored_t</i>
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private Callback<UserStatsStored_t> onUserStatsStored;
        /// <summary>
        /// Is called when achievements have been successfully stored after calling <see cref="SteamUserStats"/>.<see cref="SteamUserStats.StoreStats"/> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#StoreStats</i> <br/>
        /// <i>https://partner.steamgames.com/doc/api/ISteamUserStats#UserAchievementStored_t</i>
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private Callback<UserAchievementStored_t> onUserAchievementStored;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.Init();
        }
        
        private void OnEnable()
        {
            MaxHeight.OnGameOver += this.ResetGame;
            Multiplier.OnMultiplierActivated += this.MultiplierActivated;
            FruitController.OnEvolve += this.FruitEvolved;
            FruitBehaviour.OnGoldenFruitSpawn += this.GoldenFruitSpawned;
            FruitBehaviour.OnSkillUsed += this.SkillUsed;
            Application.quitting += this.ApplicationIsQuitting;
        }
        
#pragma warning disable CS0162 // Unreachable code detected
        /// <summary>
        /// <see cref="Application.quitting"/> <br/>
        /// <i>Called before <see cref="OnDisable"/></i>
        /// </summary>
        private void ApplicationIsQuitting()
        {
#if UNITY_EDITOR
            return;
#endif
            if (SteamManager.Initialized)
            {
                this.AddPlaytime();
                SteamUserStats.StoreStats(); // ReSharper disable once HeuristicUnreachableCode
            }
        }
#pragma warning restore CS0162 // Unreachable code detected
        
        private void OnDisable()
        {
            MaxHeight.OnGameOver -= this.ResetGame;
            Multiplier.OnMultiplierActivated -= this.MultiplierActivated;
            FruitController.OnEvolve -= this.FruitEvolved;
            FruitBehaviour.OnGoldenFruitSpawn -= this.GoldenFruitSpawned;
            FruitBehaviour.OnSkillUsed -= this.SkillUsed;
            Application.quitting -= this.ApplicationIsQuitting;
        }
        
        /// <summary>
        /// Loads all stats from Steam
        /// </summary>
        private void Init()
        {
            instance = this;
            
            if (SteamManager.Initialized)
            {
                this.onUserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
                this.onUserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
                this.onUserAchievementStored = Callback<UserAchievementStored_t>.Create(OnUserAchievementsStored);
                this.successfulStatsRequest = SteamUserStats.RequestCurrentStats();
            }
        }

        /// <summary>
        /// <see cref="onUserStatsReceived"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private void OnUserStatsReceived(UserStatsReceived_t _Callback)
        {
            if (_Callback.m_eResult != EResult.k_EResultOK) // TODO: Maybe call "RequestCurrentStats()" again on failure (until success)
            {
                Debug.LogError($"Could not receive the UserStats: {_Callback.m_eResult}");
            }
            
            var _storeStats = false;
            this.stats = Stats.LoadAllStats();
            
            this.LoadAchievementStatus();
            this.UnlockAchievements(ref _storeStats);
            this.OverwriteStats(ref _storeStats);
            this.CurrentPlaytime(ref _storeStats);

            if (_storeStats)
            {
                SteamUserStats.StoreStats();
            }
        }
        
        /// <summary>
        /// <see cref="onUserStatsStored"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private void OnUserStatsStored(UserStatsStored_t _Callback)
        {
            this.LoadAchievementStatus();
        }

        /// <summary>
        /// <see cref="onUserAchievementStored"/>
        /// </summary>
        /// <param name="_Callback">The received callback</param>
        private void OnUserAchievementsStored(UserAchievementStored_t _Callback)
        {
            this.LoadAchievementStatus();
        }
        
        /// <summary>
        /// Loads the unlocked state for all achievements in <see cref="achievementStatus"/> <br/>
        /// <i>Should be all achievements that are not dependant on stats</i>
        /// </summary>
        private void LoadAchievementStatus()
        {
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < this.achievementStatus.Count; i++)
            {
                var _key = this.achievementStatus.ElementAt(i).Key;
                if (SteamUserStats.GetAchievement(_key, out var _achieved))
                {
                    this.achievementStatus[_key] = _achieved;
                }
            }
        }

        /// <summary>
        /// Retroactively unlocks achievements that were not unlocked, even though they should've
        /// </summary>
        /// <param name="_AnyAchievementSet">Indicates whether any achievement has been unlocked through this method or not</param>
        private void UnlockAchievements(ref bool _AnyAchievementSet)
        {
            var _stats = GlobalStats.Instance.Stats;
            this.UnlockAchievement(MULTIPLIER_10, _stats.BestMultiplier, 10, ref _AnyAchievementSet);
            this.UnlockAchievement(MULTIPLIER_20, _stats.BestMultiplier, 20, ref _AnyAchievementSet);
            this.UnlockAchievement(FIRST_WATERMELON, _stats.WatermelonEvolvedCount, 1, ref _AnyAchievementSet);
            this.UnlockAchievement(POWER_SKILL, _stats.PowerSkillUsedCount, 1, ref _AnyAchievementSet);
            this.UnlockAchievement(EVOLVE_SKILL, _stats.EvolveSkillUsedCount, 1, ref _AnyAchievementSet);
            this.UnlockAchievement(DESTROY_SKILL, _stats.DestroySkillUsedCount, 1, ref _AnyAchievementSet);
        }

        /// <summary>
        /// Unlocks the achievement for the given <see cref="_APIName"/>, if the given <see cref="_CurrentValue"/> is >= than <see cref="_ValueToUnlockAt"/>
        /// </summary>
        /// <param name="_APIName">The API name of the achievement to unlock</param>
        /// <param name="_CurrentValue">Current value of the achievement</param>
        /// <param name="_ValueToUnlockAt">Value at which the achievement should be unlocked</param>
        /// <param name="_AchievementSet">Indicates whether the achievement has been unlocked through this method or not</param>
        private void UnlockAchievement(string _APIName, int _CurrentValue, int _ValueToUnlockAt, ref bool _AchievementSet)
        {
            if (!this.achievementStatus[_APIName]) // Not yet unlocked
            {
                if (_CurrentValue >= _ValueToUnlockAt) // Should be unlocked
                {
                    SteamUserStats.SetAchievement(_APIName);
                    _AchievementSet = true;
                }
            }
        }
        
        /// <summary>
        /// Retroactively overwrites stats with the values from <see cref="GlobalStats"/> if they're greater 
        /// </summary>
        /// <param name="_AnyStatOverwritten">Indicates whether any stat has been overwritten</param>
        private void OverwriteStats(ref bool _AnyStatOverwritten)
        {
            if (this.stats != null)
            {
                var _statsMenuInstance = GlobalStats.Instance;
                var _stats = _statsMenuInstance.Stats;
                this.OverwriteStat(this.stats.Highscore, _statsMenuInstance.BestScore, ref _AnyStatOverwritten);
                this.OverwriteStat(this.stats.Grapes, _stats.GrapeEvolvedCount, ref _AnyStatOverwritten);
                this.OverwriteStat(this.stats.Cherries, _stats.CherryEvolvedCount, ref _AnyStatOverwritten);
                this.OverwriteStat(this.stats.Strawberries, _stats.StrawberryEvolvedCount, ref _AnyStatOverwritten);
                this.OverwriteStat(this.stats.Lemons, _stats.LemonEvolvedCount, ref _AnyStatOverwritten);
                this.OverwriteStat(this.stats.Oranges, _stats.OrangeEvolvedCount, ref _AnyStatOverwritten);
                this.OverwriteStat(this.stats.Apples, _stats.AppleEvolvedCount, ref _AnyStatOverwritten);
                this.OverwriteStat(this.stats.Pears, _stats.PearEvolvedCount, ref _AnyStatOverwritten);
                this.OverwriteStat(this.stats.Pineapples, _stats.PineappleEvolvedCount, ref _AnyStatOverwritten);
                this.OverwriteStat(this.stats.Honeymelons, _stats.HoneymelonEvolvedCount, ref _AnyStatOverwritten);
                this.OverwriteStat(this.stats.Watermelons, _stats.WatermelonEvolvedCount, ref _AnyStatOverwritten);
                this.OverwriteStat(this.stats.SkillCount, _stats.PowerSkillUsedCount + _stats.EvolveSkillUsedCount + _stats.DestroySkillUsedCount, ref _AnyStatOverwritten);   
            }
        }

        /// <summary>
        /// Sets the stat for the given <see cref="_APIName"/> to <see cref="_StatsMenuValue"/>, if <see cref="_StatsMenuValue"/> is greater
        /// </summary>
        /// <param name="_APIName">The API name of the stat</param>
        /// <param name="_StatsMenuValue">The value in <see cref="GlobalStats"/></param>
        /// <param name="_StatSet">Indicates whether the value has been overwritten or not</param>
        private void OverwriteStat(string _APIName, int _StatsMenuValue, ref bool _StatSet)
        {
            if (_StatsMenuValue > this.stats?.GetStat(_APIName))
            {
                this.stats.SetStat<int>(_APIName, _StatsMenuValue, Operation.Set);
                _StatSet = true;
            }
        }
        
        /// <summary>
        /// Checks the current playtime of the player and sets the achievements, if the threshold is reached
        /// </summary>
        /// <param name="_AchievementSet">Indicates whether the achievement has been unlocked through this method or not</param>
        private void CurrentPlaytime(ref bool _AchievementSet)
        {
            var _currentPlaytime = this.stats?.GetStat(_Stats => _Stats.Playtime);
            var _10H = _currentPlaytime >= 10 && !this.achievementStatus[PLAYTIME_10_H];
            var _100H = _currentPlaytime >= 100 && !this.achievementStatus[PLAYTIME_100_H];
            var _1000H = _currentPlaytime >= 1000 && !this.achievementStatus[PLAYTIME_1000_H];
            
            if (_10H)
            {
                SteamUserStats.SetAchievement(PLAYTIME_10_H);
            }
            if (_100H)
            {
                SteamUserStats.SetAchievement(PLAYTIME_100_H);
            }
            if (_1000H)
            {
                SteamUserStats.SetAchievement(PLAYTIME_1000_H);
            }
            
            if (_10H || _100H || _1000H)
            {
                _AchievementSet = true;
            }
        }
        
        /// <summary>
        /// Is called on <see cref="MaxHeight"/>.<see cref="MaxHeight.OnGameOver"/>
        /// </summary>
        /// <param name="_SteamId">Not needed here</param>
        private void ResetGame(ulong _SteamId)
        {
            if (!SteamManager.Initialized || this.stats == null)
            {
                return;
            }
            
            this.stats.SetStat(_Stats => _Stats.GamesFinished, 1, Operation.Add);

            var _newHighscore = PointsController.CurrentPoints > this.stats.GetStat(_Stats => _Stats.Highscore);
            if (_newHighscore)
            {
                this.stats.SetStat(_Stats => _Stats.Highscore, (int)PointsController.CurrentPoints.Value, Operation.Set);
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
            if (!SteamManager.Initialized)
            {
                return;
            }
            
            if (_CurrentMultiplier >= 10 && !this.achievementStatus[MULTIPLIER_10])
            {
                SteamUserStats.SetAchievement(MULTIPLIER_10);
            }
            if (_CurrentMultiplier >= 20 && !this.achievementStatus[MULTIPLIER_20])
            {
                SteamUserStats.SetAchievement(MULTIPLIER_20);
            }
        }

        /// <summary>
        /// Is called on <see cref="FruitController"/>.<see cref="FruitController.OnEvolve"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> that has been combined</param>
        private void FruitEvolved(Fruit _Fruit)
        {
            if (!SteamManager.Initialized || this.stats == null)
            {
                return;
            }
            
            switch (_Fruit)
            {
                case Fruit.Cherry:
                    this.stats.SetStat(_Stats => _Stats.Cherries, 1, Operation.Add);
                    break;
                case Fruit.Strawberry:
                    this.stats.SetStat(_Stats => _Stats.Strawberries, 1, Operation.Add);
                    break;
                case Fruit.Lemon:
                    this.stats.SetStat(_Stats => _Stats.Lemons, 1, Operation.Add);
                    break;
                case Fruit.Orange:
                    this.stats.SetStat(_Stats => _Stats.Oranges, 1, Operation.Add);
                    break;
                case Fruit.Apple:
                    this.stats.SetStat(_Stats => _Stats.Apples, 1, Operation.Add);
                    break;
                case Fruit.Pear:
                    this.stats.SetStat(_Stats => _Stats.Pears, 1, Operation.Add);
                    break;
                case Fruit.Dragonfruit:
                    this.stats.SetStat(_Stats => _Stats.Grapes, 1, Operation.Add);
                    break;
                case Fruit.Pineapple:
                    this.stats.SetStat(_Stats => _Stats.Pineapples, 1, Operation.Add);
                    break;
                case Fruit.Coconut:
                    this.stats.SetStat(_Stats => _Stats.Honeymelons, 1, Operation.Add);
                    break;
                case Fruit.Watermelon:
                    this.stats.SetStat(_Stats => _Stats.Watermelons, 1, Operation.Add);
                    break;
            }
            
            var _evolvedFruitIsWatermelon = (int)_Fruit + 1 == (int)Fruit.Watermelon;
            var _isFirstWatermelon = !this.achievementStatus[FIRST_WATERMELON];

            if (_evolvedFruitIsWatermelon && _isFirstWatermelon)
            {
                SteamUserStats.SetAchievement(FIRST_WATERMELON);
            }
        }

        /// <summary>
        /// Is called on <see cref="FruitBehaviour"/>.<see cref="FruitBehaviour.OnGoldenFruitSpawn"/>
        /// </summary>
        /// <param name="_IsUpgradedGoldenFruit">Indicates whether the golden fruit is anj upgraded golden fruit or not</param>
        private void GoldenFruitSpawned(bool _IsUpgradedGoldenFruit)
        {
            if (!SteamManager.Initialized || this.stats == null)
            {
                return;
            }

            if (_IsUpgradedGoldenFruit)
            {
                this.stats.SetStat(_Stats => _Stats.UpgradedGoldenFruits, 1, Operation.Add);
            }
            else
            {
                this.stats.SetStat(_Stats => _Stats.GoldenFruits, 1, Operation.Add);
            }
        }

        /// <summary>
        /// Is called on <see cref="SkillController"/>.<see cref="SkillController.OnSkillUsed"/>
        /// </summary>
        /// <param name="_Skill">The <see cref="Skill"/> that was used</param>
        private void SkillUsed(Skill? _Skill)
        {
            if (!SteamManager.Initialized || this.stats == null)
            {
                return;
            }
            
            var _firstPowerSkillUsed = _Skill is Skill.Power && !this.achievementStatus[POWER_SKILL];
            var _firstEvolveSkillUsed = _Skill is Skill.Evolve && !this.achievementStatus[EVOLVE_SKILL];
            var _firstDestroySkillUsed = _Skill is Skill.Destroy && !this.achievementStatus[DESTROY_SKILL];

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
            
            this.stats.SetStat(_Stats => _Stats.SkillCount, 1, Operation.Add);
        }

        /// <summary>
        /// Adds the <see cref="Time.time"/> (converted to hours), to <see cref="Stats.Playtime"/>
        /// </summary>
        private void AddPlaytime()
        {
            this.stats?.SetStat(_Stats => _Stats.Playtime, Time.time / 3600, Operation.Add);
        }
        
        /// <summary>
        /// Destroys the <see cref="StatsAndAchievementsManager"/> component of the <see cref="instance"/> GameObject
        /// </summary>
        public static void Destroy()
        {
            GameObject.Destroy(instance);
        }

#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// Resets all Stats, Achievements and PlayerPrefs <br/>
        /// <i>Game must be restarted for the PlayerPref changes</i>
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private void ResetAll()
        {
            Debug.LogWarning("Resetting");
            this.stats?.ResetAllStats_DEVELOPMENT();
            GlobalStats.ResetPlayerPrefs_DEVELOPMENT();
            SteamUserStats.ResetAllStats(true);
            SteamUserStats.StoreStats();
        }
#endif
        #endregion
    }
}
#endif