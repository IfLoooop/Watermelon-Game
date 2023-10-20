using JetBrains.Annotations;
using UnityEngine;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Fruit;

namespace Watermelon_Game.Development
{
    public class DevelopmentTools : MonoBehaviour
    {
#if DEBUG || DEVELOPMENT_BUILD
        #region Fields
#pragma warning disable CS0109
        private new Camera camera;
#pragma warning restore CS0109
        private AudioSource audioSource;
        
        [CanBeNull] private FruitBehaviour currentFruit;
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
#endif
        }

#if DEBUG || DEVELOPMENT_BUILD
        private void Update()
        {
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
            this.DeleteFruit();
            this.SpawnUpgradedFruit();
            this.SetBackgroundMusic();
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
                this.currentFruit = FruitBehaviour.SpawnFruit(_mouseWorldPosition.WithZ(0), _Fruit, false);
                this.currentFruit!.CanNotBeAddedToFruitCollection_DEBUG();
                this.currentFruit!.gameObject.SetActive(true);
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
                    this.currentFruit.Release(null, Vector2.down);
                    this.currentFruit = null;
                }   
            }
        }

        private void DeleteFruit()
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (this.currentFruit == null)
                {
                    var _ray = this.camera.ScreenPointToRay(Input.mousePosition);
                    var _raycastHit2D = Physics2D.Raycast(_ray.origin, _ray.direction, Mathf.Infinity, LayerMask.GetMask("Fruit"));
                    Debug.DrawRay(_ray.origin, _ray.direction * 100, Color.red, 5);
                    
                    if (_raycastHit2D)
                    {
                        _raycastHit2D.transform.gameObject.GetComponent<FruitBehaviour>().Destroy();
                    }
                }
            }
        }
        
        private void SpawnUpgradedFruit()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                var _fruitBehaviour = FruitBehaviour.SpawnFruit(base.transform.position, Fruit.Fruit.Grape, false);
                _fruitBehaviour.CanNotBeAddedToFruitCollection_DEBUG();
                _fruitBehaviour.gameObject.SetActive(true);
                _fruitBehaviour.GoldenFruit_Debug();
                _fruitBehaviour.Release(null, Vector2.down);
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
#endif
        #endregion  
    }
}
