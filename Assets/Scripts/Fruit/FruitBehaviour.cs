using System.Linq;
using UnityEngine;

namespace Watermelon_Game.Fruit
{
    internal sealed class FruitBehaviour : MonoBehaviour
    {
        #region Fields
        private new Rigidbody2D rigidbody2D;
        private BlockRelease blockRelease;
        #endregion

        #region Properties
        public Fruit Fruit { get; private set; }
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.rigidbody2D = this.GetComponent<Rigidbody2D>();
            this.blockRelease = this.GetComponent<BlockRelease>();
        }

        /// <summary>
        /// Drops the <see cref="Fruit"/> from the <see cref="FruitSpawner"/>
        /// </summary>
        /// <param name="_FruitSpawner"></param>
        public void Release(FruitSpawner _FruitSpawner)
        {
            this.blockRelease.FruitSpawner = _FruitSpawner;
            
            this.rigidbody2D.simulated = true;
            this.rigidbody2D.constraints = RigidbodyConstraints2D.None;
        }

        public static FruitBehaviour GetFruit(Vector2 _Position, Transform _Parent, Fruit? _PreviousFruit)
        {
            var _fruitPrefab = GetRandomFruit(_PreviousFruit);
            var _fruitBehaviour = Instantiate(_fruitPrefab.Prefab, _Position, Quaternion.identity, _Parent).GetComponent<FruitBehaviour>();

            _fruitBehaviour.Fruit = _fruitPrefab.Fruit;

            return _fruitBehaviour;
        }

        private static FruitPrefab GetRandomFruit(Fruit? _PreviousFruit)
        {
            if (_PreviousFruit == null)
            {
                return FruitPrefabs.Instance.Fruits.First(_FruitPrefab => _FruitPrefab.Fruit == Fruit.Grape);
            }
         
            FruitPrefabs.Instance.SetWeightMultiplier(_PreviousFruit.Value);

            var _max = FruitPrefabs.Instance.Fruits.Sum(_Fruit => _Fruit.GetSpawnWeight());
            var _randomNumber = Random.Range(0, _max);
            var _spawnWeight = 0;

            foreach (var _fruitPrefab in FruitPrefabs.Instance.Fruits)
            {
                if (_randomNumber <= _fruitPrefab.GetSpawnWeight() + _spawnWeight)
                {
                    return _fruitPrefab;
                }

                _spawnWeight += _fruitPrefab.GetSpawnWeight();
            }
            
            return null;
        }
        #endregion
    }
}
