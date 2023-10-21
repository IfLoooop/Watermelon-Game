using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Fruit;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Points;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Development
{
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
        private new Camera camera;
#pragma warning restore CS0109
        private AudioSource audioSource;
        private TextMeshProUGUI savedText;
        
        [CanBeNull] private FruitBehaviour currentFruit;
        private readonly List<SavedFruit> savedFruits = new();
        
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
            this.SpawnGrape();
            this.SpawnFruit(KeyCode.F1, Fruit.Fruit.Grape);
            this.SpawnFruit(KeyCode.F2, Fruit.Fruit.Cherry);
            this.SpawnFruit(KeyCode.F3, Fruit.Fruit.Strawberry);
            this.SpawnFruit(KeyCode.F4, Fruit.Fruit.Lemon);
            this.SpawnFruit(KeyCode.F5, Fruit.Fruit.Orange);
            this.SpawnFruit(KeyCode.F6, Fruit.Fruit.Apple);
            this.SpawnFruit(KeyCode.F7, Fruit.Fruit.Pear);
            this.SpawnFruit(KeyCode.F8, Fruit.Fruit.Pineapple);
            this.SpawnFruit(KeyCode.F9, Fruit.Fruit.HoneyMelon);
            this.SpawnFruit(KeyCode.F10, Fruit.Fruit.Melon);
            this.FollowMouse();
            this.ReleaseFruit();
            this.MoveFruit();
            this.DeleteFruit();
            this.SaveFruitsOnMap();
            this.LoadFruit();
            this.SpawnUpgradedFruit();
            this.SetPoints();
            this.SetBackgroundMusic();
        }

        private void SpawnGrape()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                FruitSpawner.Instance.ResetFruitSpawner(false, Fruit.Fruit.Grape);
            }
        }
        
        private void SpawnFruit(KeyCode _KeyCode, Fruit.Fruit _Fruit)
        {
            if (Input.GetKeyDown(_KeyCode))
            {
                if (this.currentFruit != null)
                {
                    Destroy(this.currentFruit!.gameObject);
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

        private void FollowMouse()
        {
            if (this.currentFruit != null)
            {
                var _mouseWorldPosition = this.camera.ScreenToWorldPoint(Input.mousePosition);
                this.currentFruit.transform.position = _mouseWorldPosition.WithZ(0);
            }
        }

        private void ReleaseFruit()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (this.currentFruit != null)
                {
                    this.currentFruit.SetAnimation(false);
                    this.currentFruit.Release(null);
                    this.currentFruit = null;
                }
            }
        }
        
        private void MoveFruit()
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
                    
                    this.currentFruit!.Rigidbody2D.velocity = Vector2.zero;
                    this.currentFruit!.Rigidbody2D.angularVelocity = 0;
                }
                
                this.currentFruit = null;
            }  
        }
        
        private void DeleteFruit()
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (this.currentFruit == null)
                {
                    var _raycastHit2D = this.FruitRaycast();
                    
                    if (_raycastHit2D)
                    {
                        _raycastHit2D.transform.gameObject.GetComponent<FruitBehaviour>().Destroy();
                    }
                }
            }
        }

        private RaycastHit2D FruitRaycast()
        {
            var _ray = this.camera.ScreenPointToRay(Input.mousePosition);
            var _raycastHit2D = Physics2D.Raycast(_ray.origin, _ray.direction, Mathf.Infinity, LayerMask.GetMask("Fruit"));
            Debug.DrawRay(_ray.origin, _ray.direction * 100, Color.red, 5);

            return _raycastHit2D;
        }
        
        private void SaveFruitsOnMap()
        {
            if (Input.GetKeyDown(KeyCode.RightShift))
            {
                this.savedFruits.Clear();
                
                foreach (var (_, _fruitBehaviour) in GameController.Fruits_Debug)
                {
                    var _savedFruit = new SavedFruit(_fruitBehaviour);
                    this.savedFruits.Add(_savedFruit);
                }
                
                this.savedText.enabled = true;
                Invoke(nameof(DisableSavedText), 1);
                
                Debug.Log($"{this.savedFruits.Count} Fruits saved.");
            }
        }

        private void DisableSavedText()
        {
            this.savedText.enabled = false;
        }
        
        private void LoadFruit()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (this.savedFruits.Count > 0)
                {
                    var _fruitsOnMap = GameController.Fruits_Debug;
                
                    // ReSharper disable once InconsistentNaming
                    for (var i = _fruitsOnMap.Count - 1; i >= 0; i--)
                    {
                        _fruitsOnMap.ElementAt(i).Value.Destroy();
                    }

                    foreach (var _savedFruit in this.savedFruits)
                    {
                        var _fruit = this.SpawnFruit(_savedFruit.Position, _savedFruit.Fruit, _savedFruit.Rotation, false);
                        _fruit.Release(null);
                    }

                    FruitSpawner.Instance.BlockRelease = false;
                    
                    Debug.Log($"{this.savedFruits.Count} Fruits spawned.");
                }
                else
                {
                    Debug.LogWarning("No fruits are currently saved.");
                }
            }
        }
        
        private void SpawnUpgradedFruit()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                var _fruitBehaviour = this.SpawnFruit(base.transform.position, Fruit.Fruit.Grape, Quaternion.identity, false);
                _fruitBehaviour.GoldenFruit_Debug();
                _fruitBehaviour.Release(null);
            }
        }
        
        private void SetPoints()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                var _maxFruit = Enum.GetValues(typeof(Fruit.Fruit)).Length - 1;
                var _randomFruit = (Fruit.Fruit)Random.Range(0, _maxFruit);
                
                PointsController.Instance.AddPoints(_randomFruit);
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                var _randomNumber = (uint)Random.Range(1, 10);
                
                PointsController.Instance.SubtractPoints(_randomNumber);
            }
        }
        
        private void SetBackgroundMusic()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                var _enabled = this.audioSource.enabled;
                this.audioSource.enabled = !_enabled;   
            }
        }
        
        private FruitBehaviour SpawnFruit(Vector3 _Position, Fruit.Fruit _Fruit, Quaternion _Rotation, bool _SetAnimation)
        {
            var _fruitBehaviour = FruitBehaviour.SpawnFruit(_Position, _Fruit, false, _Rotation);
            _fruitBehaviour!.CanNotBeAddedToFruitCollection_DEBUG();
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
