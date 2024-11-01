using System;
using System.IO;
using UnityEngine;

namespace Watermelon_Game.Utility
{
    /// <summary>
    /// Logs debug messages in build to a .txt file
    /// </summary>
    internal static class ExceptionLogger
    {
        #region Constants
        /// <summary>
        /// Separator between each log entry
        /// </summary>
        private const string SEPARATOR = "----------------------------------------------------------------------------";
        #endregion
        
        #region Fields
        /// <summary>
        /// The file path of the error log .txt file
        /// </summary>
        private static readonly string filePath;
        /// <summary>
        /// <see cref="FileStream"/>
        /// </summary>
        private static readonly FileStream fileStream;
        /// <summary>
        /// <see cref="StreamWriter"/>
        /// </summary>
        private static readonly StreamWriter streamWriter;
        #endregion

        #region Constructor
        static ExceptionLogger()
        {
            try
            {
                // Will be the "Assets"-Folder in Editor
                var _directoryPath = Application.dataPath;
#if UNITY_EDITOR
                _directoryPath = Directory.GetParent(_directoryPath)!.FullName;
#endif
                filePath = Path.Combine(_directoryPath, "Error Log.txt");
                fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                streamWriter = new StreamWriter(fileStream);
            }
            catch { /* Ignored */ }
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Is called right after the static initialisation
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Subscribe()
        {
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
            Application.quitting += OnApplicationQuit;
        }

        /// <summary>
        /// Writes the message to the .txt file
        /// </summary>
        /// <param name="_Condition">The message</param>
        /// <param name="_Stacktrace">The stacktrace</param>
        /// <param name="_LogType"><see cref="LogType"/></param>
        private static async void OnLogMessageReceived(string _Condition, string _Stacktrace, LogType _LogType)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                return;
            }
#endif

#if (!DEBUG && !DEVELOPMENT_BUILD) || UNITY_EDITOR
            if (_LogType == LogType.Log)
            {
                return;
            }
#endif
            try
            {
                var _message = string.Concat(_LogType, Environment.NewLine, _Condition, Environment.NewLine, _Stacktrace, SEPARATOR, Environment.NewLine);

                // TODO: When Debug.Logs are called right after another, sometimes not all of them are written to the .txt file
                await streamWriter.WriteAsync(_message);
                await streamWriter.FlushAsync();
            }
            catch { /* Ignored */ }
        }
        
        /// <summary>
        /// Closes the <see cref="streamWriter"/> and deletes the file if nothing has been written to it
        /// </summary>
        private static void OnApplicationQuit()
        {
            try
            {
                var _fileIsEmpty = fileStream.Length == 0;
            
                streamWriter.Close();

                if (_fileIsEmpty)
                {
                    File.Delete(filePath);
                }
            }
            catch { /* Ignored */ }
        }
        #endregion
    }
}