using System.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if !DISABLESTEAMWORKS
using Watermelon_Game.Steamworks.NET;
#endif

namespace Watermelon_Game.Web
{
    public sealed class VersionControl : WebBase
    {
#if DEBUG || DEVELOPMENT_BUILD
        #region Inspector Fields
        [SerializeField] private bool skipVersionCheck;
        #endregion
#endif
        
        #region Constants
        private const string REQUEST_URI = "https://raw.githubusercontent.com/MarkHerdt/Watermelon-Game/main/CurrentVersion";
        private const string VERSION_KEY = "CURRENT_VERSION";
        #endregion

        #region Fields
        public static VersionControl Instance;
        
        private TextMeshProUGUI version;
        private Image updatesAvailable;
        #endregion
        
        #region Methods
        private void Awake()
        {
            Instance = this;
            this.version = base.GetComponent<TextMeshProUGUI>();
            this.updatesAvailable = base.GetComponentInChildren<Image>();
            this.CheckLatestVersion();
        }
        
        private void Start()
        {
            this.version.text = string.Concat('v', Application.version);
            this.updatesAvailable.gameObject.SetActive(false);
        }

        public async void CheckLatestVersion()
        {
#if  WINDOWS_UWP && !UNITY_EDITOR
            return;
#endif
            
#if DEBUG || DEVELOPMENT_BUILD
            if (this.skipVersionCheck)
            {
                return;
            }
#endif
            
#if UNITY_EDITOR
            goto skipSteamManager;
#endif

#if !DISABLESTEAMWORKS
#pragma warning disable CS0162
            if (SteamManager.Initialized)
            {
                return;
            }
#pragma warning restore CS0162
#endif
            
#if UNITY_EDITOR
            skipSteamManager:
#endif
            
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