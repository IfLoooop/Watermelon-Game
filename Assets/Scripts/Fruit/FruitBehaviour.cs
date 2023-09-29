using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Fruit
{
    /// <summary>
    /// Main logic for all fruits
    /// </summary>
    internal sealed class FruitBehaviour : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private Fruit fruit;
        #endregion
        
        #region Fields
        private new Rigidbody2D rigidbody2D;
        private BlockRelease blockRelease;
        #endregion

        #region Properties
        public Fruit Fruit => this.fruit;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.rigidbody2D = this.GetComponent<Rigidbody2D>();
            this.blockRelease = this.GetComponent<BlockRelease>();
        }

        private void OnEnable()
        {
            GameController.AddFruit(this);
        }

        private void OnCollisionEnter2D(Collision2D _Other)
        {
            if (_Other.gameObject.layer == GameController.FRUIT_LAYER) 
            {
                GameController.FruitCollision(this.gameObject.GetHashCode(), _Other.gameObject.GetHashCode());
            }
            
        }

        /// <summary>
        /// Drops the <see cref="Fruit"/> from the <see cref="FruitSpawner"/>
        /// </summary>
        /// <param name="_FruitSpawner"></param>
        public void Release(FruitSpawner _FruitSpawner)
        {
            this.blockRelease.FruitSpawner = _FruitSpawner;
            
            this.InitializeRigidBody();
        }

        private void InitializeRigidBody()
        {
            this.rigidbody2D.simulated = true;
            this.rigidbody2D.constraints = RigidbodyConstraints2D.None;
        }
        
        /// <summary>
        /// Instantiates a random fruit
        /// </summary>
        /// <param name="_Position">Where to spawn the fruit</param>
        /// <param name="_Parent">The parent object of the spawned fruit</param>
        /// <param name="_PreviousFruit">The previously spawned <see cref="Watermelon_Game.Fruit.Fruit"/></param>
        /// <returns>The <see cref="FruitBehaviour"/> of the spawned fruit <see cref="GameObject"/></returns>
        public static FruitBehaviour SpawnFruit(Vector2 _Position, Transform _Parent, Fruit? _PreviousFruit)
        {
            var _fruitData = GetRandomFruit(_PreviousFruit);
            var _fruitBehaviour = Instantiate(_fruitData.Prefab, _Position, Quaternion.identity, _Parent).GetComponent<FruitBehaviour>();

            return _fruitBehaviour;
        }

        /// <summary>
        /// Instantiates a specific fruit
        /// </summary>
        /// <param name="_Position">Where to spawn the fruit</param>
        /// <param name="_Fruit">The <see cref="Watermelon_Game.Fruit.Fruit"/> to spawn</param>
        public static void SpawnFruit(Vector2 _Position, Fruit _Fruit)
        {
            var _fruitData = FruitCollection.Instance.Fruits.First(_FruitData => _FruitData.Fruit == _Fruit);
            var _fruitBehavior = Instantiate(_fruitData.Prefab, _Position, Quaternion.identity).GetComponent<FruitBehaviour>();

            _fruitBehavior.InitializeRigidBody();
        }
        
        private static FruitData GetRandomFruit(Fruit? _PreviousFruit)
        {
            if (_PreviousFruit == null)
            {
                return FruitCollection.Instance.Fruits.First(_FruitData => _FruitData.Fruit == Fruit.Grape);
            }
         
            FruitCollection.Instance.SetWeightMultiplier(_PreviousFruit.Value);

            var _spawnableFruits = FruitCollection.Instance.Fruits.TakeWhile(_Fruit => (int)_Fruit.Fruit < (int)Fruit.Apple).ToArray();
            
            var _max = _spawnableFruits.Sum(_Fruit => _Fruit.GetSpawnWeight());
            var _randomNumber = Random.Range(0, _max);
            var _spawnWeight = 0;

            foreach (var _fruitData in _spawnableFruits)
            {
                if (_randomNumber <= _fruitData.GetSpawnWeight() + _spawnWeight)
                {
                    return _fruitData;
                }

                _spawnWeight += _fruitData.GetSpawnWeight();
            }
            
            return null;
        }
        #endregion
    }
}
