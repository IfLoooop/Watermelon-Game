using JetBrains.Annotations;
using Mirror;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using Steamworks;
using TMPro;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Container;
using Watermelon_Game.Controls;
using Watermelon_Game.Fruits;
using Watermelon_Game.Menus;
using Watermelon_Game.Menus.Lobbies;
using Watermelon_Game.Networking;
using Watermelon_Game.Skills;
using Watermelon_Game.Steamworks.NET;
using Watermelon_Game.Utility;
using Watermelon_Game.Utility.Pools;

namespace Watermelon_Game.Fruit_Spawn
{
    /// <summary>
    /// Player character
    /// </summary>
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
        [Tooltip("Particle system that is played when a skill is used")]
        [SerializeField] private ParticleSystem shootExplosion;
        [Tooltip("Holds the steam usernames")]
        [SerializeField] private new TextMeshProUGUI name;
        
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
        [SerializeField] private ProtectedFloat colliderSizeOffset = 2f;
        
        [Header("Debug")]
        [Tooltip("Connection id to the server")]
        [SyncVar(hook = nameof(SetConnectionId))]
        [SerializeField][ReadOnly] private int connectionId = -1;
        [Tooltip("The steam username for the client of this FruitSpawner")]
        [SyncVar(hook = nameof(SetUsername))]
        // ReSharper disable once NotAccessedField.Local
        [SerializeField][ReadOnly] private string username;
        [Tooltip("The steam id for the client of this FruitSpawner")]
        [SyncVar(hook = nameof(SetSteamId))]
        // ReSharper disable once NotAccessedField.Local
        [SerializeField][ReadOnly] private ulong steamId;
        #endregion
        
#pragma warning disable CS0109
        #region Fields
        /// <summary>
        /// Container for this <see cref="FruitSpawner"/>
        /// </summary>
        [CanBeNull] private ContainerBounds containerBounds;
        
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
        /// Blocks all input while this field is set to true
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
        private ProtectedBool anyActiveSkill;
        #endregion
#pragma warning restore CS0109

        #region Properties
        /// <summary>
        /// Singleton of <see cref="FruitSpawner"/> <br/>
        /// <i>Will always be the instance of the local player</i>
        /// </summary>
        public static FruitSpawner Instance { get; private set; }
        
        /// <summary>
        /// <see cref="rotationSpeed"/>
        /// </summary>
        public static ProtectedFloat RotationSpeed => Instance.rotationSpeed;
        /// <summary>
        /// <see cref="maxRotationAngle"/>
        /// </summary>
        public static ProtectedFloat MaxRotationAngle => Instance.maxRotationAngle;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.rigidbody2D = base.GetComponent<Rigidbody2D>();
            this.fruitSpawnerCollider = base.GetComponent<BoxCollider2D>();
            this.fruitTrigger = base.GetComponentInChildren<CircleCollider2D>();
        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            
            if (base.isLocalPlayer)
            {
                Instance = this;
                
                InputController.OnMouseMove += MovePosition;
                GameController.OnGameStart += this.GameStarted;
                GameController.OnResetGameStarted += this.ResetGameStarted;
                GameController.OnResetGameFinished += this.ResetGameFinished;
                FruitsFirstCollision.OnCollision += this.UnblockRelease;
                SkillController.OnSkillActivated += this.SetActiveSkill;
                FruitBehaviour.OnSkillUsed += DeactivateRotation;
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
            if (base.isLocalPlayer)
            {
                this.blockedReleaseIndex = AudioPool.CreateAssignedAudioWrapper(AudioClipName.BlockedRelease, base.transform);
            }
        }
        
        /// <summary>
        /// Initializes all values the server needs
        /// </summary>
        /// <param name="_ConnectionToClient">The clients connection to the server</param>
        /// <param name="_ContainerIndex">Index in <see cref="GameController.Containers"/> of the <see cref="ContainerBounds"/> that will be assigned to this <see cref="FruitSpawner"/></param>
        [Server]
        public void Init(NetworkConnectionToClient _ConnectionToClient, int _ContainerIndex)
        {
            this.connectionId = _ConnectionToClient.connectionId;
            this.TargetInit(_ConnectionToClient, _ContainerIndex);
        }
        
        /// <summary>
        /// Sets the connection id 
        /// </summary>
        /// <param name="_OldValue">Not needed here</param>
        /// <param name="_NewValue">The clients connection id to the server</param>
        // ReSharper disable once UnusedParameter.Global
        public void SetConnectionId(int _OldValue, int _NewValue)
        {
            this.connectionId = _NewValue;  
        }
        
        /// <summary>
        /// <see cref="Init"/>
        /// </summary>
        /// <param name="_ConnectionToClient">The clients connection to the server</param>
        /// <param name="_ContainerIndex">Index in <see cref="GameController.Containers"/> of the <see cref="ContainerBounds"/> that will be assigned to this <see cref="FruitSpawner"/></param>
        [TargetRpc] // ReSharper disable once UnusedParameter.Local
        private void TargetInit(NetworkConnectionToClient _ConnectionToClient, int _ContainerIndex)
        {
            GameController.Containers[_ContainerIndex].AssignToPlayer(this);
            NetworkStoneFruitCharge.SetFruitSpawner(this);
            
            if (SteamManager.Initialized)
            {
                this.CmdSetSteamData(SteamUser.GetSteamID().m_SteamID, SteamFriends.GetPersonaName());   
            }
            
            if (SteamLobby.CurrentLobbyId == null)
            {
                GameController.StartGame();   
            }
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
        /// Sends the clients steam data to the server so it can be synced across the network
        /// </summary>
        /// <param name="_SteamId"><see cref="steamId"/></param>
        /// <param name="_Username"><see cref="username"/></param>
        [Command(requiresAuthority = false)]
        private void CmdSetSteamData(ulong _SteamId, string _Username)
        {
            this.steamId = _SteamId;
            this.username = _Username;
            LobbyHostMenu.PlayerConnected(_SteamId);
        }
        
        /// <summary>
        /// Sets the clients steam id
        /// </summary>
        /// <param name="_OldValue">Not needed here</param>
        /// <param name="_NewValue">The clients steam id</param>
        // ReSharper disable once UnusedParameter.Local
        private void SetSteamId(ulong _OldValue, ulong _NewValue)
        {
            this.steamId = _NewValue;
        }
        
        /// <summary>
        /// Sets the clients username
        /// </summary>
        /// <param name="_OldValue">Not needed here</param>
        /// <param name="_NewValue">The clients steam username</param>
        // ReSharper disable once UnusedParameter.Local
        private void SetUsername(string _OldValue, string _NewValue)
        {
            this.name.text = _NewValue;
        }
        
        /// <summary>
        /// <see cref="GameController.OnGameStart"/>
        /// </summary>
        private void GameStarted()
        {
            this.ResetFruitSpawner(true);
            this.BlockInput(false);
            this.UnblockRelease();
            this.CmdGameStarted();
        }

        /// <summary>
        /// <see cref="GameStarted"/>
        /// </summary>
        [Command(requiresAuthority = false)]
        private void CmdGameStarted()
        {
            this.RpcGameStarted();
        }

        /// <summary>
        /// <see cref="GameStarted"/>
        /// </summary>
        [ClientRpc]
        private void RpcGameStarted()
        {
            this.fruitSpawnerAim.EnableAim(true);
            this.name.enabled = true;
        }
        
        /// <summary>
        /// <see cref="GameController.OnResetGameStarted"/>
        /// </summary>
        [Client]
        private void ResetGameStarted()
        {
            this.CmdResetGameStarted();
        }

        /// <summary>
        /// <see cref="ResetGameStarted"/>
        /// </summary>
        [Command(requiresAuthority = false)]
        private void CmdResetGameStarted()
        {
            this.RpcResetGameStarted();
        }
        
        /// <summary>
        /// <see cref="ResetGameStarted"/>
        /// </summary>
        [ClientRpc]
        private void RpcResetGameStarted()
        {
            this.BlockInput(true);
            this.fruitSpawnerAim.EnableAim(false);
            this.fruitSpawnerAim.EnableRotation(false);
            this.name.enabled = false;
        }
        
        /// <summary>
        /// <see cref="GameController.OnResetGameFinished"/>
        /// </summary>
        /// <param name="_ResetReason"><see cref="ResetReason"/></param>
        private void ResetGameFinished(ResetReason _ResetReason)
        {
            this.CmdResetGameFinished(_ResetReason);
        }

        /// <summary>
        /// <see cref="ResetGameFinished"/>
        /// </summary>
        /// <param name="_ResetReason"><see cref="ResetReason"/></param>
        [Command(requiresAuthority = false)]
        private void CmdResetGameFinished(ResetReason _ResetReason)
        {
            this.RpcResetGameFinished(_ResetReason);
        }
        
        /// <summary>
        /// <see cref="ResetGameFinished"/>
        /// </summary>
        /// <param name="_ResetReason"><see cref="ResetReason"/></param>
        [ClientRpc]
        private void RpcResetGameFinished(ResetReason _ResetReason)
        {
            if (this.fruitBehaviour != null)
            {
                this.fruitBehaviour.DestroyFruit();   
            }
            this.fruitBehaviour = null;
            this.anyActiveSkill = false;
            this.BlockInput(false);

            if (SteamLobby.CurrentLobbyId == null && _ResetReason == ResetReason.ManualRestart)
            {
                GameController.StartGame();
            }
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
            
            if (MenuController.IsAnyMenuOpen || this.inputBlocked || !GameController.ActiveGame)
            {
                return;
            }
            
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
                if (_mouseInput && this.containerBounds is {} _containerBounds)
                {
                    _mouseInput = ContainerBounds.Contains(InputController.MouseWorldPosition);
                }   
            }
                
            if ((Input.GetKey(KeyCode.Space) || _mouseInput) && !this.blockRelease)
            {
                this.ReleaseFruit();
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
            if (!this.anyActiveSkill && ContainerBounds.Contains(_Position))
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
                this.rigidbody2D.MovePosition(this.containerBounds!.StartingPosition);
            
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
            this.fruitTrigger.radius = _Fruit.ColliderHalfSize;
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
            this.CmdReleaseFruit(this.fruitBehaviour, -this.fruitSpawnerAim.transform.up, this.anyActiveSkill);
            this.ResetFruitSpawner(false);
        }
        
        /// <summary>
        /// <see cref="ReleaseFruit"/>
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to release</param>
        /// <param name="_AimRotation">The direction, the <see cref="fruitSpawnerAim"/> is pointing at</param>
        /// <param name="_AnyActiveSkill">Is a skill currently active?</param>
        /// <param name="_Sender"><see cref="NetworkBehaviour.connectionToClient"/></param>
        [Command(requiresAuthority = false)] // ReSharper disable once UnusedParameter.Local
        private void CmdReleaseFruit(FruitBehaviour _FruitBehaviour, Vector2 _AimRotation, bool _AnyActiveSkill, NetworkConnectionToClient _Sender = null)
        {
            if (_Sender == base.netIdentity.connectionToClient)
            {
                this.TargetReleaseFruit(_Sender, _FruitBehaviour, _AimRotation);

                if (_AnyActiveSkill)
                {
                    this.RpcReleaseFruit();
                }
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
            _FruitBehaviour.CmdRelease(_AimRotation, this.anyActiveSkill);
        }

        /// <summary>
        /// <see cref="ReleaseFruit"/>
        /// </summary>
        [ClientRpc]
        private void RpcReleaseFruit()
        {
            this.shootExplosion.Play();
        }
        
        /// <summary>
        /// Shoots the given <see cref="StoneFruitBehaviour"/> in the direction of <see cref="ContainerBounds.StoneFruitTarget"/>
        /// </summary>
        /// <param name="_StoneFruit">The <see cref="StoneFruitBehaviour"/> to shoot</param>
        /// <param name="_ShootForce">Multiplier for the force with which the fruit is shot</param>
        [Client]
        public void ShootStoneFruit(StoneFruitBehaviour _StoneFruit, float _ShootForce)
        {
            this.CmdShootStoneFruit();
            _StoneFruit.Shoot(this.containerBounds!.StoneFruitTarget.localPosition - base.transform.position, _ShootForce);
        }

        /// <summary>
        /// <see cref="ShootStoneFruit"/>
        /// </summary>
        [Command]
        private void CmdShootStoneFruit()
        {
            this.RpcShootStoneFruit();
        }

        /// <summary>
        /// <see cref="ShootStoneFruit"/>
        /// </summary>
        [ClientRpc]
        private void RpcShootStoneFruit()
        {
            this.shootExplosion.Play();
        }
        
        /// <summary>
        /// Sets the currently active <see cref="Skill"/> on the <see cref="fruitBehaviour"/> currently held by the <see cref="FruitSpawner"/>
        /// </summary>
        /// <param name="_ActiveSkill">
        /// The currently active <see cref="Skill"/> <br/>
        /// <i>Null means no <see cref="Skill"/> is currently active</i>
        /// </param>
        [Client]
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
        /// Disables this <see cref="FruitSpawner"/> <see cref="GameObject"/>
        /// </summary>
        public void GameModeTransitionStarted()
        {
            this.BlockInput(true);
        }
        
        /// <summary>
        /// Enables this <see cref="FruitSpawner"/> <see cref="GameObject"/> and sets its <see cref="Transform.position"/> to <see cref="ContainerBounds.StartingPosition"/>
        /// </summary>
        public void GameModeTransitionEnded()
        {
            base.transform.position = this.containerBounds!.StartingPosition.Value;
            this.BlockInput(false);
        }
        
#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// Forces the <see cref="fruitBehaviour"/> that is currently held by the <see cref="FruitSpawner"/> to become a golden fruit <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        public static void ForceGoldenFruit_DEVELOPMENT()
        {
            Instance.fruitBehaviour.ForceGoldenFruit_DEVELOPMENT(true);
        }
        
        /// <summary>
        /// Forces the <see cref="FruitSpawner"/> tp spawn the given <see cref="Fruit"/> <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to give to the <see cref="FruitSpawner"/></param>
        public static void ForceFruit_DEVELOPMENT(Fruit _Fruit)
        {
            Instance.CmdForceFruit_DEVELOPMENT(_Fruit);
        }

        /// <summary>
        /// <see cref="ForceFruit_DEVELOPMENT"/>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to give to the <see cref="FruitSpawner"/></param>
        /// <param name="_Sender">The caller</param>
        [Command(requiresAuthority = false)]
        private void CmdForceFruit_DEVELOPMENT(Fruit _Fruit, NetworkConnectionToClient _Sender = null)
        {
            if (Instance.fruitBehaviour != null)
            {
                Destroy(Instance.fruitBehaviour.gameObject);
            }
            
            var _transform = Instance.transform;
            Instance.fruitBehaviour = FruitBehaviour.SpawnFruit(_transform, _transform.position, Quaternion.identity, (int)_Fruit, true, _Sender);
            Instance.fruitBehaviour.SetScale();
            Instance.fruitBehaviour.IncreaseSortingOrder();
            Instance.fruitSpawnerAim.ResetAimRotation();
            Instance.fruitSpawnerCollider.size = new Vector2(Instance.fruitBehaviour.GetSize() + Instance.colliderSizeOffset, Instance.fruitSpawnerCollider.size.y);
            Instance.SetFruitTriggerSize(Instance.fruitBehaviour);
        }
#endif
        #endregion
    }   
}