using System.Linq;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Contains data of 2 <see cref="Fruit"/>s that are evolving with each other
    /// </summary>
    internal sealed class EvolvingFruits
    {
        #region Properties
        /// <summary>
        /// <b><see cref="FruitBehaviour"/>:</b> The <see cref="Fruit"/>
        /// </summary>
        public FruitBehaviour Fruit1 { get; }
        /// <summary>
        /// <b><see cref="FruitBehaviour"/>:</b> The <see cref="Fruit"/
        /// </summary>
        public FruitBehaviour Fruit2 { get; }
        #endregion

        #region Constructor
        /// <param name="_FruitBehaviour1">Evolving <see cref="Fruit"/> 1</param>
        /// <param name="_FruitBehaviour2">Evolving <see cref="Fruit"/> 2</param>
        public EvolvingFruits(FruitBehaviour _FruitBehaviour1, FruitBehaviour _FruitBehaviour2)
        {
            this.Fruit1 = _FruitBehaviour1;
            this.Fruit2 = _FruitBehaviour2;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns true if any of the given <see cref="FruitBehaviour"/> matched with <see cref="Fruit1"/> or <see cref="Fruit2"/>
        /// </summary>
        /// <param name="_Fruits">The <see cref="FruitBehaviour"/> to match</param>
        /// <returns>True if any of the given <see cref="FruitBehaviour"/> matched with <see cref="Fruit1"/> or <see cref="Fruit2"/></returns>
        public bool Contains(params FruitBehaviour[] _Fruits)
        {
            return _Fruits.Any(_Fruit => this.Fruit1 == _Fruit || this.Fruit2 == _Fruit);
        }
        #endregion
    }
}