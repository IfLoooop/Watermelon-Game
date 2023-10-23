using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private const uint BUILD_INFO_THRESHOLD_IN_SECONDS = 120;
        private const string BUILD_INFO = "BUILDINFO";
        private const string DEVELOPMENT_BUILD = "DEVELOPMENT_BUILD";
        private const string RELEASE_BUILD = "RELEASE_BUILD";

        private const string DEBUG = "Debug";
        private const string UWP = "UWP";
        private const string MAC = "Mac";
        private const string LINUX = "Linux";
        private const string WINDOWS = "Windows";

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
            var _activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            var _buildTargetGroup = BuildPipeline.GetBuildTargetGroup(_activeBuildTarget);
            // TODO: Check if this is needed
            //PlayerSettings.SetArchitecture(_buildTargetGroup, 2);

            var _reportedBuildTarget = _Report.summary.platform;
            if (_reportedBuildTarget == BuildTarget.WSAPlayer)
            {
                EditorUserBuildSettings.wsaArchitecture = "x64";
                EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.D3D;
                EditorUserBuildSettings.wsaUWPSDK = GetLatestInstalledWSAVersion();
                EditorUserBuildSettings.wsaMinUWPSDK = "10.0.10240.0";
                EditorUserBuildSettings.wsaMinUWPSDK = "Visual Studio 2022"; // TODO: Check if this info can be gotten automatically from somewhere
                EditorUserBuildSettings.wsaBuildAndRunDeployTarget = WSABuildAndRunDeployTarget.LocalMachine;
                EditorUserBuildSettings.development = false;
                UnityEditor.UWP.UserBuildSettings.buildConfiguration = WSABuildType.Master;
            }
            else if (_reportedBuildTarget == BuildTarget.StandaloneOSX)
            {
                UnityEditor.OSXStandalone.UserBuildSettings.architecture = OSArchitecture.x64ARM64;
            }
            
#if DEVELOPMENT_BUILD
            PlayerSettings.SetIl2CppCompilerConfiguration(_buildTargetGroup, Il2CppCompilerConfiguration.Debug);
            PlayerSettings.SetManagedStrippingLevel(_buildTargetGroup, ManagedStrippingLevel.Minimal);
#else
#if !WINDOWS_UWP
            PlayerSettings.SetIl2CppCompilerConfiguration(_buildTargetGroup, Il2CppCompilerConfiguration.Master);
#endif
            PlayerSettings.SetManagedStrippingLevel(_buildTargetGroup, ManagedStrippingLevel.High);
#endif
        }
        
        private static string GetLatestInstalledWSAVersion()
        {
            const string FOLDER_PATH = @"Windows Kits\10";
            const string SDK_MANIFEST = "SDKManifest.xml";
            var _programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var _windowsSDKPath = Path.Combine(_programFilesX86, FOLDER_PATH, SDK_MANIFEST);
            
            if (File.Exists(_windowsSDKPath))
            {
                const string SDK_MANIFEST_PATTERN = "PlatformIdentity\\s*=\\s*\".*Version\\s*=\\s*(\\d+[.]{1}\\d+[.]?)+";
                var _sdkManifest = File.ReadAllText(_windowsSDKPath);
                var _match = Regex.Match(_sdkManifest, SDK_MANIFEST_PATTERN);

                if (_match.Success)
                {
                    var _lastEqualsIndex = _match.Value.LastIndexOf('=');
                    var _versionNumber = _match.Value[(_lastEqualsIndex + 1)..].Trim();

                    return _versionNumber;
                }

                throw new FormatException($"Could not match the pattern: {SDK_MANIFEST_PATTERN}{Environment.NewLine}{_sdkManifest}");
            }

            throw new FileNotFoundException($"Could not find the file \"{SDK_MANIFEST}\" at \"{_windowsSDKPath}\"");
        }
        
        public async void OnPostprocessBuild(BuildReport _Report)
        {
            var _emptyBuildInfo = !GetBuildInfo(out _, out _, out _, false, nameof(OnPostprocessBuild));

            SetBuildInfo(string.Empty, BuildTarget.NoTarget, BuildInfoPath, true);
            
            if (_emptyBuildInfo)
            {
                return;
            }
            
#if DEVELOPMENT_BUILD
            return;
#endif
#pragma warning disable CS0162
            // Method is called somewhere at the end of the build, not exactly when the build has finished, so need to wait for the previous build to completely finish
            await Task.Delay(TASK_DELAY);
            
            Build(_Report, BuildTarget.WSAPlayer, UWP, BuildTarget.StandaloneLinux64, LINUX);
            Build(_Report, BuildTarget.StandaloneLinux64, LINUX, BuildTarget.StandaloneOSX, MAC);
            Build(_Report, BuildTarget.StandaloneOSX, MAC, BuildTarget.StandaloneWindows64, WINDOWS);
            Build(_Report, BuildTarget.StandaloneWindows64, WINDOWS, null, string.Empty);
#pragma warning restore CS0162
        }

        private void Build(BuildReport _Report, BuildTarget _CurrentBuildTarget, string _CurrentOS, BuildTarget? _NextBuildTarget, string _NextOS)
        {
            if (_Report.summary.platform == _CurrentBuildTarget)
            {
                var _outputPath = _Report.summary.outputPath;
                Debug.Log($"<color=green>{_CurrentOS} Build finished</color> {_outputPath}");
                var _buildsFolder = Directory.GetParent(_outputPath)!.Parent!.Parent!.FullName;
                var _installPath = CreateInstallPath(_buildsFolder, _CurrentOS, true);
                CleanUp(_installPath, _CurrentOS);
                
                if (_NextBuildTarget != null)
                {
                    _installPath = CreateInstallPath(_buildsFolder, _NextOS, false);
                    CreatePlatformFolder(_buildsFolder, _NextBuildTarget.Value);
                    BuildPlayer(_installPath, _NextBuildTarget.Value);   
                }
            }
        }
        
        public static string CreateDebugFolder(string _Directory)
        {
            var _debug = Path.Combine(_Directory, DEBUG);

            Directory.CreateDirectory(_debug);
            
            DeleteAllFilesInDirectory(_debug);
            
            return Path.Combine(_debug, GetApplicationName(true));
        }
        
        public static string CreatePlatformFolder(string _Directory, BuildTarget _BuildTarget)
        {
            var _baseDirectoryName = GetApplicationName(false);
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            var _path = _BuildTarget switch
            {
                BuildTarget.WSAPlayer => Path.Combine(_Directory, UWP, _baseDirectoryName),
                BuildTarget.StandaloneLinux64 => Path.Combine(_Directory, LINUX, _baseDirectoryName),
                BuildTarget.StandaloneOSX => Path.Combine(_Directory, MAC, _baseDirectoryName),
                BuildTarget.StandaloneWindows64 => Path.Combine(_Directory, WINDOWS, _baseDirectoryName),
                _ => throw new ArgumentException($"The passed {nameof(BuildTarget)} {_BuildTarget} can currently not be used")
            };

            Directory.CreateDirectory(_path);
            DeleteAllFilesInDirectory(_path);

            var _applicationName = GetApplicationName(true);

            return Path.Combine(_path, _applicationName);
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
        
        private static string CreateInstallPath(string _BuildsFolder, string _AddDirectory, bool _OnlyFolder)
        {
            var _baseDirectory = GetApplicationName(false);
            var _fileExtension = _OnlyFolder ? "" : GetApplicationName(true);
            
            return Path.Combine(_BuildsFolder, _AddDirectory, _baseDirectory, _fileExtension);
        }
        
        public static void BuildPlayer(string _Path)
        {
            SwitchPlatform(BuildTarget.StandaloneWindows64);
            SetBuildInfo(DEVELOPMENT_BUILD, BuildTarget.StandaloneWindows64, _Path);
            SetScriptingDefineSymbols(DEVELOPMENT_BUILD);
        }
        
        public static void BuildPlayer(string _Path, BuildTarget _BuildTarget)
        {
            SwitchPlatform(_BuildTarget);
            SetBuildInfo(RELEASE_BUILD, _BuildTarget, _Path);
            SetScriptingDefineSymbols(RELEASE_BUILD);
        }

        private static void SwitchPlatform(BuildTarget _BuildTarget)
        {
            var _buildTargetGroup = BuildPipeline.GetBuildTargetGroup(_BuildTarget);
            EditorUserBuildSettings.SwitchActiveBuildTarget(_buildTargetGroup, _BuildTarget);
            
            if (_BuildTarget == BuildTarget.WSAPlayer)
            {
                PlayerSettings.SetScriptingBackend(_buildTargetGroup, ScriptingImplementation.IL2CPP);
            }
            else if (_BuildTarget == BuildTarget.StandaloneLinux64)
            {
                PlayerSettings.SetScriptingBackend(_buildTargetGroup, ScriptingImplementation.IL2CPP);
            }
            else if (_BuildTarget == BuildTarget.StandaloneOSX)
            {
                PlayerSettings.SetScriptingBackend(_buildTargetGroup, ScriptingImplementation.Mono2x);
            }
            else if (_BuildTarget == BuildTarget.StandaloneWindows64)
            {
                PlayerSettings.SetScriptingBackend(_buildTargetGroup, ScriptingImplementation.IL2CPP);
            }
        }
        
        private static void SetBuildInfo(string _BuildInfo, BuildTarget _BuildTarget, string _Path, bool _Reset = false)
        {
            var _text = string.Empty;
            
            if (!_Reset)
            {
                _text = string.Concat(_BuildInfo, Environment.NewLine, _BuildTarget, Environment.NewLine, _Path, Environment.NewLine, DateTime.Now);
            }
            
            File.WriteAllText(BuildInfoPath, _text);
        }

        /// <summary>
        /// Gets the contents of BUILDINFO.txt
        /// </summary>
        /// <param name="_BuildInfo">Can be <see cref="DEVELOPMENT_BUILD"/> or <see cref="RELEASE_BUILD"/></param>
        /// <param name="_BuildTarget"><see cref="BuildTarget"/></param>
        /// <param name="_Path">The path to the executable</param>
        /// <param name="_CheckTimestamp">Skips the timestamp check if set to false</param>
        /// <param name="_MessageSender">Optional indicator, where this message was called from</param>
        /// <returns>True when the BUILDINFO.txt contains any content and the timestamp is not under <see cref="BUILD_INFO_THRESHOLD_IN_SECONDS"/></returns>
        private static bool GetBuildInfo(out string _BuildInfo, out BuildTarget _BuildTarget, out string _Path, bool _CheckTimestamp, string _MessageSender = "")
        {
            var _text = File.ReadAllText(BuildInfoPath);
            
            _BuildInfo = string.Empty;
            _BuildTarget = BuildTarget.NoTarget;
            _Path = string.Empty;

            if (string.IsNullOrWhiteSpace(_text))
            {
                return false;
            }
            
            var _buildInfos = _text.Split(Environment.NewLine);
            _BuildInfo = _buildInfos[0];
            _BuildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), _buildInfos[1]);
            _Path = _buildInfos[2];
            var _timeStamp = DateTime.Parse(_buildInfos[3]);

            if (_CheckTimestamp)
            {
                Debug.Log($"[{_MessageSender}] Seconds since last timestamp: {DateTime.Now.Subtract(_timeStamp).Seconds}");
                
                // Safety check, so the build won't start on a random recompile, when the "BUILDINFO.txt" wasn't emptied due to an error
                if (DateTime.Now.Subtract(TimeSpan.FromSeconds(BUILD_INFO_THRESHOLD_IN_SECONDS)) > _timeStamp)
                {
                    Debug.LogWarning($"Skipping build!\nTimestamp set to long ago: {_timeStamp}");
                    SetBuildInfo(string.Empty, BuildTarget.NoTarget, BuildInfoPath, true);
                    return false;
                }   
            }

            return true;
        }
        
        /// <summary>
        /// Adds or removes the <see cref="DEVELOPMENT_BUILD"/> scripting define symbol
        /// </summary>
        /// <param name="_BuildInfo">Use <see cref="DEVELOPMENT_BUILD"/> or <see cref="RELEASE_BUILD"/></param>
        private static void SetScriptingDefineSymbols(string _BuildInfo)
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WSAPlayer)
            {
                EditorUtility.RequestScriptReload();
                return;
            }
            
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
        private static void OnScriptsRecompiled()
        {
            if (GetBuildInfo(out var _buildInfo, out var _buildTarget, out var _path, true, nameof(OnScriptsRecompiled)))
            {
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
        }
        
        private static void CleanUp(string _InstallDirectory, string _Platform)
        {
            foreach (var _path in Directory.GetFileSystemEntries(_InstallDirectory))
            {
                if (_path.Contains(BURST_DEBUG_INFORMATION) ||_path.Contains(BACKUP_THIS_FOLDER))
                {
                    Directory.Delete(_path, true);
                }
            }

            // Unity is still not completely done at this point, so need to wait a little before creating the .zip file 
            Task.Delay(TASK_DELAY);
            
            var _buildFolder = Directory.GetParent(_InstallDirectory)!.Parent!.FullName;
            var _zipFileName = string.Concat(GetApplicationName(false), " ", _Platform, ".zip");
            var _destinationArchiveFileName = Path.Combine(_buildFolder, _zipFileName);
            
            if (File.Exists(_destinationArchiveFileName))
            {
                File.Delete(_destinationArchiveFileName);
            }
            ZipFile.CreateFromDirectory(_InstallDirectory, _destinationArchiveFileName, CompressionLevel.Optimal, true, Encoding.UTF8);
        }
        #endregion
    }
}