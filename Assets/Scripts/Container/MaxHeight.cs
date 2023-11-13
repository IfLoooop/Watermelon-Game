using System;
using System.Collections;
using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruits;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Container
{
    /// <summary>
    /// Contains logic for the game over conditions
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D), typeof(Animation), typeof(GodRayFlicker))]
    internal sealed class MaxHeight : MonoBehaviour
    {
        #region Inspector Fields
#if DEBUG || DEVELOPMENT_BUILD
        [Header("Development")]
        [Tooltip("Disables the loosing condition (Editor only)")]
        [ShowInInspector] public static bool DisableCountDown;
#endif
        [Header("Settings")]
        [Tooltip("Total duration in seconds of the countdown")]
        [SerializeField] private ProtectedUInt32 countdownTime = 8;
        [Tooltip("Duration in seconds the god ray stays visible before deactivating itself again")]
        [SerializeField] private float godRayDuration = 1.5f;
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="MaxHeight"/>
        /// </summary>
        private static MaxHeight instance;
        
        /// <summary>
        /// Detects only <see cref="Fruits.Fruit"/>s
        /// </summary>
        private BoxCollider2D trigger;
        
        /// <summary>
        /// <see cref="Animation"/> that indicates, a <see cref="Fruits.Fruit"/> has crossed the height limit
        /// </summary>
        private Animation borderLineAnimation;
        /// <summary>
        /// <see cref="Animation"/> that plays after a <see cref="Fruits.Fruit"/> has been to long above the height limit
        /// </summary>
        private Animation countdownAnimation;
        
        /// <summary>
        /// Displays the current countdown
        /// </summary>
        private TextMeshProUGUI countdownText;
        /// <summary>
        /// The current value of the countdown
        /// </summary>
        private ProtectedUInt32 currentCountdownTime;
        
        /// <summary>
        /// Disables the <see cref="GodRayFlicker.godRay"/> when enabled
        /// </summary>
        private GodRayFlicker godRayFlicker;
        /// <summary>
        /// <see cref="EnableFlicker"/>
        /// </summary>
        [CanBeNull] private IEnumerator enableFlicker;
        /// <summary>
        /// Time in seconds, before the <see cref="godRayFlicker"/> <see cref="Component"/> is enabled
        /// </summary>
        private WaitForSeconds timeBeforeStart;
        #endregion

        #region Events
        /// <summary>
        /// Is called when <see cref="currentCountdownTime"/> reaches 0
        /// </summary>
        public static event Action OnGameOver;
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
            this.trigger = base.GetComponent<BoxCollider2D>();
            this.borderLineAnimation = base.GetComponentInChildren<SpriteRenderer>().gameObject.GetComponent<Animation>();
            this.countdownAnimation = base.GetComponent<Animation>();
            this.countdownText = base.GetComponentInChildren<TextMeshProUGUI>();
            this.currentCountdownTime = this.countdownTime;
            this.godRayFlicker = base.GetComponent<GodRayFlicker>();
            this.timeBeforeStart = new WaitForSeconds(this.godRayDuration);
        }

        private void OnEnable()
        {
            FruitBehaviour.OnUpgradeToGoldenFruit += EnableGodRay;
        }

        private void OnDisable()
        {
            FruitBehaviour.OnUpgradeToGoldenFruit -= EnableGodRay;
        }

        private void OnTriggerEnter2D(Collider2D _Other)
        {
            if (!this.countdownAnimation.enabled)
            {
                this.countdownAnimation.enabled = true;
            }
        }

        private void OnTriggerStay2D(Collider2D _Other)
        {
            if (!this.borderLineAnimation.isPlaying)
            {
                this.borderLineAnimation.Play();
            }
        }

        private void OnTriggerExit2D(Collider2D _Other)
        {
            var _fruitInTrigger = this.trigger.IsTouchingLayers(LayerMaskController.FruitMask);
            if (!_fruitInTrigger)
            {
                this.Reset();
            }
        }
        
        /// <summary>
        /// Is called at the end of <see cref="countdownAnimation"/>
        /// </summary>
        public void CountDown()
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (DisableCountDown)
            {
                return;
            }
#endif
            
            this.currentCountdownTime--;
            this.countdownText.text = this.currentCountdownTime.ToString();

            if (this.currentCountdownTime == 5)
            {
                this.countdownText.enabled = true;
            }
            else if (this.currentCountdownTime == 0)
            {
                this.Reset();
                OnGameOver?.Invoke();
            }

            if (this.countdownText.enabled)
            {
                AudioPool.PlayClip(AudioClipName.Countdown);
            }
        }

        /// <summary>
        /// Resets all values to their default state
        /// </summary>
        private void Reset()
        {
            this.currentCountdownTime = this.countdownTime;
            this.countdownText.enabled = false;
            this.countdownAnimation.enabled = false;
            this.countdownAnimation.Rewind();
        }
        
        /// <summary>
        /// Sets the <see cref="GodRayFlicker.godRay"/> <see cref="GameObject"/> active
        /// </summary>
        public static void EnableGodRay()
        {
            if (instance.enableFlicker != null)
            {
                instance.StopCoroutine(instance.enableFlicker);
            }
            
            instance.godRayFlicker.enabled = false;
            instance.godRayFlicker.EnableGodRay();

            instance.enableFlicker = instance.EnableFlicker();
            instance.StartCoroutine(instance.enableFlicker);
        }

        /// <summary>
        /// Enables <see cref="godRayFlicker"/> after <see cref="timeBeforeStart"/>
        /// </summary>
        /// <returns></returns>
        private IEnumerator EnableFlicker()
        {
            yield return this.timeBeforeStart;
            this.godRayFlicker.enabled = true;
            this.enableFlicker = null;
        }
        #endregion
    }
}