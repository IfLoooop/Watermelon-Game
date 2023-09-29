using System;
using UnityEngine;

namespace Watermelon_Game.Fruit
{
    [Serializable]
    internal sealed class FruitPrefab
    {
        #region Inspector Fields
        [SerializeField] private GameObject prefab;
        [SerializeField] private Fruit fruit;
        [SerializeField] private int spawnWeight;
        #endregion

        #region Properties
        public GameObject Prefab => this.prefab;
        public Fruit Fruit => this.fruit;
        public bool WeightMultiplier { get; set; }
        #endregion

        #region Methods
        public void ResetWeightMultiplier()
        {
            this.WeightMultiplier = false;
        }

        public int GetSpawnWeight()
        {
            var _spawnWeight = this.spawnWeight;

            if (this.WeightMultiplier)
            {
                _spawnWeight += FruitPrefabs.Instance.WeightMultiplier;
            }

            return _spawnWeight;
        }
        #endregion
    }
}