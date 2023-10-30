using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Watermelon_Game.Web
{
    /// <summary>
    /// Helper class to download data from the internet
    /// </summary>
    public static class Download
    {
        #region Methods
        /// <summary>
        /// Download the contents of the website at the given <see cref="_RequestUri"/> as a <see cref="Stream"/>
        /// </summary>
        /// <param name="_RequestUri">Website to download the contents of</param>
        /// <param name="_Action"><see cref="Action"/> to perform on each line that is being downloaded</param>
        public static async Task DownloadAsStreamAsync(string _RequestUri, Action<string> _Action)
        {
            var _client = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            });

            try
            {
                await using var _websiteStream = await _client.GetStreamAsync(_RequestUri);
                
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
            catch { /* Ignored */ }
        }

        /// <summary>
        /// Gets the field name of web settings entry <br/>
        /// <i>Expected format: fieldName = value</i>
        /// </summary>
        /// <param name="_String">A string that contains one web settings entry</param>
        /// <returns>The field name of a web settings entry</returns>
        public static string GetKey(string _String)
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
        public static string GetValue(string _String)
        {
            const char EQUALS = '=';
            var _index = _String.IndexOf(EQUALS) + 1;
            return _String[_index..].Trim();
        }
        #endregion
    }
}