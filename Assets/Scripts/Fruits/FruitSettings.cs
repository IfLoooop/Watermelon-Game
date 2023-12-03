using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Contains settings for fruits
    /// </summary>
    [CreateAssetMenu(menuName = "Scriptable Objects/FruitSettings", fileName = "FruitSettings")]
    internal sealed class FruitSettings : ScriptableObject
    {
        #region Websettings
        [Header("WebSettings")]
        // TODO: Maybe use a different spawn weight for each individual fruit
        [Tooltip("Modifier for the spawn weight in FruitData")]
        [ShowInInspector] private static ProtectedInt32 spawnWeightModifier = -25;
        [Tooltip("Enables the spawn weight multiplier for Fruits which index is lower than that of the previous Fruit")]
        [ShowInInspector] private static ProtectedBool lowerIndexWeight = true;
        [Tooltip("Enables the spawn weight multiplier for Fruits which index is higher than that of the previous Fruit")]
        [ShowInInspector] private static ProtectedBool higherIndexWeight = false;
        [Tooltip("Enables the spawn weight multiplier for Fruits which index is the same as that of the previous Fruit")]
        [ShowInInspector] private static ProtectedBool sameIndexWeight = true;
        [Tooltip("Base spawn weight of Grapes")]
        [ShowInInspector] private static ProtectedInt32 grapeSpawnWeight = 50;
        [Tooltip("Base spawn weight of Cherries")]
        [ShowInInspector] private static ProtectedInt32 cherrySpawnWeight = 50;
        [Tooltip("Base spawn weight of Strawberries")]
        [ShowInInspector] private static ProtectedInt32 strawberrySpawnWeight = 50;
        [Tooltip("Base spawn weight of Lemons")]
        [ShowInInspector] private static ProtectedInt32 lemonSpawnWeight = 25;
        [Tooltip("Base spawn weight of Oranges")]
        [ShowInInspector] private static ProtectedInt32 orangeSpawnWeight = 15;
        [Tooltip("Base spawn weight of Apples")]
        [ShowInInspector] private static ProtectedInt32 appleSpawnWeight = 5;
        [Tooltip("Base spawn weight of Pears")]
        [ShowInInspector] private static ProtectedInt32 pearSpawnWeight = 0;
        [Tooltip("Base spawn weight of Pineapples")]
        [ShowInInspector] private static ProtectedInt32 pineappleSpawnWeight = 0;
        [Tooltip("Base spawn weight of Honeymelons")]
        [ShowInInspector] private static ProtectedInt32 honeymelonSpawnWeight = 0;
        [Tooltip("Base spawn weight of Watermelons")]
        [ShowInInspector] private static ProtectedInt32 watermelonSpawnWeight = 0;
        [Tooltip("Chance for a Golden Fruit in %")]
        [ShowInInspector] private static ProtectedFloat goldenFruitChance = 0.01f;
        [Tooltip("Multiplier for a Fruits mass on first release")]
        [ShowInInspector] private static ProtectedFloat massMultiplier = 2.5f;
        #endregion

        #region Inspector Fields
        [Header("Settings")]
        [Tooltip("How many fruits need to be on the map for a golden fruit spawn to be possible")]
        [SerializeField] private ProtectedUInt32 canSpawnAfter = 10; // TODO: Rename
        [Tooltip("The mass of a fruit while it's evolving")]
        [SerializeField] private ProtectedFloat evolveMass = 100;
        [Tooltip("Time in seconds between each movement, while a fruit is evolving")]
        [SerializeField] private ProtectedFloat moveTowardsWaitTime = .01f;
        [Tooltip("Multiplier for the max distance a fruit can move, while evolving")]
        [SerializeField] private ProtectedFloat moveTowardsStepMultiplier = .2f;
        [Tooltip("Time in seconds between each size increase, while evolving")]
        [SerializeField] private ProtectedFloat evolveWaitTime = .005f;
        [Tooltip("Value must be a multiple of 5, otherwise it will overshoot the targeted scale")]
        [SerializeField] private ProtectedVector3 evolveStep = new(new Vector3(75, 75, 75));
        #endregion
        
        #region Properties
        /// <summary>
        /// Singleton of <see cref="FruitSettings"/>
        /// </summary>
        private static FruitSettings instance;
        /// <summary>
        /// <see cref="spawnWeightModifier"/>
        /// </summary>
        public static ProtectedInt32 SpawnWeightModifier => spawnWeightModifier;
        /// <summary>
        /// <see cref="lowerIndexWeight"/>
        /// </summary>
        public static ProtectedBool LowerIndexWeight => lowerIndexWeight;
        /// <summary>
        /// <see cref="HigherIndexWeight"/>
        /// </summary>
        public static ProtectedBool HigherIndexWeight => higherIndexWeight;
        /// <summary>
        /// <see cref="sameIndexWeight"/>
        /// </summary>
        public static ProtectedBool SameIndexWeight => sameIndexWeight;
        /// <summary>
        /// <see cref="grapeSpawnWeight"/>
        /// </summary>
        public static ProtectedInt32 GrapeSpawnWeight => grapeSpawnWeight;
        /// <summary>
        /// <see cref="cherrySpawnWeight"/>
        /// </summary>
        public static ProtectedInt32 CherrySpawnWeight => cherrySpawnWeight;
        /// <summary>
        /// <see cref="strawberrySpawnWeight"/>
        /// </summary>
        public static ProtectedInt32 StrawberrySpawnWeight => strawberrySpawnWeight;
        /// <summary>
        /// <see cref="lemonSpawnWeight"/>
        /// </summary>
        public static ProtectedInt32 LemonSpawnWeight => lemonSpawnWeight;
        /// <summary>
        /// <see cref="orangeSpawnWeight"/>
        /// </summary>
        public static ProtectedInt32 OrangeSpawnWeight => orangeSpawnWeight;
        /// <summary>
        /// <see cref="appleSpawnWeight"/>
        /// </summary>
        public static ProtectedInt32 AppleSpawnWeight => appleSpawnWeight;
        /// <summary>
        /// <see cref="pearSpawnWeight"/>
        /// </summary>
        public static ProtectedInt32 PearSpawnWeight => pearSpawnWeight;
        /// <summary>
        /// <see cref="pineappleSpawnWeight"/>
        /// </summary>
        public static ProtectedInt32 PineappleSpawnWeight => pineappleSpawnWeight;
        /// <summary>
        /// <see cref="honeymelonSpawnWeight"/>
        /// </summary>
        public static ProtectedInt32 HoneymelonSpawnWeight => honeymelonSpawnWeight;
        /// <summary>
        /// <see cref="watermelonSpawnWeight"/>
        /// </summary>
        public static ProtectedInt32 WatermelonSpawnWeight => watermelonSpawnWeight;
        /// <summary>
        /// <see cref="goldenFruitChance"/>
        /// </summary>
        public static ProtectedFloat GoldenFruitChance => goldenFruitChance;
        /// <summary>
        /// <see cref="massMultiplier"/>
        /// </summary>
        public static ProtectedFloat MassMultiplier => massMultiplier;
        /// <summary>
        /// <see cref="canSpawnAfter"/>
        /// </summary>
        public static ProtectedUInt32 CanSpawnAfter => instance.canSpawnAfter;
        /// <summary>
        /// <see cref="FruitSpawnWeights"/>
        /// </summary>
        public ProtectedInt32[] FruitSpawnWeights { get; } =
        {
            grapeSpawnWeight,
            cherrySpawnWeight,
            strawberrySpawnWeight,
            lemonSpawnWeight,
            orangeSpawnWeight,
            appleSpawnWeight,
            pearSpawnWeight,
            pineappleSpawnWeight,
            honeymelonSpawnWeight,
            watermelonSpawnWeight
        };
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
        /// <see cref="moveTowardsWaitTime"/>
        /// </summary>
        public static WaitForSeconds MoveTowardsWaitForSeconds { get; private set; }
        /// <summary>
        /// <see cref="evolveWaitTime"/>
        /// </summary>
        public static WaitForSeconds GrowFruitWaitForSeconds { get; private set; }
        #endregion

        #region Methods
        // /// <summary>
        // /// Tries to set the values from the web settings
        // /// </summary>
        // public static void ApplyWebSettings()
        // {
        //     var _callerType = typeof(FruitController);
        //     WebSettings.TrySetValue(nameof(SpawnWeightModifier), ref spawnWeightModifier, _callerType);
        //     WebSettings.TrySetValue(nameof(LowerIndexWeight), ref lowerIndexWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(HigherIndexWeight), ref higherIndexWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(SameIndexWeight), ref sameIndexWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(GoldenFruitChance), ref goldenFruitChance, _callerType);
        //     WebSettings.TrySetValue(nameof(MassMultiplier), ref massMultiplier, _callerType);
        //     WebSettings.TrySetValue(nameof(GrapeSpawnWeight), ref grapeSpawnWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(CherrySpawnWeight), ref cherrySpawnWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(StrawberrySpawnWeight), ref strawberrySpawnWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(LemonSpawnWeight), ref lemonSpawnWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(OrangeSpawnWeight), ref orangeSpawnWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(AppleSpawnWeight), ref appleSpawnWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(PearSpawnWeight), ref pearSpawnWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(PineappleSpawnWeight), ref pineappleSpawnWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(HoneymelonSpawnWeight), ref honeymelonSpawnWeight, _callerType);
        //     WebSettings.TrySetValue(nameof(WatermelonSpawnWeight), ref watermelonSpawnWeight, _callerType);
        // }

        /// <summary>
        /// Initializes all needed values
        /// </summary>
        public void Init()
        {
            instance = this;
            MoveTowardsWaitForSeconds = new(moveTowardsWaitTime);
            GrowFruitWaitForSeconds = new(evolveWaitTime);
        }
        #endregion
    }
}