using Unity.Mathematics;
using UnityEngine;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Fruit;
using Watermelon_Game.Skills;
using Watermelon_Game.Utility;

namespace Watermelon_Game.Fruit_Spawn
{
    internal sealed class FruitSpawner : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private float movementSpeed = 30f;
        [SerializeField] private float rotationStep = 50f;
        [SerializeField] private float maxRotationAngle = 60f;
        [SerializeField] private AudioClip release;
        [SerializeField] private float releaseClipVolume = .01f;
        [SerializeField] private AudioClip blockedRelease;
        [SerializeField] private float blockedReleaseVolume = .0175f;
        [SerializeField] private AudioClip shoot;
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
        public bool BlockRelease { get; set; }
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
            this.fruitBehaviour.SetOrderInLayer(0);
            FruitSpawnerAim.Enable(false);
            this.fruitBehaviour.transform.SetParent(null, true);
            this.fruitBehaviour.Release(this, -this.transform.up);

            if (this.fruitBehaviour.ActiveSkill != null)
            {
                this.audioSource.Play(this.shootClipStartTime, this.shoot, this.shootClipVolume);
                
                var _value = SkillController.Instance.SkillPointRequirementsMap[this.fruitBehaviour.ActiveSkill.Value];
                PointsController.Instance.SubtractPoints(_value);

                if (this.fruitBehaviour.ActiveSkill is Skill.Evolve or Skill.Destroy)
                {
                    this.fruitBehaviour.Shoot(-this.transform.up);
                }
            }
            else
            {
                this.audioSource.Play(0, this.release, this.releaseClipVolume);
            }
            
            SkillController.Instance.DeactivateActiveSkill(true);
            
            this.ResetFruitSpawner(false);
        }

        /// <summary>
        /// Resets the Fruit Spawner to its original position
        /// </summary>
        /// <param name="_ResetPosition">If true, resets the <see cref="FruitSpawner"/> position to <see cref="startingPosition"/></param>
        public void ResetFruitSpawner(bool _ResetPosition)
        {
            this.BlockInput = true;
            if (_ResetPosition)
                this.rigidbody2D.MovePosition(this.startingPosition);
            this.fruitBehaviour = NextFruit.Instance.GetFruit(this.transform);
            this.fruitBehaviour.SetOrderInLayer(1);
            this.fruitSpawnerCollider.size = new Vector2(this.fruitBehaviour.transform.localScale.x + COLLIDER_SIZE_OFFSET, this.fruitSpawnerCollider.size.y);
            this.SetFruitTrigger(this.fruitBehaviour);
            this.BlockInput = false;
        }

        private void SetFruitTrigger(FruitBehaviour _Fruit)
        {
            this.fruitTrigger.transform.localScale = _Fruit.transform.localScale;
            this.fruitTrigger.radius = _Fruit.ColliderRadius;
        }
        
        /// <summary>
        /// Rotates the <see cref="FruitSpawner"/> in the given direction
        /// </summary>
        /// <param name="_Direction">Negative value = left, positive value = right</param>
        public static void Rotate(int _Direction)
        {
            var _zRotation = _Direction * Instance.rotationStep * Time.deltaTime;
            var _currentRotation = Mathfx.SignedAngle(Instance.transform.eulerAngles.z);
            var _canRotateLeft = _Direction < 0 && _currentRotation > Instance.maxRotationAngle * -1;
            var _canRotateRight = _Direction > 0 && _currentRotation < Instance.maxRotationAngle;
            
            if (_canRotateLeft || _canRotateRight)
            {
                Instance.transform.Rotate(new Vector3(0, 0,  _zRotation));   
            }
        }

        /// <summary>
        /// Sets the rotation of the <see cref="FruitSpawner"/> back to zero
        /// </summary>
        public static void ResetAimRotation()
        {
            Instance.transform.rotation = quaternion.Euler(0, 0, 0);
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
        #endregion
    }   
}
