using Unity.Mathematics;
using UnityEngine;
using Watermelon_Game.Fruit;
using Watermelon_Game.Skills;

namespace Watermelon_Game.Fruit_Spawn
{
    internal sealed class FruitSpawner : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private float movementSpeed = 30f;
        [SerializeField] private float rotationStep = 50f;
        [SerializeField] private float maxRotationAngle = 60f;
        #endregion

        #region Statics
        private static FruitSpawner instance;
        #endregion
        
        #region Fields
        /// <summary>
        /// Uses the position the GameObject has at start of game <br/>
        /// <b>Should not be modified afterwards</b>
        /// </summary>
        private Vector2 startingPosition;
        
        private new Rigidbody2D rigidbody2D;
        private BoxCollider2D boxCollider2D;
        
        /// <summary>
        /// The <see cref="FruitBehaviour"/> that is currently attached to this <see cref="FruitSpawner"/> 
        /// </summary>
        private FruitBehaviour fruitBehaviour;
        private Fruit.Fruit? lastFruit;
        
        /// <summary>
        /// Blocks movement input while this field is set to true
        /// </summary>
        private bool blockInput;
        #endregion

        #region Properties
        /// <summary>
        /// Blocks fruit release as while this Property is set to true 
        /// </summary>
        public bool BlockRelease { get; set; }
        #endregion
        
        #region Methods
        private void Awake()
        {
            instance = this;
            this.rigidbody2D = this.GetComponent<Rigidbody2D>();
            this.boxCollider2D = this.GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            this.startingPosition = this.transform.position;
            this.Reset();
        }

        private void Update()
        {
            this.GetInput();
        }

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
            this.BlockRelease = true;
            this.fruitBehaviour.transform.SetParent(null, true);
            this.fruitBehaviour.Release(this, -this.transform.up);

            if (this.fruitBehaviour.ActiveSkill != null)
            {
                var _value = SkillController.Instance.SkillPointRequirementsMap[this.fruitBehaviour.ActiveSkill.Value];
                PointsController.Instance.SubtractPoints(_value);
            }
            
            SkillController.Instance.DeactivateActiveSkill(true);
            
            this.Reset();
        }

        /// <summary>
        /// Resets the Fruit Spawner to its original position
        /// </summary>
        private void Reset()
        {
            this.blockInput = true;
            this.rigidbody2D.MovePosition(this.startingPosition);
            this.fruitBehaviour = NextFruit.Instance.GetFruit(this.transform, this.lastFruit);
            this.lastFruit = fruitBehaviour.Fruit;
            this.boxCollider2D.size = new Vector2(this.fruitBehaviour.transform.localScale.x, boxCollider2D.size.y);
            this.blockInput = false;
        }
        
        /// <summary>
        /// Rotates the <see cref="FruitSpawner"/> in the given direction
        /// </summary>
        /// <param name="_Direction">Negative value = left, positive value = right</param>
        public static void Rotate(int _Direction)
        {
            var _zRotation = _Direction * instance.rotationStep * Time.deltaTime;
            var _currentRotation = Mathfx.SignedAngle(instance.transform.eulerAngles.z);
            var _canRotateLeft = _Direction < 0 && _currentRotation > instance.maxRotationAngle * -1;
            var _canRotateRight = _Direction > 0 && _currentRotation < instance.maxRotationAngle;
            
            if (_canRotateLeft || _canRotateRight)
            {
                instance.transform.Rotate(new Vector3(0, 0,  _zRotation));   
            }
        }

        /// <summary>
        /// Sets the rotation of the <see cref="FruitSpawner"/> back to zero
        /// </summary>
        public static void ResetAimRotation()
        {
            instance.transform.rotation = quaternion.Euler(0, 0, 0);
        }

        /// <summary>
        /// Sets the currently active <see cref="Skill"/> on the <see cref="fruitBehaviour"/> currently held by the <see cref="FruitSpawner"/>
        /// </summary>
        /// <param name="_ActiveSkill">The currently active <see cref="Skill"/></param>
        public static void SetActiveSkill(Skill _ActiveSkill)
        {
            instance.fruitBehaviour.SetActiveSkill(_ActiveSkill);
        }

        /// <summary>
        /// Deactivates the <see cref="Skill"/> on the <see cref="fruitBehaviour"/> currently held by the <see cref="FruitSpawner"/>
        /// </summary>
        public static void DeactivateSkill()
        {
            instance.fruitBehaviour.DeactivateSkill();
        }
        #endregion
    }   
}
