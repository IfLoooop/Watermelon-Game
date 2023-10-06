using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Watermelon_Game.Web
{
    internal abstract class WebBase : MonoBehaviour
    {
        #region Fields
        private static readonly HttpClientHandler clientHandler = new()
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        private readonly HttpClient client = new(clientHandler);
        #endregion
        
        #region Methods
        protected async Task DownloadAsStreamAsync(string _RequestUri, Action<string> _Action)
        {
            await using var _websiteStream = await this.client.GetStreamAsync(_RequestUri);

            if (_websiteStream == null)
            {
                return;
            }
            
            using var _streamReader = new StreamReader(_websiteStream);

            while (await _streamReader.ReadLineAsync() is {} _currentLine)
            {
                _Action(_currentLine);
            }
        }

        /// <summary>
        /// Gets the field name of web settings entry <br/>
        /// <i>Expected format: fieldName = value</i>
        /// </summary>
        /// <param name="_String">A string that contains one web settings entry</param>
        /// <returns>The field name of a web settings entry</returns>
        protected string GetKey(string _String)
        {
            const char EQUALS = '=';
            var _index = _String.IndexOf(EQUALS);
            return _String[..(_index - 1)].Trim();
        }
        
        /// <summary>
        /// Gets the value of a web settings entry <br/>
        /// <i>Expected format: fieldName = value</i>
        /// </summary>
        /// <param name="_String">A string that contains one web settings entry</param>
        /// <returns>The value of a web settings entry</returns>
        protected string GetValue(string _String)
        {
            const char EQUALS = '=';
            var _index = _String.IndexOf(EQUALS) + 1;
            return _String[_index..].Trim();
        }
        #endregion
    }
}