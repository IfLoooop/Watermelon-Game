using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruits;
using Watermelon_Game.Skills;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Fruit_Spawn
{
    internal sealed class FruitSpawner : MonoBehaviour
    {
        #region Inspector Fields
#if UNITY_EDITOR
        [Header("Development")]
        [Tooltip("Deactivates the release block cool down if true (Development only!)")]
        [SerializeField] private bool noReleaseBlock;
#endif
        
        [Header("References")] 
        [Tooltip("Reference to the FruitSpawnerAim component of this FruitSpawner")]
        [SerializeField] private FruitSpawnerAim fruitSpawnerAim;
        
        [Header("Settings")]
        [Tooltip("Speed the FruitSpawner moves with")]
        [SerializeField] private float movementSpeed = 30f;
        [Tooltip("Speed the aim rotates with")]
        [SerializeField] private float rotationSpeed = 50f;
        [Tooltip("The maximum angle of FruitSpawnerAim (Relative to the FruitSpawners y-axis)")]
        [SerializeField] private float maxRotationAngle = 60f;
        [Tooltip("Minimum cooldown between fruit releases (In Seconds)")]
        [SerializeField] private float releaseCooldown = .375f;
        [Tooltip("Is added to the size of the FruitSpawners BoxCollider2D")]
        [SerializeField] private float colliderSizeOffset = 3.85f; 
        #endregion
        
        #region Fields
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
        private float lastRelease;
        
        // TODO: Use InputController
        /// <summary>
        /// Blocks movement input while this field is set to true
        /// </summary>
        private bool blockInput;
        /// <summary>
        /// Blocks fruit release while true
        /// </summary>
        private bool blockRelease;
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
        /// Singleton of <see cref="FruitSpawner"/>
        /// </summary>
        public static FruitSpawner Instance { get; private set; }
        /// <summary>
        /// <see cref="rotationSpeed"/>
        /// </summary>
        public float RotationSpeed => this.rotationSpeed;
        /// <summary>
        /// <see cref="maxRotationAngle"/>
        /// </summary>
        public float MaxRotationAngle => this.maxRotationAngle;
        #endregion
        
        #region Methods
        private void Awake()
        {
            Instance = this;
            
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
        }

        /// <summary>
        /// <see cref="GameController.OnResetGameStarted"/>
        /// </summary>
        private void ResetGameStarted()
        {
            this.FlipBlockInput();
            this.fruitSpawnerAim.EnableAim(false);
        }
        
        /// <summary>
        /// <see cref="GameController.OnResetGameFinished"/>
        /// </summary>
        private void ResetGameFinished()
        {
            Destroy(this.fruitBehaviour.gameObject);
            this.fruitBehaviour = null;
            this.FlipBlockInput();
        }
        
        private void Update()
        {
            this.GetInput();
        }
        
        /// <summary>
        /// Flips the value of <see cref="blockInput"/>
        /// </summary>
        private void FlipBlockInput() // TODO: Check if safe, or remove the flip
        {
            this.blockInput = !this.blockInput;
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
        
        /// <summary>
        /// Handles the input for the <see cref="FruitSpawner"/>
        /// </summary>
        private void GetInput()
        {
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
        /// Releases <see cref="fruitBehaviour"/> from this <see cref="FruitSpawner"/>
        /// </summary>
        private void ReleaseFruit()
        {
            var _releaseCooldown = Time.time - this.releaseCooldown < this.lastRelease;
            var _fruitInTrigger = this.fruitTrigger.IsTouchingLayers(LayerMaskController.FruitMask);
            if (_releaseCooldown || _fruitInTrigger)
            {
                if (!AudioPool.IsAssignedClipPlaying(this.blockedReleaseIndex))
                {
                    AudioPool.PlayAssignedClip(this.blockedReleaseIndex);
                }
                return;
            }
            
#if UNITY_EDITOR
            if (this.noReleaseBlock) goto skipCooldown;
#endif
            this.BlockRelease(true);
            
#if UNITY_EDITOR
            skipCooldown:;
#endif
            this.lastRelease = Time.time;
            this.fruitBehaviour.Release(-this.fruitSpawnerAim.transform.up);
            this.ResetFruitSpawner(false);
        }

#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// <see cref="ResetFruitSpawner"/> <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        /// <param name="_Fruit">The <see cref="Fruit"/> to give to the <see cref="FruitSpawner"/></param>
        public static void ResetFruitSpawner_DEVELOPMENT(Fruit _Fruit)
        {
            if (Instance.fruitBehaviour != null)
            {
                Destroy(Instance.fruitBehaviour.gameObject);
            }
            
            Instance.fruitBehaviour = NextFruit.Instance.GetFruit(Instance.transform, _Fruit);
            Instance.fruitBehaviour.IncreaseSortingOrder();
            Instance.fruitSpawnerAim.ResetAimRotation();
            Instance.fruitSpawnerCollider.size = new Vector2(Instance.fruitBehaviour.GetSize() + Instance.colliderSizeOffset, Instance.fruitSpawnerCollider.size.y);
            Instance.SetFruitTriggerSize(Instance.fruitBehaviour);
        }
#endif
        
        /// <summary>
        /// Resets the Fruit Spawner to its original position
        /// </summary>
        /// <param name="_ResetPosition">If true, resets the <see cref="FruitSpawner"/> position to <see cref="startingPosition"/></param>
        private void ResetFruitSpawner(bool _ResetPosition)
        {
            if (_ResetPosition)
                this.rigidbody2D.MovePosition(this.startingPosition);
            
            this.fruitBehaviour = NextFruit.Instance.GetFruit(this.transform);
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
        #endregion
    }   
}