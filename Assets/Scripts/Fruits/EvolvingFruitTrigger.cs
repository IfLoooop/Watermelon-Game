using System;
using Mirror;
using UnityEngine;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Fruits
{
    // TODO: Probably not needed anymore
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
        /// <summary>
        /// Disables the <see cref="OnTriggerEnter2D"/>-method if true
        /// </summary>
        private bool disableTrigger;
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
            if (this.disableTrigger)
            {
                return;
            }
            
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
        
        /// <summary>
        /// Forces the fruit to evolve -> <see cref="OnCanEvolve"/> <br/>
        /// <i>Use when the fruit is stuck and can't move any further towards <see cref="fruitToEvolveWith"/></i>
        /// </summary>
        /// <param name="_HasAuthority"></param>
        public void Evolve(bool _HasAuthority) // TODO
        {
            this.disableTrigger = true;
            OnCanEvolve?.Invoke(this.fruitToEvolveWith, base.transform.position);
        }
        #endregion
    }
}