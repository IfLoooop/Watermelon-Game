using UnityEngine;
using UnityEngine.UI;

namespace Watermelon_Game.Audio
{
    /// <summary>
    /// Controls all audio in game
    /// </summary>
    internal sealed class AudioSettings : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the disabled icon")]
        [SerializeField] private GameObject bgmDisabledIcon;
        [Tooltip("Reference to the Slider")]
        [SerializeField] private Slider slider;
        #endregion

        #region Constants
        /// <summary>
        /// <see cref="PlayerPrefs"/> for <see cref="isMuted"/>
        /// </summary>
        private const string BGM = "BGM";
        /// <summary>
        /// <see cref="PlayerPrefs"/> key for the volume
        /// </summary>
        private const string VOLUME = "Volume";
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="AudioSettings"/>
        /// </summary>
        private static AudioSettings instance;
        
        /// <summary>
        /// Index of the <see cref="AudioWrapper"/> in <see cref="AudioPool.assignedAudioWrappers"/>, for the <see cref="AudioClipName.Bgm"/> <see cref="AudioClip"/>
        /// </summary>
        private int bgmIndex;
        /// <summary>
        /// Indicates whether the BGM is currently muted or not
        /// </summary>
        private bool isMuted;
        #endregion
        
        #region Methods

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            bgmIndex = AudioPool.CreateAssignedAudioWrapper(AudioClipName.Bgm, base.transform, true);
            
            this.LoadSettings();
        }

        private void OnApplicationQuit()
        {
            this.SaveSettings();
        }
        
        /// <summary>
        /// Flips the current state of <see cref="isMuted"/>
        /// </summary>
        public static void FlipMuteState() // bool _IsSetFromButton = true
        {
            instance.isMuted = !instance.isMuted;
            instance.SetBGM();
        }

        /// <summary>
        /// Enables/disables the BGM, depending on the value of <see cref="isMuted"/>
        /// </summary>
        private void SetBGM()
        {
            this.bgmDisabledIcon.SetActive(this.isMuted);
            
            if (this.isMuted)
            {
                AudioPool.PauseAssignedClip(this.bgmIndex);
            }
            else
            {
                AudioPool.PlayAssignedClip(this.bgmIndex);
            }
        }

#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// FLips the BGM on and off, without enabling <see cref="bgmDisabledIcon"/> <br/>
        /// <b>Development only!</b>
        /// </summary>
        public static void FlipBGM_DEVELOPMENT()
        {
            instance.isMuted = !instance.isMuted;
            
            if (instance.isMuted)
            {
                AudioPool.PauseAssignedClip(instance.bgmIndex);
            }
            else
            {
                AudioPool.PlayAssignedClip(instance.bgmIndex);
            }
        }
#endif
        
        /// <summary>
        /// Sets the volume of the global <see cref="AudioListener"/> to the <see cref="Slider.value"/> of the <see cref="slider"/>
        /// </summary>
        public void SetVolume()
        {
            AudioListener.volume = this.slider.value;
        }

        /// <summary>
        /// Saves the settings using <see cref="PlayerPrefs"/>
        /// </summary>
        private void SaveSettings()
        {
            PlayerPrefs.SetString(BGM, this.isMuted.ToString());
            PlayerPrefs.SetFloat(VOLUME, this.slider.value);
        }

        /// <summary>
        /// Loads the settings using <see cref="PlayerPrefs"/>
        /// </summary>
        private void LoadSettings()
        {
            var _bgm = PlayerPrefs.GetString(BGM, false.ToString());
            var _volume = PlayerPrefs.GetFloat(VOLUME, .5f);
            
            this.isMuted = bool.Parse(_bgm);
            this.SetBGM();
            this.slider.value = _volume;
        }
        #endregion
    }
}