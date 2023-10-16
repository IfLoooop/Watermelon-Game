using UnityEngine;

namespace Watermelon_Game.MaxHeight
{
    internal sealed class GodRayFlicker : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [SerializeField] private ParticleSystem godRay;
        [SerializeField] private AudioClip floodLight;
        [SerializeField] private AudioClip flickerSound;
        [Header("Settings")]
        [SerializeField] private float maxFlickerDuration = .25f;
        [SerializeField] private Vector2 flickerStep = new(.035f, .045f);
        [SerializeField] private float floodLightVolume = .05f;
        [SerializeField] private float flickerVolume = .375f;
        #endregion

        #region Fields
        private AudioSource audioSource;
        private float currentFlickerDuration;
        private float nextFlicker;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.audioSource = this.godRay.gameObject.GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            this.currentFlickerDuration = 0;
            this.nextFlicker = 0;
            this.audioSource.clip = this.flickerSound;
            this.audioSource.volume = this.flickerVolume;
        }

        private void OnDisable()
        {
            this.godRay.gameObject.SetActive(false);
        }

        private void Update()
        {
            this.Flicker();
        }
        
        private void Flicker()
        {
            if (this.currentFlickerDuration >= nextFlicker)
            {
                var _godRayActiveState = this.godRay.gameObject.activeSelf;
                this.godRay.gameObject.SetActive(!_godRayActiveState);
                
                nextFlicker = this.currentFlickerDuration + Random.Range(this.flickerStep.x, this.flickerStep.y);
            }

            this.currentFlickerDuration += Time.deltaTime;

            var _hasFinished = this.currentFlickerDuration >= this.maxFlickerDuration;
            if (_hasFinished)
            {
                this.enabled = false;   
            }
        }

        public void EnableGodRay()
        {
            this.audioSource.clip = this.floodLight;
            this.audioSource.volume = this.floodLightVolume;
            this.godRay.gameObject.SetActive(true);
        }
        #endregion
    }
}