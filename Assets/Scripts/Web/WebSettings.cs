using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Watermelon_Game.Fruit;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Web
{
    internal sealed class WebSettings : WebBase
    {
        #region Constants
        private const string REQUEST_URI = "https://raw.githubusercontent.com/MarkHerdt/Watermelon-Game/main/Settings";
        #endregion

        #region Fields
        private static WebSettings instance;
        
        private readonly Dictionary<string, object> settingsMap = new()
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
            // [SkillController]
            { "powerPointsRequirement", null },
            { "evolvePointsRequirement", null },
            { "destroyPointsRequirement", null }
        };
        
        private readonly ReadOnlyDictionary<uint, string> fruitSpawnWeightMap = new(new Dictionary<uint, string>
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
        private void Awake()
        {
            instance = this;
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void Init()
        {
#if !UNITY_EDITOR // TODO
   
#endif
            await instance.CheckSettings().ContinueWith(_ =>
            {
                foreach (var _kvp in instance.settingsMap)
                {
                    Debug.Log(_kvp);
                }
            
                GameController.Instance.FruitCollection.ApplyWebSettings(instance.settingsMap, instance.fruitSpawnWeightMap);
                SkillController.Instance.ApplyWebSettings(instance.settingsMap);    
            });
        }
        
        private async Task CheckSettings()
        {
            await base.DownloadAsStreamAsync(REQUEST_URI, _Line =>
            {
                if (Regex.IsMatch(_Line, @"\w*\s*[=]\s*\S*"))
                {
                    var _key = base.GetKey(_Line);
                    var _value = base.GetValue(_Line);
                    
                    if (this.settingsMap.ContainsKey(_key))
                    {
                        this.settingsMap[_key] = _value;
                    }
                }
            });
        }

        public static void TrySetValue<T>(Dictionary<string, object> _Settings, string _Key, ref T _Field)
        {
            _Settings.TryGetValue(_Key, out var _value);
            if (_value != null)
            {
                try
                {
                    _Field = (T)Convert.ChangeType(_value, typeof(T));
                }
                catch { /* ignored */ }
            }
        }
        
        public static void TrySetValue(Dictionary<string, object> _Settings, string _Key, FruitData _FruitData)
        {
            _Settings.TryGetValue(_Key, out var _value);
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