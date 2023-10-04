using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.Fruit;

namespace Watermelon_Game.Fruit_Spawn
{
    /// <summary>
    /// Holds the next fruit that will be given to the <see cref="FruitSpawner"/>
    /// </summary>
    internal sealed class NextFruit : MonoBehaviour
    {
        #region Fields
        [CanBeNull] private FruitBehaviour fruitBehaviour;
        #endregion

        #region Properties
        public static NextFruit Instance { get; private set; }
        #endregion
        
        #region Methods
        private void Awake()
        {
            Instance = this;
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
            _fruitBehaviour.EnableAnimation(false);
            _fruitBehaviour.transform.SetParent(_Parent, false);
            
            this.fruitBehaviour = FruitBehaviour.SpawnFruit(_transform.position, _transform, _fruitBehaviour.Fruit);
            this.fruitBehaviour!.EnableAnimation(true);
                
            return _fruitBehaviour;
        }
        #endregion
    }
}