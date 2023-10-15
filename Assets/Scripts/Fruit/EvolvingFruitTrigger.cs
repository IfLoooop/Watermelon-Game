using UnityEngine;

namespace Watermelon_Game.Fruit
{
    internal sealed class EvolvingFruitTrigger : MonoBehaviour
    {
        #region Fields
        private FruitBehaviour fruitToEvolveWith;
        #endregion
        
        #region Methods
        private void OnTriggerEnter2D(Collider2D _Other)
        {
            var _otherHashcode = _Other.gameObject.GetHashCode();
            var _fruitToEvolveWithHashcode = this.fruitToEvolveWith.gameObject.GetHashCode();

            if (_otherHashcode == _fruitToEvolveWithHashcode)
            {
                GameController.Evolve(this.fruitToEvolveWith, base.transform.position);
            }
        }

        public void SetFruitToEvolveWith(FruitBehaviour _FruitBehaviour)
        {
            this.fruitToEvolveWith = _FruitBehaviour;
            base.gameObject.layer = LayerMask.NameToLayer("Fruit");
        }
        #endregion
    }
}