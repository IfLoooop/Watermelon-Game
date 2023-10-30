using UnityEngine;
using Watermelon_Game.Fruits;

namespace Watermelon_Game.Development
{
    /// <summary>
    /// Holds information of a <see cref="Fruit"/> <br/>
    /// <i>For development only</i>
    /// </summary>
    internal readonly struct SavedFruit
    {
        #region Properties
        /// <summary>
        /// <see cref="Watermelon_Game.Fruits.Fruit"/>
        /// </summary>
        public Fruit Fruit { get; }
        /// <summary>
        /// Position of the <see cref="Watermelon_Game.Fruits.Fruit"/>
        /// </summary>
        public Vector3 Position { get; }
        /// <summary>
        /// Rotation of the <see cref="Watermelon_Game.Fruits.Fruit"/>
        /// </summary>
        public Quaternion Rotation { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="SavedFruit"/>
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to get the needed values of</param>
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