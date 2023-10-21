using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.Background;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Menu;
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
        [Header("References")]
        [SerializeField] private SpriteRenderer fruitSprite;
        [SerializeField] private SpriteRenderer faceSprite;
        [Header("Settings")]
        [SerializeField] private Fruit fruit;
        #endregion

        #region Constants
        // TODO: Move all these to a separate class
        private const float EVOLVE_MASS = 100;
        private const float MOVE_TOWARDS_WAIT_TIME = .01f;
        private const float MOVE_TOWARDS_STEP_MULTIPLIER = .2f;
        private const float EVOLVE_WAIT_TIME = .005f;
        // Value must be a multiple of 5, otherwise it will overshoot the targeted scale
        private const float EVOLVE_STEP = .5f;
        #endregion
        
        #region Fields
#pragma warning disable CS0109
        private new Rigidbody2D rigidbody2D;
#pragma warning restore CS0109
        private CircleCollider2D circleCollider2D;
        private BlockRelease blockRelease;
#pragma warning disable CS0109
        private new Animation animation;
#pragma warning restore CS0109
        private EvolvingFruitTrigger evolvingFruitTrigger;
        
        private bool isHurt;
        private bool hasBeenEvolved;
        private bool collisionWithMaxHeight;
        private bool isUpgradedGoldenFruit;
        private bool disableEvolving;
        
        [CanBeNull] private IEnumerator moveTowards;
        private static readonly WaitForSeconds moveTowardsWaitTime = new(MOVE_TOWARDS_WAIT_TIME);
        [CanBeNull] private IEnumerator evolve;
        private static readonly WaitForSeconds evolveWaitTime = new(EVOLVE_WAIT_TIME);

        private static readonly List<FruitBehaviour> goldenFruits = new();
        #endregion

        #region Properties
        public Fruit Fruit => this.fruit;
        public Skill? ActiveSkill { get; private set; }
        public Rigidbody2D Rigidbody2D => this.rigidbody2D;
        public bool IsGoldenFruit { get; private set; }
        public bool IsEvolving { get; private set; }
        public bool CanBeAddedToFruitCollection { get; private set; } = true;
        public float ColliderRadius => this.circleCollider2D.radius;

        public static int GoldenFruitsCount => goldenFruits.Count;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.rigidbody2D = base.GetComponent<Rigidbody2D>();
            this.circleCollider2D = base.GetComponent<CircleCollider2D>();
            this.blockRelease = base.GetComponent<BlockRelease>();
            this.animation = base.GetComponent<Animation>();
            this.evolvingFruitTrigger = base.GetComponentInChildren<EvolvingFruitTrigger>();
        }
        
        private void Start()
        {
            GameController.AddFruit(this);
            this.GoldenFruit();
        }

        private void OnCollisionEnter2D(Collision2D _Other)
        {
            if (!this.isHurt)
            {
                this.isHurt = true;
                this.faceSprite.sprite = GameController.Instance.FruitCollection.FaceHurt;
                this.Invoke(nameof(this.ResetFace), 1);
            }

            if (this.disableEvolving)
            {
                return;
            }
            
            var _powerSkillNotActive = this.ActiveSkill is not Skill.Power;
            var _doesntUseAutoMass = this.rigidbody2D.useAutoMass == false;
            if (_powerSkillNotActive && _doesntUseAutoMass)
            {
                this.SetMass(true, 0);
            }
            
            var _otherIsFruit = _Other.gameObject.layer == LayerMask.NameToLayer("Fruit"); 
            
            if (_otherIsFruit)
            {
                var _otherHashCode = _Other.gameObject.GetHashCode();
                
                if (this.IsGoldenFruit)
                {
                    if (this.isUpgradedGoldenFruit)
                    {
                        GameController.UpgradedGoldenFruitCollision(base.gameObject, _Other.gameObject);
                    }
                    else
                    {
                        GameController.GoldenFruitCollision(_Other.gameObject);
                    }
                    return;
                }
                if (this.ActiveSkill is Skill.Evolve)
                {
                    this.DeactivateSkill();
                    GameController.EvolveFruit(_otherHashCode);
                    return;
                }
                if (this.ActiveSkill is Skill.Destroy)
                {
                    this.DeactivateSkill();
                    GameController.DestroyFruit(_otherHashCode);
                    return;
                }
                
                GameController.FruitCollision(this.gameObject.GetHashCode(), _otherHashCode);
            }
            else if (this.IsGoldenFruit)
            {
                if (_Other.gameObject.CompareTag("Wall Bottom"))
                {
                    GameController.GoldenFruitCollision(base.gameObject);
                }
            }

        }

        private void ResetFace()
        {
            this.faceSprite.sprite = GameController.Instance.FruitCollection.FaceDefault;
            this.isHurt = false;
        }
        
        private void OnTriggerEnter2D(Collider2D _Other)
        {
            var _otherIsMaxHeight = _Other.gameObject.layer == LayerMask.NameToLayer("MaxHeight");
            
            if (_otherIsMaxHeight)
            {
                this.collisionWithMaxHeight = true;
            }
        }

        private void OnTriggerExit2D(Collider2D _Other)
        {
            var _otherIsMaxHeight = _Other.gameObject.layer == LayerMask.NameToLayer("MaxHeight");

            if (_otherIsMaxHeight)
            {
                this.collisionWithMaxHeight = false;
            }
        }

        public bool canBecomeGoldenFruit = true;
        
        private void OnBecameInvisible()
        {
            if (this.collisionWithMaxHeight)
            {
                var _centerPoint = base.transform.position.y;
                var _extends = base.transform.localScale.y / 2;
                var _lowestPoint = _centerPoint + _extends;
                var _isOutOfScreen = _lowestPoint > BackgroundController.Instance.YPosition;
                
                if (_isOutOfScreen)
                {
                    if (!canBecomeGoldenFruit)
                    {
                        return;
                    }
                    
                    this.GoldenFruit(true);
                    MaxHeight.MaxHeight.Instance.SetGodRays(true);   
                }
            }
        }

        private void OnDestroy()
        {
            // TODO: Temporary
            GameController.RemoveFruit(base.gameObject.GetHashCode());
            if (this.evolve != null)
            {
                base.StopCoroutine(this.evolve);
            }
            if (this.moveTowards != null)
            {
                base.StopCoroutine(this.moveTowards);
            }
            if (this.isUpgradedGoldenFruit)
            {
                goldenFruits.Remove(this);
                MaxHeight.MaxHeight.Instance.SetGodRays(false);
            }
        }

#if DEBUG || DEVELOPMENT_BUILD
        public void GoldenFruit_Debug()
        {
            this.GoldenFruit(true);
            MaxHeight.MaxHeight.Instance.SetGodRays(true); 
        }
#endif
        
        private void GoldenFruit(bool _ForceEnable = false)
        {
            if (!_ForceEnable)
            {
                var _notEnoughFruitsOnMap =
#if UNITY_EDITOR
                    (GameController.Instance.FruitCollection.GoldenFruitChance >= 100 ? GameController.Instance.FruitCollection.CanSpawnAfter : GameController.GetFruitCount())
#else
                    GameController.GetFruitCount()
#endif
                    < GameController.Instance.FruitCollection.CanSpawnAfter;
                
                if (this.hasBeenEvolved || _notEnoughFruitsOnMap)
                {
                    return;
                }
                
                var _maxNumber = (int)(100 / GameController.Instance.FruitCollection.GoldenFruitChance); 
                var _numberToGet = Random.Range(1, _maxNumber);
                var _randomNumber = Random.Range(1, _maxNumber);
                
                if (_numberToGet != _randomNumber)
                {
                    return;
                }
            }
            
            goldenFruits.Add(this);
            this.IsGoldenFruit = true;
            this.isUpgradedGoldenFruit = _ForceEnable;
            Instantiate(GameController.Instance.FruitCollection.GoldenFruitPrefab, base.transform.position, Quaternion.identity, base.transform);
            GameOverMenu.Instance.Stats.GoldenFruitCount++;
            StatsMenu.Instance.Stats.GoldenFruitCount++;
        }

        public void SetOrderInLayer(int _OrderInLayer)
        {
            this.fruitSprite.sortingOrder = _OrderInLayer;
            this.faceSprite.sortingOrder = _OrderInLayer + 1;
        }

#if DEBUG || DEVELOPMENT_BUILD
        public void CanNotBeAddedToFruitCollection_DEBUG()
        {
            this.CanBeAddedToFruitCollection = false;
        }
#endif
        
        /// <summary>
        /// Drops the <see cref="Fruit"/> from the <see cref="FruitSpawner"/>
        /// </summary>
        /// <param name="_FruitSpawner"><see cref="FruitSpawner"/></param>
        public void Release(FruitSpawner _FruitSpawner)
        {
            base.transform.SetParent(null, true);
            this.SetOrderInLayer(0);
            this.blockRelease.FruitSpawner = _FruitSpawner;
            this.CanBeAddedToFruitCollection = true;
            this.InitializeRigidBody();
            GameController.AddFruit(this);
        }

        public void SetMass(bool _Reset, float _Mass)
        {
            if (_Reset)
            {
                this.rigidbody2D.useAutoMass = true;
            }
            else
            {
                this.rigidbody2D.useAutoMass = false;
                this.rigidbody2D.mass = _Mass;
            }
        }
        
        private void InitializeRigidBody()
        {
            this.rigidbody2D.simulated = true;
            this.rigidbody2D.constraints = RigidbodyConstraints2D.None;
        }
        
        /// <summary>
        /// Instantiates a random fruit <br/>
        /// <i>For <see cref="FruitSpawner"/> and <see cref="NextFruit"/></i>
        /// </summary>
        /// <param name="_Position">Where to spawn the fruit</param>
        /// <param name="_Parent">The parent object of the spawned fruit</param>
        /// <param name="_PreviousFruit">The previously spawned <see cref="Watermelon_Game.Fruit.Fruit"/></param>
        /// <returns>The <see cref="FruitBehaviour"/> of the spawned fruit <see cref="GameObject"/></returns>
        public static FruitBehaviour SpawnFruit(Vector2 _Position, Transform _Parent, Fruit? _PreviousFruit)
        {
            var _fruitData = GetRandomFruit(_PreviousFruit);
            var _fruitBehaviour = Instantiate(_fruitData.Prefab, _Position, Quaternion.identity, _Parent).GetComponent<FruitBehaviour>();

            _fruitBehaviour.CanBeAddedToFruitCollection = false;
            _fruitBehaviour.gameObject.SetActive(true);
            _fruitBehaviour.SetAnimation(true);

            return _fruitBehaviour;
        }

        public void SetAnimation(bool _Value)
        {
            this.animation.enabled = _Value;
        }
        
        /// <summary>
        /// Instantiates a specific fruit <br/>
        /// <i>For evolved fruits</i> <br/>
        /// <b>The fruit GameObject will not be active</b>
        /// </summary>
        /// <param name="_Position">Where to spawn the fruit</param>
        /// <param name="_Fruit">The <see cref="Watermelon_Game.Fruit.Fruit"/> to spawn</param>
        /// <param name="_Evolve">Is the fruit being evolved or is it a regular spawn</param>
        /// <param name="_Rotation">The rotation of the spawned fruit</param>
        /// <returns>The <see cref="FruitBehaviour"/> of the spawned fruit <see cref="GameObject"/></returns>
        public static FruitBehaviour SpawnFruit(Vector2 _Position, Fruit _Fruit, bool _Evolve, Quaternion? _Rotation = null)
        {
            var _fruitData = GameController.Instance.FruitCollection.Fruits.First(_FruitData => _FruitData.Fruit == _Fruit);
            var _fruitBehavior = Instantiate(_fruitData.Prefab, _Position, _Rotation ?? Quaternion.identity).GetComponent<FruitBehaviour>();

            _fruitBehavior.hasBeenEvolved = _Evolve;

            return _fruitBehavior;
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

        public void MoveTowards(FruitBehaviour _FruitBehaviour)
        {
            this.IsEvolving = true;
            this.evolvingFruitTrigger.SetFruitToEvolveWith(_FruitBehaviour);
            base.gameObject.layer = LayerMask.NameToLayer("EvolvingFruit");
            this.SetMass(false, EVOLVE_MASS);
            this.moveTowards = InternalMoveTowards(_FruitBehaviour);
            base.StartCoroutine(this.moveTowards);
        }

        private IEnumerator InternalMoveTowards(FruitBehaviour _FruitBehaviour)
        {
            this.rigidbody2D.velocity = Vector2.zero;
            var _maxDistanceDelta = this.GetSize() * MOVE_TOWARDS_STEP_MULTIPLIER;
            
            while (base.transform.position != _FruitBehaviour.transform.position)
            {
                var _newPosition = Vector2.MoveTowards(base.transform.position, _FruitBehaviour.transform.position, _maxDistanceDelta);
                this.rigidbody2D.MovePosition(_newPosition);
                yield return moveTowardsWaitTime;
            }
            
            base.StopCoroutine(this.moveTowards);
        }

        private float GetSize()
        {
            var _localScale = base.transform.localScale;
            return (_localScale.x + _localScale.y + _localScale.z) / 3;
        }
        
        public void Evolve()
        {
            var _targetScale = base.transform.localScale;
            base.transform.localScale = Vector3.zero;
            base.gameObject.SetActive(true);
            this.InitializeRigidBody();
            
            this.evolve = this.Evolve(_targetScale);
            base.StartCoroutine(this.evolve);
        }
        
        private IEnumerator Evolve(Vector3 _TargetScale)
        {
            var _scaleStep = new Vector3(EVOLVE_STEP, EVOLVE_STEP, EVOLVE_STEP);
            
            while (base.transform.localScale.x < _TargetScale.x)
            {
                base.transform.localScale += _scaleStep;
                yield return evolveWaitTime;
            }
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

        public void DisableEvolving()
        {
            this.disableEvolving = true;
        }
        
        /// <summary>
        /// Destroys this <see cref="GameObject"/>
        /// </summary>
        public void Destroy()
        {
            // TODO: Add a visual animation
            Destroy(this.gameObject);
        }
        #endregion
    }
}
