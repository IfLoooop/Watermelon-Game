using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Container;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Skills;
using Watermelon_Game.Utility;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Main logic for all fruits
    /// </summary>
    internal sealed class FruitBehaviour : MonoBehaviour
    {
        #region Inspector Fields
#if DEBUG || DEVELOPMENT_BUILD
        [Header("Development")]
        [Tooltip("Set to true for fruits that are not spawned through a script (Only for Development!)")]
        public bool debugFruit;
#endif
        [Header("References")]
        [Tooltip("SpriteRenderer of the Fruit")]
        [SerializeField] private SpriteRenderer fruitSpriteRenderer;
        [Tooltip("SpriteRenderer of the fruits face")]
        [SerializeField] private SpriteRenderer faceSpriteRenderer;
        
        [Header("Settings")]
        [Tooltip("The type of this fruit")]
        [SerializeField] private Fruit fruit;
        #endregion
        
        #region Fields
        /// <summary>
        /// <see cref="CircleCollider2D"/>
        /// </summary>
        private CircleCollider2D circleCollider2D;
#pragma warning disable CS0109
        /// <summary>
        /// <see cref="UnityEngine.Rigidbody2D"/>
        /// </summary>
        private new Rigidbody2D rigidbody2D;
        /// <summary>
        /// Rotation animation while the fruit is in <see cref="NextFruit"/>
        /// </summary>
        private new Animation animation;
#pragma warning restore CS0109
        /// <summary>
        /// <see cref="FruitsFirstCollision"/>
        /// </summary>
        private FruitsFirstCollision fruitsFirstCollision;
        /// <summary>
        /// <see cref="EvolvingFruitTrigger"/>
        /// </summary>
        private EvolvingFruitTrigger evolvingFruitTrigger;
        
        /// <summary>
        /// The currently active <see cref="Skill"/> on this fruit <br/>
        /// <i>Will be null of none is active</i>
        /// </summary>
        private Skill? activeSkill; 
        /// <summary>
        /// Is true for a period of time, after the fruit had a collision with something
        /// </summary>
        private bool isHurt;
        /// <summary>
        /// Is true when teh fruit is inside the <see cref="MaxHeight"/> trigger
        /// </summary>
        private bool collisionWithMaxHeight;
        /// <summary>
        /// Indicates whether this fruit is an upgraded golden fruit
        /// </summary>
        private bool isUpgradedGoldenFruit;
        /// <summary>
        /// Disables most of the collision logic if true
        /// </summary>
        private bool disableEvolving;
        
        /// <summary>
        /// Coroutine for the movement towards another fruit, during evolving
        /// </summary>
        [CanBeNull] private IEnumerator moveTowards;
        /// <summary>
        /// Coroutine for the size grow, during evolving
        /// </summary>
        [CanBeNull] private IEnumerator evolve;
        #endregion

        #region Properties
#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// <see cref="rigidbody2D"/> <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public Rigidbody2D Rigidbody2D_DEVELOPMENT => this.rigidbody2D;
#endif
        
        /// <summary>
        /// <see cref="fruit"/>
        /// </summary>
        public Fruit Fruit => this.fruit;
        /// <summary>
        /// Indicates whether a fruit has been released from the <see cref="FruitSpawner"/> or not
        /// </summary>
        public bool HasBeenReleased { get; private set; }
        /// <summary>
        /// Indicates if a fruit has been spawned through evolving or not
        /// </summary>
        public bool HasBeenEvolved { get; private set; }
        /// <summary>
        /// Indicates whether this fruit is a golden fruit
        /// </summary>
        public bool IsGoldenFruit { get; private set; }
        /// <summary>
        /// Indicates whether this fruit is currently evolving with another fruit 
        /// </summary>
        public bool IsEvolving { get; private set; }
        /// <summary>
        /// <see cref="CircleCollider2D.radius"/> of the <see cref="circleCollider2D"/>
        /// </summary>
        public float ColliderRadius => this.circleCollider2D.radius;
        #endregion
        
        #region Events
        /// <summary>
        /// Is called when a fruit is released from the <see cref="FruitSpawner"/> <br/>
        /// <b>Parameter:</b> The <see cref="FruitBehaviour"/> that was released
        /// </summary>
        public static event Action<FruitBehaviour> OnFruitRelease;
        /// <summary>
        /// Is called when two fruits collide with each other <br/>
        /// <b>Parameter1:</b> Hashcode of the 1st fruit <br/>
        /// <b>Parameter2:</b> Hashcode of the 2nd fruit
        /// </summary>
        public static event Action<int, int> OnFruitCollision;
        /// <summary>
        /// Is called when an upgraded golden fruit collides with another fruit <br/>
        /// <b>Parameter1:</b> Hashcode of the golden fruit <br/>
        /// <b>Parameter2:</b> Hashcode of the other fruit
        /// </summary>
        public static event Action<int, int> OnUpgradedGoldenFruitCollision;
        /// <summary>
        /// Is called when a golden fruit collides with another fruit <br/>
        /// <b>Parameter1:</b> Hashcode of the fruit to destroy <br/>
        /// <b>Parameter2:</b> Indicates whether to force destroy the given fruit <br/>
        /// <b>Parameter2:</b> Indicates if the other fruit was a golden fruit or not -> <see cref="FruitController.GoldenFruitCollision"/>
        /// </summary>
        public static event Func<int, bool, bool> OnGoldenFruitCollision;
        /// <summary>
        /// Is called when a fruit leaves the visible area of the map
        /// </summary>
        public static event Action OnUpgradeToGoldenFruit;
        /// <summary>
        /// Is called when any kind of golden fruits spawns
        /// </summary>
        public static event Action OnGoldenFruitSpawn;
        /// <summary>
        /// Is called when a <see cref="Skill"/> is used <br/>
        /// <b>Parameter:</b> The <see cref="Skill"/> that was used <br/>
        /// <i>Parameter is nullable, but will never null (Needed for <see cref="FruitSpawner.DeactivateRotation"/>)</i>
        /// </summary>
        public static event Action<Skill?> OnSkillUsed;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.rigidbody2D = base.GetComponent<Rigidbody2D>();
            this.circleCollider2D = base.GetComponent<CircleCollider2D>();
            this.animation = base.GetComponent<Animation>();
            this.fruitsFirstCollision = base.GetComponent<FruitsFirstCollision>();
            this.evolvingFruitTrigger = base.GetComponentInChildren<EvolvingFruitTrigger>();
        }
        
        private void Start()
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (this.debugFruit)
            {
                this.HasBeenReleased = true;
                this.HasBeenEvolved = true;
                FruitController.AddFruit_DEVELOPMENT(this);
            }
#endif
            this.GoldenFruit();
        }

        private void OnBecameInvisible()
        {
            if (this.collisionWithMaxHeight)
            {
                var _centerPoint = base.transform.position.y;
                var _extends = base.transform.localScale.y / 2;
                var _lowestPoint = _centerPoint + _extends;
                var _isOutOfScreen = _lowestPoint > CameraUtils.YFrustumPosition;
                
                if (_isOutOfScreen)
                {
                    this.GoldenFruit(true);
                    OnUpgradeToGoldenFruit?.Invoke();
                }
            }
        }
        
#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// Force upgrades this <see cref="FruitBehaviour"/> to a upgraded golden fruit
        /// </summary>
        public void GoldenFruit_Debug()
        {
            this.GoldenFruit(true);
            OnUpgradeToGoldenFruit?.Invoke();
        }
#endif
        
        /// <summary>
        /// Decides whether a fruit can become a golden fruit or upgraded golden fruit
        /// </summary>
        /// <param name="_ForceEnable">Force upgrade if true</param>
        private void GoldenFruit(bool _ForceEnable = false)
        {
            if (!_ForceEnable)
            {
                var _notEnoughFruitsOnMap =
#if UNITY_EDITOR
                    (FruitSettings.GoldenFruitChance >= 100 ? FruitSettings.CanSpawnAfter : FruitController.FruitCount)
#else
                    FruitController.FruitCount
#endif
                    < FruitSettings.CanSpawnAfter;
                
                if (this.HasBeenEvolved || _notEnoughFruitsOnMap)
                {
                    return;
                }
                
                var _maxNumber = (int)(100 / FruitSettings.GoldenFruitChance); 
                var _numberToGet = Random.Range(1, _maxNumber);
                var _randomNumber = Random.Range(1, _maxNumber);
                
                if (_numberToGet != _randomNumber)
                {
                    return;
                }
            }
            
            this.IsGoldenFruit = true;
            this.isUpgradedGoldenFruit = _ForceEnable;
            Instantiate(FruitPrefabSettings.GoldenFruitPrefab, base.transform.position, Quaternion.identity, base.transform);
            OnGoldenFruitSpawn?.Invoke();
        }
        
        private void OnCollisionEnter2D(Collision2D _Other)
        {
            this.SetFruitFace();

            if (this.disableEvolving)
            {
                return;
            }
            
            var _powerSkillNotActive = this.activeSkill is not Skill.Power;
            var _doesntUseAutoMass = this.rigidbody2D.useAutoMass == false;
            if (_powerSkillNotActive && _doesntUseAutoMass)
            {
                this.SetMass(0, Operation.Set);
            }
            
            var _otherIsFruit = _Other.gameObject.layer == LayerMaskController.FruitLayer;
            var _thisHashcode = base.gameObject.GetHashCode();
            var _otherHashCode = _Other.gameObject.GetHashCode();
            
            if (_otherIsFruit)
            {
                if (this.IsGoldenFruit)
                {
                    if (this.isUpgradedGoldenFruit)
                    {
                        OnUpgradedGoldenFruitCollision?.Invoke(_thisHashcode, _otherHashCode);
                    }
                    else
                    {
                        OnGoldenFruitCollision?.Invoke(_otherHashCode, false);
                    }
                    return;
                }
                
                switch (this.activeSkill)
                {
                    case Skill.Evolve:
                        this.DeactivateSkill();
                        SkillController.Skill_Evolve(_otherHashCode);
                        return;
                    case Skill.Destroy:
                        this.DeactivateSkill();
                        SkillController.Skill_Destroy(_otherHashCode);
                        return;
                    default:
                        OnFruitCollision?.Invoke(_thisHashcode, _otherHashCode);
                        break;
                }
            }
            else if (this.IsGoldenFruit)
            {
                if (_Other.gameObject.CompareTag(TagController.WallBottom))
                {
                    this.DestroyFruit();
                }
            }

        }

        /// <summary>
        /// Sets the <see cref="Sprite"/> in <see cref="faceSpriteRenderer"/>
        /// </summary>
        private void SetFruitFace()
        {
            if (!this.isHurt)
            {
                this.isHurt = true;
                this.faceSpriteRenderer.sprite = FruitPrefabSettings.FaceHurt;
                this.Invoke(nameof(this.ResetFace), 1);
            }
        }
        
        /// <summary>
        /// Reset the <see cref="Sprite"/> in <see cref="faceSpriteRenderer"/> to its initial state
        /// </summary>
        private void ResetFace()
        {
            this.faceSpriteRenderer.sprite = FruitPrefabSettings.FaceDefault;
            this.isHurt = false;
        }
        
        private void OnTriggerEnter2D(Collider2D _Other)
        {
            var _otherIsMaxHeight = _Other.gameObject.layer == LayerMaskController.MaxHeightLayer;
            
            if (_otherIsMaxHeight)
            {
                this.collisionWithMaxHeight = true;
            }
        }

        private void OnTriggerExit2D(Collider2D _Other)
        {
            var _otherIsMaxHeight = _Other.gameObject.layer == LayerMaskController.MaxHeightLayer;

            if (_otherIsMaxHeight)
            {
                this.collisionWithMaxHeight = false;
            }
        }
        
        private void OnDestroy()
        {
            if (!GameController.IsApplicationQuitting)
            {
                if ((this.HasBeenReleased || this.HasBeenEvolved) && !this.IsEvolving)
                {
                    AudioPool.PlayClip(AudioClipName.FruitDestroy);
                }
            }
            if (this.evolve != null)
            {
                base.StopCoroutine(this.evolve);
            }
            if (this.moveTowards != null)
            {
                base.StopCoroutine(this.moveTowards);
            }
        }
        
        /// <summary>
        /// Enables/disables the <see cref="animation"/>
        /// </summary>
        /// <param name="_Value">The value, to set <see cref="animation"/>.<see cref="Animation.enabled"/> to</param>
        public void SetAnimation(bool _Value)
        {
            this.animation.enabled = _Value;
        }
        
        /// <summary>
        /// Releases the <see cref="Fruit"/> from the <see cref="FruitSpawner"/>
        /// </summary>
        /// <param name="_AimRotation">
        /// The rotation of <see cref="FruitSpawnerAim"/> <br/>
        /// <i>Only needed when a <see cref="Skill"/> is used</i>
        /// </param>
        public void Release(Vector2? _AimRotation = null)
        {
            base.transform.SetParent(null, true);
            this.HasBeenReleased = true;
            this.DecreaseSortingOrder();
            this.InitializeRigidBody();
            this.fruitsFirstCollision.SetActive();
            this.UseSkill(_AimRotation);
            OnFruitRelease?.Invoke(this);
        }
        
        /// <summary>
        /// Increase the <see cref="SpriteRenderer.sortingOrder"/> of the fruit
        /// </summary>
        public void IncreaseSortingOrder()
        {
            const int VALUE = 1;
            
            this.SetSortingOrder(VALUE);
        }
        
        /// <summary>
        /// Decreases the <see cref="SpriteRenderer.sortingOrder"/> of the fruit
        /// </summary>
        private void DecreaseSortingOrder()
        {
            const int VALUE = -1;
            
            this.SetSortingOrder(VALUE);
        }
        
        /// <summary>
        /// Adds the given value to the <see cref="SpriteRenderer.sortingOrder"/> of <see cref="fruitSpriteRenderer"/> and <see cref="faceSpriteRenderer"/> <br/>
        /// <i>Positive and negative values work</i>
        /// </summary>
        /// <param name="_Value">The value to add to the <see cref="SpriteRenderer.sortingOrder"/> of <see cref="fruitSpriteRenderer"/> and <see cref="faceSpriteRenderer"/></param>
        private void SetSortingOrder(int _Value)
        {
            this.fruitSpriteRenderer.sortingOrder += _Value;
            this.faceSpriteRenderer.sortingOrder += _Value;
        }
        
        /// <summary>
        /// Enables <see cref="Rigidbody2D.simulated"/> on this <see cref="rigidbody2D"/> and removes all <see cref="Rigidbody2D.constraints"/>
        /// </summary>
        private void InitializeRigidBody()
        {
            this.rigidbody2D.simulated = true;
            this.rigidbody2D.constraints = RigidbodyConstraints2D.None;
        }

        /// <summary>
        /// Uses the <see cref="Skill"/> set in <see cref="activeSkill"/>, if not null
        /// </summary>
        /// <param name="_AimRotation">The rotation of <see cref="FruitSpawnerAim"/></param>
        private void UseSkill(Vector2? _AimRotation = null)
        {
            if (this.activeSkill != null)
            {
                AudioPool.PlayClip(AudioClipName.Shoot);

                var _direction = _AimRotation ?? new Vector2(0, -1);
                
                switch (this.activeSkill)
                {
                    case Skill.Evolve or Skill.Destroy:
                        this.Shoot(_direction);
                        break;
                    case Skill.Power:
                        SkillController.Skill_Power(this, _direction);
                        break;
                }
                
                OnSkillUsed?.Invoke(this.activeSkill.Value);
            }
            else
            {
                this.SetMass(FruitSettings.MassMultiplier, Operation.Multiply);
                AudioPool.PlayClip(AudioClipName.ReleaseFruit);
            }
        }
        
        /// <summary>
        /// Sets the <see cref="Rigidbody2D.mass"/> of this <see cref="rigidbody2D"/> <br/>
        /// <i>Resets the mass, if the given value is less or equal to 0</i>
        /// </summary>
        /// <param name="_Mass">The value to use</param>
        /// <param name="_Operation"><see cref="Operation"/></param>
        public void SetMass(float _Mass, Operation _Operation)
        {
            if (_Mass <= 0)
            {
                this.rigidbody2D.useAutoMass = true;
            }
            else
            {
                this.rigidbody2D.useAutoMass = false;
                switch (_Operation)
                {
                    case Operation.Set:
                        this.rigidbody2D.mass = _Mass;
                        break;
                    case Operation.Add:
                        this.rigidbody2D.mass += _Mass;
                        break;
                    case Operation.Multiply:
                        this.rigidbody2D.mass *= _Mass;
                        break;
                }
            }
        }

        /// <summary>
        /// Applies a force on the <see cref="rigidbody2D"/>
        /// </summary>
        /// <param name="_Direction">The direction of the force</param>
        /// <param name="_ForceMode"><see cref="ForceMode2D"/></param>
        public void AddForce(Vector2 _Direction, ForceMode2D _ForceMode)
        {
            this.rigidbody2D.AddForce(_Direction, _ForceMode);
        }
        
        /// <summary>
        /// Starts moving towards the given <see cref="FruitBehaviour"/>, to evolve with it
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to evolve with</param>
        public void MoveTowards(FruitBehaviour _FruitBehaviour)
        {
            this.IsEvolving = true;
            this.evolvingFruitTrigger.SetFruitToEvolveWith(_FruitBehaviour);
            base.gameObject.layer = LayerMaskController.EvolvingFruitLayer;
            this.SetMass(FruitSettings.EvolveMass, Operation.Set);
            this.moveTowards = InternalMoveTowards(_FruitBehaviour);
            base.StartCoroutine(this.moveTowards);
        }

        /// <summary>
        /// Moves this <see cref="FruitBehaviour"/> towards the given <see cref="FruitBehaviour"/> 
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to move towards</param>
        /// <returns></returns>
        private IEnumerator InternalMoveTowards(FruitBehaviour _FruitBehaviour)
        {
            this.rigidbody2D.velocity = Vector2.zero;
            var _maxDistanceDelta = this.GetSize() * FruitSettings.MoveTowardsStepMultiplier;
            
            while (base.transform.position != _FruitBehaviour.transform.position)
            {
                var _newPosition = Vector2.MoveTowards(base.transform.position, _FruitBehaviour.transform.position, _maxDistanceDelta);
                this.rigidbody2D.MovePosition(_newPosition);
                yield return FruitSettings.MoveTowardsWaitForSeconds;
            }
            
            base.StopCoroutine(this.moveTowards);
        }

        /// <summary>
        /// Returns the size of this fruit
        /// </summary>
        /// <returns>The size of this fruit</returns>
        public float GetSize()
        {
            var _localScale = base.transform.localScale;
            return (_localScale.x + _localScale.y + _localScale.z) / 3;
        }
        
        /// <summary>
        /// Finalizes the evolve process
        /// </summary>
        public void Evolve()
        {
            var _targetScale = base.transform.localScale;
            base.transform.localScale = Vector3.zero;
            base.gameObject.SetActive(true);
            this.fruitsFirstCollision.DestroyComponent();
            this.InitializeRigidBody();
            
            this.evolve = this.Evolve(_targetScale);
            base.StartCoroutine(this.evolve);
        }
        
        /// <summary>
        /// Grow this fruit to its target scale
        /// </summary>
        /// <param name="_TargetScale">The targeted scale of this fruit</param>
        /// <returns></returns>
        private IEnumerator Evolve(Vector3 _TargetScale)
        {
            var _scaleStep = FruitSettings.EvolveStep;
            
            while (base.transform.localScale.x < _TargetScale.x)
            {
                base.transform.localScale += _scaleStep;
                yield return FruitSettings.EvolveWaitForSeconds;
            }
        }
        
        /// <summary>
        /// Sets the given <see cref="Skill"/> as currently active
        /// </summary>
        /// <param name="_ActiveSkill">The <see cref="Skill"/> to activate</param>
        public void SetActiveSkill(Skill? _ActiveSkill)
        {
            this.activeSkill = _ActiveSkill;
        }
        
        /// <summary>
        /// Deactivates the currently active <see cref="Skill"/>
        /// </summary>
        private void DeactivateSkill()
        {
            this.activeSkill = null;
        }

        /// <summary>
        /// Shoots the fruit with increased force in  the given direction
        /// </summary>
        /// <param name="_Direction">The direction to shoot the fruit in</param>
        private void Shoot(Vector2 _Direction)
        {
            this.rigidbody2D.AddForce(_Direction * (SkillController.ShootForceMultiplier * this.rigidbody2D.mass), ForceMode2D.Impulse);
        }

        /// <summary>
        /// <see cref="disableEvolving"/>
        /// </summary>
        public void DisableEvolving()
        {
            this.disableEvolving = true;
        }
        
        /// <summary>
        /// Destroys this <see cref="GameObject"/> and removes its entry from <see cref="FruitController.fruits"/>
        /// </summary>
        public void DestroyFruit()
        {
            // TODO: Add a visual animation
            FruitController.RemoveFruit(base.gameObject.GetHashCode());
            Destroy(base.gameObject);
        }
        
        /// <summary>
        /// Instantiates a random fruit <br/>
        /// <i>For <see cref="FruitSpawner"/> and <see cref="NextFruit"/></i>
        /// </summary>
        /// <param name="_Position">Where to spawn the fruit</param>
        /// <param name="_Parent">The parent object of the spawned fruit</param>
        /// <param name="_PreviousFruit">The previously spawned <see cref="Fruits.Fruit"/></param>
        /// <returns>The <see cref="FruitBehaviour"/> of the spawned fruit <see cref="GameObject"/></returns>
        public static FruitBehaviour SpawnFruit(Vector2 _Position, Transform _Parent, Fruit? _PreviousFruit)
        {
            var _fruitData = GetRandomFruit(_PreviousFruit);
            var _fruitBehaviour = Instantiate(_fruitData.Prefab, _Position, Quaternion.identity, _Parent).GetComponent<FruitBehaviour>();
            
            _fruitBehaviour.gameObject.SetActive(true);
            _fruitBehaviour.SetAnimation(true);

            return _fruitBehaviour;
        }

        /// <summary>
        /// Returns a random <see cref="Fruits.Fruit"/> which spawn weight depend on the given <see cref="_PreviousFruit"/>
        /// </summary>
        /// <param name="_PreviousFruit">The previously spawned <see cref="Fruits.Fruit"/></param>
        /// <returns>A random <see cref="Fruits.Fruit"/> which spawn weight depend on the given <see cref="_PreviousFruit"/></returns>
        private static FruitPrefab GetRandomFruit(Fruit? _PreviousFruit)
        {
            if (_PreviousFruit == null)
            {
                return FruitPrefabSettings.FruitPrefabs.First(_FruitData => _FruitData.Fruit == Fruit.Grape);
            }
         
            FruitController.SetWeightMultiplier(_PreviousFruit.Value);

            var _highestFruitSpawn = FruitPrefabSettings.FruitPrefabs.First(_Fruit => _Fruit.GetSpawnWeight() == 0).Fruit;
            var _spawnableFruits = FruitPrefabSettings.FruitPrefabs.TakeWhile(_Fruit => (int)_Fruit.Fruit < (int)_highestFruitSpawn).ToArray();
            
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
        /// Instantiates a specific fruit <br/>
        /// <i>For evolved fruits</i> <br/>
        /// <b>The fruit GameObject will not be active</b>
        /// </summary>
        /// <param name="_Position">Where to spawn the fruit</param>
        /// <param name="_Fruit">The <see cref="Fruits.Fruit"/> to spawn</param>
        /// <param name="_Rotation">The rotation of the spawned fruit</param>
        /// <returns>The <see cref="FruitBehaviour"/> of the spawned fruit <see cref="GameObject"/></returns>
        public static FruitBehaviour SpawnFruit(Vector2 _Position, Fruit _Fruit, Quaternion? _Rotation = null)
        {
            var _fruitData = FruitPrefabSettings.FruitPrefabs.First(_FruitData => _FruitData.Fruit == _Fruit);
            var _fruitBehavior = Instantiate(_fruitData.Prefab, _Position, _Rotation ?? Quaternion.identity).GetComponent<FruitBehaviour>();
            
            _fruitBehavior.HasBeenEvolved = true;

            return _fruitBehavior;
        }
        #endregion
    }
}
