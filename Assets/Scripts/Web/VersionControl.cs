using System;
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
    /// <summary>
    /// Contains methods to get the latest released version of the game
    /// </summary>
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(CanvasRenderer))]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class VersionControl : MonoBehaviour
    {
#if DEBUG || DEVELOPMENT_BUILD
        #region Inspector Fields
        [Tooltip("Won't check for a new version if true (Development only)")]
        [SerializeField] private bool skipVersionCheck;
        #endregion
#endif
        
        #region Constants
        /// <summary>
        /// Website where the latest version is stored
        /// </summary>
        private const string REQUEST_URI = "https://raw.githubusercontent.com/MarkHerdt/Watermelon-Game/main/CurrentVersion";
        /// <summary>
        /// Key identifier for the version e.g. "CURRENT_VERSION = v1.0.0.0"
        /// </summary>
        private const string VERSION_KEY = "CURRENT_VERSION";
        /// <summary>
        /// Prefix before the version number
        /// </summary>
        private const char VERSION_PREFIX = 'v';
        #endregion

        #region Fields
        /// <summary>
        /// Singleton of <see cref="VersionControl"/>
        /// </summary>
        private static VersionControl instance;
        /// <summary>
        /// Displays the version number in game
        /// </summary>
        private TextMeshProUGUI version;
        /// <summary>
        /// Is shown in game, when a new version is available <br/>
        /// <i>Only if enabled on that platform</i>
        /// </summary>
        private Image updatesAvailable;
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
            
            this.version = base.GetComponent<TextMeshProUGUI>();
            this.updatesAvailable = base.GetComponentInChildren<Image>();
            this.version.text = string.Concat(VERSION_PREFIX, Application.version);
        }

        private void Start()
        {
            CheckIfNewVersionIsAvailable();
        }

        /// <summary>
        /// Checks if a newer version than <see cref="Application.version"/> is available and enables <see cref="updatesAvailable"/> <br/>
        /// <i>Only enables <see cref="updatesAvailable"/> if not disabled on that platform</i>
        /// </summary>
        private static async void CheckIfNewVersionIsAvailable()
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (instance.skipVersionCheck)
            {
                return;
            }
#endif
            
#if  WINDOWS_UWP && !UNITY_EDITOR
            return;
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
            await GetLatestVersion(_LatestVersion =>
            {
                if (Application.version != _LatestVersion)
                {
                    instance.updatesAvailable.enabled = true;       
                }
            });
        }
        
        /// <summary>
        /// Tries to fet the latest released version from <see cref="REQUEST_URI"/>
        /// </summary>
        /// <returns>The version number or null, when the version couldn't be retrieved</returns>
        [CanBeNull]
        public static async Task<string> TryGetLatestVersion()
        {
            string _version = null;

            await GetLatestVersion(_LatestVersion =>
            {
                _version = _LatestVersion;
            });
            
            return _version;
        }
        
        /// <summary>
        /// Gets the latest released version from &lt;see cref="REQUEST_URI"/&gt;
        /// </summary>
        /// <param name="_Action"><see cref="Action"/> to perform on the line that contains the version number</param>
        private static async Task GetLatestVersion(Action<string> _Action)
        {
            await Download.DownloadAsStreamAsync(REQUEST_URI, _Line =>
            {
                if (_Line.Contains(VERSION_KEY))
                {
                    var _latestVersion = Download.GetValue(_Line);
                    
                    _Action(_latestVersion);
                } 
            });
        }
        #endregion
    }
}