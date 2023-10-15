using JetBrains.Annotations;
using UnityEngine;

namespace Watermelon_Game.ExtensionMethods
{
    internal static class AudioSourceExtensions
    {
        #region Methods
        /// <summary>
        /// Sets <see cref="AudioSource.time"/> to the given <see cref="_StartTime"/> and plays this <see cref="AudioSource"/>
        /// </summary>
        /// <param name="_AudioSource">The <see cref="AudioSource"/> to play the <see cref="AudioSource.clip"/> of</param>
        /// <param name="_StartTime">The time in seconds to start the <see cref="AudioSource.clip"/> at</param>
        /// <param name="_AudioClip">Uses this <see cref="AudioClip"/> if not null</param>
        /// <param name="_Volume">Sets this value as the volume if not null</param>
        public static void Play(this AudioSource _AudioSource, float _StartTime, [CanBeNull] AudioClip _AudioClip = null, float? _Volume = null)
        {
            if (_AudioClip != null)
            {
                _AudioSource.clip = _AudioClip;
            }
            if (_Volume != null)
            {
                _AudioSource.volume = _Volume.Value;
            }
            
            _AudioSource.time = _StartTime;
            _AudioSource.Play();
        }
        #endregion
    }
}