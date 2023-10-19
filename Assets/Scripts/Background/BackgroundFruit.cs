using UnityEngine;
using Watermelon_Game.ExtensionMethods;

namespace Watermelon_Game.Background
{
    internal sealed class BackgroundFruit : MonoBehaviour
    {
        #region Fields
        private Vector3 baseSize;
        private SpriteRenderer spriteRenderer;
#pragma warning disable CS0109
        private new Rigidbody2D rigidbody2D;
#pragma warning restore CS0109
        private CircleCollider2D circleCollider2D;
        #endregion
        
        #region Methods
        private void Awake()
        {
            this.baseSize = base.transform.localScale;
            this.spriteRenderer = base.GetComponent<SpriteRenderer>();
            this.rigidbody2D = base.GetComponent<Rigidbody2D>();
            this.circleCollider2D = base.GetComponent<CircleCollider2D>();
            
            this.spriteRenderer.color = this.spriteRenderer.color.WithAlpha(BackgroundController.Instance.SpriteAlphaValue);
        }

        private void OnBecameInvisible()
        {
            BackgroundController.Instance.ReturnToPool(this);
            base.transform.localScale = this.baseSize;
        }
        
        public void SetSprite((Sprite sprite, float sizeMultiplier) _Sprite)
        {
            this.spriteRenderer.sprite = _Sprite.sprite;
            base.transform.localScale *= _Sprite.sizeMultiplier * BackgroundController.Instance.SizeMultiplier;
        }

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
            var _bounds = this.circleCollider2D.bounds;
            var _min = _bounds.min;
            var _max = _bounds.max;
            var _randomPosition = GetRandomPosition(_min.x, _max.x, _min.y, _max.y);
            
            this.rigidbody2D.AddForceAtPosition(_direction * BackgroundController.Instance.ForceMultiplier, _randomPosition, BackgroundController.Instance.ForceMode);
        }

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