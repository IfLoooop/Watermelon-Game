using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon_Game.Points
{
    /// <summary>
    /// Contains logic for the current multiplier
    /// </summary>
    internal sealed class Multiplier : MonoBehaviour
    {
        #region Inspector Fields

        [Header("References")]
        [Tooltip("Reference to the animation component that plays the popup clip")]
        [SerializeField] private Animation popup;
        [Tooltip("Reference to the background image component")]
        [SerializeField] private Image background;
        [Tooltip("Reference to the TMP component that displays the multiplier amount")]
        [SerializeField] private TextMeshProUGUI multiplier;
        
        [Header("Settings")]
        [Tooltip("Maximum Duration in seconds, the multiplier stays visible")]
        [SerializeField] private ProtectedFloat multiplierDuration = 1f;
        [Tooltip("Wait time in seconds for the MultiplierDuration coroutine")]
        [SerializeField] private ProtectedFloat multiplierWaitTime = .1f;
        [Tooltip("Colors for the background image")]
        [SerializeField] private List<Color> multiplierColors;
        #endregion

        #region Fields
        /// <summary>
        /// Duration in seconds, the multiplier will be visible
        /// </summary>
        private ProtectedFloat currentMultiplierDuration;

        /// <summary>
        /// Disables the multiplier after <see cref="multiplierDuration"/>
        /// </summary>
        [CanBeNull] private IEnumerator multiplierCoroutine;
        /// <summary>
        /// TODO: Needs description
        /// </summary>
        private WaitForSeconds multiplierWaitForSeconds;
        #endregion

        #region Properties
        /// <summary>
        /// Current multiplier value
        /// </summary>
        public ProtectedUInt32 CurrentMultiplier { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Is called when the multiplier is activated <br/>
        /// <b>Parameter:</b> <see cref="CurrentMultiplier"/>
        /// </summary>
        public static event Action<uint> OnMultiplierActivated;
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
        
        /// <summary>
        /// Activates the multiplier
        /// </summary>
        public void StartMultiplier()
        {
            this.currentMultiplierDuration = this.multiplierDuration;
            
            var _newMultiplier = ++this.CurrentMultiplier;
            this.SetMultiplier(_newMultiplier);
            
            this.popup.Play();
            
            if (this.multiplierCoroutine == null)
            {
                this.multiplierCoroutine = this.MultiplierDuration();
                this.StartCoroutine(this.multiplierCoroutine);
            }
        }
        
        /// <summary>
        /// Adjusts the values for the multiplier
        /// </summary>
        /// <param name="_NewMultiplier">The new multiplier value</param>
        private void SetMultiplier(uint _NewMultiplier)
        {
            this.CurrentMultiplier = _NewMultiplier;
            this.multiplier.text = string.Concat("x", this.CurrentMultiplier);
            this.background.color = this.GetMultiplierColor();
            this.gameObject.SetActive(true);

            if (_NewMultiplier > 0)
            {
                OnMultiplierActivated?.Invoke(this.CurrentMultiplier);   
            }
        }
        
        /// <summary>
        /// <see cref="multiplierCoroutine"/>
        /// </summary>
        /// <returns></returns>
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
        
        /// <summary>
        /// Returns the <see cref="Color"/> from <see cref="multiplierColors"/> which index corresponds with the <see cref="CurrentMultiplier"/>
        /// </summary>
        /// <returns>The <see cref="Color"/> from <see cref="multiplierColors"/> which index corresponds with the <see cref="CurrentMultiplier"/></returns>
        private Color GetMultiplierColor()
        {
            if (this.CurrentMultiplier == 0)
            {
                return Color.white;
            }
            if (this.CurrentMultiplier > this.multiplierColors.Count)
            {
                return this.multiplierColors[^1];
            }
            
            return this.multiplierColors[(int)this.CurrentMultiplier.Value - 1];
        }
        #endregion
    }
}