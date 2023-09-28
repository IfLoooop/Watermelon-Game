using UnityEngine;

namespace Watermelon_Game
{
    public class FruitBehaviour : MonoBehaviour
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
            var _fruitBehaviour = Instantiate(_fruitPrefab, _Position, Quaternion.identity, _Parent).GetComponent<FruitBehaviour>();

            return _fruitBehaviour;
        }

        private static GameObject GetRandomFruit(Fruit? _PreviousFruit)
        {
            
        }
        #endregion
    }
}
