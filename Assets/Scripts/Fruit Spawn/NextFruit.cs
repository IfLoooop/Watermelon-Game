using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Fruit;

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
        [SerializeField] private AudioClip nextNextFruitEnabledAudio;
        [SerializeField] private AudioClip nextNextFruitDisabledAudio;
        #endregion
        
        #region Fields
        private FruitBehaviour nextFruitBehaviour;
        private FruitBehaviour nextNextFruitBehaviour;
        private TextMeshProUGUI timerText;
        private AudioSource audioSource;
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
            this.audioSource = base.GetComponent<AudioSource>();
            
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
            _fruitBehaviour.EnableAnimation(false);
            _fruitBehaviour.transform.SetParent(_NewParent, false);
            
            // Take from from NextNextFruit
            this.nextFruitBehaviour = this.nextNextFruitBehaviour;
            this.nextNextFruitBehaviour.transform.SetParent(this.nextFruit.transform, false);
            this.nextFruitBehaviour.EnableAnimation(true);
            
            // Spawn new fruit
            this.nextNextFruitBehaviour = FruitBehaviour.SpawnFruit(this.nextNextFruit.gameObject.transform.position, this.nextNextFruit.transform, this.nextFruitBehaviour.Fruit);
            this.nextNextFruitBehaviour.EnableAnimation(true);

            return _fruitBehaviour;
        }

        public void ShowNextNextFruit()
        {
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
                this.audioSource.Play(.1f, nextNextFruitEnabledAudio);
                this.nextNextFruitBehaviour.EnableAnimation(true);
                this.nextNextFruit.clip = this.nextNextFruitEnabledAnimation;
                this.nextNextFruit.Play();
                this.timer.clip = this.nextNextFruitEnabledAnimation;
                this.timer.Play();
            }
            else
            {
                this.audioSource.Play(0, nextNextFruitDisabledAudio);
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

        public void GameOVer()
        {
            this.currentNextNextFruitTimer = 0;
            this.SpawnFruits();
        }

        private void SpawnFruits()
        {
            this.nextFruitBehaviour = FruitBehaviour.SpawnFruit(this.nextFruit.transform.position, this.nextFruit.transform, null);
            this.nextNextFruitBehaviour = FruitBehaviour.SpawnFruit(this.nextNextFruit.gameObject.transform.position, this.nextNextFruit.transform, this.nextFruitBehaviour.Fruit);
            this.nextFruitBehaviour.EnableAnimation(true);
        }
        #endregion
    }
}