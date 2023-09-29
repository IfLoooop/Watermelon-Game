using UnityEngine;
using Watermelon_Game.Fruit;

namespace Watermelon_Game
{
    internal sealed class FruitSpawner : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private float movementSpeed = 25;
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
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                {
                    this.Move(Vector2.left);
                }
                else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                {
                    this.Move(Vector2.right);
                }

                // TODO: Check if the previous fruit has collided with any fruit before release is possible 
                if (Input.GetKeyDown(KeyCode.Space) && !this.BlockRelease)
                {
                    this.ReleaseFruit();
                }   
            }
        }

        private void Reset()
        {
            this.blockInput = true;
            this.rigidbody2D.MovePosition(this.startingPosition);
            this.fruitBehaviour = FruitBehaviour.GetFruit(this.rigidbody2D.position, this.transform, this.lastFruit);
            this.lastFruit = fruitBehaviour.Fruit;
            this.boxCollider2D.size = new Vector2(this.fruitBehaviour.transform.localScale.x, boxCollider2D.size.y);
            this.blockInput = false;
        }
        
        private void Move(Vector2 _Direction)
        {
            var _direction = _Direction * (this.movementSpeed * Time.deltaTime);
            var _position = this.rigidbody2D.position + _direction;
            
            this.rigidbody2D.MovePosition(_position);
        }

        private void ReleaseFruit()
        {
            this.BlockRelease = true;
            this.fruitBehaviour.transform.SetParent(null, true);
            this.fruitBehaviour.Release(this);
            
            this.Reset();
        }
        #endregion
    }   
}
