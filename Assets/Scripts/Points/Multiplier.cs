using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Watermelon_Game.Menu;

namespace Watermelon_Game.Points
{
    internal sealed class Multiplier : MonoBehaviour
    {
        #region Inspector Fields

        [Header("References")]
        [SerializeField] private Animation popup;
        [SerializeField] private TextMeshProUGUI textMeshPro;
        [SerializeField] private SpriteRenderer background;
        [Header("Settings")]
        [SerializeField] private float multiplierDuration = 1f;
        [SerializeField] private float multiplierWaitTime = .1f;
        [SerializeField] private List<Color> multiplierColors;
        #endregion

        #region Fields
        private float currentMultiplierDuration;

        [CanBeNull] private IEnumerator multiplierCoroutine;
        private WaitForSeconds multiplierWaitForSeconds;
        #endregion

        #region Properties
        public uint CurrentMultiplier { get; private set; }
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.multiplierWaitForSeconds = new WaitForSeconds(this.multiplierWaitTime);
        }
        
        private void OnDisable()
        {
            this.CurrentMultiplier = 0;
        }
        
        public void StartMultiplier()
        {
            this.currentMultiplierDuration = this.multiplierDuration;
            
            var _multiplier = ++this.CurrentMultiplier;
            this.SetMultiplier(_multiplier);

            this.gameObject.SetActive(true);
            this.popup.Play();
            
            if (this.multiplierCoroutine == null)
            {
                this.multiplierCoroutine = this.MultiplierDuration();
                this.StartCoroutine(this.multiplierCoroutine);
            }
        }
        
        private IEnumerator MultiplierDuration()
        {
            while (this.currentMultiplierDuration > 0)
            {
                yield return this.multiplierWaitForSeconds;
                this.currentMultiplierDuration -= multiplierWaitTime;
            }
            
            this.SetMultiplier(0);
            this.StopCoroutine(this.multiplierCoroutine);
            this.multiplierCoroutine = null;
            
            this.gameObject.SetActive(false);
        }
        
        private void SetMultiplier(uint _CurrentMultiplier)
        {
            this.CurrentMultiplier = _CurrentMultiplier;
            this.textMeshPro.text = string.Concat("x", this.CurrentMultiplier);
            this.background.color = this.GetMultiplierColor(_CurrentMultiplier);
            this.gameObject.SetActive(true);

            if (_CurrentMultiplier > GameOverMenu.Instance.Stats.HighestMultiplier)
            {
                GameOverMenu.Instance.Stats.HighestMultiplier = _CurrentMultiplier;
            }
        }

        private Color GetMultiplierColor(uint _CurrentMultiplier)
        {
            if (_CurrentMultiplier == 0)
            {
                return Color.white;
            }
            if (_CurrentMultiplier > this.multiplierColors.Count)
            {
                return this.multiplierColors[^1];
            }

            return this.multiplierColors[(int)_CurrentMultiplier - 1];
        }
        #endregion
    }
}