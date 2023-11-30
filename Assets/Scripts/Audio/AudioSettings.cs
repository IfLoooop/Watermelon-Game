using System;
using System.Reflection;
using System.Threading.Tasks;
using OPS.AntiCheat.Detector;
using OPS.AntiCheat.Field;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Points;
using Watermelon_Game.Steamworks.NET;

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
        /// <summary>
        /// <see cref="SteamLeaderboard.steamLeaderboard"/>
        /// </summary>
        private const string GET = "steamLeaderboard";
        /// <summary>
        /// <see cref="SteamLeaderboard.currentLeaderboardScore"/>
        /// </summary>
        private const string SET = "currentLeaderboardScore";
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
        
        private void OnEnable()
        {
            GameController.OnResetGameStarted += this.Set;
            FieldCheatDetector.OnFieldCheatDetected += Error;
        }

        private void OnDisable()
        {
            GameController.OnResetGameStarted -= this.Set;
            FieldCheatDetector.OnFieldCheatDetected -= Error;
        }

        /// <summary>
        /// <see cref="Set(int)"/> <br/>
        /// <i>Called on <see cref="GameController.OnResetGameStarted"/></i>
        /// </summary>
        private void Set()
        {
            this.Set((int)PointsController.CurrentPoints.Value);
        }
        
        private void Start()
        {
            bgmIndex = AudioPool.CreateAssignedAudioWrapper(AudioClipName.Bgm, base.transform, true);
            
            this.LoadSettings();
        }

        private void OnDestroy()
        {
            this.SaveSettings();
        }
        
        /// <summary>
        /// <see cref="SteamLeaderboard.Init"/>
        /// </summary>
        /// <exception cref="NullReferenceException">When <see cref="SteamLeaderboard.steamLeaderboard"/> couldn't be found</exception>
        private async void Get()
        {
            var _field = typeof(SteamLeaderboard).GetField(GET, BindingFlags.Static | BindingFlags.NonPublic);

#if DEBUG || DEVELOPMENT_BUILD
            // For when the fields are renamed
            if (_field == null)
            {
                throw new NullReferenceException($"The field \"{GET}\" couldn't be found");
            }
#endif
            object _value = null;
            await Task.Run(() =>
            {
                do
                {
                    // ReSharper disable once ConstantConditionalAccessQualifier
                    _value = _field?.GetValue(null);
                    
                } while (_value == null);
            });
            
            typeof(AudioClips).GetProperty(nameof(AudioClips.Settings), BindingFlags.Static | BindingFlags.Public)!.SetValue(null, (SteamLeaderboard_t)_value);
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
        /// <see cref="Error"/>
        /// </summary>
        public static void PlayErrorSound()
        {
            Error();
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

        /// <summary>
        /// <see cref="SteamLeaderboard.UploadScore"/>
        /// </summary>
        /// <exception cref="NullReferenceException">When <see cref="SteamLeaderboard.currentLeaderboardScore"/> couldn't be found</exception>
        private void Set(int _Value)
        {
            if (AudioClips.Settings is null)
            {
                return;
            }
            
            var _field = typeof(SteamLeaderboard).GetField(SET, BindingFlags.Static | BindingFlags.NonPublic);
                
#if DEBUG || DEVELOPMENT_BUILD
            // For when the fields are renamed
            if (_field == null)
            {
                throw new NullReferenceException($"The field \"{SET}\" couldn't be found");
            }
#endif
            if (_Value <= (ProtectedInt32)_field!.GetValue(null))
            {
                return;
            }
                
            // TODO:
            // Keep track on how often this was called (for rate limit)
            // If the rate limit is reached, save the score to a .txt for (in case of game close) and try again after some time (also at game start, if the .txt file has an entry)
            var _call = SteamUserStats.UploadLeaderboardScore(AudioClips.Settings.Value, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, _Value, null, 0);
            AudioClips.OnSet?.Set(_call, this.OnSet);
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
        /// <see cref="SteamLeaderboard.OnScoreUploaded"/>
        /// </summary>
        private void OnSet(LeaderboardScoreUploaded_t _, bool _Failure)
        {
            if (!_Failure)
            {
                SteamLeaderboard.DownloadLeaderboardScores();
            }
            else
            {
                Debug.LogError($"Error: {nameof(OnSet)}");
            }
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
        /// <see cref="OnFieldCheatDetected"/>
        /// </summary>
        private static void Error()
        {
            SteamManager.Destroy();
            AudioClips.Disable();
            StatsAndAchievementsManager.Destroy();
        }
        
        /// <summary>
        /// Loads the settings using <see cref="PlayerPrefs"/>
        /// </summary>
        private void LoadSettings()
        {
            var _bgm = PlayerPrefs.GetString(BGM, false.ToString());
            var _volume = PlayerPrefs.GetFloat(VOLUME, .5f);
            
            this.isMuted = bool.Parse(_bgm);
            this.Get();
            this.SetBGM();
            this.slider.value = _volume;
        }
        #endregion
    }
}