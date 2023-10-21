using UnityEngine;
using UnityEngine.UI;

namespace Watermelon_Game
{
    internal sealed class AudioController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private GameObject bgmDisabledIcon;
        [SerializeField] private Slider slider;
        #endregion

        #region Constants
        private const string BGM = "BGM";
        private const string VOLUME = "Volume";
        #endregion
        
        #region Fields
        private AudioSource bgm;
        private bool isMuted;
        #endregion
        
        #region Methods

        private void Awake()
        {
            this.bgm = Camera.main!.GetComponent<AudioSource>();
            // Without this the BGM will still play even when it is turned of on start
            // ReSharper disable once Unity.InefficientPropertyAccess
            this.bgm.enabled = false; this.bgm.enabled = true;
        }

        private void Start()
        {
            this.LoadSettings();
        }

        private void OnApplicationQuit()
        {
            this.SaveSettings();
        }

        public void Mute(bool _IsSetFromButton)
        {
            if (_IsSetFromButton)
            {
                this.isMuted = !this.isMuted;
            }

            this.bgmDisabledIcon.SetActive(this.isMuted);  
            
            if (this.isMuted)
            {
                this.bgm.Pause();
            }
            else
            {
                this.bgm.Play();
            }
        }

        public void SetVolume()
        {
            AudioListener.volume = this.slider.value;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetString(BGM, this.isMuted.ToString());
            PlayerPrefs.SetFloat(VOLUME, this.slider.value);
        }

        private void LoadSettings()
        {
            var _bgm = PlayerPrefs.GetString(BGM, "false");
            var _volume = PlayerPrefs.GetFloat(VOLUME, .5f);

            this.isMuted = bool.Parse(_bgm);
            this.Mute(false);
            this.slider.value = _volume;
        }
        #endregion
    }
}