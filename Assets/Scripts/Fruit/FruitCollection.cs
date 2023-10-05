using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

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
        [SerializeField] private int spawnWeightMultiplier = 25;
        [SerializeField] private List<FruitData> fruits = new();
        [SerializeField] private AudioSource evolveSoundPrefab;
        [SerializeField] private GameObject goldenFruitPrefab;
        [Tooltip("How many fruits need to be on the map for a golden fruit spawn to be possible")]
        [SerializeField] private uint canSpawnAfter = 10;
        [Tooltip("Chance for a Golden Fruit in %")]
        [SerializeField] private float goldenFruitChance = 0.1f;
        [SerializeField] private Sprite faceDefault;
        [SerializeField] private Sprite faceHurt;
        #endregion

        #region Properties
        public int SpawnWeightMultiplier => this.spawnWeightMultiplier;
        public ReadOnlyCollection<FruitData> Fruits => this.fruits.AsReadOnly();
        public AudioSource EvolveSoundPrefab => this.evolveSoundPrefab;
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
        /// <summary>
        /// Enables the spawn weight multiplier based on the given previous fruit
        /// </summary>
        /// <param name="_PreviousFruit">Previous fruit spawn</param>
        public void SetWeightMultiplier(Fruit _PreviousFruit)
        {
            this.fruits.ForEach(_Fruit => _Fruit.ResetWeightMultiplier());
            
            var _index = this.fruits.FindIndex(_Fruit => _Fruit.Fruit == _PreviousFruit);

            if (_index - 1 >= 0)
            {
                this.fruits[_index - 1].SpawnWeightMultiplier = true;
            }
            // if (_index + 1 <= this.fruits.Count - 1)
            // {
            //     this.fruits[_index + 1].SpawnWeightMultiplier = true;
            // }

            this.fruits[_index].SpawnWeightMultiplier = true;
        }

        public void PlayEvolveSound()
        {
            var _gameObject = Instantiate(this.evolveSoundPrefab.gameObject);
            Destroy(_gameObject, this.evolveSoundPrefab.clip.length);
        }
        #endregion
    }
}
