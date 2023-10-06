using UnityEngine.UI;

namespace Watermelon_Game.Web
{
    internal sealed class VersionControl : WebBase
    {
        #region Constants
        private const string CURRENT_VERSION = "0.0";
        private const string REQUEST_URI = "https://raw.githubusercontent.com/MarkHerdt/Watermelon-Game/main/CurrentVersion";
        #endregion

        #region Fields
        private Image updatesAvailable;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.updatesAvailable = this.GetComponentInChildren<Image>();
            this.CheckLatestVersion();
        }

        private void Start()
        {
            this.updatesAvailable.gameObject.SetActive(false);
        }

        private async void CheckLatestVersion()
        {
            await base.DownloadAsStreamAsync(REQUEST_URI, _Line =>
            {
                if (_Line.Contains(nameof(CURRENT_VERSION)))
                {
                    var _latestVersion = base.GetValue(_Line);

                    if (CURRENT_VERSION != _latestVersion)
                    {
                        updatesAvailable.enabled = true;
                        updatesAvailable.gameObject.SetActive(true);
                    }
                } 
            });
        }
        #endregion
    }
}