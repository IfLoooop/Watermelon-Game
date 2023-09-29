using UnityEngine;

namespace Watermelon_Game.Fruit
{
    /// <summary>
    /// Contains logic for a <see cref="FruitBehaviour"/>'s first collision <br/>
    /// This component is destroyed afterwards
    /// </summary>
    internal sealed class BlockRelease : MonoBehaviour
    {
        #region Properties
        public FruitSpawner FruitSpawner { get; set; }
        #endregion
        
        #region Methods
        private void OnCollisionEnter2D(Collision2D _Other)
        {
            this.FruitSpawner.BlockRelease = false;
            Destroy(this);
        }
        #endregion
    }
}
