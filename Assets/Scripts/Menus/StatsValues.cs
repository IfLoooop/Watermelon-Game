using System;
using System.Linq;
using System.Reflection;
using OPS.AntiCheat.Field;

namespace Watermelon_Game.Menus
{
    /// <summary>
    /// Contains the values for <see cref="Stats"/>
    /// </summary>
    internal struct StatsValues
    {
        #region Fields
        /// <summary>
        /// All time best score
        /// </summary>
        public ProtectedInt32 BestScore { get; set; }
        /// <summary>
        /// Total amount of games played
        /// </summary>
        public ProtectedInt32 GamesPlayed { get; set; }
        /// <summary>
        /// Total amount of time spend in game
        /// </summary>
        public TimeSpan TimeSpendInGame { get; set; }
        #endregion
        
        #region Properties
        /// <summary>
        /// Best multiplier
        /// </summary>
        public ProtectedInt32 BestMultiplier { get; set; }
        /// <summary>
        /// Total evolved grape count
        /// </summary>
        public ProtectedInt32 GrapeEvolvedCount { get; set; }
        /// <summary>
        /// Total evolved cherry count
        /// </summary>
        public ProtectedInt32 CherryEvolvedCount { get; set; }
        /// <summary>
        /// Total evolved strawberry count
        /// </summary>
        public ProtectedInt32 StrawberryEvolvedCount { get; set; }
        /// <summary>
        /// Total evolved lemon count
        /// </summary>
        public ProtectedInt32 LemonEvolvedCount { get; set; }
        /// <summary>
        /// Total evolved orange count
        /// </summary>
        public ProtectedInt32 OrangeEvolvedCount { get; set; }
        /// <summary>
        /// Total evolved apple count
        /// </summary>
        public ProtectedInt32 AppleEvolvedCount { get; set; }
        /// <summary>
        /// Total evolved pear count
        /// </summary>
        public ProtectedInt32 PearEvolvedCount { get; set; }
        /// <summary>
        /// Total evolved pineapple count
        /// </summary>
        public ProtectedInt32 PineappleEvolvedCount { get; set; }
        /// <summary>
        /// Total evolved honeymelon count
        /// </summary>
        public ProtectedInt32 HoneymelonEvolvedCount { get; set; }
        /// <summary>
        /// Total evolved watermelon count
        /// </summary>
        public ProtectedInt32 WatermelonEvolvedCount { get; set; }
        /// <summary>
        /// Total golden fruit count
        /// </summary>
        public ProtectedInt32 GoldenFruitCount { get; set; }
        /// <summary>
        /// Total amount of power skills used
        /// </summary>
        public ProtectedInt32 PowerSkillUsedCount { get; set; }
        /// <summary>
        /// Total amount of evolve skills used
        /// </summary>
        public ProtectedInt32 EvolveSkillUsedCount { get; set; }
        /// <summary>
        /// Total amount of destroy skills used
        /// </summary>
        public ProtectedInt32 DestroySkillUsedCount { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// <see cref="StatsValues"/> <br/>
        /// <i>Needed for <see cref="StatsMenu"/>.<see cref="StatsMenu.Load"/></i>
        /// </summary>
        /// <param name="_BestScore"><see cref="BestScore"/></param>
        /// <param name="_BestMultiplier"><see cref="BestMultiplier"/></param>
        /// <param name="_GrapeEvolvedCount"><see cref="GrapeEvolvedCount"/></param>
        /// <param name="_CherryEvolvedCount"><see cref="CherryEvolvedCount"/></param>
        /// <param name="_StrawberryEvolvedCount"><see cref="StrawberryEvolvedCount"/></param>
        /// <param name="_LemonEvolvedCount"><see cref="LemonEvolvedCount"/></param>
        /// <param name="_OrangeEvolvedCount"><see cref="OrangeEvolvedCount"/></param>
        /// <param name="_AppleEvolvedCount"><see cref="AppleEvolvedCount"/></param>
        /// <param name="_PearEvolvedCount"><see cref="PearEvolvedCount"/></param>
        /// <param name="_PineappleEvolvedCount"><see cref="PineappleEvolvedCount"/></param>
        /// <param name="_HoneymelonEvolvedCount"><see cref="HoneymelonEvolvedCount"/></param>
        /// <param name="_WatermelonEvolvedCount"><see cref="WatermelonEvolvedCount"/></param>
        /// <param name="_GoldenFruitCount"><see cref="GoldenFruitCount"/></param>
        /// <param name="_PowerSkillUsedCount"><see cref="PowerSkillUsedCount"/></param>
        /// <param name="_EvolveSkillUsedCount"><see cref="EvolveSkillUsedCount"/></param>
        /// <param name="_DestroySkillUsedCount"><see cref="DestroySkillUsedCount"/></param>
        /// <param name="_GamesPlayed"><see cref="GamesPlayed"/></param>
        /// <param name="_TimeSpendInGame"><see cref="TimeSpendInGame"/></param>
        public StatsValues(int _BestScore, int _BestMultiplier, int _GrapeEvolvedCount, int _CherryEvolvedCount, int _StrawberryEvolvedCount, int _LemonEvolvedCount, int _OrangeEvolvedCount, int _AppleEvolvedCount, int _PearEvolvedCount, int _PineappleEvolvedCount, int _HoneymelonEvolvedCount, int _WatermelonEvolvedCount, int _GoldenFruitCount, int _PowerSkillUsedCount, int _EvolveSkillUsedCount, int _DestroySkillUsedCount, int _GamesPlayed, TimeSpan _TimeSpendInGame)
        {
            this.BestScore = _BestScore;
            this.BestMultiplier = _BestMultiplier;
            this.GrapeEvolvedCount = _GrapeEvolvedCount;
            this.CherryEvolvedCount = _CherryEvolvedCount;
            this.StrawberryEvolvedCount = _StrawberryEvolvedCount;
            this.LemonEvolvedCount = _LemonEvolvedCount;
            this.OrangeEvolvedCount = _OrangeEvolvedCount;
            this.AppleEvolvedCount = _AppleEvolvedCount;
            this.PearEvolvedCount = _PearEvolvedCount;
            this.PineappleEvolvedCount = _PineappleEvolvedCount;
            this.HoneymelonEvolvedCount = _HoneymelonEvolvedCount;
            this.WatermelonEvolvedCount = _WatermelonEvolvedCount;
            this.GoldenFruitCount = _GoldenFruitCount;
            this.PowerSkillUsedCount = _PowerSkillUsedCount;
            this.EvolveSkillUsedCount = _EvolveSkillUsedCount;
            this.DestroySkillUsedCount = _DestroySkillUsedCount;
            this.GamesPlayed = _GamesPlayed;
            this.TimeSpendInGame = _TimeSpendInGame;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks each property of <see cref="StatsValues"/>, for the bigger value, in this and the given <see cref="StatsValues"/> and returns a new <see cref="StatsValues"/> object with all properties set to the bigger value
        /// </summary>
        /// <param name="_LoadedStatsValues">The <see cref="StatsValues"/> object to compare the values of</param>
        /// <returns>A new <see cref="StatsValues"/> object with all properties set to the bigger value</returns>
        public StatsValues CheckIfBigger(StatsValues _LoadedStatsValues)
        {
            var _loadedProperties = _LoadedStatsValues.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var _currentProperties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var _statsValues = new StatsValues();
            
            foreach (var _loadedPropertyInfo in _loadedProperties)
            {
                var _currentPropertyInfo = _currentProperties.First(_PropertyInfo => _PropertyInfo.Name == _loadedPropertyInfo.Name);
                
                var _loadedPropertyValue = _loadedPropertyInfo.GetValue(_LoadedStatsValues);
                var _currentPropertyValue = _currentPropertyInfo.GetValue(this);
                
                var _biggerValue = GetBiggerValue(_loadedPropertyValue, _currentPropertyValue);
                var _property = _statsValues.GetType().GetProperty(_loadedPropertyInfo.Name, BindingFlags.Instance | BindingFlags.Public)!;
                object _updatedStatsValues = _statsValues;
                
                _property.SetValue(_updatedStatsValues, _biggerValue);
                _statsValues = (StatsValues)_updatedStatsValues;
            }
            
            return _statsValues;
        }

        /// <summary>
        /// Returns the bigger value of the given objects
        /// </summary>
        /// <param name="_Value1">First value to compare</param>
        /// <param name="_Value2">Second value to compare</param>
        /// <returns>The bigger value of the given objects</returns>
        /// <exception cref="ArgumentException">When one of the given objects has a <see cref="Type"/> other than <see cref="int"/> or <see cref="TimeSpan"/></exception>
        private static object GetBiggerValue(object _Value1, object _Value2)
        {
            var _isInt = _Value1 is ProtectedInt32 && _Value2 is ProtectedInt32;
            var _isTimeSpan = _Value1 is TimeSpan && _Value2 is TimeSpan;
            
            if (_isInt)
            {
                return (ProtectedInt32)_Value1 > (ProtectedInt32)_Value2 ? _Value1 : _Value2;
            }
            if (_isTimeSpan)
            {
                return (TimeSpan)_Value1 > (TimeSpan)_Value2 ? _Value1 : _Value2;
            }

            throw new ArgumentException($"The types of the given arguments [{_Value1.GetType()}] [{_Value2.GetType()}], can't be handled right now");
        }
        #endregion
    }
}