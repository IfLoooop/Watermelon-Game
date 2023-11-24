using UnityEngine;
using Watermelon_Game.ExtensionMethods;

namespace Watermelon_Game.Background
{
    /// <summary>
    /// Logic for all fruits in the background
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D))]
    internal sealed class BackgroundFruit : MonoBehaviour
    {
#pragma warning disable CS0109
        #region Fields
        /// <summary>
        /// The <see cref="SpriteRenderer"/> component of ths <see cref="GameObject"/>
        /// </summary>
        private SpriteRenderer spriteRenderer;
        /// <summary>
        /// The <see cref="Rigidbody2D"/> component of this <see cref="GameObject"/>
        /// </summary>
        private new Rigidbody2D rigidbody2D;
        #endregion
#pragma warning restore CS0109
        
        #region Methods
        private void Awake()
        {
            this.spriteRenderer = base.GetComponent<SpriteRenderer>();
            this.rigidbody2D = base.GetComponent<Rigidbody2D>();
            
            this.spriteRenderer.color = this.spriteRenderer.color.WithAlpha(BackgroundFruitController.SpriteAlphaValue);
        }

        private void OnBecameInvisible()
        {
            BackgroundFruitController.ReturnToPool(this);
        }
        
        /// <summary>
        /// Sets the <see cref="Sprite"/> and <see cref="Transform.localScale"/> of this <see cref="GameObject"/>
        /// </summary>
        /// <param name="_FruitData"><see cref="Sprite"/> and size multiplier</param>
        public void Set((Sprite sprite, float fruitPrefabSize) _FruitData)
        {
            this.spriteRenderer.sprite = _FruitData.sprite;
            base.transform.localScale = Vector3.one * (_FruitData.fruitPrefabSize * BackgroundFruitController.SizeMultiplier);
        }

        /// <summary>
        /// Shoots the fruit in a random direction, depending on <see cref="GetRandomPosition"/>
        /// </summary>
        public void SetForce()
        {
            var _position = (Vector2)base.transform.position;
            var _horizontalPosition = _position.x;
            var _verticalPosition = _position.y;
            var _isLeft = _horizontalPosition < 0;
            var _isRight = _horizontalPosition > 0;
            Vector2 _targetPosition;
            
            if (_isLeft)
            {
                var _maxX = _horizontalPosition + 1;
                var _minY = _verticalPosition - 1;

                _targetPosition = GetRandomPosition(_horizontalPosition, _maxX, _minY, _verticalPosition);
            }
            else if (_isRight)
            {
                var _minX = _horizontalPosition - 1;
                var _minY = _verticalPosition - 1;

                _targetPosition = GetRandomPosition(_minX, _horizontalPosition, _minY, _verticalPosition);
            }
            else
            {
                var _minX = _horizontalPosition - 1;
                var _maxX = _horizontalPosition + 1;
                var _minY = _verticalPosition - 1;

                _targetPosition = GetRandomPosition(_minX, _maxX, _minY, _verticalPosition);
            }

            var _direction = _targetPosition - _position;
            var _forcePosition = _targetPosition + BackgroundFruitController.RotationForce;
            
            this.rigidbody2D.AddForceAtPosition(_direction * BackgroundFruitController.ForceMultiplier, _forcePosition, BackgroundFruitController.ForceMode);
        }
        
        /// <summary>
        /// Returns a random <see cref="Vector2"/> clamped to the given values
        /// </summary>
        /// <param name="_MinX">Minimum x value</param>
        /// <param name="_MaxX">Maximum x value</param>
        /// <param name="_MinY">Minimum y value</param>
        /// <param name="_MaxY">Maximum y value</param>
        /// <returns>A random <see cref="Vector2"/> clamped to the given values</returns>
        private static Vector2 GetRandomPosition(float _MinX, float _MaxX, float _MinY, float _MaxY)
        {
            var _x = Random.Range(_MinX, _MaxX);
            var _y = Random.Range(_MinY, _MaxY);
            var _randomPosition = new Vector2(_x, _y);
            
            return _randomPosition;
        }
        #endregion
    }
}