using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Watermelon_Game.Editor.Steam
{
    /// <summary>
    /// Updates the contents of the localization .csv for Steam updates 
    /// </summary>
    [CreateAssetMenu(menuName = "Scriptable Objects/SteamUpdateLocalizer", fileName = "SteamUpdateLocalizer")]
    internal sealed class SteamUpdateLocalizer : ScriptableObject
    {
        #region Inspector Fields
        [Tooltip("Filepath of the localization .csv")]
        [LabelWidth(62.5f)][DisableIf(nameof(readonlyFilePath))]
        [FilePath(AbsolutePath = true, RequireExistingPath = true, ParentFolder = "Assets", Extensions = ".csv")]
        [SerializeField] private string filePath;
        
        [Tooltip("The language to set the values for")]
        [LabelWidth(62.5f)][OnValueChanged(nameof(SetChatGPTTranslationLanguage))]
        [SerializeField] private Language language;
        
        [InfoBox(nameof(TextAreaType.Title))]
        [HideLabel][TextArea(1, 1)]
        [ValidateInput(nameof(MustNotBeEmpty), "Title required")]
        [SerializeField] private string title;
        
        [InfoBox(nameof(TextAreaType.Subtitle))] 
        [HideLabel][TextArea(1, 2)]
        [SerializeField] private string subtitle;
        
        [InfoBox(nameof(TextAreaType.Summary))] 
        [HideLabel][TextArea(1, 3)]
        [SerializeField] private string summary;
        
        [InfoBox(nameof(TextAreaType.Body))] 
        [HideLabel][TextArea(10, 10)]
        [ValidateInput(nameof(MustNotBeEmpty), "Body required")]
        [SerializeField] private string body;
        
        [Tooltip("Insert the missing '*' between '[]' in [list] and [olist]")]
        [HorizontalGroup(nameof(Add), Width = .1f, LabelWidth = 115, Gap = 115)]
        [SerializeField] private bool insertListAsterisks;
        
        [Title("Load", TitleAlignment = TitleAlignments.Centered)]
        [InfoBox("Loads the contents for the selected language from the localization .csv file")]
        [PropertyOrder(10)][HideLabel][InlineButton(nameof(Load))]
        [SerializeField] private Language languageToLoad;
        
        [Title("Translate", TitleAlignment = TitleAlignments.Centered)]
        [InfoBox("ChatGPT translation prompt (Select the TextAreaType to translate, pick a language and click Generate)")]
        [Tooltip("The language to use for the translation prompt")]
        [PropertyOrder(11)][HideLabel][OnValueChanged(nameof(SetLanguage))]
        // ReSharper disable once InconsistentNaming
        [SerializeField] private Language chatGPTTranslationLanguage;
        
        [Tooltip("The textarea to translate the text of")]
        [PropertyOrder(12)][HideLabel][EnumToggleButtons]
        [SerializeField] private TextAreaType textAreaType;
        
        [Tooltip("Contains the prompt for ChatGPT")]
        [PropertyOrder(14)][HideLabel][TextArea(10, 10)]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once NotAccessedField.Local
        [SerializeField] private string chatGPTTranslationPromt;
        #endregion
        
        #region Fields
        /// <summary>
        /// Indicates whether <see cref="filePath"/> is currently only readonly or not
        /// </summary>
        private static bool readonlyFilePath;
        /// <summary>
        /// Indicates whether the <see cref="Reset"/> button is shown or hidden
        /// </summary>
        private static bool showResetButton;
        /// <summary>
        /// Indicates that the contents of a line should be skipped (for multiline)
        /// </summary>
        private const string REMOVE = "[REMOVE]";
        /// <summary>
        /// Regex pattern to check for multilines
        /// </summary>
        private static readonly string multiLinePattern = $"({nameof(title)}|{nameof(subtitle)}|{nameof(summary)}|{nameof(body)}),[a-z]+," + "{1}";
        #endregion
        
        #region Methods
        /// <summary>
        /// Is called when scripts have finished recompiling
        /// </summary>
        [DidReloadScripts]
        private static void OnRecompile()
        {
            readonlyFilePath = true;
            showResetButton = false;
        }
        
        /// <summary>
        /// Flips the readonly state of <see cref="filePath"/>
        /// </summary>
        [PropertyOrder(-1)][Button("Edit Filepath")]
        private void FlipFilePathReadonlyState()
        {
            readonlyFilePath = !readonlyFilePath;
        }

        /// <summary>
        /// Sets <see cref="chatGPTTranslationLanguage"/> to <see cref="language"/>
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private void SetChatGPTTranslationLanguage()
        {
            this.chatGPTTranslationLanguage = this.language;
        }

        /// <summary>
        /// Sets <see cref="language"/> to <see cref="chatGPTTranslationLanguage"/>
        /// </summary>
        private void SetLanguage()
        {
            this.language = this.chatGPTTranslationLanguage;
            this.GenerateChatGPTPrompt();
        }
        
        /// <summary>
        /// Returns false if the given value is empty
        /// </summary>
        /// <param name="_Value">The value to check</param>
        /// <returns>False if the given value is empty</returns>
        private bool MustNotBeEmpty(string _Value)
        {
            return !string.IsNullOrWhiteSpace(_Value);
        }

        /// <summary>
        /// Disables the <see cref="Add"/> button, if <see cref="title"/> or <see cref="body"/> are empty
        /// </summary>
        /// <returns>True when <see cref="title"/> or <see cref="body"/> are empty</returns>
        private bool RequiredFieldsAreEmpty()
        {
            return !this.MustNotBeEmpty(this.title) || !this.MustNotBeEmpty(this.body);
        }

        /// <summary>
        /// Creates a new <see cref="Regex"/> pattern for the text area type
        /// </summary>
        /// <param name="_TextAreaType">The <see cref="TextAreaType"/> to look for</param>
        /// <param name="_Language">The <see cref="Language"/> to look for</param>
        /// <returns>A new <see cref="Regex"/> pattern for the text area type</returns>
        private static Regex GetTextAreaTypePattern(TextAreaType _TextAreaType, Language _Language)
        {
            return new Regex($"^{_TextAreaType.ToString().ToLower()},{_Language}+," + "{1}");
        }
        
        /// <summary>
        /// Adds the values in <see cref="title"/>, <see cref="subtitle"/>, <see cref="summary"/> and <see cref="body"/>, for the currently set <see cref="language"/> to the .csv at <see cref="filePath"/> <br/>
        /// <i>Overwrites existing values</i>
        /// </summary>
        [HorizontalGroup(nameof(Add))]
        [Button][DisableIf(nameof(RequiredFieldsAreEmpty))]
        private void Add()
        {
            var _lines = File.ReadAllLines(this.filePath);
            var _updatedLines = new List<string>();
            
            this.SetTextAreaValue(_lines, TextAreaType.Title, this.title);
            this.SetTextAreaValue(_lines, TextAreaType.Subtitle, this.subtitle);
            this.SetTextAreaValue(_lines, TextAreaType.Summary, this.summary);
            this.SetTextAreaValue(_lines, TextAreaType.Body, this.body);

            foreach (var _line in _lines)
            {
                if (_line == REMOVE)
                {
                    continue;
                }

                if (_line.Contains('\n'))
                {
                    _updatedLines.AddRange(_line.Split('\n'));
                }
                else
                {
                    _updatedLines.Add(_line);
                }
            }

            var _contents = string.Join('\n', _updatedLines);
            File.WriteAllText(this.filePath, _contents);
            
            this.Load();
            
            Debug.ClearDeveloperConsole();
            Debug.Log($"<color=green>Added <b>{this.language}</b> localization</color>");
        }

        /// <summary>
        /// Sets the given value for the given text area type and current <see cref="language"/> in <see cref="_Lines"/>
        /// </summary>
        /// <param name="_Lines">Text are type and values will be looked for and set here</param>
        /// <param name="_TextAreaType">The type of the text area, to set the value for</param>
        /// <param name="_NewValue">The new value to set</param>
        private void SetTextAreaValue(IList<string> _Lines, TextAreaType _TextAreaType, string _NewValue)
        {
            var _textAreaTypePattern = GetTextAreaTypePattern(_TextAreaType, this.language);
            var _multilinePattern = new Regex(multiLinePattern);
            var _maxIndex = _Lines.Count - 1;
            
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i <= _maxIndex; i++)
            {
                var _isTextAreaType = _textAreaTypePattern.IsMatch(_Lines[i]);
                if (_isTextAreaType)
                {
                    var _containsMultiline = _NewValue.Contains('\n');
                    if (_containsMultiline)
                    {
                        _NewValue = string.Concat("\"", _NewValue, "\"");
                    }

                    if (this.insertListAsterisks)
                    {
                        _NewValue = InsertListAsterisks(_NewValue);
                    }
                    
                    _Lines[i] = string.Concat(_TextAreaType.ToString().ToLower(), ",", this.language, ",", _NewValue);

                    var _nextIndex = i + 1;
                    if (_nextIndex <= _maxIndex)
                    {
                        // ReSharper disable once InconsistentNaming
                        for (var j = _nextIndex; j <= _maxIndex; j++)
                        {
                            var _isMultiline = !_multilinePattern.IsMatch(_Lines[j]);
                            if (_isMultiline)
                            {
                                _Lines[j] = REMOVE;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Inserts the missing '*' in between the '[]' in the given string
        /// </summary>
        /// <param name="_NewValue">The string to insert the '*'</param>
        /// <returns>The given string with the inserted '*'</returns>
        private static string InsertListAsterisks(string _NewValue)
        {
            const string START_LIST = "[list]";
            const string END_LIST = "[/list]";
            const string START_OLIST = "[olist]";
            const string END_OLIST = "[/olist]";
            const string LIST_ENTRY = "[]";
            var _openList = false;
            
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < _NewValue.Length; i++)
            {
                var _isStartList = IsSubstring(_NewValue, i, START_LIST);
                var _isStartOList = IsSubstring(_NewValue, i, START_OLIST);
                if (_isStartList || _isStartOList)
                {
                    _openList = true;
                }

                if (_openList)
                {
                    var _isListEntry = IsSubstring(_NewValue, i, LIST_ENTRY);
                    if (_isListEntry)
                    {
                        _NewValue = _NewValue.Insert(i + 1, "*");
                    }
                }
                
                var _isEndList = IsSubstring(_NewValue, i, END_LIST);
                var _isEndOList = IsSubstring(_NewValue, i, END_OLIST);
                if (_isEndList || _isEndOList)
                {
                    _openList = false;
                }
            }

            return _NewValue;
        }

        /// <summary>
        /// Checks if the given <see cref="_Index"/> + <see cref="_Match"/>.<see cref="string.Length"/> in the given <see cref="_String"/> is == <see cref="_Match"/>
        /// </summary>
        /// <param name="_String">The string to look in</param>
        /// <param name="_Index">The index to start the match at</param>
        /// <param name="_Match">The match to look for</param>
        /// <returns>True, if the given <see cref="_Index"/> + <see cref="_Match"/>.<see cref="string.Length"/> in the given <see cref="_String"/> is == <see cref="_Match"/></returns>
        private static bool IsSubstring(string _String, int _Index, string _Match)
        {
            var _length = _Match.Length;
            var _isSubstring = false;
            
            if (_Index + _length <= _String.Length)
            {
                _isSubstring = _String.Substring(_Index, _length) == _Match;
            }

            return _isSubstring;
        }
        
        /// <summary>
        /// Enables the <see cref="Reset"/> button
        /// </summary>
        [InfoBox("Clears all user entries from the localization .csv file", InfoMessageType.Warning)]
        [Button]
        private void ResetFile()
        {
            showResetButton = !showResetButton;
        }

        /// <summary>
        /// Clears all user entries in the localization .csv at <see cref="filePath"/>
        /// </summary>
        [Button][ShowIf(nameof(showResetButton))]
        private void Reset()
        {
            showResetButton = false;
            
            var _languages = Enum.GetValues(typeof(Language)).Cast<Language>();
            var _lines = File.ReadAllLines(this.filePath);
            var _updatedContent = new List<string>();
            
            foreach (var _language in _languages)
            {
                this.language = _language;
                
                this.SetTextAreaValue(_lines, TextAreaType.Title, string.Empty);
                this.SetTextAreaValue(_lines, TextAreaType.Subtitle, string.Empty);
                this.SetTextAreaValue(_lines, TextAreaType.Summary, string.Empty);
                this.SetTextAreaValue(_lines, TextAreaType.Body, string.Empty);
            }

            foreach (var _line in _lines)
            {
                if (_line == REMOVE)
                {
                    continue;
                }
                
                _updatedContent.Add(_line);
            }
            
            File.WriteAllLines(this.filePath, _updatedContent);
            
            this.language = Language.english;
            this.languageToLoad = Language.english;

            this.title = string.Empty;
            this.subtitle = string.Empty;
            this.summary = string.Empty;
            this.body = string.Empty;
            this.chatGPTTranslationPromt = string.Empty;
            
            Debug.Log("<color=yellow>User entries have been removed</color>");
        }
        
        /// <summary>
        /// Loads the contents of all textareas for the selected <see cref="languageToLoad"/>, from the localization .csv file
        /// </summary>
        private void Load()
        {
            var _lines = File.ReadAllLines(this.filePath);

            this.title = this.GetTextAreaValue(_lines, TextAreaType.Title);
            this.subtitle = this.GetTextAreaValue(_lines, TextAreaType.Subtitle);
            this.summary = this.GetTextAreaValue(_lines, TextAreaType.Summary);
            this.body = this.GetTextAreaValue(_lines, TextAreaType.Body);
        }

        /// <summary>
        /// Gets the value for the given <see cref="TextAreaType"/> and the currently set <see cref="languageToLoad"/> from the localization .csv file at <see cref="filePath"/>
        /// </summary>
        /// <param name="_Lines">The contents of the localization .csv file</param>
        /// <param name="_TextAreaType">The <see cref="TextAreaType"/> to get the value for</param>
        /// <returns>The value for the given <see cref="TextAreaType"/> and the currently set <see cref="languageToLoad"/> from the localization .csv file at <see cref="filePath"/></returns>
        private string GetTextAreaValue(IList<string> _Lines, TextAreaType _TextAreaType)
        {
            var _content = string.Empty;
            var _textAreaTypePattern = GetTextAreaTypePattern(_TextAreaType, this.languageToLoad);
            var _multilinePattern = new Regex(multiLinePattern);
            var _maxIndex = _Lines.Count - 1;
            var _isMultiline = false;
            
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i <= _maxIndex; i++)
            {
                var _textAreaTypeMatch = _textAreaTypePattern.Match(_Lines[i]);
                if (_textAreaTypeMatch.Success)
                {
                    _content += _Lines[i].Replace(_textAreaTypeMatch.Value, string.Empty);

                    var _nextIndex = i + 1;
                    if (_nextIndex <= _maxIndex)
                    {
                        // ReSharper disable once InconsistentNaming
                        for (var j = _nextIndex; j <= _maxIndex; j++)
                        {
                            if (!_multilinePattern.IsMatch(_Lines[j]))
                            {
                                _isMultiline = true;
                                _content += $"\n{_Lines[j]}";
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
                }
            }
            
            if (_isMultiline)
            {
                _content = _content.Remove(0, 1);
                _content = _content.Remove(_content.Length - 1, 1);
            }
            
            return _content;
        }
        
        /// <summary>
        /// Generates the prompt for ChatGPT, from the currently set <see cref="chatGPTTranslationLanguage"/> and the value of the textarea that matches the selected <see cref="textAreaType"/>
        /// </summary>
        [PropertyOrder(13)][Button("Generate")]
        // ReSharper disable once InconsistentNaming
        private void GenerateChatGPTPrompt() // TODO: Change "textAreaType" to include/exclude the textareas
        {
            const string PROMPT_TEMPLATE = "Translate the following into {0}{1}:\n{2}";
            var _languageMap = new ReadOnlyDictionary<Language, string>(new Dictionary<Language, string>
            {
                { Language.english, "English" },
                { Language.german, "German" },
                { Language.french, "French" },
                { Language.italian, "Italian" },
                { Language.korean, "Korean" },
                { Language.spanish, "Spanish (Spain)" },
                { Language.schinese, "Simplified Chinese" },
                { Language.tchinese, "Traditional Chinese" },
                { Language.russian, "Russian" },
                { Language.japanese, "Japanese" },
                { Language.portuguese, "Portuguese (Portugal)" },
                { Language.brazilian, "Portuguese (Brazil)" },
                { Language.latam, "Spanish (Latin America)" }
            });
            
            switch (this.textAreaType)
            {
                case TextAreaType.Title:
                    this.chatGPTTranslationPromt = string.Format(PROMPT_TEMPLATE, _languageMap[this.chatGPTTranslationLanguage], AddMarkupNotifier(this.title), this.title);
                    break;
                case TextAreaType.Subtitle:
                    this.chatGPTTranslationPromt = string.Format(PROMPT_TEMPLATE, _languageMap[this.chatGPTTranslationLanguage], AddMarkupNotifier(this.subtitle), this.subtitle);
                    break;
                case TextAreaType.Summary:
                    this.chatGPTTranslationPromt = string.Format(PROMPT_TEMPLATE, _languageMap[this.chatGPTTranslationLanguage], AddMarkupNotifier(this.summary), this.summary);
                    break;
                case TextAreaType.Body:
                    this.chatGPTTranslationPromt = string.Format(PROMPT_TEMPLATE, _languageMap[this.chatGPTTranslationLanguage], AddMarkupNotifier(this.body), this.body);
                    break;
                case TextAreaType.All:
                    var _combinedTextAreas = string.Concat(this.title, "\n------------------------------------\n", this.summary, "\n------------------------------------\n", this.body);
                    this.chatGPTTranslationPromt = string.Format(PROMPT_TEMPLATE, _languageMap[this.chatGPTTranslationLanguage], AddMarkupNotifier(_combinedTextAreas), _combinedTextAreas);
                    break;
            }
        }

        /// <summary>
        /// Returns a markup notifier, if the given string contains any markup code
        /// </summary>
        /// <param name="_String">The string to search for the given symbols</param>
        /// <returns>A markup notifier if the given string contains any markup code, otherwise <see cref="string"/>.<see cref="string.Empty"/></returns>
        private static string AddMarkupNotifier(string _String)
        {
            var _markupCode = new List<string> { "[b]", "[/b]", "[u]", "[/u]", "[i]", "[/i]", "[strike]", "[/strike]", "[url=", "[/url]", "[list]", "[/list]", "[olist]", "[/olist]", "[*]", "[h1]", "[/h1]", "[h2]", "[/h2]", "[h3]", "[/h3]", "[previewyoutube=", "[/previewyoutube]", "[img]", "[/img]" };
            const string MARKUP_NOTIFIER = " (Leave the markup code exactly as it is)";
            return _markupCode.Any(_String.Contains) ? MARKUP_NOTIFIER : string.Empty;
        }
        #endregion
    }
}