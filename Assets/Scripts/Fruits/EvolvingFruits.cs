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
        /// <b><see cref="FruitBehaviour"/>:</b> The <see cref="Fruit"/> <br/>
        /// <b>HasReachedTarget:</b> Indicates whether the <see cref="Fruit"/> has reached the position to evolve
        /// </summary>
        public (FruitBehaviour FruitBehaviour, bool HasReachedTarget) Fruit1 { get; private set; }
        /// <summary>
        /// <b><see cref="FruitBehaviour"/>:</b> The <see cref="Fruit"/> <br/>
        /// <b>HasReachedTarget:</b> Indicates whether the <see cref="Fruit"/> has reached the position to evolve
        /// </summary>
        public (FruitBehaviour FruitBehaviour, bool HasReachedTarget) Fruit2 { get; private set; }
        #endregion

        #region Constructor
        /// <param name="_FruitBehaviour1">Evolving <see cref="Fruit"/> 1</param>
        /// <param name="_FruitBehaviour2">Evolving <see cref="Fruit"/> 2</param>
        public EvolvingFruits(FruitBehaviour _FruitBehaviour1, FruitBehaviour _FruitBehaviour2)
        {
            this.Fruit1 = (_FruitBehaviour1, false);
            this.Fruit2 = (_FruitBehaviour2, false);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets <see cref="Fruit1"/>.HasReachedTarget to true
        /// </summary>
        public void Fruit1HasReachedTarget()
        {
            this.Fruit1 = (this.Fruit1.FruitBehaviour, true);
        }
        
        /// <summary>
        /// Sets <see cref="Fruit2"/>.HasReachedTarget to true
        /// </summary>
        public void Fruit2HasReachedTarget()
        {
            this.Fruit2 = (this.Fruit2.FruitBehaviour, true);
        }

        /// <summary>
        /// Returns true if HasReachedTarget of <see cref="Fruit1"/> and <see cref="Fruit2"/> are true
        /// </summary>
        /// <returns>True if HasReachedTarget of <see cref="Fruit1"/> and <see cref="Fruit2"/> are true</returns>
        public bool HaveBothFinished()
        {
            return this.Fruit1.HasReachedTarget && this.Fruit2.HasReachedTarget;
        }

        /// <summary>
        /// Returns true if any of the given <see cref="FruitBehaviour"/> matched with <see cref="Fruit1"/> or <see cref="Fruit2"/>
        /// </summary>
        /// <param name="_Fruits">The <see cref="FruitBehaviour"/> to match</param>
        /// <returns>True if any of the given <see cref="FruitBehaviour"/> matched with <see cref="Fruit1"/> or <see cref="Fruit2"/></returns>
        public bool Contains(params FruitBehaviour[] _Fruits)
        {
            return _Fruits.Any(_Fruit => this.Fruit1.FruitBehaviour == _Fruit || this.Fruit2.FruitBehaviour == _Fruit);
        }
        #endregion
    }
}