using UnityEditor;
using UnityEngine;
using Watermelon_Game.Web;

namespace Watermelon_Game.Editor
{
    internal static class Shortcuts
    {
        #region Methods
        [MenuItem("Build/Debug Build")]
        private static void DebugBuild()
        {
            var _path = EditorUtility.SaveFolderPanel("Debug Build", @"C:\Users\herdt\OneDrive\Desktop\Builds", "");
            
            if (!string.IsNullOrWhiteSpace(_path))
            {
                _path = BuildSettings.CreateDebugFolder(_path);
                
                BuildSettings.BuildPlayer(_path); 
            }
        }
        
        [MenuItem("Build/Release Build")]
        private static async void StartBuild()
        {
            var _version = await VersionControl.GetLatestVersion();
            
            if (_version == Application.version)
            {
                Debug.LogWarning("<color=orange>Version number has not changed</color>");
            }
            else
            {
                var _path = EditorUtility.SaveFolderPanel("Release Build", @"C:\Users\herdt\OneDrive\Desktop\Builds", "");
            
                if (!string.IsNullOrWhiteSpace(_path))
                {
                    _path = BuildSettings.CreatePlatformFolders(_path);
                    
                    BuildSettings.BuildPlayer(_path, BuildTarget.StandaloneOSX);   
                }
            }
        }
        #endregion
    }
}