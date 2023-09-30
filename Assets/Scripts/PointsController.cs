using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Watermelon_Game.Skills;

namespace Watermelon_Game
{
    internal sealed class PointsController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private TextMeshProUGUI points;
        [SerializeField] private TextMeshProUGUI multiplier;
        [SerializeField] private float multiplierDuration = 2f;
        #endregion
        
        #region Fields
        private uint currentPoints;
        private uint currentMultiplier;
        private float currentMultiplierDuration;
        private const float WAIT_TIME = .1f;

        [CanBeNull] private IEnumerator multiplierCoroutine;
        private readonly WaitForSeconds waitForSeconds = new(WAIT_TIME);
        #endregion

        #region Properties
        public static PointsController Instance { get; private set; }
        #endregion

        #region Methods
        private void Awake()
        {
            Instance = this;
        }

        public void AddPoints(Fruit.Fruit _Fruit)
        {
            this.currentMultiplierDuration = this.multiplierDuration;
            
            var _multiplier = this.currentMultiplier + 1;
            this.SetMultiplier(_multiplier);

            if (this.multiplierCoroutine == null)
            {
                this.multiplierCoroutine = MultiplierDuration();
                this.StartCoroutine(this.multiplierCoroutine);
            }
            
            var _points = this.currentPoints + (uint)_Fruit + this.currentMultiplier;
            this.SetPoints(_points);
            
            SkillController.Instance.PointsChanged(this.currentPoints);
        }

        public void SubtractPoints(uint _Value) 
        {
            var _points = (uint)Mathf.Clamp(this.currentPoints - _Value, 0, uint.MaxValue);
            this.SetPoints(_points);
            
            SkillController.Instance.PointsChanged(this.currentPoints);
        }
        
        private IEnumerator MultiplierDuration()
        {
            while (this.currentMultiplierDuration > 0)
            {
                yield return this.waitForSeconds;
                this.currentMultiplierDuration -= WAIT_TIME;
            }
            
            this.SetMultiplier(0);
            this.StopCoroutine(this.multiplierCoroutine);
            this.multiplierCoroutine = null;
            
            this.multiplier.enabled = false;
        }

        // TODO: Animate multiplier on enable
        private void SetMultiplier(uint _Value)
        {
            this.currentMultiplier = _Value;
            this.multiplier.text = string.Concat("x", this.currentMultiplier);
            this.multiplier.enabled = true;
        }
        
        // TODO: Add points over a period of time in a coroutine
        private void SetPoints(uint _Value)
        {
            this.currentPoints = _Value;
            this.points.text = string.Concat(this.currentPoints, "P");
        }
        #endregion
    }
}