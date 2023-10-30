using System;
using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.Fruits;

namespace Watermelon_Game.Fruit_Spawn
{
    /// <summary>
    /// Holds the next fruit that will be given to the <see cref="FruitSpawner"/>
    /// </summary>
    internal sealed class NextFruit : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private GameObject nextFruit;
        [SerializeField] private Animation nextNextFruit;
        [SerializeField] private Animation timer;
        [Tooltip("Time in seconds, the NextNextFruit will be visible")]
        [SerializeField] private uint nextNextFruitTime = 300;
        [SerializeField] private AnimationClip nextNextFruitEnabledAnimation;
        [SerializeField] private AnimationClip nextNextFruitDisabledAnimation;
        #endregion
        
        #region Fields
        private FruitBehaviour nextFruitBehaviour;
        private FruitBehaviour nextNextFruitBehaviour;
        private TextMeshProUGUI timerText;
        private uint currentNextNextFruitTimer;
        #endregion

        #region Properties
        public static NextFruit Instance { get; private set; }
        #endregion
        
        #region Methods
        private void Awake()
        {
            Instance = this;
            this.timerText = this.timer.GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            GameController.OnResetGameStarted += this.ResetGame;
            FruitController.OnEvolve += ShowNextNextFruit;
        }

        private void OnDisable()
        {
            GameController.OnResetGameStarted -= this.ResetGame;
            FruitController.OnEvolve -= ShowNextNextFruit;
        }

        private void Start()
        {
            this.SpawnFruits();
        }

        /// <summary>
        /// Returns the <see cref="nextFruitBehaviour"/> currently held by <see cref="NextFruit"/>
        /// </summary>
        /// <param name="_NewParent">The new parent of the fruit</param>
        /// <returns>The <see cref="FruitBehaviour"/> of the spawned fruit <see cref="GameObject"/></returns>
        public FruitBehaviour GetFruit(Transform _NewParent)
        {
            // Give fruit to FruitSpawner
            var _fruitBehaviour = this.nextFruitBehaviour;
            _fruitBehaviour.SetAnimation(false);
            _fruitBehaviour.transform.SetParent(_NewParent, false);
            
            // Take from from NextNextFruit
            this.nextFruitBehaviour = this.nextNextFruitBehaviour;
            this.nextNextFruitBehaviour.transform.SetParent(this.nextFruit.transform, false);
            
            // Spawn new fruit
            this.nextNextFruitBehaviour = FruitBehaviour.SpawnFruit(this.nextNextFruit.gameObject.transform.position, this.nextNextFruit.transform, this.nextFruitBehaviour.Fruit);

            return _fruitBehaviour;
        }

#if DEBUG || DEVELOPMENT_BUILD
        public FruitBehaviour GetFruit(Transform _NewParent, Fruit _Fruit)
        {
            var _fruitBehaviour = FruitBehaviour.SpawnFruit(_NewParent.transform.position, _Fruit);
            _fruitBehaviour.transform.SetParent(_NewParent, true);
            _fruitBehaviour.gameObject.SetActive(true);
            _fruitBehaviour.SetAnimation(false);
            
            return _fruitBehaviour;
        }
#endif
        
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
        
        private void EnableNextNextFruit(bool _Value)
        {
            if (_Value)
            {
                AudioPool.PlayClip(AudioClipName.NextNextFruitEnabled);
                this.nextNextFruitBehaviour.SetAnimation(true);
                this.nextNextFruit.clip = this.nextNextFruitEnabledAnimation;
                this.nextNextFruit.Play();
                this.timer.clip = this.nextNextFruitEnabledAnimation;
                this.timer.Play();
            }
            else
            {
                AudioPool.PlayClip(AudioClipName.NextNextFruitDisabled);
                this.nextNextFruit.clip = this.nextNextFruitDisabledAnimation;
                this.nextNextFruit.Play();
                this.timer.clip = this.nextNextFruitDisabledAnimation;
                this.timer.Play();
            }
        }
        
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
            this.SpawnFruits();
        }

        private void SpawnFruits()
        {
            DestroyFruit(this.nextFruitBehaviour);
            DestroyFruit(this.nextNextFruitBehaviour);
            
            this.nextFruitBehaviour = FruitBehaviour.SpawnFruit(this.nextFruit.transform.position, this.nextFruit.transform, null);
            this.nextNextFruitBehaviour = FruitBehaviour.SpawnFruit(this.nextNextFruit.gameObject.transform.position, this.nextNextFruit.transform, this.nextFruitBehaviour.Fruit);
        }

        private void DestroyFruit([CanBeNull] FruitBehaviour _FruitBehaviour)
        {
            if (_FruitBehaviour != null)
            {
                Destroy(_FruitBehaviour.gameObject);
            }
        }
        #endregion
    }
}