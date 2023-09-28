using UnityEngine;

namespace Watermelon_Game
{
    /// <summary>
    /// Contains logic for a <see cref="FruitBehaviour"/>'s first collision <br/>
    /// This component is destroyed afterwards
    /// </summary>
    public class BlockRelease : MonoBehaviour
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
