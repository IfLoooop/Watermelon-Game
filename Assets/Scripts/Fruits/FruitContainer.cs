using UnityEngine;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Singletons;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Holds all fruits that are released from the <see cref="FruitSpawner"/>
    /// </summary>
    internal sealed class FruitContainer : PersistantMonoBehaviour<FruitContainer>
    {
        #region Properties
        /// <summary>
        /// <see cref="UnityEngine.Transform"/> component of <see cref="PersistantMonoBehaviour{T}.Instance"/>
        /// </summary>
        public static Transform Transform => Instance.transform;
        #endregion
    }
}