using UnityEngine;
using Watermelon_Game.Fruit;

namespace Watermelon_Game.Development
{
    internal readonly struct SavedFruit
    {
        #region Properties
        public Fruit.Fruit Fruit { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        #endregion

        #region Constructor
        public SavedFruit(FruitBehaviour _FruitBehaviour)
        {
            this.Fruit = _FruitBehaviour.Fruit;
            var _transform = _FruitBehaviour.transform;
            this.Position = _transform.position;
            this.Rotation = _transform.rotation;
        }
        #endregion
    }
}