using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Fruits;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Points;
using Watermelon_Game.Utility;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Development
{
    /// <summary>
    /// Tool for use only in editor or in development builds
    /// </summary>
    public class DevelopmentTools : MonoBehaviour
    {
#if UNITY_EDITOR
        #region Inspector Fields
        [Tooltip("Displays which key was last pressed in a Debug.Log")]
        [SerializeField] private bool logKey;
        #endregion
#endif
        
#if DEBUG || DEVELOPMENT_BUILD
        #region Fields
#pragma warning disable CS0109
        /// <summary>
        /// Main <see cref="Camera"/> in the scene
        /// </summary>
        private new Camera camera;
#pragma warning restore CS0109
        /// <summary>
        /// <see cref="AudioSource"/> <see cref="Component"/> on <see cref="camera"/>
        /// </summary>
        private AudioSource audioSource;
        /// <summary>
        /// <see cref="TextMeshProUGUI"/>
        /// </summary>
        private TextMeshProUGUI savedText;
        
        /// <summary>
        /// The currently selected <see cref="FruitBehaviour"/>
        /// </summary>
        [CanBeNull] private FruitBehaviour currentFruit;
        private readonly List<SavedFruit> savedFruits = new();
        
        /// <summary>
        /// The key that was last pressed
        /// </summary>
        private KeyCode? lastPressedKey;
        #endregion
#endif
        
        #region Methods
        private void Awake()
        {
#if DEBUG || !DEVELOPMENT_BUILD
            if (!Application.isEditor && !Debug.isDebugBuild)
            {
                Destroy(this.gameObject);
                return;
            }      
#endif

#if DEBUG || DEVELOPMENT_BUILD
            this.camera = Camera.main;
            this.audioSource = this.camera!.gameObject.GetComponent<AudioSource>();
            this.savedText = base.GetComponentInChildren<TextMeshProUGUI>();
#endif
        }

#if DEBUG || DEVELOPMENT_BUILD
        private void Update()
        {
            this.ReplaceWithGrape();
            this.SpawnFruit(KeyCode.F1, Fruit.Grape);
            this.SpawnFruit(KeyCode.F2, Fruit.Cherry);
            this.SpawnFruit(KeyCode.F3, Fruit.Strawberry);
            this.SpawnFruit(KeyCode.F4, Fruit.Lemon);
            this.SpawnFruit(KeyCode.F5, Fruit.Orange);
            this.SpawnFruit(KeyCode.F6, Fruit.Apple);
            this.SpawnFruit(KeyCode.F7, Fruit.Pear);
            this.SpawnFruit(KeyCode.F8, Fruit.Pineapple);
            this.SpawnFruit(KeyCode.F9, Fruit.Honeymelon);
            this.SpawnFruit(KeyCode.F10, Fruit.Watermelon);
            this.FollowMouse();
            this.ReleaseFruit();
            this.SetCurrentFruit();
            this.DeleteFruit();
            this.SaveFruitsOnMap();
            this.LoadFruit();
            this.SpawnUpgradedFruit();
            this.SetPoints();
            this.SetBackgroundMusic();
        }

        /// <summary>
        /// Replaces the <see cref="Fruit"/> on the <see cref="FruitSpawner"/> with a <see cref="Fruit.Grape"/>
        /// </summary>
        private void ReplaceWithGrape()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                FruitSpawner.ResetFruitSpawner_DEVELOPMENT(Fruit.Grape);
            }
        }
        
        /// <summary>
        /// Spawns the given <see cref="Fruit"/> at the position of the mouse (in world space)
        /// </summary>
        /// <param name="_KeyCode">Key that needs to be pressed to spawn the <see cref="Fruit"/></param>
        /// <param name="_Fruit">The <see cref="Fruit"/> to spawn</param>
        private void SpawnFruit(KeyCode _KeyCode, Fruit _Fruit)
        {
            if (Input.GetKeyDown(_KeyCode))
            {
                if (this.currentFruit != null)
                {
                    this.currentFruit!.DestroyFruit();
                    this.currentFruit = null;
                    
                    if (this.lastPressedKey == _KeyCode)
                    {
                        return;   
                    }
                }
                
                this.lastPressedKey = _KeyCode;
                var _mouseWorldPosition = this.camera.ScreenToWorldPoint(Input.mousePosition);
                this.currentFruit = this.SpawnFruit(_mouseWorldPosition.WithZ(0), _Fruit, Quaternion.identity, true);
            }
        }

        /// <summary>
        /// Makes the <see cref="currentFruit"/> follow the position of the mouse (in world space)
        /// </summary>
        private void FollowMouse()
        {
            if (this.currentFruit != null)
            {
                var _mouseWorldPosition = this.camera.ScreenToWorldPoint(Input.mousePosition).WithZ(0);
                if (this.currentFruit.Rigidbody2D_DEVELOPMENT.simulated)
                {
                    this.currentFruit.Rigidbody2D_DEVELOPMENT.MovePosition(_mouseWorldPosition);   
                }
                else
                {
                    this.currentFruit.transform.position = _mouseWorldPosition;
                }
            }
        }

        /// <summary>
        /// Releases the <see cref="currentFruit"/> from the mouse and sets <see cref="currentFruit"/> to null
        /// </summary>
        private void ReleaseFruit()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (this.currentFruit != null)
                {
                    this.currentFruit.SetAnimation(false);
                    this.currentFruit.Release();
                    this.currentFruit = null;
                }
            }
        }
        
        /// <summary>
        /// Makes the clicked on <see cref="Fruit"/> the <see cref="currentFruit"/> so that it follows the mouse, while the mouse button is held down <br/>
        /// <see cref="Fruit"/> will be dropped on mouse release
        /// </summary>
        private void SetCurrentFruit()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (this.currentFruit == null)
                {
                    var _raycastHit2D = this.FruitRaycast();

                    if (_raycastHit2D)
                    {
                        var _fruitBehaviour = _raycastHit2D.transform.gameObject.GetComponent<FruitBehaviour>();
                        
                        this.currentFruit = _fruitBehaviour;
                    }   
                }
            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                if (this.currentFruit != null)
                {
                    
                    this.currentFruit!.Rigidbody2D_DEVELOPMENT.velocity = Vector2.zero;
                    this.currentFruit!.Rigidbody2D_DEVELOPMENT.angularVelocity = 0;
                }
                
                this.currentFruit = null;
            }  
        }
        
        /// <summary>
        /// Destroys the <see cref="Fruit"/> under the mouse
        /// </summary>
        private void DeleteFruit()
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (this.currentFruit == null)
                {
                    var _raycastHit2D = this.FruitRaycast();
                    
                    if (_raycastHit2D)
                    {
                        _raycastHit2D.transform.gameObject.GetComponent<FruitBehaviour>().DestroyFruit();
                    }
                }
            }
        }

        /// <summary>
        /// Sends a raycast from the mouse position into world space that checks if it hit a <see cref="Fruit"/>
        /// </summary>
        /// <returns></returns>
        private RaycastHit2D FruitRaycast()
        {
            var _ray = this.camera.ScreenPointToRay(Input.mousePosition);
            var _raycastHit2D = Physics2D.Raycast(_ray.origin, _ray.direction, Mathf.Infinity, LayerMaskController.FruitMask);
            Debug.DrawRay(_ray.origin, _ray.direction * 100, Color.red, 5);

            return _raycastHit2D;
        }
        
        /// <summary>
        /// Saves the position and rotation of all <see cref="Fruit"/>s on the map
        /// </summary>
        private void SaveFruitsOnMap()
        {
            if (Input.GetKeyDown(KeyCode.RightShift))
            {
                this.savedFruits.Clear();
                
                foreach (var _fruitBehaviour in FruitController.Fruits)
                {
                    var _savedFruit = new SavedFruit(_fruitBehaviour);
                    this.savedFruits.Add(_savedFruit);
                }
                
                this.savedText.enabled = true;
                Invoke(nameof(DisableSavedText), 1);
                
                Debug.Log($"{this.savedFruits.Count} Fruits saved.");
            }
        }

        /// <summary>
        /// Disables <see cref="savedText"/>
        /// </summary>
        private void DisableSavedText()
        {
            this.savedText.enabled = false;
        }
        
        /// <summary>
        /// Instantiates the <see cref="Fruit"/>s that have been saved to <see cref="savedFruits"/>
        /// </summary>
        private void LoadFruit()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (this.savedFruits.Count > 0)
                {
                    var _fruitsOnMap = FruitController.Fruits;
                
                    // ReSharper disable once InconsistentNaming
                    for (var i = _fruitsOnMap.Count - 1; i >= 0; i--)
                    {
                        _fruitsOnMap.ElementAt(i).DestroyFruit();
                    }

                    foreach (var _savedFruit in this.savedFruits)
                    {
                        this.SpawnFruit(_savedFruit.Position, _savedFruit.Fruit, _savedFruit.Rotation, false).Release();
                    }
                    
                    Debug.Log($"{this.savedFruits.Count} Fruits spawned.");
                }
                else
                {
                    Debug.LogWarning("No fruits are currently saved.");
                }
            }
        }
        
        /// <summary>
        /// Spawns an upgraded golden <see cref="Fruit"/>
        /// </summary>
        private void SpawnUpgradedFruit()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                var _fruitBehaviour = this.SpawnFruit(base.transform.position.WithY(CameraUtils.YFrustumPosition), Fruit.Grape, Quaternion.identity, false);
                _fruitBehaviour.GoldenFruit_Debug();
                _fruitBehaviour.Release();
            }
        }
        
        /// <summary>
        /// Adds or subtract a random amount of points
        /// </summary>
        private void SetPoints()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                var _maxFruit = Enum.GetValues(typeof(Fruit)).Length - 1;
                var _randomFruit = (Fruit)Random.Range(0, _maxFruit);
                
                PointsController.AddPoints_DEVELOPMENT(_randomFruit);
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                var _randomNumber = (uint)Random.Range(1, 10);
                
                PointsController.SubtractPoints_DEVELOPMENT(_randomNumber);
            }
        }
        
        /// <summary>
        /// Enables/disables the background music
        /// </summary>
        private void SetBackgroundMusic()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                var _enabled = this.audioSource.enabled;
                this.audioSource.enabled = !_enabled;   
            }
        }
        
        /// <summary>
        /// Spawns a <see cref="Fruit"/>
        /// </summary>
        /// <param name="_Position">Position to spawn the <see cref="Fruit"/> at</param>
        /// <param name="_Fruit">The <see cref="Fruit"/> to spawn</param>
        /// <param name="_Rotation">The rotation to spawn the <see cref="Fruit"/> with</param>
        /// <param name="_SetAnimation">Enable/disable the rotation animation of the <see cref="Fruit"/></param>
        /// <returns></returns>
        private FruitBehaviour SpawnFruit(Vector3 _Position, Fruit _Fruit, Quaternion _Rotation, bool _SetAnimation)
        {
            var _fruitBehaviour = FruitBehaviour.SpawnFruit(_Position, _Fruit, _Rotation);
            _fruitBehaviour!.gameObject.SetActive(true);
            _fruitBehaviour!.SetAnimation(_SetAnimation);

            return _fruitBehaviour;
        }
#endif

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (this.logKey)
            {
                var _currentEvent = Event.current;
                if (_currentEvent.isKey)
                {
                    Debug.Log(_currentEvent.keyCode);
                }
            }
        }
#endif
        #endregion  
    }
}
