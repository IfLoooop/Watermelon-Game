using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        private const string BUILD_INFO = "BUILDINFO";
        private const string DEVELOPMENT_BUILD = "DEVELOPMENT_BUILD";
        private const string RELEASE_BUILD = "RELEASE_BUILD";
        
        private const string WINDOWS = "Windows";
        private const string LINUX = "Linux";
        private const string MAC = "Mac";
        private const string DEBUG = "Debug";

        private const string BURST_DEBUG_INFORMATION = "_BurstDebugInformation_DoNotShip";
        private const string BACKUP_THIS_FOLDER = "_BackUpThisFolder_ButDontShipItWithYourGame";

        private const int TASK_DELAY = 1000;
        #endregion
        
        #region Properties
        public int callbackOrder { get; }
        
        private static string BuildInfoPath { get; } = Path.Combine(Application.dataPath, $"{BUILD_INFO}.txt");
        #endregion

        #region Methods
        public void OnPreprocessBuild(BuildReport _Report)
        {
            var _buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var _buildTargetGroup = BuildPipeline.GetBuildTargetGroup(_buildTarget);
            // TODO: Check if this is needed
            //PlayerSettings.SetArchitecture(_buildTargetGroup, 2);

            var _isWindowsBuild = _Report.summary.platform == BuildTarget.StandaloneWindows64;
            if (_isWindowsBuild)
            {
                PlayerSettings.SetScriptingBackend(_buildTargetGroup, ScriptingImplementation.IL2CPP);
            }
            else
            {
                PlayerSettings.SetScriptingBackend(_buildTargetGroup, ScriptingImplementation.Mono2x);
            }
            
#if DEVELOPMENT_BUILD
            PlayerSettings.SetIl2CppCompilerConfiguration(_buildTargetGroup, Il2CppCompilerConfiguration.Debug);
            PlayerSettings.SetManagedStrippingLevel(_buildTargetGroup, ManagedStrippingLevel.Minimal);
#else
            PlayerSettings.SetIl2CppCompilerConfiguration(_buildTargetGroup, Il2CppCompilerConfiguration.Master);
            PlayerSettings.SetManagedStrippingLevel(_buildTargetGroup, ManagedStrippingLevel.High);
#endif
        }
        
        public async void OnPostprocessBuild(BuildReport _Report)
        {
            // ReSharper disable once MethodHasAsyncOverload
            File.WriteAllText(BuildInfoPath, string.Empty);
            
#if DEVELOPMENT_BUILD
            return;
#endif
#pragma warning disable CS0162
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
#pragma warning restore CS0162
        }

        public static string CreateDebugFolder(string _Directory)
        {
            var _debug = Path.Combine(_Directory, DEBUG);

            Directory.CreateDirectory(_debug);
            
            DeleteAllFilesInDirectory(_debug);
            
            return Path.Combine(_debug, GetApplicationName(true));
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
        
        public static void BuildPlayer(string _Path)
        {
            SetBuildInfo(DEVELOPMENT_BUILD, _Path, BuildTarget.StandaloneWindows64);
        }
        
        public static void BuildPlayer(string _Path, BuildTarget _BuildTarget)
        {
            SetBuildInfo(RELEASE_BUILD, _Path, _BuildTarget);
        }

        private static void SetBuildInfo(string _BuildInfo, string _Path, BuildTarget _BuildTarget)
        {
            var _text = string.Concat(_BuildInfo, Environment.NewLine, _Path, Environment.NewLine, _BuildTarget);
            File.WriteAllText(BuildInfoPath, _text);
            
            SetScriptingDefineSymbols(_BuildInfo);
        }
        
        /// <summary>
        /// Adds or removes the <see cref="DEVELOPMENT_BUILD"/> scripting define symbol
        /// </summary>
        /// <param name="_BuildInfo">Use <see cref="DEVELOPMENT_BUILD"/> or <see cref="RELEASE_BUILD"/></param>
        private static void SetScriptingDefineSymbols(string _BuildInfo)
        {
            var _buildTargetGroup = BuildPipeline.GetBuildTargetGroup(BuildTarget.StandaloneWindows64);
            var _namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(_buildTargetGroup);
            var _scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(_namedBuildTarget);
            var _splitScriptingDefineSymbols = _scriptingDefineSymbols.Split(';').ToList();

            if (_BuildInfo == DEVELOPMENT_BUILD && !_splitScriptingDefineSymbols.Contains(DEVELOPMENT_BUILD))
            {
                _splitScriptingDefineSymbols.Add(DEVELOPMENT_BUILD);
            }
            else if (_BuildInfo == RELEASE_BUILD && _splitScriptingDefineSymbols.Contains(DEVELOPMENT_BUILD))
            {
                _splitScriptingDefineSymbols.Remove(DEVELOPMENT_BUILD);
            }
            else
            {
                EditorUtility.RequestScriptReload();
            }
            
            PlayerSettings.SetScriptingDefineSymbols(_namedBuildTarget, _splitScriptingDefineSymbols.ToArray());
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScripsRecompiled()
        {
            var _text = File.ReadAllText(BuildInfoPath);

            if (string.IsNullOrWhiteSpace(_text))
            {
                return;
            }
            
            var _buildInfos = _text.Split(Environment.NewLine);
            var _buildInfo = _buildInfos[0];
            var _path = _buildInfos[1];
            var _buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), _buildInfos[2]);
            var _levels = new[] { SceneManager.GetActiveScene().path };
            
            if (_buildInfo == DEVELOPMENT_BUILD)
            {
                BuildPipeline.BuildPlayer(_levels, _path, _buildTarget, BuildOptions.CompressWithLz4);
            }
            else if (_buildInfo == RELEASE_BUILD)
            {
                BuildPipeline.BuildPlayer(_levels, _path, _buildTarget, BuildOptions.CompressWithLz4HC);
            }
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