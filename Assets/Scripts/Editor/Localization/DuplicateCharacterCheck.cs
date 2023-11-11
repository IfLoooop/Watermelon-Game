using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Watermelon_Game.Editor.Localization
{
    [CreateAssetMenu(menuName = "Scriptable Objects/DuplicateCharacterCheck", fileName = "DuplicateCharacterCheck")]
    internal sealed class DuplicateCharacterCheck : ScriptableObject
    {
        #region Inspector Fields
        [FilePath(AbsolutePath = true, RequireExistingPath = true, ParentFolder = "Assets/Scripts/Editor/Localization", Extensions = ".txt")]
        [Tooltip("Filepath to the file that holds all characters")]
        [LabelWidth(125)]
        [SerializeField] private string charactersFilepath;

        [Title("Input", TitleAlignment = TitleAlignments.Centered)]
        [HideLabel][TextArea(10, 10)]
        [SerializeField] private string inputTextarea;

        [InfoBox("All new characters will be displayed here")]
        [PropertyOrder(6)][HideLabel][TextArea(10, 10)]
        [SerializeField] private string outputTextarea;
        #endregion
        
        #region Methods
        /// <summary>
        /// Checks if any of the characters in <see cref="inputTextarea"/> is not yet contained in the .txt file ate <see cref="charactersFilepath"/> <br/>
        /// Writes all new characters to <see cref="outputTextarea"/>
        /// </summary>
        [Button][HorizontalGroup("Button", Order = 5)]
        private void CheckCharacters()
        {
            var _characters = File.ReadAllText(this.charactersFilepath);

            foreach (var _char in this.inputTextarea.ToCharArray())
            {
                if (!_characters.Contains(_char) && !this.outputTextarea.Contains(_char))
                {
                    this.outputTextarea += _char;
                }
            }

            if (string.IsNullOrWhiteSpace(this.outputTextarea))
            {
                Debug.Log("The given characters are all known");
            }
        }
        
        /// <summary>
        /// Adds the characters from <see cref="outputTextarea"/> to the .txt file at <see cref="charactersFilepath"/>
        /// </summary>
        [PropertyOrder(7)][Button]
        private void AddCharacters()
        {
            if (!string.IsNullOrWhiteSpace(this.outputTextarea))
            {
                var _charactersToAdd = string.Empty;
                var _characters = File.ReadAllText(this.charactersFilepath);
                
                foreach (var _char in this.outputTextarea.ToCharArray().Distinct())
                {
                    if (!_characters.Contains(_char))
                    {
                        _charactersToAdd += _char;
                    }
                }
                
                File.AppendAllText(this.charactersFilepath, _charactersToAdd);
                
                Debug.Log($"The following characters have been added to the file:\n{_charactersToAdd}");

                this.outputTextarea = string.Empty;
                this.inputTextarea = string.Empty;
            }
        }
        #endregion
    }
}