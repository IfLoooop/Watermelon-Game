using System;
using Mirror;
using UnityEngine;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Contains logic for the first collision of a <see cref="FruitBehaviour"/> <br/>
    /// <i>This component is destroyed afterwards</i>
    /// </summary>
    internal sealed class FruitsFirstCollision : NetworkBehaviour
    {
        #region Fields
        /// <summary>
        /// Indicates whether the <see cref="OnCollisionEnter2D"/> method can be used or not
        /// </summary>
        private bool isActive;
        #endregion
        
        #region Events
        /// <summary>
        /// Is called after the fruits first collision with something
        /// </summary>
        public static event Action OnCollision;
        #endregion
        
        #region Methods
        private void OnCollisionEnter2D(Collision2D _)
        {
            if (this.isActive)
            {
                OnCollision?.Invoke();

                Destroy(this);   
            }
        }

        /// <summary>
        /// Sets <see cref="isActive"/> to true
        /// </summary>
        public void SetActive()
        {
            this.isActive = true;
        }

        /// <summary>
        /// Destroy this <see cref="Component"/> <br/>
        /// <i>For evolved Fruits</i>
        /// </summary>
        public void DestroyComponent()
        {
            Destroy(this);
        }
        #endregion
    }
}
