using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Watermelon_Game.MaxHeight
{
    internal sealed class MaxHeight : MonoBehaviour
    {
        #region Inspector Fields
#if UNITY_EDITOR
        [SerializeField] private bool disableCountDown;
#endif
        [Header("Settings")]
        [SerializeField] private uint countdownTime = 8;
        [SerializeField] private float godRayDuration = 1.5f;
        #endregion
        
        #region Fields
        private BoxCollider2D boxCollider2D;
        private Animation borderLineAnimation;
        private Animation countdownAnimation;
        private TextMeshProUGUI countdownText;
        private AudioSource audioSource;
        private GodRayFlicker godRayFlicker;
        [CanBeNull] private IEnumerator enableFlicker;
        private WaitForSeconds timeBeforeStart;
        
        private uint currentCountdownTime;
        #endregion
        
        #region Properties
        public static MaxHeight Instance { get; private set; }
        #endregion
        
        #region Methods
        private void Awake()
        {
            Instance = this;
            this.boxCollider2D = base.GetComponent<BoxCollider2D>();
            this.borderLineAnimation = base.GetComponentInChildren<SpriteRenderer>().gameObject.GetComponent<Animation>();
            this.countdownAnimation = base.GetComponent<Animation>();
            this.countdownText = base.GetComponentInChildren<TextMeshProUGUI>();
            this.audioSource = base.GetComponent<AudioSource>();
            this.godRayFlicker = base.GetComponent<GodRayFlicker>();
            this.timeBeforeStart = new WaitForSeconds(this.godRayDuration);
        }

        private void Start()
        {
            this.currentCountdownTime = this.countdownTime;
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
            var _fruitInTrigger = this.boxCollider2D.IsTouchingLayers(LayerMask.GetMask("Fruit"));
            if (!_fruitInTrigger)
            {
                this.Reset();
            }
        }
        
        public void CountDown()
        {
#if UNITY_EDITOR
            if (this.disableCountDown)
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
                GameController.GameOver();
            }

            if (this.countdownText.enabled)
            {
                this.audioSource.Play();
            }
        }

        private void Reset()
        {
            this.currentCountdownTime = this.countdownTime;
            this.countdownText.enabled = false;
            this.countdownAnimation.enabled = false;
            this.countdownAnimation.Rewind();
            this.audioSource.Stop();
        }
        
        public void EnableGodRay()
        {
            if (this.enableFlicker != null)
            {
                base.StopCoroutine(this.enableFlicker);
            }
            
            this.godRayFlicker.enabled = false;
            this.godRayFlicker.EnableGodRay();

            this.enableFlicker = this.EnableFlicker();
            base.StartCoroutine(this.enableFlicker);
        }

        private IEnumerator EnableFlicker()
        {
            yield return this.timeBeforeStart;
            this.godRayFlicker.enabled = true;
            this.enableFlicker = null;
        }
        #endregion
    }
}