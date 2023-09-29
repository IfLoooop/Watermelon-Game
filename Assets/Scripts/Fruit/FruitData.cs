using System;
using UnityEngine;

namespace Watermelon_Game.Fruit
{
    /// <summary>
    /// Contains data for a single fruit
    /// </summary>
    [Serializable]
    internal sealed class FruitData
    {
        #region Inspector Fields
        [SerializeField] private GameObject prefab;
        [SerializeField] private Fruit fruit;
        [SerializeField] private int spawnWeight;
        #endregion

        #region Properties
        public GameObject Prefab => this.prefab;
        public Fruit Fruit => this.fruit;
        public bool SpawnWeightMultiplier { get; set; }
        #endregion

        #region Methods
        public void ResetWeightMultiplier()
        {
            this.SpawnWeightMultiplier = false;
        }

        public int GetSpawnWeight()
        {
            var _spawnWeight = this.spawnWeight;

            if (this.SpawnWeightMultiplier)
            {
                _spawnWeight += FruitCollection.Instance.SpawnWeightMultiplier;
            }

            return _spawnWeight;
        }
        #endregion
    }
}