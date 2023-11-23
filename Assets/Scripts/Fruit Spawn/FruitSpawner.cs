using Mirror;
using OPS.AntiCheat.Field;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Container;
using Watermelon_Game.Controls;
using Watermelon_Game.Fruits;
using Watermelon_Game.Menus;
using Watermelon_Game.Networking;
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
        
#pragma warning disable CS0109
        #region Fields
        /// <summary>
        /// Singleton of <see cref="FruitSpawner"/>
        /// </summary>
        private static FruitSpawner instance;

        /// <summary>
        /// <see cref="NetworkConnectionToClient.connectionId"/>
        /// </summary>
        private ProtectedInt32 connectionId;
        /// <summary>
        /// Container for this <see cref="FruitSpawner"/>
        /// </summary>
        private ContainerBounds containerBounds;
        
        /// <summary>
        /// <see cref="Rigidbody2D"/>
        /// </summary>
        private new Rigidbody2D rigidbody2D;
        /// <summary>
        /// <see cref="BoxCollider2D"/>
        /// </summary>
        private BoxCollider2D fruitSpawnerCollider;
        /// <summary>
        /// Detects if <see cref="fruitBehaviour"/> is overlapping with a fruit in <see cref="FruitController.fruits"/>
        /// </summary>
        private CircleCollider2D fruitTrigger;
        
        // TODO: Use InputController
        /// <summary>
        /// Blocks movement input while this field is set to true
        /// </summary>
        private ProtectedBool inputBlocked;
        /// <summary>
        /// Blocks fruit release while true
        /// </summary>
        private ProtectedBool blockRelease;
        /// <summary>
        /// <see cref="Time.time"/> in seconds, of the last fruit release -> <see cref="ReleaseFruit"/>
        /// </summary>
        private ProtectedFloat lastRelease;
        /// <summary>
        /// Index of the <see cref="AudioWrapper"/> in <see cref="AudioPool.assignedAudioWrappers"/>, for the <see cref="AudioClipName.BlockedRelease"/> <see cref="AudioClip"/>
        /// </summary>
        private int blockedReleaseIndex;
        
        /// <summary>
        /// The <see cref="FruitBehaviour"/> that is currently attached to this <see cref="FruitSpawner"/> 
        /// </summary>
        private FruitBehaviour fruitBehaviour;
        /// <summary>
        /// Indicates whether any <see cref="Skill"/> is currently active
        /// </summary>
        private bool anyActiveSkill;
        #endregion
#pragma warning restore CS0109

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
        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            
            if (base.isLocalPlayer)
            {
                InputController.OnMouseMove += MovePosition;
                GameController.OnGameStart += this.GameStarted;
                GameController.OnResetGameStarted += this.ResetGameStarted;
                GameController.OnResetGameFinished += this.ResetGameFinished;
                FruitsFirstCollision.OnCollision += this.UnblockRelease;
                SkillController.OnSkillActivated += this.SetActiveSkill;
                FruitBehaviour.OnSkillUsed += DeactivateRotation;
                
                this.SetPlayerContainers();
            }
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            
            if (base.isLocalPlayer)
            {
                InputController.OnMouseMove -= MovePosition;
                GameController.OnGameStart -= this.GameStarted;
                GameController.OnResetGameStarted -= this.ResetGameStarted;
                GameController.OnResetGameFinished -= this.ResetGameFinished;
                FruitsFirstCollision.OnCollision -= this.UnblockRelease;
                SkillController.OnSkillActivated -= this.SetActiveSkill;
                FruitBehaviour.OnSkillUsed -= DeactivateRotation;
            }
        }

        private void Start()
        {
            this.blockedReleaseIndex = AudioPool.CreateAssignedAudioWrapper(AudioClipName.BlockedRelease, base.transform);
        }

        /// <summary>
        /// Request to the server to set the <see cref="containerBounds"/>
        /// </summary>
        [Client]
        private void SetPlayerContainers()
        {
            this.CmdSetPlayerContainers();
        }

        /// <summary>
        /// Server gets the container that is assigned to the given player connection
        /// </summary>
        /// <param name="_Sender">The client who requested the container</param>
        [Command]
        private void CmdSetPlayerContainers(NetworkConnectionToClient _Sender = null)
        {
            var _containerIndex = CustomNetworkManager.GetContainerIndex(_Sender);
            this.TargetSetPlayerContainers(_Sender, _Sender!.connectionId, _containerIndex);
        }

        /// <summary>
        /// Server assigns a container to the client with the given connection and starts the game for this player
        /// </summary>
        /// <param name="_Target">The client to assign the container for</param>
        /// <param name="_ConnectionId">
        /// The connection id of the client <br/>
        /// <i>Is null when trying to get from <see cref="_Target"/>, so must be passed separately</i>
        /// </param>
        /// <param name="_ContainerIndex">The index of the container in <see cref="CustomNetworkManager.containers"/> to assign to this client</param>
        [TargetRpc] // ReSharper disable once UnusedParameter.Local
        private void TargetSetPlayerContainers(NetworkConnectionToClient _Target, int _ConnectionId, int _ContainerIndex)
        {
            this.SetConnectionId(_ConnectionId);
            CustomNetworkManager.AssignPlayerContainer(this, _ContainerIndex);
            GameController.StartGame();
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
            this.fruitSpawnerAim.EnableRotation(false);
        }
        
        /// <summary>
        /// <see cref="GameController.OnResetGameFinished"/>
        /// </summary>
        private void ResetGameFinished()
        {
            Destroy(this.fruitBehaviour.gameObject);
            this.fruitBehaviour = null;
            this.anyActiveSkill = false;
            this.BlockInput(false);
        }
        
        /// <summary>
        /// Sets <see cref="inputBlocked"/> to true
        /// </summary>
        /// <param name="_Value">The value to set <see cref="inputBlocked"/> to</param>
        private void BlockInput(bool _Value)
        {
            this.inputBlocked = _Value;
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
            
            if (!this.inputBlocked)
            {
                if (Input.GetKey(KeyCode.A))
                {
                    this.MoveDirection(Vector2.left);
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    this.MoveDirection(Vector2.right);
                }

                var _mouseInput = false;
                if (!MenuController.IsAnyMenuOpen)
                {
                    _mouseInput = Input.GetKey(KeyCode.Mouse0);
                    if (_mouseInput)
                    {
                        _mouseInput = this.containerBounds.Contains(InputController.MouseWorldPosition);
                    }   
                }
                
                if ((Input.GetKey(KeyCode.Space) || _mouseInput) && !this.blockRelease)
                {
                    this.ReleaseFruit();
                }    
            }
        }
        
        /// <summary>
        /// Moves the <see cref="FruitSpawner"/> in the given <see cref="_Direction"/> <br/>
        /// <i>For keyboard input</i>
        /// </summary>
        /// <param name="_Direction">The direction to move the <see cref="FruitSpawner"/> in</param>
        private void MoveDirection(Vector2 _Direction)
        {
            var _direction = _Direction * (this.movementSpeed * Time.deltaTime);
            this.rigidbody2D.AddForce(_direction);
        }

        /// <summary>
        /// Moves the <see cref="FruitSpawner"/> to the given <see cref="_Position"/> <br/>
        /// <i>For mouse input</i>
        /// </summary>
        /// <param name="_Position">The direction to move the <see cref="FruitSpawner"/> to</param>
        private void MovePosition(Vector2 _Position)
        {
            if (!this.anyActiveSkill && this.containerBounds.Contains(_Position))
            {
                this.rigidbody2D.MovePosition(_Position);   
            }
        }
        
        /// <summary>
        /// Resets the Fruit Spawner to its original position
        /// </summary>
        /// <param name="_ResetPosition">If true, resets the <see cref="FruitSpawner"/> position to <see cref="ContainerBounds.StartingPosition"/></param>
        [Client]
        private void ResetFruitSpawner(bool _ResetPosition)
        {
            if (_ResetPosition)
                this.rigidbody2D.MovePosition(this.containerBounds.StartingPosition);
            
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
                var _fruitBehaviour = FruitBehaviour.SpawnFruit(base.transform, base.transform.position, _Rotation, _Fruit, false, _Sender);
                
                this.RpcResetFruitSpawner(_fruitBehaviour);
                this.TargetResetFruitSpawner(_Sender, _fruitBehaviour);
            }
        }

        /// <summary>
        /// <see cref="ResetFruitSpawner"/>
        /// </summary>
        /// <param name="_FruitBehaviour">The spawned <see cref="FruitBehaviour"/></param>
        [ClientRpc]
        private void RpcResetFruitSpawner(FruitBehaviour _FruitBehaviour)
        {
            _FruitBehaviour.transform.SetParent(base.transform, false);
            _FruitBehaviour.SetScale();
            _FruitBehaviour.IncreaseSortingOrder();
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
                this.TargetReleaseFruit(_Sender, _FruitBehaviour, _AimRotation);
            }
        }
        
        /// <summary>
        /// <see cref="ReleaseFruit"/>
        /// </summary>
        /// <param name="_Target"><see cref="NetworkBehaviour.connectionToClient"/></param>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to release</param>
        /// <param name="_AimRotation">The direction, the <see cref="fruitSpawnerAim"/> is pointing at</param>
        [TargetRpc] // ReSharper disable once UnusedParameter.Local
        private void TargetReleaseFruit(NetworkConnectionToClient _Target, FruitBehaviour _FruitBehaviour, Vector2 _AimRotation)
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
                this.anyActiveSkill = true;
                this.fruitBehaviour.SetActiveSkill(_ActiveSkill);
                this.fruitSpawnerAim.EnableRotation(true);
            }
            else
            {
                this.anyActiveSkill = false;
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
            this.anyActiveSkill = false;
            this.fruitSpawnerAim.EnableRotation(false);  
            this.fruitSpawnerAim.ResetAimRotation();
        }

        /// <summary>
        /// Sets <see cref="connectionId"/> to the given <see cref="NetworkConnectionToClient.connectionId"/>
        /// </summary>
        /// <param name="_ConnectionId">The connection the server has to this client</param>
        public void SetConnectionId(int _ConnectionId)
        {
            this.connectionId = _ConnectionId;
        }
        
        /// <summary>
        /// Sets <see cref="containerBounds"/> to the given <see cref="ContainerBounds"/>
        /// </summary>
        /// <param name="_ContainerBounds">The container for this <see cref="FruitSpawner"/></param>
        /// <returns><see cref="connectionId"/></returns>
        public int SetContainerBounds(ContainerBounds _ContainerBounds)
        {
            this.containerBounds = _ContainerBounds;

            return this.connectionId;
        }

        /// <summary>
        /// Disables this <see cref="FruitSpawner"/> <see cref="GameObject"/>
        /// </summary>
        public void GameModeTransitionStarted()
        {
            base.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Enables this <see cref="FruitSpawner"/> <see cref="GameObject"/> and sets its <see cref="Transform.position"/> to <see cref="ContainerBounds.StartingPosition"/>
        /// </summary>
        public void GameModeTransitionEnded()
        {
            base.transform.position = this.containerBounds.StartingPosition.Value;
            base.gameObject.SetActive(true);
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