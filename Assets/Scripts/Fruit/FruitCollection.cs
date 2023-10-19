using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Watermelon_Game.Web;
using static Watermelon_Game.Web.WebSettings;

namespace Watermelon_Game.Fruit
{
    /// <summary>
    /// Contains data and logic for all spawnable fruits
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObjects/FruitCollection", fileName = "FruitCollection")]
    internal sealed class FruitCollection : ScriptableObject, IWebSettings
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
        [SerializeField] private float massMultiplier = 2.5f;
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
        public float MassMultiplier => this.massMultiplier;
        #endregion

        #region Methods
        public void ApplyWebSettings()
        {
            TrySetValue(nameof(this.spawnWeightMultiplier), ref this.spawnWeightMultiplier);
            TrySetValue(nameof(this.lowerIndexWeight), ref this.lowerIndexWeight);
            TrySetValue(nameof(this.higherIndexWeight), ref this.higherIndexWeight);
            TrySetValue(nameof(this.indexWeight), ref this.indexWeight);
            TrySetValue(nameof(this.goldenFruitChance), ref this.goldenFruitChance);
            TrySetValue(nameof(this.massMultiplier), ref this.massMultiplier);
            TrySetValue(FruitSpawnWeightMap[0], this.fruits[0]);
            TrySetValue(FruitSpawnWeightMap[1], this.fruits[1]);
            TrySetValue(FruitSpawnWeightMap[2], this.fruits[2]);
            TrySetValue(FruitSpawnWeightMap[3], this.fruits[3]);
            TrySetValue(FruitSpawnWeightMap[4], this.fruits[4]);
            TrySetValue(FruitSpawnWeightMap[5], this.fruits[5]);
            TrySetValue(FruitSpawnWeightMap[6], this.fruits[6]);
            TrySetValue(FruitSpawnWeightMap[7], this.fruits[7]);
            TrySetValue(FruitSpawnWeightMap[8], this.fruits[8]);
            TrySetValue(FruitSpawnWeightMap[9], this.fruits[9]);
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
