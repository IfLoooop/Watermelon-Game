using Mirror;
using OPS.AntiCheat.Field;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruits;
using Watermelon_Game.Skills;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Fruit_Spawn
{
    internal sealed class FruitSpawner : NetworkBehaviour
    {
        #region Inspector Fields
#if DEBUG || DEVELOPMENT_BUILD
        [Header("Development")]
        [Tooltip("Deactivates the release block cool down if true (Development only!)")]
        [Sirenix.OdinInspector.ShowInInspector] public static bool NoReleaseBlock;
#endif
        
        [Header("References")] 
        [Tooltip("Reference to the FruitSpawnerAim component of this FruitSpawner")]
        [SerializeField] private FruitSpawnerAim fruitSpawnerAim;
        
        [Header("Settings")]
        [Tooltip("Speed the FruitSpawner moves with")]
        [SerializeField] private ProtectedFloat movementSpeed = 2f;
        [Tooltip("Speed the aim rotates with")]
        [SerializeField] private ProtectedFloat rotationSpeed = 50f;
        [Tooltip("The maximum angle of FruitSpawnerAim (Relative to the FruitSpawners y-axis)")]
        [SerializeField] private ProtectedFloat maxRotationAngle = 80f;
        [Tooltip("Minimum cooldown between fruit releases (In Seconds)")]
        [SerializeField] private ProtectedFloat releaseCooldown = .375f;
        [Tooltip("Is added to the size of the FruitSpawners BoxCollider2D")]
        [SerializeField] private ProtectedFloat colliderSizeOffset = 3.85f; 
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="FruitSpawner"/>
        /// </summary>
        private static FruitSpawner instance;
        
#pragma warning disable CS0109
        /// <summary>
        /// <see cref="Rigidbody2D"/>
        /// </summary>
        private new Rigidbody2D rigidbody2D;
#pragma warning restore CS0109
        /// <summary>
        /// <see cref="BoxCollider2D"/>
        /// </summary>
        private BoxCollider2D fruitSpawnerCollider;
        /// <summary>
        /// Detects if <see cref="fruitBehaviour"/> is overlapping with a fruit in <see cref="FruitController.fruits"/>
        /// </summary>
        private CircleCollider2D fruitTrigger;
        
        /// <summary>
        /// Uses the position the GameObject has at start of game <br/>
        /// <b>Should not be modified afterwards</b>
        /// </summary>
        private Vector2 startingPosition;
        /// <summary>
        /// <see cref="Time.time"/> in seconds, of the last fruit release -> <see cref="ReleaseFruit"/>
        /// </summary>
        private ProtectedFloat lastRelease;
        
        // TODO: Use InputController
        /// <summary>
        /// Blocks movement input while this field is set to true
        /// </summary>
        private ProtectedBool blockInput;
        /// <summary>
        /// Blocks fruit release while true
        /// </summary>
        private ProtectedBool blockRelease;
        /// <summary>
        /// Index of the <see cref="AudioWrapper"/> in <see cref="AudioPool.assignedAudioWrappers"/>, for the <see cref="AudioClipName.BlockedRelease"/> <see cref="AudioClip"/>
        /// </summary>
        private int blockedReleaseIndex;
        
        /// <summary>
        /// The <see cref="FruitBehaviour"/> that is currently attached to this <see cref="FruitSpawner"/> 
        /// </summary>
        private FruitBehaviour fruitBehaviour;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="rotationSpeed"/>
        /// </summary>
        public static ProtectedFloat RotationSpeed => instance.rotationSpeed;
        /// <summary>
        /// <see cref="maxRotationAngle"/>
        /// </summary>
        public static ProtectedFloat MaxRotationAngle => instance.maxRotationAngle;
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
            
            this.rigidbody2D = base.GetComponent<Rigidbody2D>();
            this.fruitSpawnerCollider = base.GetComponent<BoxCollider2D>();
            this.fruitTrigger = base.GetComponentInChildren<CircleCollider2D>();
            
            this.startingPosition = base.transform.position;
        }

        private void OnEnable()
        {
            GameController.OnGameStart += this.GameStarted;
            GameController.OnResetGameStarted += this.ResetGameStarted;
            GameController.OnResetGameFinished += this.ResetGameFinished;
            FruitsFirstCollision.OnCollision += this.UnblockRelease;
            SkillController.OnSkillActivated += this.SetActiveSkill;
            FruitBehaviour.OnSkillUsed += DeactivateRotation;
        }

        private void OnDisable()
        {
            GameController.OnGameStart -= this.GameStarted;
            GameController.OnResetGameStarted -= this.ResetGameStarted;
            GameController.OnResetGameFinished -= this.ResetGameFinished;
            FruitsFirstCollision.OnCollision -= this.UnblockRelease;
            SkillController.OnSkillActivated -= this.SetActiveSkill;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            if (base.isLocalPlayer)
            {
                GameController.StartGame(); // TODO: Only for testing
            }
        }

        private void Start()
        {
            this.blockedReleaseIndex = AudioPool.CreateAssignedAudioWrapper(AudioClipName.BlockedRelease, base.transform);
        }

        /// <summary>
        /// <see cref="GameController.OnGameStart"/>
        /// </summary>
        private void GameStarted()
        {
            this.ResetFruitSpawner(true);
            this.fruitSpawnerAim.EnableAim(true);
            
            this.BlockInput(false);
            this.UnblockRelease();
        }

        /// <summary>
        /// <see cref="GameController.OnResetGameStarted"/>
        /// </summary>
        private void ResetGameStarted()
        {
            this.BlockInput(true);
            this.fruitSpawnerAim.EnableAim(false);
        }
        
        /// <summary>
        /// <see cref="GameController.OnResetGameFinished"/>
        /// </summary>
        private void ResetGameFinished()
        {
            Destroy(this.fruitBehaviour.gameObject);
            this.fruitBehaviour = null;
            this.BlockInput(false);
        }
        
        /// <summary>
        /// Sets <see cref="blockInput"/> to true
        /// </summary>
        /// <param name="_Value">The value to set <see cref="blockInput"/> to</param>
        private void BlockInput(bool _Value)
        {
            this.blockInput = _Value;
        }
        
        /// <summary>
        /// Sets <see cref="blockRelease"/> to false
        /// </summary>
        private void UnblockRelease()
        {
            this.BlockRelease(false);
        }
        
        /// <summary>
        /// Sets <see cref="blockRelease"/> to the given value
        /// </summary>
        /// <param name="_Value">The value <see cref="blockRelease"/> should be set to</param>
        private void BlockRelease(bool _Value)
        {
            this.blockRelease = _Value;
            this.fruitSpawnerAim.EnableAim(!this.blockRelease);
        }
        
        private void Update()
        {
            this.GetInput();
        }
        
        /// <summary>
        /// Handles the input for the <see cref="FruitSpawner"/>
        /// </summary>
        private void GetInput()
        {
            if (!base.isLocalPlayer)
            {
                return;
            }
            
            if (!this.blockInput)
            {
                if (Input.GetKey(KeyCode.A))
                {
                    this.Move(Vector2.left);
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    this.Move(Vector2.right);
                }

                if (Input.GetKey(KeyCode.Space) && !this.blockRelease)
                {
                    this.ReleaseFruit();
                }   
            }
        }
        
        /// <summary>
        /// Moves the <see cref="FruitSpawner"/> in the given <see cref="_Direction"/>
        /// </summary>
        /// <param name="_Direction">The direction to move the <see cref="FruitSpawner"/> in</param>
        private void Move(Vector2 _Direction)
        {
            var _direction = _Direction * (this.movementSpeed * Time.deltaTime);

            this.rigidbody2D.AddForce(_direction);
        }
        
                /// <summary>
        /// Resets the Fruit Spawner to its original position
        /// </summary>
        /// <param name="_ResetPosition">If true, resets the <see cref="FruitSpawner"/> position to <see cref="startingPosition"/></param>
        [Client]
        private void ResetFruitSpawner(bool _ResetPosition)
        {
            if (_ResetPosition)
                this.rigidbody2D.MovePosition(this.startingPosition);
            
            var _fruit = NextFruit.GetFruit(out var _rotation);
            this.CmdResetFruitSpawner(_fruit, _rotation);
        }
        
        /// <summary>
        /// <see cref="ResetFruitSpawner"/>
        /// </summary>
        /// <param name="_Fruit">The type of <see cref="Fruit"/> to spawn</param>
        /// <param name="_Rotation">The rotation for the spawned fruit</param>
        /// <param name="_Sender"><see cref="NetworkBehaviour.connectionToClient"/></param>
        [Command(requiresAuthority = false)]
        private void CmdResetFruitSpawner(int _Fruit, Quaternion _Rotation, NetworkConnectionToClient _Sender = null)
        {
            if (_Sender == base.netIdentity.connectionToClient)
            {
                var _fruitBehaviour = FruitBehaviour.SpawnFruit(base.transform, base.transform.position, _Rotation, _Fruit, false);
                NetworkServer.Spawn(_fruitBehaviour.gameObject);
                _fruitBehaviour.netIdentity.AssignClientAuthority(_Sender);
                this.TargetResetFruitSpawner(_Sender, _fruitBehaviour);
            }
        }
        
        /// <summary>
        /// <see cref="ResetFruitSpawner"/>
        /// </summary>
        /// <param name="_Target"><see cref="NetworkBehaviour.connectionToClient"/></param>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/>, the <see cref="FruitSpawner"/> got from <see cref="NextFruit"/></param>
        [TargetRpc] // ReSharper disable once UnusedParameter.Local
        private void TargetResetFruitSpawner(NetworkConnectionToClient _Target, FruitBehaviour _FruitBehaviour)
        {
            this.fruitBehaviour = _FruitBehaviour;
            this.fruitBehaviour.transform.SetParent(base.transform, false);
            this.fruitBehaviour.IncreaseSortingOrder();
            this.fruitSpawnerAim.ResetAimRotation();
            this.fruitSpawnerCollider.size = new Vector2(this.fruitBehaviour.GetSize() + colliderSizeOffset, this.fruitSpawnerCollider.size.y);
            this.SetFruitTriggerSize(this.fruitBehaviour);
        }
        
        /// <summary>
        /// Sets the size of <see cref="fruitTrigger"/> to the size of the currently held <see cref="fruitBehaviour"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="FruitBehaviour"/> to get the size of</param>
        private void SetFruitTriggerSize(FruitBehaviour _Fruit)
        {
            this.fruitTrigger.transform.localScale = _Fruit.transform.localScale;
            this.fruitTrigger.radius = _Fruit.ColliderRadius;
        }
        
        /// <summary>
        /// Releases <see cref="fruitBehaviour"/> from this <see cref="FruitSpawner"/>
        /// </summary>
        [Client]
        private void ReleaseFruit()
        {
            if (this.fruitBehaviour == null)
            {
                return;
            }
            
            var _time = Time.time - this.releaseCooldown;
            var _releaseCooldown = _time < this.lastRelease;
            var _fruitInTrigger = this.fruitTrigger.IsTouchingLayers(LayerMaskController.Fruit_EvolvingFruit_Mask);
            if (_releaseCooldown || _fruitInTrigger)
            {
                if (!AudioPool.IsAssignedClipPlaying(this.blockedReleaseIndex) && _time > this.lastRelease)
                {
                    AudioPool.PlayAssignedClip(this.blockedReleaseIndex);
                }
                return;
            }
            
#if DEBUG || DEVELOPMENT_BUILD
            if (NoReleaseBlock) goto skipCooldown;
#endif
            
            this.BlockRelease(true);
            
#if DEBUG || DEVELOPMENT_BUILD
            skipCooldown:;
#endif
            this.lastRelease = Time.time;
            this.CmdReleaseFruit(this.fruitBehaviour, -this.fruitSpawnerAim.transform.up);
            this.ResetFruitSpawner(false);
        }
        
        /// <summary>
        /// <see cref="ReleaseFruit"/>
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to release</param>
        /// <param name="_AimRotation">The direction, the <see cref="fruitSpawnerAim"/> is pointing at</param>
        /// <param name="_Sender"><see cref="NetworkBehaviour.connectionToClient"/></param>
        [Command(requiresAuthority = false)] // ReSharper disable once UnusedParameter.Local
        private void CmdReleaseFruit(FruitBehaviour _FruitBehaviour, Vector2 _AimRotation, NetworkConnectionToClient _Sender = null)
        {
            if (_Sender == base.netIdentity.connectionToClient)
            {
                this.TargetRelease(_Sender, _FruitBehaviour, _AimRotation);
            }
        }
        
        /// <summary>
        /// <see cref="ReleaseFruit"/>
        /// </summary>
        /// <param name="_Target"><see cref="NetworkBehaviour.connectionToClient"/></param>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to release</param>
        /// <param name="_AimRotation">The direction, the <see cref="fruitSpawnerAim"/> is pointing at</param>
        [TargetRpc] // ReSharper disable once UnusedParameter.Local
        private void TargetRelease(NetworkConnectionToClient _Target, FruitBehaviour _FruitBehaviour, Vector2 _AimRotation)
        {
            _FruitBehaviour.CmdRelease(_AimRotation);
        }
        
        /// <summary>
        /// Sets the currently active <see cref="Skill"/> on the <see cref="fruitBehaviour"/> currently held by the <see cref="FruitSpawner"/>
        /// </summary>
        /// <param name="_ActiveSkill">
        /// The currently active <see cref="Skill"/> <br/>
        /// <i>Null means no <see cref="Skill"/> is currently active</i>
        /// </param>
        private void SetActiveSkill(Skill? _ActiveSkill)
        {
            if (_ActiveSkill != null)
            {
                this.fruitBehaviour.SetActiveSkill(_ActiveSkill);
                this.fruitSpawnerAim.ActivateRotationButtons(true);   
            }
            else
            {
                this.fruitBehaviour.SetActiveSkill(null);
                this.DeactivateRotation(null);
            }
        }

        /// <summary>
        /// Deactivates the rotation of <see cref="FruitSpawnerAim"/>
        /// </summary>
        /// <param name="_">Not needed here</param>
        private void DeactivateRotation(Skill? _)
        {
            this.fruitSpawnerAim.ActivateRotationButtons(false);  
            this.fruitSpawnerAim.ResetAimRotation();
        }
        
#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// Forces the <see cref="fruitBehaviour"/> that is currently held by the <see cref="FruitSpawner"/> to become a golden fruit <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        public static void ForceGoldenFruit_DEVELOPMENT()
        {
            instance.fruitBehaviour.ForceGoldenFruit_DEVELOPMENT(true);
        }
        
        /// <summary>
        /// Forces the <see cref="FruitSpawner"/> tp spawn the given <see cref="Fruit"/> <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to give to the <see cref="FruitSpawner"/></param>
        public static void ForceFruit_DEVELOPMENT(Fruit _Fruit)
        {
            if (instance.fruitBehaviour != null)
            {
                Destroy(instance.fruitBehaviour.gameObject);
            }

            var _transform = instance.transform;
            instance.fruitBehaviour = FruitBehaviour.SpawnFruit(_transform, _transform.position, Quaternion.identity, (int)_Fruit, true);
            instance.fruitBehaviour.IncreaseSortingOrder();
            instance.fruitSpawnerAim.ResetAimRotation();
            instance.fruitSpawnerCollider.size = new Vector2(instance.fruitBehaviour.GetSize() + instance.colliderSizeOffset, instance.fruitSpawnerCollider.size.y);
            instance.SetFruitTriggerSize(instance.fruitBehaviour);
        }
#endif
        #endregion
    }   
}