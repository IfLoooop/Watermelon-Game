using OPS.AntiCheat.Field;
using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Contains settings for fruits
    /// </summary>
    [CreateAssetMenu(menuName = "Scriptable Objects/FruitSettings", fileName = "FruitSettings")]
    internal sealed class FruitSettings : ScriptableObject
    {
        #region Inspector Fields
        [Header("Settings")]
        [Tooltip("Modifier for the spawn weight in FruitData")]
        [SerializeField] private ProtectedInt32 spawnWeightModifier = -25;
        [Tooltip("Enables the spawn weight multiplier for Fruits which index is lower than that of the previous Fruit")]
        [SerializeField] private ProtectedBool lowerIndexWeight = true;
        [Tooltip("Enables the spawn weight multiplier for Fruits which index is higher than that of the previous Fruit")]
        [SerializeField] private ProtectedBool higherIndexWeight = false;
        [Tooltip("Enables the spawn weight multiplier for Fruits which index is the same as that of the previous Fruit")]
        [SerializeField] private ProtectedBool sameIndexWeight = true;
        [Tooltip("Base spawn weight of Cherries")]
        [SerializeField] private ProtectedInt32 cherrySpawnWeight = 50;
        [Tooltip("Base spawn weight of Strawberries")]
        [SerializeField] private ProtectedInt32 strawberrySpawnWeight = 50;
        [Tooltip("Base spawn weight of Lemons")]
        [SerializeField] private ProtectedInt32 lemonSpawnWeight = 50;
        [Tooltip("Base spawn weight of Oranges")]
        [SerializeField] private ProtectedInt32 orangeSpawnWeight = 25;
        [Tooltip("Base spawn weight of Apples")]
        [SerializeField] private ProtectedInt32 appleSpawnWeight = 15;
        [Tooltip("Base spawn weight of Pears")]
        [SerializeField] private ProtectedInt32 pearSpawnWeight = 5;
        [Tooltip("Base spawn weight of DragonFruits")]
        [SerializeField] private ProtectedInt32 dragonfruitSpawnWeight = 0;
        [Tooltip("Base spawn weight of Pineapples")]
        [SerializeField] private ProtectedInt32 pineappleSpawnWeight = 0;
        [Tooltip("Base spawn weight of Honeymelons")]
        [SerializeField] private ProtectedInt32 coconutSpawnWeight = 0;
        [Tooltip("Base spawn weight of Watermelons")]
        [SerializeField] private ProtectedInt32 watermelonSpawnWeight = 0;
        [Tooltip("Chance for a Golden Fruit in %")]
        [SerializeField] private ProtectedFloat goldenFruitChance = 0.01f;
        [Tooltip("How many fruits need to be on the map for a golden fruit spawn to be possible")]
        [SerializeField] private ProtectedUInt32 canSpawnAfter = 10; // TODO: Rename
        [Tooltip("Multiplier for the force that is applied onto a golden fruit when it collides with another fruit")]
        [SerializeField] private ProtectedFloat goldenFruitPushForceMultiplier = 10f;
        [Tooltip("Multiplier for a Fruits mass on first release")]
        [SerializeField] private ProtectedFloat massMultiplier = 2.5f;
        [Tooltip("The mass of a fruit while it's evolving")]
        [SerializeField] private ProtectedFloat evolveMass = 100;
        [Tooltip("Time in seconds between each movement, while a fruit is evolving")]
        [SerializeField] private ProtectedFloat moveTowardsWaitTime = .01f;
        [Tooltip("Multiplier for the max distance a fruit can move, while evolving")]
        [SerializeField] private ProtectedFloat moveTowardsStepMultiplier = .2f;
        [Tooltip("Time in seconds between each size increase/decrease, while evolving/destroying")]
        [SerializeField] private ProtectedFloat scaleWaitTime = .005f;
        [Tooltip("Value must be a multiple of 5, otherwise it will overshoot the targeted scale")]
        [SerializeField] private ProtectedVector3 evolveStep = new(new Vector3(75, 75, 75));
        [Tooltip("Is subtracted from the fruit size during each shrink step, while the fruit is being destroyed")]
        [SerializeField] private ProtectedVector3 shrinkStep = new(new Vector3(25, 25, 25));
        #endregion
        
        #region Properties
        /// <summary>
        /// Singleton of <see cref="FruitSettings"/>
        /// </summary>
        private static FruitSettings instance;
        /// <summary>
        /// <see cref="spawnWeightModifier"/>
        /// </summary>
        public static ProtectedInt32 SpawnWeightModifier => instance.spawnWeightModifier;
        /// <summary>
        /// <see cref="lowerIndexWeight"/>
        /// </summary>
        public static ProtectedBool LowerIndexWeight => instance.lowerIndexWeight;
        /// <summary>
        /// <see cref="HigherIndexWeight"/>
        /// </summary>
        public static ProtectedBool HigherIndexWeight => instance.higherIndexWeight;
        /// <summary>
        /// <see cref="sameIndexWeight"/>
        /// </summary>
        public static ProtectedBool SameIndexWeight => instance.sameIndexWeight;
        /// <summary>
        /// <see cref="goldenFruitChance"/>
        /// </summary>
        public static ProtectedFloat GoldenFruitChance => instance.goldenFruitChance;
        /// <summary>
        /// <see cref="massMultiplier"/>
        /// </summary>
        public static ProtectedFloat MassMultiplier => instance.massMultiplier;
        /// <summary>
        /// <see cref="canSpawnAfter"/>
        /// </summary>
        public static ProtectedUInt32 CanSpawnAfter => instance.canSpawnAfter;
        /// <summary>
        /// <see cref="goldenFruitPushForceMultiplier"/>
        /// </summary>
        public static ProtectedFloat GoldenFruitPushForceMultiplier => instance.goldenFruitPushForceMultiplier;
        /// <summary>
        /// <see cref="FruitSpawnWeights"/>
        /// </summary>
        public ProtectedInt32[] FruitSpawnWeights { get; private set; }
        /// <summary>
        /// <see cref="evolveMass"/>
        /// </summary>
        public static ProtectedFloat EvolveMass => instance.evolveMass;
        /// <summary>
        /// <see cref="moveTowardsStepMultiplier"/>
        /// </summary>
        public static ProtectedFloat MoveTowardsStepMultiplier => instance.moveTowardsStepMultiplier;
        /// <summary>
        /// <see cref="evolveStep"/>
        /// </summary>
        public static ProtectedVector3 EvolveStep => instance.evolveStep;
        /// <summary>
        /// <see cref="shrinkStep"/>
        /// </summary>
        public static ProtectedVector3 ShrinkStep => instance.shrinkStep;
        /// <summary>
        /// <see cref="moveTowardsWaitTime"/>
        /// </summary>
        public static WaitForSeconds MoveTowardsWaitForSeconds { get; private set; }
        /// <summary>
        /// <see cref="scaleWaitTime"/>
        /// </summary>
        public static WaitForSeconds SetScaleWaitForSeconds { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes all needed values
        /// </summary>
        public void Init()
        {
            instance = this;
            MoveTowardsWaitForSeconds = new(moveTowardsWaitTime);
            SetScaleWaitForSeconds = new(scaleWaitTime);
            this.FruitSpawnWeights = new[]
            {
                this.cherrySpawnWeight,
                this.strawberrySpawnWeight,
                this.lemonSpawnWeight,
                this.orangeSpawnWeight,
                this.appleSpawnWeight,
                this.pearSpawnWeight,
                this.dragonfruitSpawnWeight,
                this.pineappleSpawnWeight,
                this.coconutSpawnWeight,
                this.watermelonSpawnWeight
            };
        }
        #endregion
    }
}