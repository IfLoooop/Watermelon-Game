using UnityEngine;
using Watermelon_Game.Audio;

namespace Watermelon_Game.Container
{
    /// <summary>
    /// Contains logic for the god ray flickering before its disabled
    /// </summary>
    internal sealed class GodRayFlicker : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the ParticleSystem of the Godray GameObject")]
        [SerializeField] private ParticleSystem godRay;
        
        [Header("Settings")]
        [Tooltip("Maximum duration in seconds, the flickering can play")]
        [SerializeField] private float maxFlickerDuration = .25f;
        [Tooltip("Higher value = more time (in seconds) between each flicker (A random value between is taken)")]
        [SerializeField] private Vector2 flickerStep = new(.04f, .06f);
        #endregion

        #region Fields
        /// <summary>
        /// Godray will flicker if this is >= <see cref="nextFlicker"/> and will be disable if >= <see cref="maxFlickerDuration"/>
        /// </summary>
        private float currentFlickerDuration;
        /// <summary>
        /// Time the next flicker can happen
        /// </summary>
        private float nextFlicker;
        #endregion
        
        #region Methods
        private void OnEnable()
        {
            var _isGodRayActive = this.godRay.gameObject.activeSelf;
            if (!_isGodRayActive)
            {
                this.enabled = false;
                return;
            }
            
            this.currentFlickerDuration = 0;
            this.nextFlicker = 0;
        }
        
        private void Update()
        {
            this.Flicker();
        }
        
        
        /// <summary>
        /// Flicker "Animation" for the <see cref="godRay"/>
        /// </summary>
        private void Flicker()
        {
            if (this.currentFlickerDuration >= nextFlicker)
            {
                var _godRayActiveState = this.godRay.gameObject.activeSelf;
                this.godRay.gameObject.SetActive(!_godRayActiveState);
                AudioPool.PlayClip(AudioClipName.GodrayFlicker);
                
                nextFlicker = this.currentFlickerDuration + Random.Range(this.flickerStep.x, this.flickerStep.y);
            }

            this.currentFlickerDuration += Time.deltaTime;

            var _hasFinished = this.currentFlickerDuration >= this.maxFlickerDuration;
            if (_hasFinished)
            {
                this.enabled = false;   
                this.godRay.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Enables the <see cref="godRay"/> <see cref="GameObject"/>
        /// </summary>
        public void EnableGodRay()
        {
            this.godRay.gameObject.SetActive(true);
            AudioPool.PlayClip(AudioClipName.Godray);
        }
        #endregion
    }
}