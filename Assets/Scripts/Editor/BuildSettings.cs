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
using Watermelon_Game.Development;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Watermelon_Game.Editor
{
    /// <summary>
    /// Contains settings for builds that are started through the <see cref="Shortcuts"/> class
    /// </summary>
    internal sealed class BuildSettings : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        #region Constants
        /// <summary>
        /// Validity duration in seconds for entries in the <see cref="BUILD_INFO"/> file
        /// </summary>
        private const uint BUILD_INFO_THRESHOLD_IN_SECONDS = 120;
        /// <summary>
        /// Name + extension for the BUILD_INFO .txt file
        /// </summary>
        private const string BUILD_INFO = "BUILD_INFO.txt";
        /// <summary>
        /// Indicates a development build
        /// </summary>
        private const string DEVELOPMENT_BUILD = "DEVELOPMENT_BUILD";
        /// <summary>
        /// Indicates a release build
        /// </summary>
        private const string RELEASE_BUILD = "RELEASE_BUILD";

        /// <summary>
        /// For debug builds
        /// </summary>
        private const string DEBUG = "Debug";
        /// <summary>
        /// For windows builds
        /// </summary>
        private const string WINDOWS = "Windows";
        /// <summary>
        /// For linux builds
        /// </summary>
        private const string LINUX = "Linux";
        /// <summary>
        /// For mac builds
        /// </summary>
        private const string MAC = "Mac";
        /// <summary>
        /// For universal windows platform builds
        /// </summary>
        private const string UWP = "UWP";

        /// <summary>
        /// After the build is done, folder names that contains this string will be deleted
        /// </summary>
        private const string BURST_DEBUG_INFORMATION = "_BurstDebugInformation_DoNotShip";
        /// <summary>
        /// After the build is done, folder names that contains this string will be deleted
        /// </summary>
        private const string BACKUP_THIS_FOLDER = "_BackUpThisFolder_ButDontShipItWithYourGame";

        /// <summary>
        /// 1 second delay
        /// </summary>
        private const int TASK_DELAY = 1000;
        #endregion
        
        #region Properties
        public int callbackOrder { get; }
        
        /// <summary>
        /// Path to <see cref="BUILD_INFO"/>
        /// </summary>
        private static string BuildInfoPath { get; } = Path.Combine(Application.dataPath, BUILD_INFO);
        /// <summary>
        /// Path to <see cref="DevelopmentTools.DEVELOPMENT_VERSION"/>
        /// </summary>
        private static string DevelopmentVersionPath { get; } = Path.Combine(Application.dataPath, DevelopmentTools.DEVELOPMENT_VERSION);
        #endregion

        #region Methods
        public void OnPreprocessBuild(BuildReport _Report)
        {
            var _activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            var _buildTargetGroup = BuildPipeline.GetBuildTargetGroup(_activeBuildTarget);
            var _reportedBuildTarget = _Report.summary.platform;
            // TODO: Check if this is needed (Maybe for Android builds)
            //PlayerSettings.SetArchitecture(_buildTargetGroup, 2);
            
            if (_reportedBuildTarget == BuildTarget.StandaloneOSX)
            {
                UnityEditor.OSXStandalone.UserBuildSettings.architecture = OSArchitecture.x64ARM64;
            }
            else if (_reportedBuildTarget == BuildTarget.WSAPlayer)
            {
                EditorUserBuildSettings.wsaArchitecture = "x64";
                EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.D3D;
                EditorUserBuildSettings.wsaUWPSDK = GetLatestInstalledWSAVersion();
                EditorUserBuildSettings.wsaMinUWPSDK = "10.0.10240.0";
                EditorUserBuildSettings.wsaMinUWPSDK = "Visual Studio 2022"; // TODO: Check if this info can be gotten automatically from somewhere
                EditorUserBuildSettings.wsaBuildAndRunDeployTarget = WSABuildAndRunDeployTarget.LocalMachine;
                UnityEditor.UWP.UserBuildSettings.buildConfiguration = WSABuildType.Master;
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
        
        /// <summary>
        /// Gets the latest WSA version from "C:\Program Files (x86)\Windows Kits\10\SDKManifest.xml"
        /// </summary>
        /// <returns>The latest installed WSA version number</returns>
        /// <exception cref="FormatException">When the SDKManifest.xml doesn't have the expected format</exception>
        /// <exception cref="FileNotFoundException">When the SDKManifest.xml couldn't be found</exception>
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
            
            // Method is called somewhere at the end of the build, not exactly when the build has finished, so need to wait for the previous build to completely finish
            await Task.Delay(TASK_DELAY);
            
#if DEVELOPMENT_BUILD
            Finalize(_Report, BuildTarget.StandaloneWindows64, DEBUG, null, string.Empty);
            return;
#endif
#pragma warning disable CS0162
            Finalize(_Report, BuildTarget.StandaloneWindows64, WINDOWS, BuildTarget.StandaloneLinux64, LINUX);
            Finalize(_Report, BuildTarget.StandaloneLinux64, LINUX, BuildTarget.StandaloneOSX, MAC);
            Finalize(_Report, BuildTarget.StandaloneOSX, MAC, BuildTarget.WSAPlayer, UWP);
            Finalize(_Report, BuildTarget.WSAPlayer, UWP, null, string.Empty);
#pragma warning restore CS0162
        }

        /// <summary>
        /// Finalizes the last build and optionally starts the next
        /// </summary>
        /// <param name="_Report">The last <see cref="BuildReport"/></param>
        /// <param name="_CurrentBuildTarget">The <see cref="BuildTarget"/> to start the build for</param>
        /// <param name="_CurrentOS">The OS to start the build for</param>
        /// <param name="_NextBuildTarget">The next <see cref="BuildTarget"/> (Otherwise null)</param>
        /// <param name="_NextOS">The next OS (Otherwise empty)</param>
        private static void Finalize(BuildReport _Report, BuildTarget _CurrentBuildTarget, string _CurrentOS, BuildTarget? _NextBuildTarget, string _NextOS)
        {
            if (_Report.summary.platform == _CurrentBuildTarget)
            {
                var _outputPath = _Report.summary.outputPath;
                Debug.Log($@"<b><color=green>{_CurrentOS} Build finished</color></b> [<b>{_Report.summary.totalTime:hh\:mm\:ss}</b>] -> <i>{_outputPath}</i>");
                var _buildsFolder = Directory.GetParent(_outputPath)!.Parent!.Parent!.FullName;
                var _installPath = CreateInstallPath(_buildsFolder, _CurrentOS, true);
                CleanUp(_installPath, _CurrentOS);
                
                if (_NextBuildTarget != null)
                {
                    _installPath = CreateInstallPath(_buildsFolder, _NextOS, false);
                    CreatePlatformFolder(_buildsFolder, _NextBuildTarget.Value);
                    BuildPlayer(_installPath, _NextBuildTarget.Value);   
                }
                else
                {
                    if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
                    {
                        var _buildTargetGroup = BuildPipeline.GetBuildTargetGroup(BuildTarget.StandaloneWindows64);
                        EditorUserBuildSettings.SwitchActiveBuildTarget(_buildTargetGroup, BuildTarget.StandaloneWindows64);   
                    }
                }

                if (_CurrentOS == DEBUG)
                {
                    var _gameDataFolder = string.Concat(GetApplicationName(false), "_Data");
                    var _gameDataPath = Path.Combine(Directory.GetParent(_outputPath)!.FullName, _gameDataFolder, DevelopmentTools.DEVELOPMENT_VERSION);
                    
                    File.Copy(DevelopmentVersionPath, _gameDataPath);
                }
            }
        }
        
        /// <summary>
        /// Creates the necessary folders for the debug build
        /// </summary>
        /// <param name="_Directory">Root folder to create all necessary folders in</param>
        /// <returns>File path where the .exe file will be at</returns>
        public static string CreateDebugFolder(string _Directory)
        {
            var _baseDirectoryName = GetApplicationName(false);
            var _debug = Path.Combine(_Directory, DEBUG, _baseDirectoryName);

            Directory.CreateDirectory(_debug);
            DeleteAllFilesInDirectory(_debug);
            
            return Path.Combine(_debug, GetApplicationName(true));
        }
        
        /// <summary>
        /// Creates all necessary folders (empties them if they already exist) to build the release build for the given <see cref="BuildTarget"/>
        /// </summary>
        /// <param name="_Directory">Root folder to create all necessary folders in</param>
        /// <param name="_BuildTarget">The <see cref="BuildTarget"/> to create the folders for</param>
        /// <returns>File path where the .exe file will be at</returns>
        /// <exception cref="ArgumentException">When a <see cref="BuildTarget"/> is used that is not supported yet</exception>
        public static string CreatePlatformFolder(string _Directory, BuildTarget _BuildTarget)
        {
            var _baseDirectoryName = GetApplicationName(false);
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            var _path = _BuildTarget switch
            {
                BuildTarget.StandaloneWindows64 => Path.Combine(_Directory, WINDOWS, _baseDirectoryName),
                BuildTarget.StandaloneLinux64 => Path.Combine(_Directory, LINUX, _baseDirectoryName),
                BuildTarget.StandaloneOSX => Path.Combine(_Directory, MAC, _baseDirectoryName),
                BuildTarget.WSAPlayer => Path.Combine(_Directory, UWP, _baseDirectoryName),
                _ => throw new ArgumentException($"The passed {nameof(BuildTarget)} {_BuildTarget} can currently not be used")
            };

            Directory.CreateDirectory(_path);
            DeleteAllFilesInDirectory(_path);

            var _applicationName = GetApplicationName(true);

            return Path.Combine(_path, _applicationName);
        }
        
        /// <summary>
        /// Deletes all files and folders under the given directory path (Includes sub files and folders)
        /// </summary>
        /// <param name="_Directory">The directory path to delete everything in</param>
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
        
        /// <summary>
        /// Returns the <see cref="Application"/>.<see cref="Application.productName"/>
        /// </summary>
        /// <param name="_WithExtension">Includes the file extension if true</param>
        /// <returns>The application name + optional file extension</returns>
        private static string GetApplicationName(bool _WithExtension)
        {
            return string.Concat(Application.productName, _WithExtension ? ".exe" : "");
        }
        
        /// <summary>
        /// Creates the path, the executable will be stored at
        /// </summary>
        /// <param name="_BuildsFolder">Root folder</param>
        /// <param name="_AddDirectory">Folder after root</param>
        /// <param name="_OnlyFolder">If false, adds <see cref="GetApplicationName"/></param>
        /// <returns>The path, the executable will be stored at</returns>
        private static string CreateInstallPath(string _BuildsFolder, string _AddDirectory, bool _OnlyFolder)
        {
            var _baseDirectory = GetApplicationName(false);
            var _fileExtension = _OnlyFolder ? "" : GetApplicationName(true);
            
            return Path.Combine(_BuildsFolder, _AddDirectory, _baseDirectory, _fileExtension);
        }
        
        /// <summary>
        /// Starts a <see cref="DEVELOPMENT_BUILD"/>
        /// </summary>
        /// <param name="_Path">Path to the executable</param>
        public static void BuildPlayer(string _Path)
        {
            SwitchPlatform(BuildTarget.StandaloneWindows64);
            SetBuildInfo(DEVELOPMENT_BUILD, BuildTarget.StandaloneWindows64, _Path);
            SetDevelopmentVersion();
            SetScriptingDefineSymbols(DEVELOPMENT_BUILD);
        }
        
        /// <summary>
        /// Starts a <see cref="RELEASE_BUILD"/>
        /// </summary>
        /// <param name="_Path">Path to the executable</param>
        /// <param name="_BuildTarget"><see cref="BuildTarget"/> to start the build for</param>
        public static void BuildPlayer(string _Path, BuildTarget _BuildTarget)
        {
            SwitchPlatform(_BuildTarget);
            SetBuildInfo(RELEASE_BUILD, _BuildTarget, _Path);
            SetScriptingDefineSymbols(RELEASE_BUILD);
        }

        /// <summary>
        /// Switches the active platform to the given <see cref="BuildTarget"/> 
        /// </summary>
        /// <param name="_BuildTarget">The <see cref="BuildTarget"/> to switch the platform to</param>
        private static void SwitchPlatform(BuildTarget _BuildTarget)
        {
            var _buildTargetGroup = BuildPipeline.GetBuildTargetGroup(_BuildTarget);
            EditorUserBuildSettings.SwitchActiveBuildTarget(_buildTargetGroup, _BuildTarget);
            
            if (_BuildTarget == BuildTarget.StandaloneWindows64)
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
            else if (_BuildTarget == BuildTarget.WSAPlayer)
            {
                PlayerSettings.SetScriptingBackend(_buildTargetGroup, ScriptingImplementation.IL2CPP);
            }
        }
        
        /// <summary>
        /// Sets the contents of <see cref="BUILD_INFO"/> at <see cref="BuildInfoPath"/>
        /// </summary>
        /// <param name="_BuildInfo">Should be <see cref="DEVELOPMENT_BUILD"/> or <see cref="RELEASE_BUILD"/></param>
        /// <param name="_BuildTarget"><see cref="BuildTarget"/></param>
        /// <param name="_Path">Path to the executable</param>
        /// <param name="_Reset">Empties the <see cref="BUILD_INFO"/> if true</param>
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
        /// Increments the build number in <see cref="DevelopmentTools.DEVELOPMENT_VERSION"/> by 1
        /// </summary>
        private static void SetDevelopmentVersion()
        {
            var _currentDevelopmentVersion = ulong.Parse(File.ReadAllText(DevelopmentVersionPath));
            File.WriteAllText(DevelopmentVersionPath, (++_currentDevelopmentVersion).ToString());
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
        
        /// <summary>
        /// Starts a <see cref="DEVELOPMENT_BUILD"/>/<see cref="RELEASE_BUILD"/> after a recompile, if the <see cref="BUILD_INFO"/> is not empty and contains valid information
        /// </summary>
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
        
        /// <summary>
        /// Deletes unnecessary folders and creates a .zip folder for every release build
        /// </summary>
        /// <param name="_InstallDirectory">The path, the executable is stored at</param>
        /// <param name="_Platform">The platform of the build</param>
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