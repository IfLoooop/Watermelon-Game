namespace Watermelon_Game.Fruit
{
    internal sealed class EvolvingFruits
    {
        #region Properties
        public (FruitBehaviour FruitBehaviour, bool HasReachedTarget) Fruit1 { get; private set; }
        public (FruitBehaviour FruitBehaviour, bool HasReachedTarget) Fruit2 { get; private set; }
        #endregion

        #region Constructor
        public EvolvingFruits(FruitBehaviour _FruitBehaviour1, FruitBehaviour _FruitBehaviour2)
        {
            this.Fruit1 = (_FruitBehaviour1, false);
            this.Fruit2 = (_FruitBehaviour2, false);
        }
        #endregion

        #region Methods
        public void Fruit1Finished()
        {
            this.Fruit1 = (this.Fruit1.FruitBehaviour, true);
        }
        
        public void Fruit2Finished()
        {
            this.Fruit2 = (this.Fruit2.FruitBehaviour, true);
        }

        public bool HaveBothFinished()
        {
            return this.Fruit1.HasReachedTarget && this.Fruit2.HasReachedTarget;
        }

        public bool Contains(FruitBehaviour _Fruit1, FruitBehaviour _Fruit2)
        {
            return this.Fruit1.FruitBehaviour == _Fruit1 || 
                   this.Fruit1.FruitBehaviour == _Fruit2 || 
                   this.Fruit2.FruitBehaviour == _Fruit1 || 
                   this.Fruit2.FruitBehaviour == _Fruit2;
        }
        #endregion
    }
}