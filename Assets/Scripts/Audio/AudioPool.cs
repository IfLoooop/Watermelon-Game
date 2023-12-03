using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Audio
{
    /// <summary>
    /// Contains methods to play pooled <see cref="AudioSource"/>s
    /// </summary>
    internal sealed class AudioPool : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to a AudioClips ScriptableObject")]
        [SerializeField] private AudioClips audioClips;
        [Tooltip("A GameObject that contains an AudioSource Component")]
        [SerializeField] private AudioWrapper audioWrapperPrefab;
        
        [Header("Settings")] 
        [Tooltip("How many GameObjects to instantiate on Awake")]
        [SerializeField] private uint startAmount = 1;
        [Tooltip("Multiplies this value to the normal volume of other clients")]
        [SerializeField] private float volumeReductionMultiplier = .25f;
        [Tooltip("Contains the GameObjects that play the AudioClip")]
        [SerializeField] private ObjectPool<AudioWrapper> audioPool;
        #endregion

        #region Fields
        /// <summary>
        /// Singleton of <see cref="AudioPool"/>
        /// </summary>
        private static AudioPool instance;
        /// <summary>
        /// <see cref="AudioWrapper"/> that are not part of <see cref="audioPool"/>, but assigned to one specific <see cref="AudioClip"/>
        /// </summary>
        private readonly List<AudioWrapper> assignedAudioWrappers = new();
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
            this.audioPool = new ObjectPool<AudioWrapper>(this.audioWrapperPrefab, this.transform, this.startAmount, true);
            this.audioClips.Init();
        }
        
        /// <summary>
        /// Creates a new <see cref="AudioWrapper"/>, adds it to <see cref="assignedAudioWrappers"/> and returns the index of it
        /// </summary>
        /// <param name="_AudioClipName"><see cref="AudioClipName"/></param>
        /// <param name="_Parent">The parent <see cref="Transform"/> of the created <see cref="AudioWrapper"/></param>
        /// <param name="_Loop">Whether the <see cref="AudioClip"/> should play in a loop or not</param>
        /// <returns>The index of the created <see cref="AudioWrapper"/> in <see cref="assignedAudioWrappers"/></returns>
        public static int CreateAssignedAudioWrapper(AudioClipName _AudioClipName, Transform _Parent, bool _Loop = false)
        {
            var _audioWrapper = instance.audioPool.Get(_Parent);
            var _audioClipSettings = AudioClips.Clips[_AudioClipName];
            
            Set(_audioWrapper, _audioClipSettings, true, _Loop);
            
            instance.assignedAudioWrappers.Add(_audioWrapper);
            return instance.assignedAudioWrappers.Count - 1;
        }
        
        /// <summary>
        /// Checks whether the <see cref="AudioWrapper"/> with the given index in <see cref="assignedAudioWrappers"/> is currently playing 
        /// </summary>
        /// <param name="_Index">Index of the <see cref="AudioWrapper"/> in <see cref="assignedAudioWrappers"/> to use</param>
        /// <returns>True if it is currently playing, otherwise false</returns>
        public static bool IsAssignedClipPlaying(int _Index)
        {
            return instance.assignedAudioWrappers[_Index].AudioSource.isPlaying;
        }
        
        /// <summary>
        /// Plays the <see cref="AudioClip"/> of the <see cref="AudioSource"/> in the <see cref="AudioWrapper"/> with the given index in <see cref="assignedAudioWrappers"/>
        /// </summary>
        /// <param name="_Index">Index of the <see cref="AudioWrapper"/> in <see cref="assignedAudioWrappers"/> to use</param>
        public static void PlayAssignedClip(int _Index)
        {
            instance.assignedAudioWrappers[_Index].AudioSource.Play();
        }
        
        /// <summary>
        /// Pauses the <see cref="AudioClip"/> of the <see cref="AudioSource"/> in the <see cref="AudioWrapper"/> with the given index in <see cref="assignedAudioWrappers"/>
        /// </summary>
        /// <param name="_Index">Index of the <see cref="AudioWrapper"/> in <see cref="assignedAudioWrappers"/> to use</param>
        public static void PauseAssignedClip(int _Index)
        {
            instance.assignedAudioWrappers[_Index].AudioSource.Pause();
        }
        
        /// <summary>
        /// Stops the <see cref="AudioClip"/> of the <see cref="AudioSource"/> in the <see cref="AudioWrapper"/> with the given index in <see cref="assignedAudioWrappers"/>
        /// </summary>
        /// <param name="_Index">Index of the <see cref="AudioWrapper"/> in <see cref="assignedAudioWrappers"/> to use</param>
        public static void StopAssignedClip(int _Index)
        {
            instance.assignedAudioWrappers[_Index].AudioSource.Stop();
        }
        
        /// <summary>
        /// Plays the <see cref="AudioClip"/> in <see cref="AudioClips.Clips"/> with the given <see cref="AudioClipName"/>
        /// </summary>
        /// <param name="_AudioClipName"><see cref="AudioClipName"/></param>
        /// <param name="_Parent">If the lifetime of the <see cref="AudioClip"/> is dependant on the lifetime of a specific <see cref="GameObject"/>, set the <see cref="Transform"/> of that <see cref="GameObject"/> as the <see cref="_Parent"/></param>
        public static void PlayClip(AudioClipName _AudioClipName, [CanBeNull] Transform _Parent = null)
        {
            var _audioWrapper = Init(_Parent, _AudioClipName, out var _audioClipSettings, out var _waitTime);
            
            Set(_audioWrapper, _audioClipSettings);
            Play(_audioWrapper, _waitTime);
        }

        /// <summary>
        /// Plays the <see cref="AudioClip"/> in <see cref="AudioClips.Clips"/> with the given <see cref="AudioClipName"/>
        /// </summary>
        /// <param name="_AudioClipName"><see cref="AudioClipName"/></param>
        /// <param name="_NormalVolume">If false, plays the clip at half volume</param>
        public static void PlayClip(AudioClipName _AudioClipName, bool _NormalVolume)
        {
            var _audioWrapper = Init(null, _AudioClipName, out var _audioClipSettings, out var _waitTime);
            
            Set(_audioWrapper, _audioClipSettings, _NormalVolume);
            Play(_audioWrapper, _waitTime);
        }

        /// <summary>
        /// Initializes all needed value for the <see cref="AudioWrapper"/>
        /// </summary>
        /// <param name="_Parent">If the lifetime of the <see cref="AudioClip"/> is dependant on the lifetime of a specific <see cref="GameObject"/>, set the <see cref="Transform"/> of that <see cref="GameObject"/> as the <see cref="_Parent"/></param>
        /// <param name="_AudioClipName"><see cref="AudioClipName"/></param>
        /// <param name="_AudioClipSettings"><see cref="AudioClipSettings"/></param>
        /// <param name="_WaitTime">Time in seconds after which the <see cref="AudioWrapper"/> should return to its pool</param>
        /// <returns></returns>
        private static AudioWrapper Init([CanBeNull] Transform _Parent, AudioClipName _AudioClipName, out AudioClipSettings _AudioClipSettings, out float _WaitTime)
        {
            _AudioClipSettings = AudioClips.Clips[_AudioClipName];
            _WaitTime = _AudioClipSettings.audioClip.length;

            return instance.audioPool.Get(_Parent);
        }
        
        /// <summary>
        /// Sets the values of the <see cref="AudioSource"/> in <see cref="AudioWrapper"/> from <see cref="AudioClipSettings"/>
        /// </summary>
        /// <param name="_AudioWrapper">The <see cref="AudioWrapper"/>to set the value in</param>
        /// <param name="_AudioClipSettings">The <see cref="AudioClipSettings"/> to take the values from</param>
        /// <param name="_NormalVolume">If false, uses <see cref="volumeReductionMultiplier"/> on the volume</param>
        /// <param name="_Loop">Whether the <see cref="AudioClip"/> should play in a loop or not</param>
        private static void Set(AudioWrapper _AudioWrapper, AudioClipSettings _AudioClipSettings, bool _NormalVolume = true, bool _Loop = false)
        {
            _AudioWrapper.AudioSource.clip = _AudioClipSettings.audioClip;
            _AudioWrapper.AudioSource.volume = _NormalVolume == false ? _AudioClipSettings.volume * instance.volumeReductionMultiplier : _AudioClipSettings.volume;
            _AudioWrapper.AudioSource.time = _AudioClipSettings.startTime;
            _AudioWrapper.AudioSource.loop = _Loop;
        }

        /// <summary>
        /// Plays the clip and invokes <see cref="AudioWrapper.ReturnToPool"/> after the given <see cref="_WaitTime"/>
        /// </summary>
        /// <param name="_AudioWrapper"><see cref="AudioWrapper"/></param>
        /// <param name="_WaitTime">Time in seconds after which the given <see cref="_AudioWrapper"/> should return to its pool</param>
        private static void Play(AudioWrapper _AudioWrapper, float _WaitTime)
        {
            _AudioWrapper.AudioSource.Play();
            _AudioWrapper.Invoke(nameof(_AudioWrapper.ReturnToPool), _WaitTime);
        }
        
        /// <summary>
        /// Returns the given <see cref="AudioWrapper"/> back to <see cref="audioPool"/>
        /// </summary>
        /// <param name="_AudioWrapper">The <see cref="AudioWrapper"/> to return to <see cref="audioPool"/></param>
        public static void ReturnToPool(AudioWrapper _AudioWrapper)
        {
            instance.audioPool.Return(_AudioWrapper);
        }

        /// <summary>
        /// Removes the given <see cref="AudioWrapper"/> from <see cref="ObjectPool{T}.objectPool"/>
        /// </summary>
        /// <param name="_AudioWrapper">The <see cref="AudioWrapper"/> to remove from <see cref="ObjectPool{T}.objectPool"/></param>
        public static void RemovePoolObject(AudioWrapper _AudioWrapper)
        {
            instance.audioPool.Remove(_AudioWrapper);
        }
        #endregion
    }
}