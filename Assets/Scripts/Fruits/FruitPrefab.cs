using System;
using System.Collections;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Contains data for a single <see cref="Fruits.Fruit"/>
    /// </summary>
    [Serializable]
    internal sealed class FruitPrefab
    {
        #region Inspector Fields
        [Tooltip("The Prefab of this Fruit")]
        [SerializeField] private GameObject prefab;
        [Tooltip("The type of this Fruit")]
        [ValueDropdown("fruits")]
        [SerializeField] private ProtectedInt32 fruit;
        // ReSharper disable once UnusedMember.Local
        private static IEnumerable fruits = new ValueDropdownList<ProtectedInt32>
        {
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Grape)}", (int)Watermelon_Game.Fruits.Fruit.Grape },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Cherry)}", (int)Watermelon_Game.Fruits.Fruit.Cherry },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Strawberry)}", (int)Watermelon_Game.Fruits.Fruit.Strawberry },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Lemon)}", (int)Watermelon_Game.Fruits.Fruit.Lemon },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Orange)}", (int)Watermelon_Game.Fruits.Fruit.Orange },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Apple)}", (int)Watermelon_Game.Fruits.Fruit.Apple },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Pear)}", (int)Watermelon_Game.Fruits.Fruit.Pear },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Pineapple)}", (int)Watermelon_Game.Fruits.Fruit.Pineapple },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Honeymelon)}", (int)Watermelon_Game.Fruits.Fruit.Honeymelon },
            { $"{nameof(Watermelon_Game.Fruits.Fruit.Watermelon)}", (int)Watermelon_Game.Fruits.Fruit.Watermelon },
        };
        #endregion

        #region Fields
        /// <summary>
        /// Determines the chance, this <see cref="Fruits.Fruit"/> has to be spawned <br/>
        /// <i>Higher number = bigger chance</i>
        /// </summary>
        private ProtectedInt32 spawnWeight;
        /// <summary>
        /// Determines whether the spawn weight multiplier is currently active for this <see cref="Watermelon_Game.Fruits.Fruit"/> type
        /// </summary>
        private ProtectedBool activeSpawnWeightMultiplier;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="prefab"/>
        /// </summary>
        public GameObject Prefab => this.prefab;
        /// <summary>
        /// <see cref="fruit"/>
        /// </summary>
        public ProtectedInt32 Fruit => this.fruit;
        #endregion

        #region Methods
        /// <summary>
        /// Sets <see cref="spawnWeight"/> to the given value
        /// </summary>
        /// <param name="_Value">The value to set <see cref="spawnWeight"/> to</param>
        public void SetSpawnWeight(int _Value)
        {
            this.spawnWeight = _Value;
        }
        
        /// <summary>
        /// Enables/disables the <see cref="activeSpawnWeightMultiplier"/> on this <see cref="FruitPrefab"/>
        /// </summary>
        /// <param name="_Value">The value to set the <see cref="activeSpawnWeightMultiplier"/> to</param>
        public void SetSpawnWeightMultiplier(bool _Value)
        {
            this.activeSpawnWeightMultiplier = _Value;
        }
        
        /// <summary>
        /// Returns this <see cref="FruitPrefab"/>s <see cref="spawnWeight"/> + the <see cref="FruitSettings.SpawnWeightModifier"/>, if <see cref="activeSpawnWeightMultiplier"/> if enabled
        /// </summary>
        /// <returns>This <see cref="FruitPrefab"/>s <see cref="spawnWeight"/> + the <see cref="FruitSettings.SpawnWeightModifier"/>, if <see cref="activeSpawnWeightMultiplier"/> if enabled</returns>
        public int GetSpawnWeight()
        {
            var _spawnWeight = this.spawnWeight;

            if (this.activeSpawnWeightMultiplier)
            {
                _spawnWeight = Mathf.Clamp(_spawnWeight + FruitSettings.SpawnWeightModifier, 0, int.MaxValue);
            }

            return _spawnWeight;
        }
        #endregion
    }
}