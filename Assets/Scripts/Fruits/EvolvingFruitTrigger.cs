using System;
using UnityEngine;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Contains information for a fruit to evolve
    /// </summary>
    internal sealed class EvolvingFruitTrigger : MonoBehaviour
    {
        #region Fields
        /// <summary>
        /// The <see cref="FruitBehaviour"/> of this <see cref="EvolvingFruitTrigger"/>, can only evolve with <see cref="fruitToEvolveWith"/>
        /// </summary>
        private FruitBehaviour fruitToEvolveWith;
        #endregion

        #region Event
        /// <summary>
        /// Is called when the fruit is ready to evolve <br/>
        /// <b>Parameter1:</b> The <see cref="FruitBehaviour"/> to evolve <br/>
        /// <b>Parameter2:</b> The position, the evolved fruit will spawn at
        /// </summary>
        public static event Action<FruitBehaviour, Vector2> OnCanEvolve; 
        #endregion
        
        #region Methods
        private void OnTriggerEnter2D(Collider2D _Other)
        {
            var _otherHashcode = _Other.gameObject.GetHashCode();
            var _fruitToEvolveWithHashcode = this.fruitToEvolveWith.gameObject.GetHashCode();

            if (_otherHashcode == _fruitToEvolveWithHashcode)
            {
                OnCanEvolve?.Invoke(this.fruitToEvolveWith, base.transform.position);
            }
        }

        /// <summary>
        /// Sets the fruit, this fruit can only evolve with
        /// </summary>
        /// <param name="_FruitBehaviour"><see cref="fruitToEvolveWith"/></param>
        public void SetFruitToEvolveWith(FruitBehaviour _FruitBehaviour)
        {
            this.fruitToEvolveWith = _FruitBehaviour;
            base.gameObject.layer = LayerMaskController.FruitLayer;
        }
        #endregion
    }
}