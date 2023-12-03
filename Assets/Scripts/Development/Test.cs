using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Watermelon_Game.Development
{
    internal sealed class Test : MonoBehaviour
    {
#if DEBUG || DEVELOPMENT_BUILD

        #region Inspector Fields
        [Tooltip("For outputting test data")]
        [SerializeField] private TextMeshProUGUI testTMP;
        #endregion
        
        #region Fields
        /// <summary>
        /// Filepath to the "Test1.txt"-file
        /// </summary>
        private static readonly string test1 = Path.Combine(Application.dataPath, "Test", "Test1.txt");
        /// <summary>
        /// Filepath to the "Test2.txt"-file
        /// </summary>
        private static readonly string test2 = Path.Combine(Application.dataPath, "Test", "Test2.txt");
        #endregion
        
        #region Methods
        [Button]
        private void TestButton1()
        {
            var _tags = new List<string>();
            
            var _filePath = test1;
            var _content = File.ReadAllText(_filePath);
            var _split = _content.Split(',');

            foreach (var _tag in _split)
            {
                if (!_tags.Contains(_tag.Trim()))
                {
                    _tags.Add(_tag.Trim());
                }
            }
            
            _filePath = test2;
            var _joined = string.Join(',', _tags);
            
            File.WriteAllText(_filePath, _joined);
            
            Debug.Log($"Initial: {_split.Length}");
            Debug.Log($"Final: {_tags.Count}");
            
            Debug.Log("TestButton Finished");
        }

        [SerializeField] private string test;

        [Button]
        private void Test2()
        {
            var _body = System.Text.Encoding.UTF8.GetBytes(test);
            Debug.Log(_body.Length);
        }
        #endregion
#endif
    }
}