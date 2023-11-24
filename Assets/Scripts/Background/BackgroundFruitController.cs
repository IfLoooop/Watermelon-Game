using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon_Game.Fruits;
using Watermelon_Game.Utility;
using Watermelon_Game.Web;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Background
{
    /// <summary>
    /// Controls the <see cref="BackgroundFruit"/> in the background of the scene
    /// </summary>
    internal sealed class BackgroundFruitController : MonoBehaviour
    {
        #region Websettings
        [Header("WebSettings")]
        [Tooltip("Time in seconds, between the fruit spawns")]
        [ShowInInspector] private static float fruitSpawnDelay = .25f;
        [Tooltip("Is multiplied on the size of every background fruit")]
        [ShowInInspector] private static float sizeMultiplier = 1;
        [Tooltip("Controls the force with which the fruits are dropped")]
        [ShowInInspector] private static float forceMultiplier = 25;
        [Tooltip("Alpha value for the sprites, must be between 0-1")]
        [Range(0, 1)]
        [ShowInInspector] private static float spriteAlphaValue = .33f;
        #endregion
        
        #region Inspector Fields
        [Header("Settings")]
        [Tooltip("BackgroundFruit-script on a prefab")]
        [SerializeField] private BackgroundFruit backgroundFruitPrefab;
        [Tooltip("The rotation of a fruit increases as the value moves farther from 0 (Negative and positive values)")]
        [ShowInInspector] private Vector2 rotationForce = new(.125f, .125f);
        [Tooltip("Option for how to apply a force using Rigidbody2D.AddForce")]
        [SerializeField] private ForceMode2D forceMode = ForceMode2D.Impulse;
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="BackgroundFruitController"/>
        /// </summary>
        private static BackgroundFruitController instance;
        /// <summary>
        /// Height of the biggest fruit
        /// </summary>
        private float biggestFruitHeight;
        /// <summary>
        /// The current spawn delay
        /// </summary>
        private float currentDelay;
        /// <summary>
        /// <see cref="ObjectPool{T}"/> for the <see cref="BackgroundFruit"/>s
        /// </summary>
        private ObjectPool<BackgroundFruit> fruitPool;
        /// <summary>
        /// Contains the <see cref="Sprite"/> and prefab-size of all spawnable fruits
        /// </summary>
        private readonly List<(Sprite sprite, float fruitPrefabSize)> fruitSprites = new();
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="fruitSpawnDelay"/>
        /// </summary>
        public static float FruitSpawnDelay => fruitSpawnDelay;
        /// <summary>
        /// <see cref="sizeMultiplier"/>
        /// </summary>
        public static float SizeMultiplier => sizeMultiplier;
        /// <summary>
        /// <see cref="forceMultiplier"/>
        /// </summary>
        public static float ForceMultiplier => forceMultiplier;
        /// <summary>
        /// <see cref="spriteAlphaValue"/>
        /// </summary>
        public static float SpriteAlphaValue => spriteAlphaValue;
        /// <summary>
        /// <see cref="rotationForce"/>
        /// </summary>
        public static Vector2 RotationForce => instance.rotationForce;
        /// <summary>
        /// <see cref="forceMode"/>
        /// </summary>
        public static ForceMode2D ForceMode => instance.forceMode;
        #endregion
        
        #region Methods
        // TODO: In some videos the settings are completely false, no idea why, try the next build without the WebSettings
        // /// <summary>
        // /// Needs to be called with <see cref="RuntimeInitializeLoadType.BeforeSplashScreen"/>
        // /// </summary>
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        // private static void SubscribeToWebSettings()
        // {
        //     WebSettings.OnApplyWebSettings += ApplyWebSettings;
        // }
        //
        // private void OnDestroy()
        // {
        //     WebSettings.OnApplyWebSettings -= ApplyWebSettings;
        // }
        //
        // /// <summary>
        // /// Tries to set the values from the web settings
        // /// </summary>
        // private static void ApplyWebSettings()
        // {
        //     var _callerType = typeof(BackgroundFruitController);
        //     WebSettings.TrySetValue(nameof(FruitSpawnDelay), ref fruitSpawnDelay, _callerType);
        //     WebSettings.TrySetValue(nameof(SizeMultiplier), ref sizeMultiplier, _callerType);
        //     WebSettings.TrySetValue(nameof(ForceMultiplier), ref forceMultiplier, _callerType);
        //     WebSettings.TrySetValue(nameof(SpriteAlphaValue), ref spriteAlphaValue, _callerType);
        // }
        
        private void Awake()
        {
            instance = this;
            
            this.fruitPool = new ObjectPool<BackgroundFruit>(this.backgroundFruitPrefab, base.transform);
            this.currentDelay = fruitSpawnDelay;
        }

        private void Start()
        {
            this.InitializeFruits();
        }

        /// <summary>
        /// Initializes all needed data
        /// </summary>
        private void InitializeFruits()
        {
            var _fruits = FruitPrefabSettings.FruitPrefabs;

            foreach (var _fruitData in _fruits)
            {
                var _sprite = _fruitData.Sprite;
                var _fruitPrefabSize = _fruitData.Scale.Value.x;
                
                this.fruitSprites.Add((_sprite, _fruitPrefabSize));
            }

            this.biggestFruitHeight = this.fruitSprites.Max(_Fruit => _Fruit.sprite.bounds.size.y);
        }
        
        private void Update()
        {
            this.SpawnFruit();
        }

        /// <summary>
        /// Spawns a new fruit after each <see cref="fruitSpawnDelay"/>
        /// </summary>
        private void SpawnFruit()
        {
            this.currentDelay -= Time.deltaTime;

            if (this.currentDelay <= 0)
            {
                this.currentDelay = fruitSpawnDelay;
                
                var _randomPosition = this.GetRandomPosition();
                var _fruit = this.fruitPool.Get(_randomPosition);
                var _fruitData = this.GetRandomSprite();
                
                _fruit.Set(_fruitData);
                _fruit.SetForce();
            }
        }
        
        /// <summary>
        /// Gets a random position in between and above the camera frustum
        /// </summary>
        /// <returns>A random position in between and above the camera frustum</returns>
        private Vector3 GetRandomPosition()
        {
            var _xPosition = Random.Range(-CameraUtils.XFrustumPosition, CameraUtils.XFrustumPosition);
            var _yPosition = CameraUtils.YFrustumPosition + this.biggestFruitHeight;
            var _randomPosition = new Vector3(_xPosition, _yPosition, base.transform.position.z);
            
            return _randomPosition;
        }

        /// <summary>
        /// Gets a random <see cref="Sprite"/> from <see cref="fruitSprites"/>
        /// </summary>
        /// <returns>A random <see cref="Sprite"/> from <see cref="fruitSprites"/></returns>
        private (Sprite sprite, float fruitPrefabSize) GetRandomSprite()
        {
            var _maxIndex = this.fruitSprites.Count;
            var _randomIndex = Random.Range(0, _maxIndex);
            var _fruitData = this.fruitSprites[_randomIndex];

            return _fruitData;
        }
        
        /// <summary>
        /// Returns the given <see cref="BackgroundFruit"/> back to <see cref="fruitPool"/>
        /// </summary>
        /// <param name="_BackgroundFruit">The <see cref="BackgroundFruit"/> to return to <see cref="fruitPool"/></param>
        public static void ReturnToPool(BackgroundFruit _BackgroundFruit)
        {
            instance.fruitPool.Return(_BackgroundFruit);
        }
        #endregion
    }
}