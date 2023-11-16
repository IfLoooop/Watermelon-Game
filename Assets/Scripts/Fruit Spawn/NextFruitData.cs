using OPS.AntiCheat.Field;
using UnityEngine;
using UnityEngine.UI;
using Watermelon_Game.Fruits;

namespace Watermelon_Game.Fruit_Spawn
{
    /// <summary>
    /// Contains all necessary of a <see cref="Fruit"/> for <see cref="NextFruit"/>
    /// </summary>
    internal sealed class NextFruitData : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("SpriteRenderer component of the fruit")]
        [SerializeField] private Image fruitImage;
        [Tooltip("SpriteRenderer component of the fruits face")]
        [SerializeField] private Image faceImage;
        #endregion

        #region Constants
        /// <summary>
        /// Fruits scale needs to be divided by this value
        /// </summary>
        private const float FRUIT_SCALE_OFFSET = 40;
        #endregion
        
        #region Fields
        /// <summary>
        /// The <see cref="Fruits.Fruit"/> type
        /// </summary>
        private ProtectedInt32 fruit;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="fruit"/>
        /// </summary>
        public ProtectedInt32 Fruit => this.fruit;
        #endregion
        
        #region Methods
        /// <summary>
        /// Copies the values from the given <see cref="NextFruitData"/> into this <see cref="NextFruitData"/>
        /// </summary>
        /// <param name="_NextFruitData">The <see cref="NextFruitData"/> to copy the values from</param>
        public void CopyFruit(NextFruitData _NextFruitData)
        {
            this.Set(_NextFruitData.fruitImage.sprite, _NextFruitData.fruit, _NextFruitData.fruitImage.transform.localScale);
        }

        /// <summary>
        /// Copies the values from the given <see cref="FruitPrefab"/> into this <see cref="NextFruitData"/>
        /// </summary>
        /// <param name="_FruitPrefab">The <see cref="FruitPrefab"/> to copy the values from</param>
        public void CopyFruit(FruitPrefab _FruitPrefab)
        {
            this.Set(_FruitPrefab.Sprite, _FruitPrefab.Fruit, _FruitPrefab.Prefab.transform.localScale  / FRUIT_SCALE_OFFSET);
        }

        /// <summary>
        /// Sets the given values into this <see cref="NextFruitData"/>
        /// </summary>
        /// <param name="_Sprite">The <see cref="Sprite"/> of the <see cref="Fruits.Fruit"/></param>
        /// <param name="_Fruit">The <see cref="Fruits.Fruit"/> type as an <see cref="ProtectedInt32"/></param>
        /// <param name="_Scale">
        /// The <see cref="Transform.localScale"/> fo the prefab for this <see cref="Fruits.Fruit"/> type <br/>
        /// <i>Must be divided by <see cref="FRUIT_SCALE_OFFSET"/>, if the <see cref="Transform.localScale"/> is taken from a <see cref="FruitPrefab"/></i>
        /// </param>
        private void Set(Sprite _Sprite, ProtectedInt32 _Fruit, Vector3 _Scale)
        {
            var _fruitTransform = (this.fruitImage.transform as RectTransform)!;
            var _faceTransform = (this.faceImage.transform as RectTransform)!;

            this.fruit = _Fruit;
            this.fruitImage.sprite = _Sprite;
            
            _fruitTransform.localScale = _Scale;
            _fruitTransform.sizeDelta = new Vector2(_Sprite.texture.width, _Sprite.texture.height);

            var _x = GetOffsetPosition(_Sprite.texture.width, _Sprite.pivot.x);
            var _y = GetOffsetPosition(_Sprite.texture.height, _Sprite.pivot.y);
            
            _faceTransform.anchoredPosition = Vector2.zero - new Vector2(_x, _y);
        }
        
        /// <summary>
        /// Calculates the face position offset for a fruit
        /// </summary>
        /// <param name="_Size">The total size of one axis in pixel</param>
        /// <param name="_PivotPosition">The <see cref="Sprite.pivot"/> point of one axis</param>
        /// <returns>The relative position on one axis</returns>
        private static float GetOffsetPosition(float _Size, float _PivotPosition)
        {
            var _extends = _Size / 2;
            var _offset = _extends - _PivotPosition;

            return _offset;
        }
        #endregion
    }
}