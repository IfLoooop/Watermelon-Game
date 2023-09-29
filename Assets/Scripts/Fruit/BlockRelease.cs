using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.Fruit_Spawn;

namespace Watermelon_Game.Fruit
{
    /// <summary>
    /// Contains logic for a <see cref="FruitBehaviour"/>'s first collision <br/>
    /// This component is destroyed afterwards
    /// </summary>
    internal sealed class BlockRelease : MonoBehaviour
    {
        #region Properties
        [CanBeNull] public FruitSpawner FruitSpawner { get; set; }
        #endregion
        
        #region Methods
        private void OnCollisionEnter2D(Collision2D _Other)
        {
            if (this.FruitSpawner is {} _fruitSpawner)
            {
                _fruitSpawner.BlockRelease = false;
            }

            Destroy(this);
        }
        #endregion
    }
}
