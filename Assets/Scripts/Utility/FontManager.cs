using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon_Game.Utility
{
    /// <summary>
    /// Helper class for TMP fonts
    /// </summary>
#if DEBUG || DEVELOPMENT_BUILD
    [RequireComponent(typeof(Canvas), typeof(CanvasRenderer), typeof(CanvasScaler))]
    [RequireComponent(typeof(TextMeshProUGUI))]
#endif
    internal sealed class FontManager : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")] 
        [Tooltip("Main font asset")]
        [SerializeField] private TMP_FontAsset watermelonGame;
        [Tooltip("Main font asset")]
        [SerializeField] private TMP_FontAsset watermelonGame2;
        #endregion

        #region Fields
        /// <summary>
        /// Singleton of <see cref="FontManager"/>
        /// </summary>
        private static FontManager instance;
        /// <summary>
        /// Will be true after <see cref="WriteToTextfield_DEVELOPMENT"/> has finished
        /// </summary>
        private TaskCompletionSource<bool> writeToTextfieldFinished;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="watermelonGame"/>
        /// </summary>
        public static TMP_FontAsset WatermelonGame => instance.watermelonGame;
        #endregion

        #region Methods
        private void Awake()
        {
            instance = this;
        }
        #endregion
        
#if DEBUG || DEVELOPMENT_BUILD
        #region Inspector Fields
        [Tooltip("TextMeshPro component")]
        [SerializeField] private TextMeshProUGUI tmp;
        [Tooltip("Path to the file that will contain all missing characters")]
        [FilePath(AbsolutePath = true, RequireExistingPath = true, ParentFolder = "Assets/Fonts", Extensions = ".txt")]
        [SerializeField] private string filePath;
        #endregion

        #region Fields
        /// <summary>
        /// All characters that can't be displayed by <see cref="watermelonGame"/> <br/>
        /// <b>Key:</b> Unique key for all missing characters of one string -> <see cref="CheckForMissingCharacters_DEVELOPMENT"/> "_Info"-Parameter <br/>
        /// <b>Value:</b> The missing characters
        /// </summary>
        private readonly ConcurrentDictionary<string, List<char>> missingCharacters = new();
        /// <summary>
        /// Every unicode character in <see cref="watermelonGame"/> and its fallback FontAssets
        /// </summary>
        private readonly ConcurrentBag<uint> unicodes = new();
        #endregion
        
        #region Methods
        /// <summary>
        /// Adds all unicode characters from <see cref="watermelonGame"/> and all its fallback FontAssets to <see cref="unicodes"/> <br/>
        /// <i>Distinct list</i>
        /// </summary>
        /// <param name="_AddFallbackFonts">If true also adds the unicodes of the fallback font assets</param>
        /// <returns></returns>
        private Task AddAllUnicodeCharactersAsync_DEVELOPMENT(bool _AddFallbackFonts = true)
        {
            return Task.Run(() =>
            {
                var _allCharacters = new List<TMP_FontAsset>(_AddFallbackFonts ? watermelonGame.fallbackFontAssetTable : Array.Empty<TMP_FontAsset>()) { watermelonGame, watermelonGame2 }.SelectMany(_FontAsset => _FontAsset.characterTable);
                Parallel.ForEach(_allCharacters, _Character =>
                {
                    if (!this.unicodes.Contains(_Character.unicode))
                    {
                        this.unicodes.Add(_Character.unicode);
                    }
                });
            });
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Checks if any character in <see cref="tmp"/>-textfield is missing in <see cref="watermelonGame"/>
        /// </summary>
        [Button("Check for missing Characters")]
        private async void CheckForMissingCharacters_EDITOR()
        {
            if (string.IsNullOrWhiteSpace(this.tmp.text))
            {
                return;
            }

            await this.AddAllUnicodeCharactersAsync_DEVELOPMENT(false);
            
            var _missingCharacters = new List<char>();
            var _unicodes = new List<uint>();
            
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < this.tmp.text.Length; i++)
            {
                var _character = this.tmp.text[i];
                var _unicode = (uint)char.ConvertToUtf32(this.tmp.text, i);

                if (!this.unicodes.Contains(_unicode))
                {
                    if (!_missingCharacters.Contains(_character))
                    {
                        _missingCharacters.Add(_character);
                        _unicodes.Add(_unicode);
                    }
                }
                if (char.IsHighSurrogate(this.tmp.text[i]))
                {
                    i++; // Skip the next character for surrogate pair
                }
            }

            if (_missingCharacters.Count == 0)
            {
                Debug.Log("<color=green>All characters are present in the given FontAsset</color>");
            }
            else
            {
                Debug.LogWarning($"{_missingCharacters.Count}/{this.tmp.text.Distinct().Count()} characters are missing:\n{string.Join(string.Empty, _missingCharacters)}\n{string.Join("\n", _unicodes)}");
            }
        }
#endif
        
        /// <summary>
        /// Writes the given usernames to the textfield of <see cref="tmp"/>, so they're added to the dynamic font assets <br/>
        /// <i>
        /// There's a small delay between writing the characters to the textfield, and them being added to the dynamic fonts <br/>
        /// await this method to make sure all characters are properly added to the font assets
        /// </i>
        /// </summary>
        /// <param name="_Usernames">The usernames to write to the textfield</param>
        public static async Task AddCharactersToFontAssetAsync_DEVELOPMENT(ConcurrentBag<string> _Usernames)
        {
            instance.writeToTextfieldFinished = new TaskCompletionSource<bool>();
            var _writeToTextField = instance.WriteToTextfield_DEVELOPMENT(_Usernames);
            instance.StartCoroutine(_writeToTextField);
            
            await instance.writeToTextfieldFinished.Task;
            await instance.AddAllUnicodeCharactersAsync_DEVELOPMENT();
        }
        
        /// <summary>
        /// Writes all given usernames to the textfield of <see cref="tmp"/>
        /// </summary>
        /// <param name="_Usernames">The usernames to write to <see cref="tmp"/></param>
        /// <returns></returns>
        private IEnumerator WriteToTextfield_DEVELOPMENT(ConcurrentBag<string> _Usernames)
        {
            var _waitForEndOfFrame = new WaitForEndOfFrame();
            foreach (var _username in _Usernames)
            {
                this.tmp.text = _username;
                yield return _waitForEndOfFrame;
            }
            
            this.tmp.text = string.Empty;
            this.writeToTextfieldFinished.SetResult(true);
        }
        
        /// <summary>
        /// Checks if the given string has any characters that can't be displayed by <see cref="watermelonGame"/> or any of the fallback font assets
        /// </summary>
        /// <param name="_String">The string whose characters to check</param>
        /// <param name="_Info">
        /// Info message that is added to the missing character in the .txt file <br/>
        /// <b>Must be unique -> <see cref="missingCharacters"/>.<see cref="KeyValuePair{TKey,TValue}.key"/></b>
        /// </param>
        /// <returns>True if any character was missing from the given string, otherwise false</returns>
        public static bool CheckForMissingCharacters_DEVELOPMENT(string _String, string _Info)
        {
            if (string.IsNullOrWhiteSpace(_String))
            {
                return false;
            }
            
            var _anyCharacterMissing = false;
            
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < _String.Length; i++)
            {
                var _character = _String[i];
                var _unicode = (uint)char.ConvertToUtf32(_String, i);
                
                if (!instance.unicodes.Contains(_unicode))
                {
                    instance.missingCharacters.TryAdd(_Info, new List<char>());
                    
                    if (!instance.missingCharacters[_Info].Contains(_character))
                    {
                        instance.missingCharacters[_Info].Add(_character);
                    }

                    _anyCharacterMissing = true;
                }
                
                if (char.IsHighSurrogate(_String[i]))
                {
                    i++; // Skip the next character for surrogate pair
                }
            }
            
            return _anyCharacterMissing;
        }

        /// <summary>
        /// Writes the contents of <see cref="missingCharacters"/> to the .txt file at <see cref="filePath"/>
        /// </summary>
        public static async void WriteToFile_DEVELOPMENT()
        {
            await File.WriteAllTextAsync(instance.filePath, string.Empty);
            
            if (instance.missingCharacters.Count > 0)
            {
                Debug.LogError($"Missing Characters: {instance.missingCharacters.Count}\n{instance.filePath}");

                await using var _streamWriter = new StreamWriter(new FileStream(instance.filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite), new UTF8Encoding(true));
                var _output = string.Empty;
                foreach (var (_info, _characters) in instance.missingCharacters)
                {
                    _output += string.Join(string.Empty, _characters) + $" | {_info}\n";
                }
                _output += $"\n{string.Join(string.Empty, instance.missingCharacters.Values.SelectMany(_Characters => _Characters))}";
                
                await _streamWriter.WriteAsync(_output);
                _streamWriter.Close();
            }
        }
        #endregion
#endif
    }
}