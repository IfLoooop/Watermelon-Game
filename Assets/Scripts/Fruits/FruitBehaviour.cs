using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using Mirror;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
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
    internal sealed class FruitBehaviour : NetworkBehaviour
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
        [ValueDropdown("fruits")]
        [SerializeField] private ProtectedInt32 fruit;
        // ReSharper disable once UnusedMember.Local
        private static IEnumerable fruits = new ValueDropdownList<ProtectedInt32>
        {
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Grape)}", (int)Watermelon_Game.Fruits.Fruit.Grape },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Cherry)}", (int)Watermelon_Game.Fruits.Fruit.Cherry },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Strawberry)}", (int)Watermelon_Game.Fruits.Fruit.Strawberry },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Lemon)}", (int)Watermelon_Game.Fruits.Fruit.Lemon },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Orange)}", (int)Watermelon_Game.Fruits.Fruit.Orange },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Apple)}", (int)Watermelon_Game.Fruits.Fruit.Apple },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Pear)}", (int)Watermelon_Game.Fruits.Fruit.Pear },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Pineapple)}", (int)Watermelon_Game.Fruits.Fruit.Pineapple },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Honeymelon)}", (int)Watermelon_Game.Fruits.Fruit.Honeymelon },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Watermelon)}", (int)Watermelon_Game.Fruits.Fruit.Watermelon },
        };
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
        /// Network synced <see cref="Transform.localScale"/>
        /// </summary>
        [SyncVar(hook = nameof(SyncScale))] // ReSharper disable once NotAccessedField.Local
        private Vector3 syncedScale;
        
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
        private ProtectedBool collisionWithMaxHeight;
        /// <summary>
        /// Indicates whether this fruit is an upgraded golden fruit
        /// </summary>
        private ProtectedBool isUpgradedGoldenFruit;
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
        public ProtectedInt32 Fruit => this.fruit;
        /// <summary>
        /// Indicates whether a fruit has been released from the <see cref="FruitSpawner"/> or not
        /// </summary>
        public ProtectedBool HasBeenReleased { get; private set; }
        /// <summary>
        /// Indicates if a fruit has been spawned through evolving or not
        /// </summary>
        public ProtectedBool HasBeenEvolved { get; private set; }
        /// <summary>
        /// Indicates whether this fruit is a golden fruit
        /// </summary>
        public ProtectedBool IsGoldenFruit { get; private set; }
        /// <summary>
        /// Indicates whether this fruit is currently evolving with another fruit 
        /// </summary>
        public ProtectedBool IsEvolving { get; private set; }
        /// <summary>
        /// <see cref="CircleCollider2D.radius"/> of the <see cref="circleCollider2D"/>
        /// </summary>
        public ProtectedFloat ColliderRadius => this.circleCollider2D.radius;
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
        /// Is called when any kind of golden fruits spawns <br/>
        /// <b>Parameter:</b> Indicates whether the golden fruit is an upgraded golden fruit or not
        /// </summary>
        public static event Action<bool> OnGoldenFruitSpawn;
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
        /// Force upgrades this <see cref="FruitBehaviour"/> to a upgraded golden fruit <br/>
        /// <b>Development only!</b>
        /// </summary>
        /// <param name="_ForceGolden">Forces this <see cref="FruitBehaviour"/> to become a golden fruit instead of an upgraded golden fruit</param>
        public void ForceGoldenFruit_DEVELOPMENT(bool _ForceGolden = false)
        {
            this.GoldenFruit(true, _ForceGolden);
            OnUpgradeToGoldenFruit?.Invoke();
        }
#endif
        
        // TODO: Calculate this in "NextFruit.cs" and already display the golden fruit there
        /// <summary>
        /// Decides whether a fruit can become a golden fruit or upgraded golden fruit
        /// </summary>
        /// <param name="_ForceEnable">Force upgrade if true</param>
        /// <param name="_ForceGolden">
        /// Forces this <see cref="FruitBehaviour"/> to become a golden fruit instead of an upgraded golden fruit <br/>
        /// <b>Works only in Development!</b>
        /// </param>
        private void GoldenFruit(bool _ForceEnable = false, bool _ForceGolden = false)
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
            
#if DEBUG || DEVELOPMENT_BUILD
            if (_ForceGolden) goto skipUpgraded;
#endif
            this.isUpgradedGoldenFruit = _ForceEnable;
            
#if DEBUG || DEVELOPMENT_BUILD
            skipUpgraded:;
#endif
            Instantiate(FruitPrefabSettings.GoldenFruitPrefab, base.transform.position, Quaternion.identity, base.transform);
            OnGoldenFruitSpawn?.Invoke(this.isUpgradedGoldenFruit);
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
        /// Releases the <see cref="Fruit"/> from the <see cref="FruitSpawner"/>
        /// </summary>
        /// <param name="_AimRotation">The rotation of <see cref="FruitSpawnerAim"/></param>
        /// <param name="_Sender"><see cref="NetworkBehaviour.connectionToClient"/></param>
        [Command(requiresAuthority = false)]
        public void CmdRelease(Vector2 _AimRotation, NetworkConnectionToClient _Sender = null)
        {
            this.TargetRelease(_Sender, _AimRotation);
        }

        /// <summary>
        /// <see cref="CmdRelease"/>
        /// </summary>
        /// <param name="_Target"><see cref="NetworkBehaviour.connectionToClient"/></param>
        /// <param name="_AimRotation">The rotation of <see cref="FruitSpawnerAim"/></param>
        [TargetRpc] // ReSharper disable once UnusedParameter.Local
        private void TargetRelease(NetworkConnectionToClient _Target, Vector2 _AimRotation)
        { 
            base.transform.SetParent(FruitController.FruitContainerTransform, true); // TODO: Fruit is still in FruitSpawner on the client (doesn't affect position, maybe a bug os supposed to be like that)
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
        private void UseSkill(Vector2 _AimRotation)
        {
            if (this.activeSkill != null)
            {
                AudioPool.PlayClip(AudioClipName.Shoot);
                
                switch (this.activeSkill)
                {
                    case Skill.Evolve or Skill.Destroy:
                        this.Shoot(_AimRotation);
                        break;
                    case Skill.Power:
                        SkillController.Skill_Power(this, _AimRotation);
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
        
        
        // TODO: Make "_targetScale" a class field and activate all fruit prefabs from th start
        /// <summary>
        /// Finalizes the evolve process
        /// </summary>
        public void Evolve()
        {
            var _targetScale = base.transform.localScale;
            base.transform.localScale = Vector3.zero;
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
            while (base.transform.localScale.x < _TargetScale.x)
            {
                var _localScale = base.transform.localScale + (_TargetScale + FruitSettings.EvolveStep.Value) * Time.deltaTime;
                this.syncedScale = _localScale.Clamp(Vector3.zero, _TargetScale);
                yield return FruitSettings.EvolveWaitForSeconds;
            }
        }
        
        /// <summary>
        /// Hook for <see cref="syncedScale"/>
        /// </summary>
        /// <param name="_OldScale">Previous value</param>
        /// <param name="_NewScale">New value</param>
        // ReSharper disable once UnusedParameter.Local
        private void SyncScale(Vector3 _OldScale, Vector3 _NewScale)
        {
            this.transform.localScale = _NewScale;
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
            //Destroy(base.gameObject); // TODO
            this.CmdDestroyFruit(base.gameObject);
        }

        /// <summary>
        /// <see cref="DestroyFruit"/>
        /// </summary>
        /// <param name="_Fruit">The fruit <see cref="GameObject"/> to destroy</param>
        [Command(requiresAuthority = false)]
        private void CmdDestroyFruit(GameObject _Fruit)
        {
            NetworkServer.Destroy(_Fruit);
        }
        
        /// <summary>
        /// Instantiates the given <see cref="Fruits.Fruit"/>
        /// </summary>
        /// <param name="_Parent">The parent <see cref="Transform"/> of the instantiated <see cref="FruitBehaviour"/></param>
        /// <param name="_Position">Where to spawn the fruit</param>
        /// <param name="_Rotation">The rotation of the spawned fruit</param>
        /// <param name="_Fruit">The <see cref="Fruits.Fruit"/> to spawn</param>
        /// <param name="_HasBeenEvolved">Is this fruit spawned because of an evolution?</param>
        /// <returns>The <see cref="FruitBehaviour"/> of the spawned fruit <see cref="GameObject"/></returns>
        public static FruitBehaviour SpawnFruit(Transform _Parent, Vector2 _Position, Quaternion _Rotation, ProtectedInt32 _Fruit, bool _HasBeenEvolved)
        {
            var _fruitData = FruitPrefabSettings.FruitPrefabs.First(_FruitData => _FruitData.Fruit == _Fruit);
            var _fruitBehaviour = Instantiate(_fruitData.Prefab, _Position, _Rotation, _Parent).GetComponent<FruitBehaviour>();
            
            _fruitBehaviour.HasBeenEvolved = _HasBeenEvolved;

            return _fruitBehaviour;
        }

#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// If true, sets <see cref="rigidbody2D"/>.<see cref="Rigidbody2D.constraints"/> to <see cref="RigidbodyConstraints2D.FreezeAll"/> <br/>
        /// if false, sets <see cref="rigidbody2D"/>.<see cref="Rigidbody2D.constraints"/> to <see cref="RigidbodyConstraints2D.None"/> <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        /// <param name="_Freeze">Whether this <see cref="FruitBehaviour"/> should be frozen or not</param>
        public void Freeze_DEVELOPMENT(bool _Freeze)
        {
            this.rigidbody2D.constraints = _Freeze ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.None;
        }
#endif
        #endregion
    }
}
