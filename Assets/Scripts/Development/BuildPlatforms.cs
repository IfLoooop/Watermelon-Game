using UnityEngine;

namespace Watermelon_Game.Development
{
    [ExecuteAlways]
    internal sealed class BuildPlatforms : MonoBehaviour
    {
        // TODO: Make builds in BuildSettings.cs selectable through this class
        #region Inspector Fields
        [SerializeField] private bool windows;
        [SerializeField] private bool mac;
        [SerializeField] private bool linux;
        [SerializeField] private bool uwp;
        #endregion

        #region Fields
        private static BuildPlatforms instance;
        #endregion
        
        #region Properties
        public bool Windows => this.windows;
        public bool Mac => this.mac;
        public bool Linux => this.linux;
        public bool UWP => this.uwp;
        #endregion

        #region Methods
        private void OnEnable()
        {
            instance = this;
        }
        #endregion
    }
}