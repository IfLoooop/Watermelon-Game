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
        /// Sets the <see cref="GameObject.layer"/> of the <see cref="EvolvingFruitTrigger"/> to <see cref="LayerMaskController.FruitLayer"/>
        /// </summary>
        public void SetFruitLayer() // TODO: Check if the layer can be set in the inspector from the beginning
        {
            base.gameObject.layer = LayerMaskController.FruitLayer;
        }
        
        /// <summary>
        /// Invokes <see cref="OnCanEvolve"/>
        /// </summary>
        /// <param name="_FruitBehaviour">The calling <see cref="FruitBehaviour"/></param>
        /// <param name="_HasAuthority">Indicates if the local client has authority over this fruit</param>
        public void Evolve(FruitBehaviour _FruitBehaviour, bool _HasAuthority)
        {
            OnCanEvolve?.Invoke(_FruitBehaviour, base.transform.position, _HasAuthority);
        }
        #endregion
    }
}