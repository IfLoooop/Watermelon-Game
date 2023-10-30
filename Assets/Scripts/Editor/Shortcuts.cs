using UnityEditor;
using UnityEngine;
using Watermelon_Game.Web;

namespace Watermelon_Game.Editor
{
    /// <summary>
    /// Contains shortcuts to start builds
    /// </summary>
    internal static class Shortcuts
    {
        #region Constants
        /// <summary>
        /// Default folder to open, when selecting a folder to save the build at
        /// </summary>
        private const string DEFAULT_BUILD_FOLDER = @"C:\Users\herdt\OneDrive\Desktop\Builds";
        #endregion
        
        #region Methods
        /// <summary>
        /// Starts a development build
        /// </summary>
        [MenuItem("Build/Debug Build")]
        private static void DebugBuild()
        {
            var _path = EditorUtility.SaveFolderPanel("Debug Build", DEFAULT_BUILD_FOLDER, "");
            
            if (!string.IsNullOrWhiteSpace(_path))
            {
                _path = BuildSettings.CreateDebugFolder(_path);

                BuildSettings.BuildPlayer(_path);
            }
        }
        
        /// <summary>
        /// Starts the release build pipeline <br/>
        /// <b>Multiple builds!</b>
        /// </summary>
        [MenuItem("Build/Release Build")]
        private static async void StartBuild()
        {
            var _version = await VersionControl.TryGetLatestVersion()!;
            
            if (_version == Application.version)
            {
                Debug.LogWarning("<color=orange>Version number has not changed</color>");
            }
            else
            {
                var _path = EditorUtility.SaveFolderPanel("Release Build", DEFAULT_BUILD_FOLDER, "");
            
                if (!string.IsNullOrWhiteSpace(_path))
                {
                    const BuildTarget BUILD_TARGET = BuildTarget.WSAPlayer;
                    
                    _path = BuildSettings.CreatePlatformFolder(_path, BUILD_TARGET);
                    
                    BuildSettings.BuildPlayer(_path, BUILD_TARGET); 
                }
            }
        }
        #endregion
    }
}