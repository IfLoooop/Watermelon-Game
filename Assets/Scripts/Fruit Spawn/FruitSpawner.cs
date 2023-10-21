using UnityEngine;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Fruit;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Fruit_Spawn
{
    internal sealed class FruitSpawner : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")] 
        [SerializeField] private FruitSpawnerAim fruitSpawnerAim;
        [SerializeField] private AudioClip blockedRelease;
        [SerializeField] private AudioClip release;
        [SerializeField] private AudioClip shoot;
        [Header("Settings")]
        [SerializeField] private float movementSpeed = 30f;
        [SerializeField] private float rotationStep = 50f;
        [SerializeField] private float maxRotationAngle = 60f;
        [SerializeField] private float blockedReleaseVolume = .0175f;
        [SerializeField] private float releaseClipVolume = .01f;
        [SerializeField] private float shootClipStartTime = .1f;
        [SerializeField] private float shootClipVolume = .05f;
        #endregion
        
        #region Fields
#pragma warning disable CS0109
        private new Rigidbody2D rigidbody2D;
#pragma warning restore CS0109
        private BoxCollider2D fruitSpawnerCollider;
        private CircleCollider2D fruitTrigger;
        private AudioSource audioSource;
        
        /// <summary>
        /// Uses the position the GameObject has at start of game <br/>
        /// <b>Should not be modified afterwards</b>
        /// </summary>
        private Vector2 startingPosition;
        private const float COLLIDER_SIZE_OFFSET = 3.85f;

        private bool blockRelease;
        
        /// <summary>
        /// The <see cref="FruitBehaviour"/> that is currently attached to this <see cref="FruitSpawner"/> 
        /// </summary>
        private FruitBehaviour fruitBehaviour;
        #endregion

        #region Properties
        public static FruitSpawner Instance { get; private set; }
        /// <summary>
        /// Blocks movement input while this field is set to true
        /// </summary>
        public bool BlockInput { get; set; }
        /// <summary>
        /// Blocks fruit release while this Property is set to true 
        /// </summary>
        public bool BlockRelease
        {
            get => this.blockRelease;
            set
            {
                this.blockRelease = value;
                FruitSpawnerAim.Enable(!this.blockRelease);
            }
        }

        public float RotationStep => this.rotationStep;
        public float MaxRotationAngle => this.maxRotationAngle;
        #endregion
        
        #region Methods
        private void Awake()
        {
            Instance = this;
            
            this.rigidbody2D = base.GetComponent<Rigidbody2D>();
            this.fruitSpawnerCollider = base.GetComponent<BoxCollider2D>();
            this.fruitTrigger = base.GetComponentInChildren<CircleCollider2D>();
            this.audioSource = base.GetComponent<AudioSource>();
        }

        private void Start()
        {
            this.startingPosition = this.transform.position;
        }
        
        private void Update()
        {
            this.GetInput();
        }
        
        private void GetInput()
        {
            if (!this.BlockInput)
            {
                if (Input.GetKey(KeyCode.A))
                {
                    this.Move(Vector2.left);
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    this.Move(Vector2.right);
                }

                if (Input.GetKeyDown(KeyCode.Space) && !this.BlockRelease)
                {
                    this.ReleaseFruit();
                }   
            }
        }
        
        private void Move(Vector2 _Direction)
        {
            var _direction = _Direction * (this.movementSpeed * Time.deltaTime);

            this.rigidbody2D.AddForce(_direction);
        }
        
        private void ReleaseFruit()
        {
            var _fruitInTrigger = this.fruitTrigger.IsTouchingLayers(LayerMask.GetMask("Fruit"));
            if (_fruitInTrigger)
            {
                this.audioSource.Play(0, this.blockedRelease, this.blockedReleaseVolume);
                return;
            }
            
            this.BlockRelease = true;
            this.fruitBehaviour.Release(this);
            
            if (this.fruitBehaviour.ActiveSkill != null)
            {
                SkillController.Instance.SkillUsed(this.fruitBehaviour.ActiveSkill.Value);
                
                this.audioSource.Play(this.shootClipStartTime, this.shoot, this.shootClipVolume);
                
                switch (this.fruitBehaviour.ActiveSkill)
                {
                    case Skill.Evolve or Skill.Destroy:
                        this.fruitBehaviour.Shoot(-this.fruitSpawnerAim.transform.up);
                        break;
                    case Skill.Power:
                        SkillController.Instance.Skill_Power(this.fruitBehaviour, -this.fruitSpawnerAim.transform.up);
                        break;
                }
                
                SkillController.Instance.DeactivateActiveSkill(true);
            }
            else
            {
                var _mass = this.fruitBehaviour.Rigidbody2D.mass * GameController.Instance.FruitCollection.MassMultiplier;
                this.fruitBehaviour.SetMass(false, _mass);
                this.audioSource.Play(0, this.release, this.releaseClipVolume);
            }
            
            this.ResetFruitSpawner(false);
        }

        /// <summary>
        /// Resets the Fruit Spawner to its original position
        /// </summary>
        /// <param name="_ResetPosition">If true, resets the <see cref="FruitSpawner"/> position to <see cref="startingPosition"/></param>
        /// <param name="_Fruit">Gives the <see cref="FruitSpawner"/> a specific fruit <b>Can only be used during development</b></param>
        public void ResetFruitSpawner
        (
            bool _ResetPosition
#if DEBUG || DEVELOPMENT_BUILD
            , Fruit.Fruit? _Fruit = null
#endif
        )
        {
            if (_ResetPosition)
                this.rigidbody2D.MovePosition(this.startingPosition);

#if DEBUG || DEVELOPMENT_BUILD
            if (_Fruit != null)
            {
                if (this.fruitBehaviour != null)
                {
                    Destroy(this.fruitBehaviour.gameObject);
                }
                this.fruitBehaviour = NextFruit.Instance.GetFruit(this.transform, _Fruit.Value);
                goto skipGetFruit;
            }
#endif
            
            this.fruitBehaviour = NextFruit.Instance.GetFruit(this.transform);

#if DEBUG || DEVELOPMENT_BUILD
            skipGetFruit:;
#endif
            
            this.fruitBehaviour.SetOrderInLayer(1);
            this.fruitSpawnerAim.ResetAim();
            this.fruitSpawnerCollider.size = new Vector2(this.fruitBehaviour.transform.localScale.x + COLLIDER_SIZE_OFFSET, this.fruitSpawnerCollider.size.y);
            this.SetFruitTrigger(this.fruitBehaviour);
        }

        private void SetFruitTrigger(FruitBehaviour _Fruit)
        {
            this.fruitTrigger.transform.localScale = _Fruit.transform.localScale;
            this.fruitTrigger.radius = _Fruit.ColliderRadius;
        }
        
        /// <summary>
        /// Sets the rotation of the <see cref="fruitSpawnerAim"/> back to zero
        /// </summary>
        public static void ResetAimRotation()
        {
            Instance.fruitSpawnerAim.ResetAim();
        }

        /// <summary>
        /// Sets the currently active <see cref="Skill"/> on the <see cref="fruitBehaviour"/> currently held by the <see cref="FruitSpawner"/>
        /// </summary>
        /// <param name="_ActiveSkill">The currently active <see cref="Skill"/></param>
        public static void SetActiveSkill(Skill _ActiveSkill)
        {
            Instance.fruitBehaviour.SetActiveSkill(_ActiveSkill);
        }

        /// <summary>
        /// Deactivates the <see cref="Skill"/> on the <see cref="fruitBehaviour"/> currently held by the <see cref="FruitSpawner"/>
        /// </summary>
        public static void DeactivateSkill()
        {
            Instance.fruitBehaviour.DeactivateSkill();
        }

        public static void GameOver()
        {
            Destroy(Instance.fruitBehaviour.gameObject);
            Instance.fruitBehaviour = null;
        }
        #endregion
    }   
}
