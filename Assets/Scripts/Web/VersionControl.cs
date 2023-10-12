using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon_Game.Web
{
    public sealed class VersionControl : WebBase
    {
        #region Constants
        private const string REQUEST_URI = "https://raw.githubusercontent.com/MarkHerdt/Watermelon-Game/main/CurrentVersion";
        private const string VERSION_KEY = "CURRENT_VERSION";
        #endregion

        #region Fields
        private static VersionControl instance;
        
        private TextMeshProUGUI version;
        private Image updatesAvailable;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.version = base.GetComponent<TextMeshProUGUI>();
            this.updatesAvailable = base.GetComponentInChildren<Image>();
            this.CheckLatestVersion();
        }
        
        private void Start()
        {
            this.version.text = string.Concat('v', Application.version);
            this.updatesAvailable.gameObject.SetActive(false);
        }

        private async void CheckLatestVersion()
        {
            await DownloadAsStreamAsync(REQUEST_URI, _Line =>
            {
                if (_Line.Contains(VERSION_KEY))
                {
                    var _latestVersion = GetValue(_Line);

                    if (Application.version != _latestVersion)
                    {
                        updatesAvailable.enabled = true;
                        updatesAvailable.gameObject.SetActive(true);
                    }
                } 
            });
        }

        [CanBeNull]
        public static async Task<string> GetLatestVersion()
        {
            string _version = null;
            
            await DownloadAsStreamAsync(REQUEST_URI, _Line =>
            {
                if (_Line.Contains(VERSION_KEY))
                {
                    _version = GetValue(_Line);
                }
            });

            return _version;
        }
        #endregion
    }
}