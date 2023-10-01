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
        [SerializeField] private int spawnWeightMultiplier = 25;
        [SerializeField] private List<FruitData> fruits = new();
        #endregion

        #region Properties
        public int SpawnWeightMultiplier => this.spawnWeightMultiplier;
        public ReadOnlyCollection<FruitData> Fruits => this.fruits.AsReadOnly();
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
        #endregion
    }
}
