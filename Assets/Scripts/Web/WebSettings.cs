using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Watermelon_Game.Background;
using Watermelon_Game.Fruit;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Web
{
    internal sealed class WebSettings : WebBase
    {
        #region Constants
        private const string REQUEST_URI = "https://raw.githubusercontent.com/MarkHerdt/Watermelon-Game/main/Assets/SETTINGS.txt";
        #endregion

        #region Fields
        private static Dictionary<string, object> SettingsMap { get; } = new()
        {
            // [FruitCollection]
            { "spawnWeightMultiplier", null },
            { "lowerIndexWeight", null },
            { "higherIndexWeight", null },
            { "indexWeight", null },
            { "grapeSpawnWeight", null },
            { "cherrySpawnWeight", null },
            { "strawberrySpawnWeight", null },
            { "lemonSpawnWeight", null },
            { "orangeSpawnWeight", null },
            { "appleSpawnWeight", null },
            { "pearSpawnWeight", null },
            { "pineAppleSpawnWeight", null },
            { "honeyMelonSpawnWeight", null },
            { "melonSpawnWeight", null },
            { "goldenFruitChance", null },
            { "massMultiplier", null },
            // [SkillController]
            { "powerPointsRequirement", null },
            { "evolvePointsRequirement", null },
            { "destroyPointsRequirement", null },
            // [BackgroundCController]
            { "fruitSpawnDelay", null },
            { "sizeMultiplier", null },
            { "forceMultiplier", null },
            { "spriteAlphaValue", null },
        };

        public static ReadOnlyDictionary<uint, string> FruitSpawnWeightMap { get; } = new(new Dictionary<uint, string>
        {
            { 0, "grapeSpawnWeight" },
            { 1, "cherrySpawnWeight" },
            { 2, "strawberrySpawnWeight" },
            { 3, "lemonSpawnWeight" },
            { 4, "orangeSpawnWeight" },
            { 5, "appleSpawnWeight" },
            { 6, "pearSpawnWeight" },
            { 7, "pineAppleSpawnWeight" },
            { 8, "honeyMelonSpawnWeight" },
            { 9, "melonSpawnWeight" },
        });

        #endregion

        #region Methods
        // Is called before all "Awake()"-Methods
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void Init()
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (!Application.isEditor)
            {
                return;   
            }
#endif
#pragma warning disable CS0162
            await CheckSettings().ContinueWith(_ =>
            {
                // TODO: Make event and subscribe from classes that need it
                GameController.Instance.FruitCollection.ApplyWebSettings();
                SkillController.Instance.ApplyWebSettings();
                BackgroundController.Instance.ApplyWebSettings();
            });
#pragma warning restore CS0162
        }
        
        private static async Task CheckSettings()
        {
            await DownloadAsStreamAsync(REQUEST_URI, _Line =>
            {
                if (Regex.IsMatch(_Line, @"\w*\s*[=]\s*\S*"))
                {
                    var _key = GetKey(_Line);
                    var _value = GetValue(_Line);
                    
                    if (SettingsMap.ContainsKey(_key))
                    {
                        SettingsMap[_key] = _value;
                    }
                    else
                    {
                        // TODO: Save which setting couldn't be found in a .txt file
                        Debug.Log($"<color=orange>\"{_key}\" is missing</color>");
                    }
                }
            });
        }

        public static void TrySetValue<T>(string _Key, ref T _Field)
        {
            SettingsMap.TryGetValue(_Key, out var _value);
            if (_value != null)
            {
                try
                {
                    _Field = (T)Convert.ChangeType(_value, typeof(T));
                }
                catch { /* ignored */ }
            }
        }
        
        public static void TrySetValue(string _Key, FruitData _FruitData)
        {
            SettingsMap.TryGetValue(_Key, out var _value);
            if (_value != null)
            {
                try
                {
                    _FruitData.SpawnWeight = (int)Convert.ChangeType(_value, typeof(int));
                }
                catch { /* Ignored */ }
            }
        }
        #endregion
    }
}