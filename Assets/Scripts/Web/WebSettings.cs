using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Watermelon_Game.Background;
using Watermelon_Game.Fruits;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Web
{
    /// <summary>
    /// Subscribe to the <see cref="OnApplyWebSettings"/>-event, to overwrite primitive data types with settings stored online 
    /// </summary>
    internal sealed class WebSettings : MonoBehaviour
    {
        #region Constants
        /// <summary>
        /// Website where the settings are stored
        /// </summary>
        private const string REQUEST_URI = "https://raw.githubusercontent.com/MarkHerdt/Watermelon-Game/main/Assets/SETTINGS.txt";
        #endregion

        #region Events
        /// <summary>
        /// Event is called in <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/>, so subscriptions need to happen in at least <see cref="RuntimeInitializeLoadType.BeforeSplashScreen"/>
        /// </summary>
        public static event Action OnApplyWebSettings;
        #endregion
        
        #region Fields
        /// <summary>
        /// <b>Key:</b> Name of the property <br/>
        /// <b>Value:</b> Value to set
        /// </summary>
        private static Dictionary<string, object> SettingsMap { get; } = new()
        {
            // TODO: Try finding a way to also change the names in SETTINGS.txt when these property names have changed 
            // [FruitCollection]
            { nameof(FruitSettings.SpawnWeightModifier), null },
            { nameof(FruitSettings.LowerIndexWeight), null },
            { nameof(FruitSettings.HigherIndexWeight), null },
            { nameof(FruitSettings.SameIndexWeight), null },
            { nameof(FruitSettings.GrapeSpawnWeight), null },
            { nameof(FruitSettings.CherrySpawnWeight), null },
            { nameof(FruitSettings.StrawberrySpawnWeight), null },
            { nameof(FruitSettings.LemonSpawnWeight), null },
            { nameof(FruitSettings.OrangeSpawnWeight), null },
            { nameof(FruitSettings.AppleSpawnWeight), null },
            { nameof(FruitSettings.PearSpawnWeight), null },
            { nameof(FruitSettings.PineappleSpawnWeight), null },
            { nameof(FruitSettings.HoneymelonSpawnWeight), null },
            { nameof(FruitSettings.WatermelonSpawnWeight), null },
            { nameof(FruitSettings.GoldenFruitChance), null },
            { nameof(FruitSettings.MassMultiplier), null },
            // [SkillController]
            { nameof(SkillController.PowerPointsRequirement), null },
            { nameof(SkillController.EvolvePointsRequirement), null },
            { nameof(SkillController.DestroyPointsRequirement), null },
            { nameof(SkillController.SkillPointIncrease), null },
            // [BackgroundFruitController]
            { nameof(BackgroundFruitController.FruitSpawnDelay), null },
            { nameof(BackgroundFruitController.SizeMultiplier), null },
            { nameof(BackgroundFruitController.ForceMultiplier), null },
            { nameof(BackgroundFruitController.SpriteAlphaValue), null }
        };
        #endregion

        #region Methods
        /// <summary>
        /// Is called before all "Awake()"-Methods
        /// </summary>
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
            await GetWebSettings().ContinueWith(_Task =>
            {
                foreach (var _propertyName in _Task.Result)
                {
                    PrintWebSettingsKeyMatchError(_propertyName, "SETTINGS.txt");
                }
                
                OnApplyWebSettings?.Invoke();
            });
#pragma warning restore CS0162
        }
        
        /// <summary>
        /// Downloads the settings at <see cref="REQUEST_URI"/>
        /// </summary>
        /// <returns>A list with property names, that couldn't be matched with any key in <see cref="SettingsMap"/></returns>
        private static async Task<List<string>> GetWebSettings()
        {
            var _keysNotFound = new List<string>();
            
            await Download.DownloadAsStreamAsync(REQUEST_URI, _Line =>
            {
                const string WEB_SETTINGS_KEY_VALUE_PATTERN = @"\w*\s*[=]\s*\S*";
                if (Regex.IsMatch(_Line, WEB_SETTINGS_KEY_VALUE_PATTERN))
                {
                    var _key = Download.GetKey(_Line);
                    var _value = Download.GetValue(_Line);
                    
                    if (SettingsMap.ContainsKey(_key))
                    {
                        SettingsMap[_key] = _value;
                    }
                    else
                    {
                        _keysNotFound.Add(_key);
                    }
                }
            });

            return _keysNotFound;
        }

        /// <summary>
        /// Tries to set the value of the given <see cref="_Field"/> <br/>
        /// <i>Prints a console warning on failure</i>
        /// </summary>
        /// <param name="_PropertyName">The name of the property</param>
        /// <param name="_Field">A reference to the field</param>
        /// <param name="_CallerType">Type of the class, where this method is called from (For error logs)</param>
        /// <typeparam name="T">Must be a primitive data type</typeparam>
        public static void TrySetValue<T>(string _PropertyName, ref T _Field, Type _CallerType)
        {
            SettingsMap.TryGetValue(_PropertyName, out var _value);
            if (_value != null)
            {
                try
                {
                    _Field = (T)Convert.ChangeType(_value, typeof(T));
                }
                catch { /* ignored */ }
            }
            else
            {
                PrintWebSettingsKeyMatchError(_PropertyName, _CallerType.Name);
            }
        }
        
        /// <summary>
        /// Prints a <see cref="Watermelon_Game.Utility.Debug.LogError(object)"/> when a field name didn't match any key in <see cref="SettingsMap"/>
        /// </summary>
        /// <param name="_PropertyName">The name of the property</param>
        /// <param name="_CallerType">Type of the class, where the field is declared</param>
        private static void PrintWebSettingsKeyMatchError(string _PropertyName, string _CallerType)
        {
            Debug.LogError($"The property name [{_PropertyName}] ({_CallerType}.cs) didn't match any key in [{nameof(SettingsMap)}] ({nameof(WebSettings)}.cs)");
        }
        #endregion
    }
}