using System;
using System.Collections;
using System.Linq;
using OPS.AntiCheat.Field;
using TMPro;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruits;
using Watermelon_Game.Singletons;
using Random = UnityEngine.Random;

namespace Watermelon_Game.Fruit_Spawn
{
    /// <summary>
    /// Holds the next fruit that will be given to the <see cref="FruitSpawner"/>
    /// </summary>
    internal sealed class NextFruit : PersistantGameModeTransition<NextFruit>
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Fruit of the NextFruit")]
        [SerializeField] private NextFruitData nextFruit;
        [Tooltip("Fruit of the NextNextFruit")]
        [SerializeField] private NextFruitData nextNextFruit;
        [Tooltip("Animation component of the NextNextFruit")]
        [SerializeField] private Animation nextNextFruitAnimation;
        [Tooltip("Animation component of the Timer")]
        [SerializeField] private Animation timer;
        [Tooltip("Is played when the NextNextFruit is enabled")]
        [SerializeField] private AnimationClip nextNextFruitEnabledAnimation;
        [Tooltip("Is played when the NextNextFruit is disabled")]
        [SerializeField] private AnimationClip nextNextFruitDisabledAnimation;
        
        [Header("Settings")]
        [Tooltip("Time in seconds, the NextNextFruit will be visible")]
        [SerializeField] private ProtectedUInt32 nextNextFruitTime = 300;
        #endregion
        
        #region Fields
        /// <summary>
        /// <see cref="TextMeshProUGUI"/> component that displays the time
        /// </summary>
        private TextMeshProUGUI timerText;
        /// <summary>
        /// The remaining time to display the next next fruit
        /// </summary>
        private ProtectedUInt32 currentNextNextFruitTimer;
        #endregion
        
        #region Methods
        protected override void Init()
        {
            base.Init();
            this.timerText = this.timer.GetComponent<TextMeshProUGUI>();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            GameController.OnGameStart += this.SpawnFruits;
            GameController.OnResetGameStarted += this.ResetGame;
            FruitController.OnEvolve += ShowNextNextFruit;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            GameController.OnGameStart -= this.SpawnFruits;
            GameController.OnResetGameStarted -= this.ResetGame;
            FruitController.OnEvolve -= ShowNextNextFruit;
        }
        
        /// <summary>
        /// Returns the <see cref="nextFruit"/> currently held by <see cref="NextFruit"/> <br/>
        /// <b>For <see cref="FruitSpawner"/></b>
        /// </summary>
        /// <param name="_Rotation">The current rotation of the fruit in <see cref="nextFruit"/></param>
        /// <returns>The type of the <see cref="Fruit"/> for the <see cref="FruitSpawner"/></returns>
        public static ProtectedInt32 GetFruit(out Quaternion _Rotation)
        {
            Instance.nextFruit.gameObject.SetActive(true);
            Instance.nextNextFruit.gameObject.SetActive(true);
            
            // Give fruit to FruitSpawner
            var _fruit = Instance.nextFruit.Fruit.Value;
            _Rotation = Instance.nextFruit.transform.rotation;
            
            // Take from from NextNextFruit
            Instance.nextFruit.CopyFruit(Instance.nextNextFruit);
            
            // Spawn new fruit
            Instance.nextNextFruit.CopyFruit(GetRandomFruit(_fruit));
            
            return _fruit;
        }
        
        /// <summary>
        /// Returns a random <see cref="Fruits.Fruit"/> which spawn weight depend on the given <see cref="_PreviousFruit"/>
        /// </summary>
        /// <param name="_PreviousFruit">The previously spawned <see cref="Fruits.Fruit"/></param>
        /// <returns>A random <see cref="Fruits.Fruit"/> which spawn weight depend on the given <see cref="_PreviousFruit"/></returns>
        private static FruitPrefab GetRandomFruit(ProtectedInt32? _PreviousFruit)
        {
            if (_PreviousFruit == null)
            {
                return FruitPrefabSettings.FruitPrefabs.First(_FruitData => (Fruit)_FruitData.Fruit.Value == Fruit.Cherry);
            }
         
            FruitController.SetWeightMultiplier(_PreviousFruit.Value);

            var _highestFruitSpawn = FruitPrefabSettings.FruitPrefabs.First(_Fruit => _Fruit.GetSpawnWeight() == 0).Fruit;
            var _spawnableFruits = FruitPrefabSettings.FruitPrefabs.TakeWhile(_Fruit => (int)_Fruit.Fruit < (int)_highestFruitSpawn).ToArray();
            
            var _combinedSpawnWeights = _spawnableFruits.Sum(_Fruit => _Fruit.GetSpawnWeight());
            var _randomNumber = Random.Range(0, _combinedSpawnWeights);
            var _spawnWeight = 0;

            foreach (var _fruitData in _spawnableFruits)
            {
                if (_randomNumber <= _fruitData.GetSpawnWeight() + _spawnWeight)
                {
                    return _fruitData;
                }

                _spawnWeight += _fruitData.GetSpawnWeight();
            }

            throw new NullReferenceException("No suitable fruit could be found, make sure the order of the fruits is always the same as declared in the \"Fruits.cs\"-Enum");
        }
        
        /// <summary>
        /// Displays the NextNextFruit, when the given <see cref="Fruit"/> is a <see cref="Fruit.Watermelon"/> -> <see cref="FruitController.OnEvolve"/>
        /// </summary>
        /// <param name="_Fruit">The evolved <see cref="Fruit"/></param>
        private void ShowNextNextFruit(Fruit _Fruit)
        {
            if (_Fruit != Fruit.Watermelon)
            {
                return;
            }
            
            var _waitTime = new WaitForSeconds(1);
            
            if (this.currentNextNextFruitTimer <= 0)
            {
                base.StartCoroutine(this.ShowNextNextFruit(_waitTime));
            }
            else
            {
                this.currentNextNextFruitTimer = this.nextNextFruitTime;
            }
        }
        
        /// <summary>
        /// Displays the next next for for a duration of <see cref="nextNextFruitTime"/>
        /// </summary>
        /// <param name="_WaitTime">Wait time between each decrement of <see cref="currentNextNextFruitTimer"/></param>
        /// <returns></returns>
        private IEnumerator ShowNextNextFruit(WaitForSeconds _WaitTime)
        {
            this.currentNextNextFruitTimer = this.nextNextFruitTime;
            this.SetTimer();
            this.EnableNextNextFruit(true);
            
            while (this.currentNextNextFruitTimer > 0)
            {
                this.currentNextNextFruitTimer--;
                yield return _WaitTime;
                
                this.SetTimer();
            }
            
            this.EnableNextNextFruit(false);
        }
        
        /// <summary>
        /// Enables/disables the next next fruit, based on the given value
        /// </summary>
        /// <param name="_Value">True to enabled, false to disable</param>
        private void EnableNextNextFruit(bool _Value)
        {
            if (_Value)
            {
                AudioPool.PlayClip(AudioClipName.NextNextFruitEnabled);
                this.nextNextFruitAnimation.clip = this.nextNextFruitEnabledAnimation;
                this.nextNextFruitAnimation.Play();
                this.timer.clip = this.nextNextFruitEnabledAnimation;
                this.timer.Play();
            }
            else
            {
                AudioPool.PlayClip(AudioClipName.NextNextFruitDisabled);
                this.nextNextFruitAnimation.clip = this.nextNextFruitDisabledAnimation;
                this.nextNextFruitAnimation.Play();
                this.timer.clip = this.nextNextFruitDisabledAnimation;
                this.timer.Play();
            }
        }
        
        /// <summary>
        /// Sets the <see cref="TextMeshProUGUI.text"/> of <see cref="timerText"/> to <see cref="currentNextNextFruitTimer"/>
        /// </summary>
        private void SetTimer()
        {
            var _time = TimeSpan.FromSeconds(this.currentNextNextFruitTimer);
            this.timerText.text = _time.ToString("m\\:ss");
        }

        /// <summary>
        /// <see cref="GameController.OnResetGameStarted"/>
        /// </summary>
        private void ResetGame()
        {
            this.currentNextNextFruitTimer = 0;
        }

        /// <summary>
        /// Spawns a new fruit for <see cref="nextFruit"/> and <see cref="nextNextFruit"/> <br/>
        /// <i>Old values are overwritten</i> <br/>
        /// <b>Subscribed to <see cref="GameController.OnGameStart"/></b>
        /// </summary>
        private void SpawnFruits()
        {
            this.nextFruit.CopyFruit(GetRandomFruit(null));
            this.nextNextFruit.CopyFruit(GetRandomFruit(this.nextFruit.Fruit));
        }
        #endregion
    }
}