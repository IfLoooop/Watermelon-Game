using UnityEngine;
using Watermelon_Game.Fruit;

namespace Watermelon_Game.Development
{
    public class DeveloperTools : MonoBehaviour
    {
        #region Methods
        private void Awake()
        {
            if (!Application.isEditor && !Debug.isDebugBuild)
            {
                Destroy(this.gameObject);
            }
        }

#if DEBUG
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                var _fruitBehaviour = FruitBehaviour.SpawnFruit(base.transform.position, Fruit.Fruit.Grape, false);
                _fruitBehaviour.gameObject.SetActive(true);
                _fruitBehaviour.GoldenFruit_Debug();
                _fruitBehaviour.Release(null, Vector2.down);
            }
        }
#endif
        #endregion  
    }
}
