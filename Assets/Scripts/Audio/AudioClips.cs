using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Steamworks;
using UnityEngine;
using Watermelon_Game.Steamworks.NET;
// ReSharper disable NotAccessedField.Local

namespace Watermelon_Game.Audio
{
    /// <summary>
    /// Contains all playable AudioClips
    /// </summary>
    [CreateAssetMenu(menuName = "Scriptable Objects/AudioClips", fileName = "AudioClips")]
    internal sealed class AudioClips : ScriptableObject
    {
        #region MyRegion
        [Header("BGM")]
        [Tooltip("Music that plays in the background")]
        [SerializeField] private AudioClipSettings bgm;
        
        [Header("Fruit")]
        [Tooltip("When a Fruit is destroyed")]
        [SerializeField] private AudioClipSettings fruitDestroy;

        [Header("Fruit Spawner")]
        [Tooltip("When the player is trying to release a Fruit, but the release ios blocked")]
        [SerializeField] private AudioClipSettings blockedRelease;
        [Tooltip("When a Fruit is released by the FruitSpawner")]
        [SerializeField] private AudioClipSettings releaseFruit;
        [Tooltip("When a Skill is used to release a Fruit")]
        [SerializeField] private AudioClipSettings shoot;
        
        [Header("Max Height")]
        [Tooltip("Plays every time the countdown is displayed")]
        [SerializeField] private AudioClipSettings countdown;
        [Tooltip("Plays every time the Godray flickers")]
        [SerializeField] private AudioClipSettings godrayFlicker;
        [Tooltip("When the Godray is being enabled")]
        [SerializeField] private AudioClipSettings godray;
        
        [Header("Menu")]
        [Tooltip("When a menu opens/closes")]
        [SerializeField] private AudioClipSettings menuPopup;
        
        [Header("Next Next Fruit")]
        [Tooltip("When NextNextFruit becomes enabled")]
        [SerializeField] private AudioClipSettings nextNextFruitEnabled;
        [Tooltip("When NextNextFruit becomes disabled")]
        [SerializeField] private AudioClipSettings nextNextFruitDisabled;
        
        [Header("Skill")]
        [Tooltip("When a Skill is selected/deselected")]
        [SerializeField] private AudioClipSettings skillSelect;
        #endregion
        
        #region Properties
        /// <summary>
        /// Contains all <see cref="AudioClipSettings"/> in <see cref="AudioClips"/>
        /// </summary>
        public static ReadOnlyDictionary<AudioClipName, AudioClipSettings> Clips { get; private set; }
        /// <summary>
        /// <see cref="SteamLeaderboard.steamLeaderboard"/>
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public static SteamLeaderboard_t? Settings { get; private set; }
        /// <summary>
        /// <see cref="SteamLeaderboard.onLeaderboardScoreUploaded"/>
        /// </summary>
        [CanBeNull] public static CallResult<LeaderboardScoreUploaded_t> OnSet { get; private set; } = new();
        #endregion
        
        #region Methods
        /// <summary>
        /// Initializes all needed values
        /// </summary>
        public void Init()
        {
            var _fields = typeof(AudioClips).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(_FieldInfo => _FieldInfo.FieldType == typeof(AudioClipSettings));
            var _dictionary = _fields.Select(_Field => (_Field.GetValue(this) as AudioClipSettings)!).ToDictionary(_AudioClipSettings => _AudioClipSettings.key);

            Clips = new ReadOnlyDictionary<AudioClipName, AudioClipSettings>(_dictionary);
        }

        /// <summary>
        /// Sets <see cref="Settings"/> and <see cref="OnSet"/> to null
        /// </summary>
        public static void Disable()
        {
            Settings = null;
            OnSet = null;
        }
        #endregion
    }
}