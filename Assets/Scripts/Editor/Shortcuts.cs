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
        [MenuItem("Build/Development Build")]
        private static void DebugBuild()
        {
            var _path = EditorUtility.SaveFolderPanel("Development Build", DEFAULT_BUILD_FOLDER, "");
            
            if (!string.IsNullOrWhiteSpace(_path))
            {
                _path = BuildSettings.CreateDevelopmentFolder(_path);
                Debug.Log("Starting <color=magenta>Development</color> Build");
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

            _version = "1.7.0.0"; // TODO: Temporary while the Github version lacks behind (Itch.io build needs to be updated) // TODO: Enable
            
            if (!string.IsNullOrWhiteSpace(_version))
            {
                if (_version == Application.version)
                {
                    Debug.LogWarning("<color=orange>Version number has not changed</color>");
                }
                else
                {
                    var _path = EditorUtility.SaveFolderPanel("Release Build", DEFAULT_BUILD_FOLDER, "");
                
                    if (!string.IsNullOrWhiteSpace(_path))
                    {
                        const BuildTarget BUILD_TARGET = BuildTarget.StandaloneWindows64;
                        
                        _path = BuildSettings.CreatePlatformFolder(_path, BUILD_TARGET);
                        Debug.Log("Starting <color=yellow>Release</color> Build");
                        BuildSettings.BuildPlayer(_path, BUILD_TARGET); 
                    }
                }
            }
            else
            {
                Debug.LogError("Downloaded version is null");
            }
        }
        #endregion
    }
}