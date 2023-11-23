using System;
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
        #endregion

        #region Event
        /// <summary>
        /// Is called when the fruit is ready to evolve <br/>
        /// <b>Parameter1:</b> The <see cref="FruitBehaviour"/> to evolve <br/>
        /// <b>Parameter2:</b> The position, the evolved fruit will spawn at <br/>
        /// <b>Parameter3:</b> Indicates if the local client has authority over this fruit
        /// </summary>
        public static event Action<FruitBehaviour, Vector2, bool> OnCanEvolve; 
        #endregion
        
        #region Methods
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
        /// <param name="_HasAuthority">Indicates if the local client has authority over this fruit</param>
        public void Evolve(bool _HasAuthority)
        {
            OnCanEvolve?.Invoke(this.fruitToEvolveWith, base.transform.position, _HasAuthority);
        }
        #endregion
    }
}