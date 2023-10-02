using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Watermelon_Game.Fruit
{
    /// <summary>
    /// Contains data and logic for all spawnable fruits
    /// </summary>
    [CreateAssetMenu(fileName = "FruitCollection", menuName = "ScriptableObjects/FruitCollection")]
    internal sealed class FruitCollection : ScriptableObject
    {
        #region Inspector Fields
        // TODO: Maybe use a different spawn weight for each individual fruit
        [SerializeField] private int spawnWeightDecrease = 25;
        [SerializeField] private List<FruitData> fruits = new();
        [SerializeField] private GameObject goldenFruitPrefab;
        [Tooltip("Chance for a Golden Fruit in %")]
        [SerializeField] private uint goldenFruitChance = 1;
        #endregion

        #region Properties
        public int SpawnWeightDecrease => this.spawnWeightDecrease;
        public ReadOnlyCollection<FruitData> Fruits => this.fruits.AsReadOnly();
        public GameObject GoldenFruitPrefab => this.goldenFruitPrefab;
        public uint GoldenFruitChance => this.goldenFruitChance;
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
                this.fruits[_index - 1].SpawnWeightDecrease = true;
            }
            // if (_index + 1 <= this.fruits.Count - 1)
            // {
            //     this.fruits[_index + 1].SpawnWeightMultiplier = true;
            // }

            this.fruits[_index].SpawnWeightDecrease = true;
        }
        #endregion
    }
}
