using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.Fruit;

namespace Watermelon_Game
{
    /// <summary>
    /// Holds the next fruit that will be given to the <see cref="FruitSpawner"/>
    /// </summary>
    internal sealed class NextFruit : MonoBehaviour
    {
        #region Statics
        private static NextFruit instance;
        #endregion
        
        #region Fields
        [CanBeNull] private FruitBehaviour fruitBehaviour;
        #endregion

        #region Properties
        public static NextFruit Instance => instance;
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Returns the <see cref="fruitBehaviour"/> currently held by <see cref="NextFruit"/>
        /// </summary>
        /// <param name="_Parent">The new parent of the fruit</param>
        /// <param name="_PreviousFruit">The previously spawned <see cref="Watermelon_Game.Fruit.Fruit"/></param>
        /// <returns>The <see cref="FruitBehaviour"/> of the spawned fruit <see cref="GameObject"/></returns>
        public FruitBehaviour GetFruit(Transform _Parent, Fruit.Fruit? _PreviousFruit)
        {
            var _transform = this.transform;
            
            if (this.fruitBehaviour == null)
            {
                this.fruitBehaviour = FruitBehaviour.SpawnFruit(_transform.position, _transform, _PreviousFruit);
            }
            
            var _fruitBehaviour = this.fruitBehaviour!;
            _fruitBehaviour.transform.SetParent(_Parent, false);
            
            this.fruitBehaviour = FruitBehaviour.SpawnFruit(_transform.position, _transform, _fruitBehaviour.Fruit);
                
            return _fruitBehaviour;
        }
        #endregion
    }
}