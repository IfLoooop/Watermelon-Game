using System;
using UnityEngine;

namespace Watermelon_Game.Audio
{
    /// <summary>
    /// Holds the settings for a specific <see cref="AudioClip"/>
    /// </summary>
    [Serializable]
    internal sealed class AudioClipSettings
    {
        #region Fields
        [Tooltip("Each AudioClipName should only be used once")]
        public AudioClipName key;
        [Tooltip("Reference to the AudioClip")]
        public AudioClip audioClip;
        [Tooltip("The volume with which the AudioClip should be played")]
        public float volume = .05f;
        [Tooltip("Time in seconds, at what point in the AudioClip to start")]
        public float startTime;
        #endregion
    }
}