using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon_Game.Web;

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
        [ShowInInspector] private static int spawnWeightModifier = -25;
        [Tooltip("Enables the spawn weight multiplier for Fruits which index is lower than that of the previous Fruit")]
        [ShowInInspector] private static bool lowerIndexWeight = true;
        [Tooltip("Enables the spawn weight multiplier for Fruits which index is higher than that of the previous Fruit")]
        [ShowInInspector] private static bool higherIndexWeight = false;
        [Tooltip("Enables the spawn weight multiplier for Fruits which index is the same as that of the previous Fruit")]
        [ShowInInspector] private static bool sameIndexWeight = true;
        [Tooltip("Base spawn weight of Grapes")]
        [ShowInInspector] private static int grapeSpawnWeight = 50;
        [Tooltip("Base spawn weight of Cherries")]
        [ShowInInspector] private static int cherrySpawnWeight = 50;
        [Tooltip("Base spawn weight of Strawberries")]
        [ShowInInspector] private static int strawberrySpawnWeight = 50;
        [Tooltip("Base spawn weight of Lemons")]
        [ShowInInspector] private static int lemonSpawnWeight = 25;
        [Tooltip("Base spawn weight of Oranges")]
        [ShowInInspector] private static int orangeSpawnWeight = 15;
        [Tooltip("Base spawn weight of Apples")]
        [ShowInInspector] private static int appleSpawnWeight = 5;
        [Tooltip("Base spawn weight of Pears")]
        [ShowInInspector] private static int pearSpawnWeight = 0;
        [Tooltip("Base spawn weight of Pineapples")]
        [ShowInInspector] private static int pineappleSpawnWeight = 0;
        [Tooltip("Base spawn weight of Honeymelons")]
        [ShowInInspector] private static int honeymelonSpawnWeight = 0;
        [Tooltip("Base spawn weight of Watermelons")]
        [ShowInInspector] private static int watermelonSpawnWeight = 0;
        [Tooltip("Chance for a Golden Fruit in %")]
        [ShowInInspector] private static float goldenFruitChance = 0.01f;
        [Tooltip("Multiplier for a Fruits mass on first release")]
        [ShowInInspector] private static float massMultiplier = 2.5f;
        #endregion

        #region Inspector Fields
        [Header("Settings")]
        [Tooltip("How many fruits need to be on the map for a golden fruit spawn to be possible")]
        [SerializeField] private uint canSpawnAfter = 10; // TODO: Rename
        [Tooltip("The mass of a fruit while it's evolving")]
        [SerializeField] private float evolveMass = 100;
        [Tooltip("Time in seconds between each movement, while a fruit is evolving")]
        [SerializeField] private float moveTowardsWaitTime = .01f;
        [Tooltip("Multiplier for the max distance a fruit can move, while evolving")]
        [SerializeField] private float moveTowardsStepMultiplier = .2f;
        [Tooltip("Time in seconds between each size increase, while evolving")]
        [SerializeField] private float evolveWaitTime = .005f;
        [Tooltip("Value must be a multiple of 5, otherwise it will overshoot the targeted scale")]
        [SerializeField] private Vector3 evolveStep = new(.5f, .5f, .5f);
        #endregion
        
        
        #region Properties
        /// <summary>
        /// Singleton of <see cref="FruitSettings"/>
        /// </summary>
        private static FruitSettings instance;
        /// <summary>
        /// <see cref="spawnWeightModifier"/>
        /// </summary>
        public static int SpawnWeightModifier => spawnWeightModifier;
        /// <summary>
        /// <see cref="lowerIndexWeight"/>
        /// </summary>
        public static bool LowerIndexWeight => lowerIndexWeight;
        /// <summary>
        /// <see cref="HigherIndexWeight"/>
        /// </summary>
        public static bool HigherIndexWeight => higherIndexWeight;
        /// <summary>
        /// <see cref="sameIndexWeight"/>
        /// </summary>
        public static bool SameIndexWeight => sameIndexWeight;
        /// <summary>
        /// <see cref="grapeSpawnWeight"/>
        /// </summary>
        public static int GrapeSpawnWeight => grapeSpawnWeight;
        /// <summary>
        /// <see cref="cherrySpawnWeight"/>
        /// </summary>
        public static int CherrySpawnWeight => cherrySpawnWeight;
        /// <summary>
        /// <see cref="strawberrySpawnWeight"/>
        /// </summary>
        public static int StrawberrySpawnWeight => strawberrySpawnWeight;
        /// <summary>
        /// <see cref="lemonSpawnWeight"/>
        /// </summary>
        public static int LemonSpawnWeight => lemonSpawnWeight;
        /// <summary>
        /// <see cref="orangeSpawnWeight"/>
        /// </summary>
        public static int OrangeSpawnWeight => orangeSpawnWeight;
        /// <summary>
        /// <see cref="appleSpawnWeight"/>
        /// </summary>
        public static int AppleSpawnWeight => appleSpawnWeight;
        /// <summary>
        /// <see cref="pearSpawnWeight"/>
        /// </summary>
        public static int PearSpawnWeight => pearSpawnWeight;
        /// <summary>
        /// <see cref="pineappleSpawnWeight"/>
        /// </summary>
        public static int PineappleSpawnWeight => pineappleSpawnWeight;
        /// <summary>
        /// <see cref="honeymelonSpawnWeight"/>
        /// </summary>
        public static int HoneymelonSpawnWeight => honeymelonSpawnWeight;
        /// <summary>
        /// <see cref="watermelonSpawnWeight"/>
        /// </summary>
        public static int WatermelonSpawnWeight => watermelonSpawnWeight;
        /// <summary>
        /// <see cref="goldenFruitChance"/>
        /// </summary>
        public static float GoldenFruitChance => goldenFruitChance;
        /// <summary>
        /// <see cref="massMultiplier"/>
        /// </summary>
        public static float MassMultiplier => massMultiplier;
        /// <summary>
        /// <see cref="canSpawnAfter"/>
        /// </summary>
        public static uint CanSpawnAfter => instance.canSpawnAfter;
        /// <summary>
        /// <see cref="FruitSpawnWeights"/>
        /// </summary>
        public int[] FruitSpawnWeights { get; } =
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
        public static float EvolveMass => instance.evolveMass;
        /// <summary>
        /// <see cref="moveTowardsStepMultiplier"/>
        /// </summary>
        public static float MoveTowardsStepMultiplier => instance.moveTowardsStepMultiplier;
        /// <summary>
        /// <see cref="evolveStep"/>
        /// </summary>
        public static Vector3 EvolveStep => instance.evolveStep;
        /// <summary>
        /// <see cref="moveTowardsWaitTime"/>
        /// </summary>
        public static WaitForSeconds MoveTowardsWaitForSeconds { get; private set; }
        /// <summary>
        /// <see cref="evolveWaitTime"/>
        /// </summary>
        public static WaitForSeconds EvolveWaitForSeconds { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Tries to set the values from the web settings
        /// </summary>
        public static void ApplyWebSettings()
        {
            var _callerType = typeof(FruitController);
            WebSettings.TrySetValue(nameof(SpawnWeightModifier), ref spawnWeightModifier, _callerType);
            WebSettings.TrySetValue(nameof(LowerIndexWeight), ref lowerIndexWeight, _callerType);
            WebSettings.TrySetValue(nameof(HigherIndexWeight), ref higherIndexWeight, _callerType);
            WebSettings.TrySetValue(nameof(SameIndexWeight), ref sameIndexWeight, _callerType);
            WebSettings.TrySetValue(nameof(GoldenFruitChance), ref goldenFruitChance, _callerType);
            WebSettings.TrySetValue(nameof(MassMultiplier), ref massMultiplier, _callerType);
            WebSettings.TrySetValue(nameof(GrapeSpawnWeight), ref grapeSpawnWeight, _callerType);
            WebSettings.TrySetValue(nameof(CherrySpawnWeight), ref cherrySpawnWeight, _callerType);
            WebSettings.TrySetValue(nameof(StrawberrySpawnWeight), ref strawberrySpawnWeight, _callerType);
            WebSettings.TrySetValue(nameof(LemonSpawnWeight), ref lemonSpawnWeight, _callerType);
            WebSettings.TrySetValue(nameof(OrangeSpawnWeight), ref orangeSpawnWeight, _callerType);
            WebSettings.TrySetValue(nameof(AppleSpawnWeight), ref appleSpawnWeight, _callerType);
            WebSettings.TrySetValue(nameof(PearSpawnWeight), ref pearSpawnWeight, _callerType);
            WebSettings.TrySetValue(nameof(PineappleSpawnWeight), ref pineappleSpawnWeight, _callerType);
            WebSettings.TrySetValue(nameof(HoneymelonSpawnWeight), ref honeymelonSpawnWeight, _callerType);
            WebSettings.TrySetValue(nameof(WatermelonSpawnWeight), ref watermelonSpawnWeight, _callerType);
        }

        /// <summary>
        /// Initializes all needed values
        /// </summary>
        public void Init()
        {
            instance = this;
            MoveTowardsWaitForSeconds = new(moveTowardsWaitTime);
            EvolveWaitForSeconds = new(evolveWaitTime);
        }
        #endregion
    }
}