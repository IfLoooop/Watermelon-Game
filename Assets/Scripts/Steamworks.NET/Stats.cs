#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using OPS.AntiCheat.Field;
using Steamworks;
using UnityEngine;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Steamworks.NET
{
    /// <summary>
    /// Contains Steam stats values
    /// </summary>
    internal class Stats
    {
        #region Stats API Names
        private const string GAMES_FINISHED = "GAMES_FINISHED";
        private const string HIGHSCORE = "HIGHSCORE";
        private const string GRAPE = "GRAPE";
        private const string CHERRY = "CHERRY";
        private const string STRAWBERRY = "STRAWBERRY";
        private const string LEMON = "LEMON";
        private const string ORANGE = "ORANGE";
        private const string APPLE = "APPLE";
        private const string PEAR = "PEAR";
        private const string PINEAPPLE = "PINEAPPLE";
        private const string HONEYMELON = "HONEYMELON";
        private const string WATERMELON = "WATERMELON";
        private const string GOLDEN_FRUIT = "GOLDEN_FRUIT";
        private const string UPGRADED_GOLDEN_FRUIT = "UPGRADED_GOLDEN_FRUIT";
        private const string SKILL_COUNT = "SKILL_COUNT";
        private const string PLAYTIME = "PLAYTIME";
        #endregion
        
        #region Fields
        /// <summary>
        /// Maps the Steam API Names to their respective value
        /// </summary>
        private readonly Dictionary<string, ProtectedFloat> statsMap = new()
        {
            { GAMES_FINISHED, 0 },
            { HIGHSCORE, 0 },
            { GRAPE, 0 },
            { CHERRY, 0 },
            { STRAWBERRY, 0 },
            { LEMON, 0 },
            { ORANGE, 0 },
            { APPLE, 0 },
            { PEAR, 0 },
            { PINEAPPLE, 0 },
            { HONEYMELON, 0 },
            { WATERMELON, 0 },
            { GOLDEN_FRUIT, 0 },
            { UPGRADED_GOLDEN_FRUIT, 0 },
            { SKILL_COUNT, 0 },
            { PLAYTIME, 0 }
        };
        #endregion
        
        #region Properties
        /// <summary>
        /// Number of games the player has finished <br/>
        /// <i>Does not include restarted games</i>
        /// </summary>
        public string GamesFinished => GAMES_FINISHED;
        /// <summary>
        /// Best score the player has ever reached
        /// </summary>
        public string Highscore => HIGHSCORE;
        /// <summary>
        /// Number of combined grapes
        /// </summary>
        public string Grapes => GRAPE;
        /// <summary>
        /// Number of combined cherries
        /// </summary>
        public string Cherries => CHERRY;
        /// <summary>
        /// Number of combined strawberries
        /// </summary>
        public string Strawberries => STRAWBERRY;
        /// <summary>
        /// Number of combined lemons
        /// </summary>
        public string Lemons => LEMON;
        /// <summary>
        /// Number of combined oranges
        /// </summary>
        public string Oranges => ORANGE;
        /// <summary>
        /// Number of combined apples
        /// </summary>
        public string Apples => APPLE;
        /// <summary>
        /// Number of combined pears
        /// </summary>
        public string Pears => PEAR;
        /// <summary>
        /// Number of combined pineapples
        /// </summary>
        public string Pineapples => PINEAPPLE;
        /// <summary>
        /// Number of combined honeymelons
        /// </summary>
        public string Honeymelons => HONEYMELON;
        /// <summary>
        /// Number of combined watermelons
        /// </summary>
        public string Watermelons => WATERMELON;
        /// <summary>
        /// Number of golden fruits found
        /// </summary>
        public string GoldenFruits => GOLDEN_FRUIT;
        /// <summary>
        /// Number of fruits, upgraded to a golden fruit
        /// </summary>
        public string UpgradedGoldenFruits => UPGRADED_GOLDEN_FRUIT;
        /// <summary>
        /// Total amount of skills used (Includes all skills)
        /// </summary>
        public string SkillCount => SKILL_COUNT;
        /// <summary>
        /// Total playtime in hours
        /// </summary>
        public string Playtime => PLAYTIME;
        #endregion

        #region Constructor
        /// <summary>
        /// <see cref="Stats"/>
        /// </summary>
        /// <param name="_GamesFinished"><see cref="GamesFinished"/></param>
        /// <param name="_Highscore"><see cref="Highscore"/></param>
        /// <param name="_Grapes"><see cref="Grapes"/></param>
        /// <param name="_Cherries"><see cref="Cherries"/></param>
        /// <param name="_Strawberries"><see cref="Strawberries"/></param>
        /// <param name="_Lemons"><see cref="Lemons"/></param>
        /// <param name="_Oranges"><see cref="Oranges"/></param>
        /// <param name="_Apples"><see cref="Apples"/></param>
        /// <param name="_Pears"><see cref="Pears"/></param>
        /// <param name="_Pineapples"><see cref="Pineapples"/></param>
        /// <param name="_Honeymelons"><see cref="Honeymelons"/></param>
        /// <param name="_Watermelons"><see cref="Watermelons"/></param>
        /// <param name="_GoldenFruits"><see cref="GoldenFruits"/></param>
        /// <param name="_UpgradedGoldenFruits"><see cref="UpgradedGoldenFruits"/></param>
        /// <param name="_SkillCount"><see cref="SkillCount"/></param>
        /// <param name="_Playtime"><see cref="Playtime"/></param>
        private Stats(int _GamesFinished, int _Highscore, int _Grapes, int _Cherries, int _Strawberries, int _Lemons, int _Oranges, int _Apples, int _Pears, int _Pineapples, int _Honeymelons, int _Watermelons, int _GoldenFruits, int _UpgradedGoldenFruits, int _SkillCount, float _Playtime)
        {
            this.statsMap[GAMES_FINISHED] = _GamesFinished;
            this.statsMap[HIGHSCORE] = _Highscore;
            this.statsMap[GRAPE] = _Grapes;
            this.statsMap[CHERRY] = _Cherries;
            this.statsMap[STRAWBERRY] = _Strawberries;
            this.statsMap[LEMON] = _Lemons;
            this.statsMap[ORANGE] = _Oranges;
            this.statsMap[APPLE] = _Apples;
            this.statsMap[PEAR] = _Pears;
            this.statsMap[PINEAPPLE] = _Pineapples;
            this.statsMap[HONEYMELON] = _Honeymelons;
            this.statsMap[WATERMELON] = _Watermelons;
            this.statsMap[GOLDEN_FRUIT] = _GoldenFruits;
            this.statsMap[UPGRADED_GOLDEN_FRUIT] = _UpgradedGoldenFruits;
            this.statsMap[SKILL_COUNT] = _SkillCount;
            this.statsMap[PLAYTIME] = _Playtime;
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Loads all stats for this user from Steam and stores them in a new <see cref="Stats"/> object
        /// </summary>
        /// <returns>A new <see cref="Stats"/> object, that contains all loaded stats from Steam for this user</returns>
        public static Stats LoadAllStats()
        {
            //ResetOldPlaytimeStats();
            
            SteamUserStats.GetStat(GAMES_FINISHED, out int _gamesFinished);
            SteamUserStats.GetStat(HIGHSCORE, out int _highscore);
            SteamUserStats.GetStat(GRAPE, out int _grapes);
            SteamUserStats.GetStat(CHERRY, out int _cherries);
            SteamUserStats.GetStat(STRAWBERRY, out int _strawberries);
            SteamUserStats.GetStat(LEMON, out int _lemons);
            SteamUserStats.GetStat(ORANGE, out int _oranges);
            SteamUserStats.GetStat(APPLE, out int _apples);
            SteamUserStats.GetStat(PEAR, out int _pears);
            SteamUserStats.GetStat(PINEAPPLE, out int _pineapples);
            SteamUserStats.GetStat(HONEYMELON, out int _honeymelons);
            SteamUserStats.GetStat(WATERMELON, out int _watermelons);
            SteamUserStats.GetStat(GOLDEN_FRUIT, out int _goldenFruits);
            SteamUserStats.GetStat(UPGRADED_GOLDEN_FRUIT, out int _upgradedGoldenFruits);
            SteamUserStats.GetStat(SKILL_COUNT, out int _skillCount);
            SteamUserStats.GetStat(PLAYTIME, out float _playtime);
            
            return new Stats(_gamesFinished, _highscore, _grapes, _cherries, _strawberries, _lemons, _oranges, _apples, _pears, _pineapples, _honeymelons, _watermelons, _goldenFruits, _upgradedGoldenFruits, _skillCount, _playtime);
        }
        
#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// Logs the values for the given API Name from <see cref="statsMap"/> and <see cref="SteamUserStats"/> <br/>
        /// <i><see cref="SteamUserStats"/>.<see cref="SteamUserStats.GetStat(string, out int)"/> is an API call, don't use to often</i>
        /// <b>Only works in Development builds!</b>
        /// </summary>
        /// <param name="_APIName">The API Name to get the value for</param>
        public void LogInt_DEVELOPMENT(Func<Stats, string> _APIName)
        {
            var _apiName = _APIName(this);
            SteamUserStats.GetStat(_apiName, out int _steamUserStatsValue);
            this.LogStat_DEVELOPMENT(_apiName, _steamUserStatsValue);
        }

        /// <summary>
        /// Logs the values for the given API Name from <see cref="statsMap"/> and <see cref="SteamUserStats"/> <br/>
        /// <i><see cref="SteamUserStats"/>.<see cref="SteamUserStats.GetStat(string, out int)"/> is an API call, don't use to often</i>
        /// <b>Only works in Development builds!</b>
        /// </summary>
        /// <param name="_APIName">The API Name to get the value for</param>
        public void LogFloat_DEVELOPMENT(Func<Stats, string> _APIName)
        {
            var _apiName = _APIName(this);
            SteamUserStats.GetStat(_apiName, out float _steamUserStatsValue);
            this.LogStat_DEVELOPMENT(_apiName, _steamUserStatsValue);
        }
        
        /// <summary>
        /// Logs the values for the given API Name from <see cref="statsMap"/> and <see cref="SteamUserStats"/> <br/>
        /// <b>Only works in Development builds!</b>
        /// </summary>
        /// <param name="_APIName">The API Name to get the value for</param>
        /// <param name="_SteamUserStatsValue">The value from <see cref="SteamUserStats"/> to log</param>
        private void LogStat_DEVELOPMENT(string _APIName, float _SteamUserStatsValue)
        {
            var _statsMapValue = this.statsMap[_APIName];
            var _message = string.Concat($"{_APIName}\n", $"StatsMap: {_statsMapValue}\n", $"SteamUserStats: {_SteamUserStatsValue}");
            
            Debug.Log(_message);
        }
#endif
        
        /// <summary>
        /// Returns the value from <see cref="statsMap"/> for the given API Name
        /// </summary>
        /// <param name="_APIName">The API Name to get the value for</param>
        /// <returns>The value from <see cref="statsMap"/> for the given API Name</returns>
        public float GetStat(Func<Stats, string> _APIName)
        {
            return this.statsMap[_APIName(this)];
        }
        
        /// <summary>
        /// Sets the value in <see cref="statsMap"/> for the given API Name, using the given <see cref="Operation"/>
        /// </summary>
        /// <param name="_APIName">The API Name to set the value for</param>
        /// <param name="_Value">The value to set</param>
        /// <param name="_Operation">The <see cref="Operation"/> to use, to set the given value</param>
        /// <returns>Returns the new set value from <see cref="statsMap"/></returns>
        public float SetStat(Func<Stats, string> _APIName, float _Value, Operation _Operation)
        {
            var _apiName = _APIName(this);
            this.Set(_apiName, _Value, _Operation);
            
            SteamUserStats.SetStat(_apiName, this.statsMap[_apiName]);
            return this.statsMap[_apiName];
        }
        
        /// <summary>
        /// Sets the value in <see cref="statsMap"/> for the given API Name, using the given <see cref="Operation"/>
        /// </summary>
        /// <param name="_APIName">The API Name to set the value for</param>
        /// <param name="_Value">The value to set</param>
        /// <param name="_Operation">The <see cref="Operation"/> to use, to set the given value</param>
        /// <returns>Returns the new set value from <see cref="statsMap"/></returns>
        public int SetStat(Func<Stats, string> _APIName, int _Value, Operation _Operation)
        {
            var _apiName = _APIName(this);
            this.Set(_apiName, _Value, _Operation);
            
            var _value = (int)this.statsMap[_apiName];
            SteamUserStats.SetStat(_apiName, _value);
            return _value;
        }

        /// <summary>
        /// Sets the value in <see cref="statsMap"/> for the given API Name, using the given <see cref="Operation"/>
        /// </summary>
        /// <param name="_APIName">The API Name to set the value for</param>
        /// <param name="_Value">The value to set</param>
        /// <param name="_Operation">The <see cref="Operation"/> to use, to set the given value</param>
        private void Set(string _APIName, float _Value, Operation _Operation)
        {
            switch (_Operation)
            {
                case Operation.Set:
                    this.statsMap[_APIName] = _Value;
                    break;
                case Operation.Add:
                    this.statsMap[_APIName] += _Value;
                    break;
                case Operation.Multiply:
                    this.statsMap[_APIName] *= _Value;
                    break;
            }
        }
        #endregion
    }
}
#endif