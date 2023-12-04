using UnityEngine;
using Watermelon_Game.Container;
using Watermelon_Game.Fruits;

namespace Watermelon_Game.Utility
{
    /// <summary>
    /// Caches all needed <see cref="LayerMask"/>
    /// </summary>
    internal static class LayerMaskController
    {
        // TODO: Maybe set all layer for each GameObject that needs one in a script, so the value can't change, when the LayerMasks are reordered in the Editor
        #region Constants
        /// <summary>
        /// Layer name of the <see cref="Container"/>
        /// </summary>
        private const string CONTAINER_LAYER = "Container";
        /// <summary>
        /// Layer name of <see cref="MaxHeight"/>
        /// </summary>
        private const string MAX_HEIGHT = "MaxHeight";
        /// <summary>
        /// Layer name of <see cref="EvolvingFruitTrigger"/> while a fruit is evolving with another fruit
        /// </summary>
        private const string EVOLVING_FRUIT = "EvolvingFruit";
        /// <summary>
        /// Default layer name of <see cref="FruitBehaviour"/>
        /// </summary>
        private const string FRUIT_LAYER = "Fruit";
        /// <summary>
        /// Layer of the <see cref="StoneFruitBehaviour"/>
        /// </summary>
        private const string STONE_FRUIT_LAYER = "StoneFruit";
        #endregion
        
        #region Properties
        /// <summary>
        /// Returns the <see cref="LayerMask"/> of the container and <see cref="FruitBehaviour"/>
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static LayerMask Container_Fruit_Mask { get; } = LayerMask.GetMask(CONTAINER_LAYER, FRUIT_LAYER, STONE_FRUIT_LAYER);
        /// <summary>
        /// Returns the <see cref="LayerMask"/> of the <see cref="FruitBehaviour"/> and <see cref="EvolvingFruits"/>
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static LayerMask Fruit_EvolvingFruit_Mask { get; } = LayerMask.GetMask(FRUIT_LAYER, EVOLVING_FRUIT);
        /// <summary>
        /// Returns the <see cref="LayerMask"/> <see cref="FRUIT_LAYER"/>
        /// </summary>
        public static LayerMask FruitMask { get; } = LayerMask.GetMask(FRUIT_LAYER);
        /// <summary>
        /// Returns the layer <see cref="EVOLVING_FRUIT"/>
        /// </summary>
        public static int EvolvingFruitLayer { get; } = LayerMask.NameToLayer(EVOLVING_FRUIT);
        /// <summary>
        /// Returns the layer <see cref="FRUIT_LAYER"/>
        /// </summary>
        public static int FruitLayer { get; } = LayerMask.NameToLayer(FRUIT_LAYER);
        /// <summary>
        /// Returns the layer <see cref="STONE_FRUIT_LAYER"/>
        /// </summary>
        public static int StoneFruitLayer { get; } = LayerMask.NameToLayer(STONE_FRUIT_LAYER);
        /// <summary>
        /// Returns the layer <see cref="MAX_HEIGHT"/>
        /// </summary>
        public static int MaxHeightLayer { get; } = LayerMask.NameToLayer(MAX_HEIGHT);
        #endregion
    }
}