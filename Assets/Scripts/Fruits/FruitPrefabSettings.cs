using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Contains the references for all prefabs relate4d to fruits
    /// </summary>
    [CreateAssetMenu(menuName = "Scriptable Objects/Fruits", fileName = "Fruits")]
    internal sealed class FruitPrefabSettings : ScriptableObject
    {
        #region Inspector Fields
        [Tooltip("Prefab of the Golden Fruit")]
        [SerializeField] private GameObject goldenFruitPrefab;
        // TODO: Move Fruit Faces to a separate class, when more fruit faces are being implemented
        [Tooltip("Default Fruit Face")]
        [SerializeField] private Sprite faceDefault;
        [Tooltip("Hurt Fruit Face")]
        [SerializeField] private Sprite faceHurt;
        [Tooltip("All spawnable Fruits")]
        [SerializeField] private List<FruitPrefab> fruitPrefabs = new();
        #endregion

        #region Fields
        /// <summary>
        /// Singleton of <see cref="FruitPrefabSettings"/>
        /// </summary>
        private static FruitPrefabSettings instance;
        #endregion
        
        #region Properties
        /// <summary>
        /// <see cref="goldenFruitPrefab"/>
        /// </summary>
        public static GameObject GoldenFruitPrefab => instance.goldenFruitPrefab;
        /// <summary>
        /// <see cref="faceDefault"/>
        /// </summary>
        public static Sprite FaceDefault => instance.faceDefault;
        /// <summary>
        /// <see cref="faceHurt"/>
        /// </summary>
        public static Sprite FaceHurt => instance.faceHurt;
        /// <summary>
        /// <see cref="fruitPrefabs"/>
        /// </summary>
        public static ReadOnlyCollection<FruitPrefab> FruitPrefabs { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes all needed values
        /// </summary>
        public void Init()
        {
            instance = this;
            fruitPrefabs.ForEach(_FruitPrefab => _FruitPrefab.Init());
            FruitPrefabs = this.fruitPrefabs.AsReadOnly();   
        }
        #endregion
    }
}