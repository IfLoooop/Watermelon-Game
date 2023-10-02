using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Skills;
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

        private bool isGoldenFruit;
        #endregion

        #region Properties
        public Fruit Fruit => this.fruit;
        public Skill? ActiveSkill { get; private set; }
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
            this.GoldenFruit();
        }

        private void OnCollisionEnter2D(Collision2D _Other)
        {
            if (_Other.gameObject.layer == LayerMask.NameToLayer("Fruit")) 
            {
                if (this.isGoldenFruit)
                {
                    GameController.GoldenFruitCollision(_Other.gameObject.GetHashCode());
                    return;
                }
                if (this.ActiveSkill is Skill.Evolve)
                {
                    this.DeactivateSkill();
                    GameController.EvolveFruit(_Other.gameObject.GetHashCode());
                    return;
                }
                if (this.ActiveSkill is Skill.Destroy)
                {
                    this.DeactivateSkill();
                    GameController.DestroyFruit(_Other.gameObject.GetHashCode());
                    return;
                }
                
                GameController.FruitCollision(this.gameObject.GetHashCode(), _Other.gameObject.GetHashCode());
            }
            else if (this.isGoldenFruit)
            {
                if (_Other.gameObject.CompareTag("Wall Bottom"))
                {
                    GameController.GoldenFruitCollision(this.gameObject.GetHashCode());
                }
            }
        }

        private void GoldenFruit()
        {
            var _maxNumber = (int)(100 / GameController.Instance.FruitCollection.GoldenFruitChance); 
            var _numberToGet = Random.Range(1, _maxNumber);
            var _randomNumber = Random.Range(1, _maxNumber);

            if (_numberToGet == _randomNumber)
            {
                this.isGoldenFruit = true;
                var _goldenFruitPrefab = Instantiate(GameController.Instance.FruitCollection.GoldenFruitPrefab, this.rigidbody2D.position, Quaternion.identity, this.transform);
                var _light2D = _goldenFruitPrefab.GetComponentInChildren<Light2D>();

                _light2D.pointLightOuterRadius = this.transform.localScale.x / 2;
            }
        }
        
        /// <summary>
        /// Drops the <see cref="Fruit"/> from the <see cref="FruitSpawner"/>
        /// </summary>
        /// <param name="_FruitSpawner"><see cref="FruitSpawner"/></param>
        /// <param name="_Direction">The direction the <see cref="FruitSpawner"/> is currently facing</param>
        public void Release(FruitSpawner _FruitSpawner, Vector2 _Direction)
        {
            this.blockRelease.FruitSpawner = _FruitSpawner;
            
            this.InitializeRigidBody();

            if (this.ActiveSkill is Skill.Power)
            {
                SkillController.Instance.Skill_Power(this.rigidbody2D, _Direction);
            }
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
            var _fruitData = GameController.Instance.FruitCollection.Fruits.First(_FruitData => _FruitData.Fruit == _Fruit);
            var _fruitBehavior = Instantiate(_fruitData.Prefab, _Position, Quaternion.identity).GetComponent<FruitBehaviour>();

            _fruitBehavior.InitializeRigidBody();
        }
        
        private static FruitData GetRandomFruit(Fruit? _PreviousFruit)
        {
            if (_PreviousFruit == null)
            {
                return GameController.Instance.FruitCollection.Fruits.First(_FruitData => _FruitData.Fruit == Fruit.Grape);
            }
         
            GameController.Instance.FruitCollection.SetWeightMultiplier(_PreviousFruit.Value);

            var _highestFruitSpawn = GameController.Instance.FruitCollection.Fruits.First(_Fruit => _Fruit.GetSpawnWeight() == 0).Fruit;
            var _spawnableFruits = GameController.Instance.FruitCollection.Fruits.TakeWhile(_Fruit => (int)_Fruit.Fruit < (int)_highestFruitSpawn).ToArray();
            
            var _combinedSpawnWeights = _spawnableFruits.Sum(_Fruit => _Fruit.GetSpawnWeight());
            var _randomNumber = Random.Range(0, _combinedSpawnWeights);
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

        /// <summary>
        /// Sets the given <see cref="Skill"/> as currently active
        /// </summary>
        /// <param name="_ActiveSkill">The <see cref="Skill"/> to activate</param>
        public void SetActiveSkill(Skill _ActiveSkill)
        {
            this.ActiveSkill = _ActiveSkill;
        }
        
        /// <summary>
        /// Deactivates the currently active <see cref="Skill"/>
        /// </summary>
        public void DeactivateSkill()
        {
            this.ActiveSkill = null;
        }

        /// <summary>
        /// Shoots the fruit with increased force in  the given direction
        /// </summary>
        /// <param name="_Direction">The direction to shoot the fruit in</param>
        public void Shoot(Vector2 _Direction)
        {
            this.rigidbody2D.AddForce(_Direction * (SkillController.Instance.ShootForceMultiplier * this.rigidbody2D.mass), ForceMode2D.Impulse);
        }

        /// <summary>
        /// Destroys this <see cref="GameObject"/>
        /// </summary>
        public void Destroy()
        {
            Destroy(this.gameObject);
        }
        #endregion
    }
}
