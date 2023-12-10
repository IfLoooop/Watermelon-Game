// ReSharper disable once CheckNamespace
namespace UnityEngine.Diagnostics
{
    /// <summary>
    /// Dummy <see cref="UnityEngine.Debug"/> class for cheat detection
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Debug
    {
        #region Methods
        /// <summary>
        /// Logs the given message with <see cref="UnityEngine.Debug"/>.<see cref="UnityEngine.Debug.LogError(object)"/> and calls <see cref="Watermelon_Game.Audio.AudioSettings.OnError"/>
        /// </summary>
        /// <param name="_Message">The message to print</param>
        public static void LogException(string _Message)
        {
            UnityEngine.Debug.LogError(_Message);
            Watermelon_Game.Audio.AudioSettings.OnError();
        }
        #endregion
    }
}