using System;
using System.IO;
using UnityEngine;

namespace Watermelon_Game
{
    internal static class ExceptionLogger
    {
        #region Constants
        private const string DIRECTORY_NAME = "Error Logs";
        private const string SEPARATOR = "----------------------------------------------------------------------------";
        #endregion
        
        #region Fields
        private static readonly string filePath;
        private static readonly FileStream fileStream;
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

                _directoryPath = Path.Combine(_directoryPath, DIRECTORY_NAME);
                Directory.CreateDirectory(_directoryPath);

                var _timeStamp = DateTime.Now.ToString(@"dd.MM.yyyy hh\hmm\mss\s");
                var _fileName = string.Concat(_timeStamp, ".txt");
            
                filePath = Path.Combine(_directoryPath, _fileName);
                fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                streamWriter = new StreamWriter(fileStream);
            }
            catch { /* Ignored */ }
        }
        #endregion
        
        #region Methods
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
            Application.quitting += OnApplicationQuit;
        }

        private static async void OnLogMessageReceived(string _Condition, string _Stacktrace, LogType _Type)
        {
#if UNITY_EDITOR
            return;
#endif
#pragma warning disable CS0162
            try
            {
                var _message = string.Concat(_Type, Environment.NewLine, _Condition, Environment.NewLine, _Stacktrace, SEPARATOR, Environment.NewLine);

                await streamWriter.WriteAsync(_message);
                await streamWriter.FlushAsync();
            }
            catch { /* Ignored */ }
#pragma warning restore CS0162
        }
        
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