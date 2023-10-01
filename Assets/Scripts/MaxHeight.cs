using TMPro;
using UnityEngine;

namespace Watermelon_Game
{
    internal sealed class MaxHeight : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private uint countdownTime = 8;
        #endregion
        
        #region Fields
        private Animation borderLineAnimation;
        private Animation countdownAnimation;
        private TextMeshProUGUI countdownText;
        
        /// <summary>
        /// How many fruits are currently inside the trigger
        /// </summary>
        private int triggerCount;
        private uint currentCountdownTime;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.borderLineAnimation = GetComponentInChildren<SpriteRenderer>().gameObject.GetComponent<Animation>();
            this.countdownAnimation = GetComponent<Animation>();
            this.countdownText = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            this.currentCountdownTime = this.countdownTime;
        }

        private void OnTriggerEnter2D(Collider2D _Other)
        {
            this.triggerCount++;
            this.countdownAnimation.enabled = true;
            this.borderLineAnimation.Play();
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
            this.triggerCount--;

            if (this.triggerCount <= 0)
            {
                this.Reset();
            }
        }

        public void CountDown()
        {
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
        }

        private void Reset()
        {
            this.currentCountdownTime = this.countdownTime;
            this.countdownText.enabled = false;
            this.countdownAnimation.enabled = false;
            this.countdownAnimation.Rewind();
        }
        #endregion
    }
}