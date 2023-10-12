using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Watermelon_Game.Web;

namespace Watermelon_Game.Fruit
{
    /// <summary>
    /// Contains data and logic for all spawnable fruits
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObjects/FruitCollection", fileName = "FruitCollection")]
    internal sealed class FruitCollection : ScriptableObject
    {
        #region Inspector Fields
        // TODO: Maybe use a different spawn weight for each individual fruit
        [SerializeField] private int spawnWeightMultiplier = -25;
        [SerializeField] private bool lowerIndexWeight = true;
        [SerializeField] private bool higherIndexWeight = false;
        [SerializeField] private bool indexWeight = true;
        [SerializeField] private List<FruitData> fruits = new();
        [SerializeField] private AudioSource evolveSoundPrefab;
        [SerializeField] private GameObject goldenFruitPrefab;
        [Tooltip("How many fruits need to be on the map for a golden fruit spawn to be possible")]
        [SerializeField] private uint canSpawnAfter = 10;
        [Tooltip("Chance for a Golden Fruit in %")]
        [SerializeField] private float goldenFruitChance = 0.01f;
        [SerializeField] private Sprite faceDefault;
        [SerializeField] private Sprite faceHurt;
        #endregion

        #region Properties
        public int SpawnWeightMultiplier => this.spawnWeightMultiplier;
        public ReadOnlyCollection<FruitData> Fruits => this.fruits.AsReadOnly();
        public GameObject GoldenFruitPrefab => this.goldenFruitPrefab;
        /// <summary>
        /// How many fruits need to be on the map for a golden fruit spawn to be possible
        /// </summary>
        public uint CanSpawnAfter => this.canSpawnAfter;
        /// <summary>
        /// Chance for a Golden Fruit in %
        /// </summary>
        public float GoldenFruitChance => this.goldenFruitChance;
        public Sprite FaceDefault => this.faceDefault;
        public Sprite FaceHurt => this.faceHurt;
        #endregion

        #region Methods
        public void ApplyWebSettings(Dictionary<string, object> _Settings, ReadOnlyDictionary<uint, string> _FruitMap)
        {
            WebSettings.TrySetValue(_Settings, nameof(this.spawnWeightMultiplier), ref this.spawnWeightMultiplier);
            WebSettings.TrySetValue(_Settings, nameof(this.lowerIndexWeight), ref this.lowerIndexWeight);
            WebSettings.TrySetValue(_Settings, nameof(this.higherIndexWeight), ref this.higherIndexWeight);
            WebSettings.TrySetValue(_Settings, nameof(this.indexWeight), ref this.indexWeight);
            WebSettings.TrySetValue(_Settings, nameof(this.goldenFruitChance), ref this.goldenFruitChance);
            WebSettings.TrySetValue(_Settings, _FruitMap[0], this.fruits[0]);
            WebSettings.TrySetValue(_Settings, _FruitMap[1], this.fruits[1]);
            WebSettings.TrySetValue(_Settings, _FruitMap[2], this.fruits[2]);
            WebSettings.TrySetValue(_Settings, _FruitMap[3], this.fruits[3]);
            WebSettings.TrySetValue(_Settings, _FruitMap[4], this.fruits[4]);
            WebSettings.TrySetValue(_Settings, _FruitMap[5], this.fruits[5]);
            WebSettings.TrySetValue(_Settings, _FruitMap[6], this.fruits[6]);
            WebSettings.TrySetValue(_Settings, _FruitMap[7], this.fruits[7]);
            WebSettings.TrySetValue(_Settings, _FruitMap[8], this.fruits[8]);
            WebSettings.TrySetValue(_Settings, _FruitMap[9], this.fruits[9]);
        }
        
        /// <summary>
        /// Enables the spawn weight multiplier based on the given previous fruit
        /// </summary>
        /// <param name="_PreviousFruit">Previous fruit spawn</param>
        public void SetWeightMultiplier(Fruit _PreviousFruit)
        {
            this.fruits.ForEach(_Fruit => _Fruit.ResetWeightMultiplier());
            
            var _index = this.fruits.FindIndex(_Fruit => _Fruit.Fruit == _PreviousFruit);

            if (this.lowerIndexWeight && _index - 1 >= 0)
            {
                this.fruits[_index - 1].SpawnWeightMultiplier = true;
            }
            if (this.higherIndexWeight && _index + 1 <= this.fruits.Count - 1)
            {
                this.fruits[_index + 1].SpawnWeightMultiplier = true;
            }
            if (this.indexWeight)
            {
                this.fruits[_index].SpawnWeightMultiplier = true;
            }
        }

        public void PlayEvolveSound()
        {
            var _gameObject = Instantiate(this.evolveSoundPrefab.gameObject);
            Destroy(_gameObject, this.evolveSoundPrefab.clip.length);
        }
        #endregion
    }
}
