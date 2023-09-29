using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Watermelon_Game.Fruit
{
    [CreateAssetMenu(fileName = "FruitPrefabs", menuName = "ScriptableObjects/FruitPrefabs")]
    internal sealed class FruitPrefabs : ScriptableObject
    {
        #region Inspector Fields
        [SerializeField] private int weightMultiplier;
        [SerializeField] private List<FruitPrefab> fruits = new();
        #endregion

        #region Fields
        private static FruitPrefabs instance;
        #endregion
        
        #region Properties
        public static FruitPrefabs Instance => instance;

        public int WeightMultiplier => this.weightMultiplier;
        public ReadOnlyCollection<FruitPrefab> Fruits => this.fruits.AsReadOnly();
        #endregion

        #region Methods
        private void OnEnable()
        {
            instance = this;
        }

        public void SetWeightMultiplier(Fruit _PreviousFruit)
        {
            this.fruits.ForEach(_Fruit => _Fruit.ResetWeightMultiplier());
            
            var _index = this.fruits.FindIndex(_Fruit => _Fruit.Fruit == _PreviousFruit);

            if (_index - 1 >= 0)
            {
                this.fruits[_index - 1].WeightMultiplier = true;
            }
            if (_index + 1 <= this.fruits.Count - 1)
            {
                this.fruits[_index + 1].WeightMultiplier = true;
            }

            this.fruits[_index].WeightMultiplier = true;
        }
        #endregion
    }
}
