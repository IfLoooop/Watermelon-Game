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
        /// Index of the <see cref="AudioWrapper"/> in <see cref="AudioPool.assignedAudioWrappers"/>, for the <see cref="AudioClipName.Bgm"/> <see cref="AudioClip"/>
        /// </summary>
        private int bgmIndex;
        /// <summary>
        /// Indicates whether the BGM is currently muted or not
        /// </summary>
        private bool isMuted;
        #endregion
        
        #region Methods
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
        /// <param name="_IsSetFromButton">Must be true, when the method is called by clicking on the button</param>
        public void FlipMuteState(bool _IsSetFromButton)
        {
            if (_IsSetFromButton)
            {
                this.isMuted = !this.isMuted;
            }

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
            var _bgm = PlayerPrefs.GetString(BGM);
            var _volume = PlayerPrefs.GetFloat(VOLUME);

            this.isMuted = bool.Parse(_bgm);
            this.FlipMuteState(false);
            this.slider.value = _volume;
        }
        #endregion
    }
}