using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon_Game.Utility;
using Watermelon_Game.Web;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Background
{
    internal sealed class BackgroundController : MonoBehaviour, IWebSettings
    {
        #region Inspector Fields
        [SerializeField] private BackgroundFruit backgroundFruitPrefab;
        [SerializeField] private float fruitSpawnDelay = .25f;
        [SerializeField] private float sizeMultiplier = 2;
        [SerializeField] private float forceMultiplier = 25;
        [SerializeField] private ForceMode2D forceMode = ForceMode2D.Impulse;
        [Range(0, 1)]
        [SerializeField] private float spriteAlphaValue = .33f;
        #endregion

        #region Fields
        private float xPosition; 
        private float yPosition;
        private ObjectPool<BackgroundFruit> fruitPool;
        private readonly List<(Sprite sprite, float sizeMultiplier)> fruitSprites = new();
        private float delay;
        private float maxFruitHeight;
        #endregion

        #region Properties
        public static BackgroundController Instance { get; private set; }
        public float SizeMultiplier => this.sizeMultiplier;
        public float ForceMultiplier => this.forceMultiplier;
        public ForceMode2D ForceMode => this.forceMode;
        public float SpriteAlphaValue => this.spriteAlphaValue;
        #endregion
        
        #region Methods
        private void Awake()
        {
            Instance = this;
            
            this.SetPositionValues();
            
            this.fruitPool = new ObjectPool<BackgroundFruit>(this.backgroundFruitPrefab, base.transform);
            this.delay = this.fruitSpawnDelay;
        }

        private void Start()
        {
            this.InitializeFruits();
        }

        private void InitializeFruits()
        {
            var _fruits = GameController.Instance.FruitCollection.Fruits;

            foreach (var _fruitData in _fruits)
            {
                var _sprite = _fruitData.Prefab.GetComponent<SpriteRenderer>().sprite;
                var _sizeMultiplier = _fruitData.Prefab.transform.localScale.x;
                this.fruitSprites.Add((_sprite, _sizeMultiplier));
            }

            this.maxFruitHeight = this.fruitSprites.Max(_Fruit => _Fruit.sprite.bounds.size.y);
        }
        
        private void Update()
        {
            this.SpawnFruit();
        }

        private void SpawnFruit()
        {
            this.delay -= Time.deltaTime;

            if (this.delay <= 0)
            {
                this.delay = this.fruitSpawnDelay;
                
                var _randomPosition = this.GetRandomPosition();
                var _fruit = this.fruitPool.Get(null, _randomPosition);
                var _sprite = this.GetRandomSprite();
                
                _fruit.SetSprite(_sprite);
                _fruit.SetForce();
            }
        }
        
        private Vector3 GetRandomPosition()
        {
            var _xPosition = Random.Range(-this.xPosition, this.xPosition);
            var _yPosition = this.yPosition + this.maxFruitHeight;
            var _randomPosition = new Vector3(_xPosition, _yPosition, base.transform.position.z);
            
            return _randomPosition;
        }

        private (Sprite sprite, float sizeMultiplier) GetRandomSprite()
        {
            var _maxIndex = this.fruitSprites.Count;
            var _randomIndex = Random.Range(0, _maxIndex);
            var _sprite = this.fruitSprites[_randomIndex];

            return _sprite;
        }

        private void OnRectTransformDimensionsChange()
        {
            this.SetPositionValues();
        }

        private void SetPositionValues()
        {
            var _camera = Camera.main!;
            var _normalizedViewportCoordinates = new Rect(0, 0, 1, 1);
            var _zPositionToCalculateAt = base.transform.position.z;
            var _frustumCorners = new Vector3[4];
            
            _camera.CalculateFrustumCorners(_normalizedViewportCoordinates, _zPositionToCalculateAt, Camera.MonoOrStereoscopicEye.Mono, _frustumCorners);

            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < _frustumCorners.Length; i++)
            {
                _frustumCorners[i] = _camera.ScreenToWorldPoint(_frustumCorners[i]);
            }

            var _xPosition = _frustumCorners.Max(_Vector3 => _Vector3.x);
            var _yPosition = _frustumCorners.Max(_Vector3 => _Vector3.y);

            this.xPosition = CeilAbsolute(_xPosition);
            this.yPosition = CeilAbsolute(_yPosition);
        }
        
        private static float CeilAbsolute(float _Value)
        {
            var _absoluteValue = Mathf.Abs(_Value);
            var _ceiledValue = Mathf.Ceil(_absoluteValue);

            return _ceiledValue;
        }

        public void ReturnToPool(BackgroundFruit _BackgroundFruit)
        {
            this.fruitPool.Return(_BackgroundFruit);
        }
        
        public void ApplyWebSettings()
        {
            WebSettings.TrySetValue(nameof(this.fruitSpawnDelay), ref this.fruitSpawnDelay);
            WebSettings.TrySetValue(nameof(this.sizeMultiplier), ref this.sizeMultiplier);
            WebSettings.TrySetValue(nameof(this.forceMultiplier), ref this.forceMultiplier);
            WebSettings.TrySetValue(nameof(this.spriteAlphaValue), ref this.spriteAlphaValue);
        }
        #endregion
    }
}