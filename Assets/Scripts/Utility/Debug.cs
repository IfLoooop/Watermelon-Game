using Watermelon_Game;

// ReSharper disable once CheckNamespace
namespace UnityEngine.Diagnostics
{
    /// <summary>
    /// Dummy <see cref="UnityEngine.Debug"/> class for cheat detection
    /// </summary>
    internal static class Debug
    {
        #region Methods
        /// <summary>
        /// Logs the given message with <see cref="UnityEngine.Debug"/>.<see cref="UnityEngine.Debug.LogError(object)"/> and calls <see cref="GameController.CheatDetected"/>
        /// </summary>
        /// <param name="_Message">The message to print</param>
        public static void LogException(string _Message)
        {
            UnityEngine.Debug.LogError(_Message);
            Watermelon_Game.Audio.AudioSettings.PlayErrorSound();
        }
        #endregion
    }
}