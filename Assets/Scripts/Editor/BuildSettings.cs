using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Watermelon_Game.Editor
{
    internal sealed class BuildSettings : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        #region Constants
        private const string WINDOWS = "Windows";
        private const string LINUX = "Linux";
        private const string MAC = "Mac";

        private const string BURST_DEBUG_INFORMATION = "_BurstDebugInformation_DoNotShip";
        private const string BACKUP_THIS_FOLDER = "_BackUpThisFolder_ButDontShipItWithYourGame";

        private const int TASK_DELAY = 1000;
        #endregion
        
        #region Properties
        public int callbackOrder { get; }
        #endregion

        #region Methods
        public void OnPreprocessBuild(BuildReport _Report)
        {
            var _target = EditorUserBuildSettings.activeBuildTarget;
            var _group = BuildPipeline.GetBuildTargetGroup(_target);
            PlayerSettings.SetArchitecture(_group, 2);
            
            if (_Report.summary.platform == BuildTarget.StandaloneWindows64)
            {
                PlayerSettings.SetScriptingBackend(_group, ScriptingImplementation.IL2CPP);
            }
            else
            {
                PlayerSettings.SetScriptingBackend(_group, ScriptingImplementation.Mono2x);
            }
        }
        
        public async void OnPostprocessBuild(BuildReport _Report)
        {
            // Method is called somewhere at the end of the build, not exactly when the build has finished, so need to wait for the previous build to completely finish
            await Task.Delay(TASK_DELAY);
            
            if (_Report.summary.platform == BuildTarget.StandaloneOSX)
            {
                Debug.Log($"<color=green>Mac Build</color> {_Report.summary.outputPath}");
                CleanUp(CreateInstallPath(_Report.summary.outputPath, MAC, true), MAC);
                BuildPlayer(CreateInstallPath(_Report.summary.outputPath, LINUX), BuildTarget.StandaloneLinux64);
            }
            else if (_Report.summary.platform == BuildTarget.StandaloneLinux64)
            {
                Debug.Log($"<color=green>Linux Build</color> {_Report.summary.outputPath}");
                CleanUp(CreateInstallPath(_Report.summary.outputPath, LINUX, true), LINUX);
                BuildPlayer(CreateInstallPath(_Report.summary.outputPath, WINDOWS), BuildTarget.StandaloneWindows64);
            }
            else if (_Report.summary.platform == BuildTarget.StandaloneWindows64)
            {
                Debug.Log($"<color=green>Windows Build</color> {_Report.summary.outputPath}");
                CleanUp(CreateInstallPath(_Report.summary.outputPath, WINDOWS, true), WINDOWS);
            }
        }
        
        public static string CreatePlatformFolders(string _Directory)
        {
            var _windows = Path.Combine(_Directory, WINDOWS);
            var _linux = Path.Combine(_Directory, LINUX);
            var _mac = Path.Combine(_Directory, MAC);

            Directory.CreateDirectory(_windows);
            Directory.CreateDirectory(_linux);
            Directory.CreateDirectory(_mac);
            
            DeleteAllFilesInDirectory(_windows);
            DeleteAllFilesInDirectory(_linux);
            DeleteAllFilesInDirectory(_mac);
            
            return Path.Combine(_mac, GetApplicationName(true));
        }
        
        private static void DeleteAllFilesInDirectory(string _Directory)
        {
            foreach (var _path in Directory.GetFileSystemEntries(_Directory))
            {
                var _isDirectory = File.GetAttributes(_path).HasFlag(FileAttributes.Directory);
                if (_isDirectory)
                {
                    Directory.Delete(_path, true);
                }
                else
                {
                    File.Delete(_path);
                }
            }
        }
        
        private static string GetApplicationName(bool _WithExtension)
        {
            return string.Concat(Application.productName, _WithExtension ? ".exe" : "");
        }
        
        private static string CreateInstallPath(string _Path, string _AddDirectory, bool _OnlyFolder = false)
        {
            return Path.Combine(Directory.GetParent(_Path)!.Parent!.FullName, _AddDirectory, _OnlyFolder ? "" : GetApplicationName(true));
        }
        
        public static void BuildPlayer(string _Path, BuildTarget _BuildTarget)
        {
            var _levels = new[] { SceneManager.GetActiveScene().path };
            
            BuildPipeline.BuildPlayer(_levels, _Path, _BuildTarget, BuildOptions.CompressWithLz4HC);
        }

        private static void CleanUp(string _Directory, string _Platform)
        {
            foreach (var _path in Directory.GetFileSystemEntries(_Directory))
            {
                if (_path.Contains(BURST_DEBUG_INFORMATION) ||_path.Contains(BACKUP_THIS_FOLDER))
                {
                    Directory.Delete(_path, true);
                }
            }

            // Unity is still not completely done at this point, so need to wait a little before creating the .zip file 
            Task.Delay(TASK_DELAY);
            
            var _destinationArchiveFileName = Path.Combine(Directory.GetParent(_Directory)!.FullName, string.Concat(GetApplicationName(false), " ", _Platform, ".zip"));
            if (File.Exists(_destinationArchiveFileName))
            {
                File.Delete(_destinationArchiveFileName);
            }
            ZipFile.CreateFromDirectory(_Directory, _destinationArchiveFileName, CompressionLevel.Optimal, false, Encoding.UTF8);
        }
        #endregion
    }
}