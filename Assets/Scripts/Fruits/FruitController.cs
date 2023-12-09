using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using OPS.AntiCheat.Field;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Watermelon_Game.Audio;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Networking;
using Watermelon_Game.Singletons;
using Watermelon_Game.Utility;
using Watermelon_Game.Utility.Pools;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Contains logic for <see cref="FruitBehaviour"/>
    /// </summary>
    internal sealed class FruitController : PersistantMonoBehaviour<FruitController>
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to a FruitSettings asset")]
        [SerializeField] private FruitSettings fruitSettings;
        [Tooltip("Reference to a Fruits asset")]
        [SerializeField] private FruitPrefabSettings fruitPrefabSettings;
        [Tooltip("Reference to the NetworkFruitController child")]
        [SerializeField] private NetworkFruitController networkFruitController;
        #endregion
        
        #region Fields
        /// <summary>
        /// Contains all instantiated <see cref="Fruit"/>s in the scene <br/>
        /// <b>Key:</b> Hashcode of the <see cref="Fruit"/> <see cref="GameObject"/> <br/>
        /// <b>Value:</b> The <see cref="FruitBehaviour"/>
        /// </summary>
        [ShowInInspector] private static readonly Dictionary<int, FruitBehaviour> fruits = new(); // TODO: Remove "[ShowInInspector]"
        /// <summary>
        /// Fruits that are currently evolving
        /// </summary>
        private static readonly List<EvolvingFruits> evolvingFruits = new();
        /// <summary>
        /// Contains all spawned <see cref="StoneFruitBehaviour"/>
        /// </summary>
        private static readonly List<StoneFruitBehaviour> stoneFruits = new();
        /// <summary>
        /// Contains every entry of the <see cref="Fruit"/> <see cref="Enum"/> in order
        /// </summary>
        private static readonly Fruit[] enumFruits = Enum.GetValues(typeof(Fruit)).Cast<Fruit>().ToArray();

        /// <summary> // TODO: Temporary
        /// The current <see cref="GameMode"/>
        /// </summary>
        private GameMode currentGameMode;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="fruits"/> <br/>
        /// <i>Only contains the <see cref="FruitBehaviour"/></i>
        /// </summary> // TODO: Maybe change "ReadOnlyCollection" see -> "GameController.ResetGame()"
        public static ReadOnlyCollection<FruitBehaviour> Fruits => new(fruits.Values.ToList());
        /// <summary>
        /// <see cref="stoneFruits"/>
        /// </summary>
        public static List<StoneFruitBehaviour> StoneFruits => stoneFruits;
        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}.Count"/> of <see cref="fruits"/>
        /// </summary>
        public static int FruitCount => fruits.Count;
        #endregion

        #region Events
        /// <summary>
        /// Is called when 2 fruits evolve with each other <br/>
        /// <b>Parameter:</b> The evolved <see cref="Fruit"/>
        /// </summary>
        public static event Action<Fruit> OnEvolve;
        /// <summary>
        /// Is called when a golden fruits collides with another fruit <br/>
        /// <b>Parameter:</b> The <see cref="Fruit"/> the golden fruit collides with
        /// </summary>
        public static event Action<Fruit> OnGoldenFruitCollision;
        #endregion
        
        #region Methods
        protected override void Init()
        {
            base.Init();
            this.fruitPrefabSettings.Init();
            this.fruitSettings.Init();
            this.InitializeSpawnWeights();
        }
        
        /// <summary>
        /// Initializes the <see cref="FruitPrefab.spawnWeight"/>s for every <see cref="FruitPrefab"/> in <see cref="FruitPrefabSettings.FruitPrefabs"/>
        /// </summary>
        private void InitializeSpawnWeights()
        {
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < FruitPrefabSettings.FruitPrefabs.Count; i++)
            {
                FruitPrefabSettings.FruitPrefabs[i].SetSpawnWeight(this.fruitSettings.FruitSpawnWeights[i]);
            }
        }

        private void OnEnable()
        {
            FruitBehaviour.OnFruitRelease += this.AddFruit;
            FruitBehaviour.OnFruitCollision += this.FruitCollision;
            FruitBehaviour.OnUpgradedGoldenFruitCollision += this.UpgradedGoldenFruitCollision;
            FruitBehaviour.OnGoldenFruitCollision += this.GoldenFruitCollision;
            EvolvingFruitTrigger.OnCanEvolve += this.CanEvolve;
            GameController.OnResetGameStarted += this.DisableFruitEvolving;
            GameController.OnResetGameFinished += this.ClearFruits;

            GameController.OnGameModeTransition += (_Mode, _) => this.currentGameMode = _Mode; // TODO: Temporary
        }
        
        private void OnDisable()
        {
            FruitBehaviour.OnFruitRelease -= this.AddFruit;
            FruitBehaviour.OnFruitCollision -= this.FruitCollision;
            FruitBehaviour.OnUpgradedGoldenFruitCollision -= this.UpgradedGoldenFruitCollision;
            FruitBehaviour.OnGoldenFruitCollision -= this.GoldenFruitCollision;
            EvolvingFruitTrigger.OnCanEvolve -= this.CanEvolve;
            GameController.OnResetGameStarted -= this.DisableFruitEvolving;
            GameController.OnResetGameFinished += this.ClearFruits;
        }

        /// <summary>
        /// Enables the spawn weight multiplier based on the given previous fruit
        /// </summary>
        /// <param name="_PreviousFruit">Previous fruit spawn</param>
        public static void SetWeightMultiplier(ProtectedInt32 _PreviousFruit)
        {
            FruitPrefabSettings.FruitPrefabs.ForEach(_Fruit => _Fruit.SetSpawnWeightMultiplier(false));
            
            var _index = FruitPrefabSettings.FruitPrefabs.FindIndex(_Fruit => _Fruit.Fruit == _PreviousFruit);

            if (FruitSettings.LowerIndexWeight && _index - 1 >= 0)
            {
                FruitPrefabSettings.FruitPrefabs[_index - 1].SetSpawnWeightMultiplier(true);
            }
            if (FruitSettings.HigherIndexWeight && _index + 1 <= FruitPrefabSettings.FruitPrefabs.Count - 1)
            {
                FruitPrefabSettings.FruitPrefabs[_index + 1].SetSpawnWeightMultiplier(true);
            }
            if (FruitSettings.SameIndexWeight)
            {
                FruitPrefabSettings.FruitPrefabs[_index].SetSpawnWeightMultiplier(true);
            }
        }

        /// <summary>
        /// Determines what happens when two fruits collide with each other
        /// </summary>
        /// <param name="_Fruit1Hashcode"><see cref="HashCode"/> of the first fruit</param>
        /// <param name="_Fruit2Hashcode"><see cref="HashCode"/> of the second fruit</param>
        private void FruitCollision(int _Fruit1Hashcode, int _Fruit2Hashcode)
        {
            var _fruit1 = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _Fruit1Hashcode).Value;
            var _fruit2 = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _Fruit2Hashcode).Value;

            if (_fruit1 != null && _fruit2 != null)
            {
                if (_fruit1.IsGoldenFruit || _fruit2.IsGoldenFruit)
                {
                    return;
                }
                if (_fruit1.IsEvolving || _fruit2.IsEvolving)
                {
                    return;
                }
                
                if (_fruit1.Fruit == _fruit2.Fruit)
                {
                    evolvingFruits.Add(new EvolvingFruits(_fruit1, _fruit2));
                    
                    _fruit1.MoveTowards(_fruit2);
                    _fruit2.MoveTowards(_fruit1);
                }
            }
        }
        
        /// <summary>
        /// Destroys the fruits with the given <see cref="HashCode"/> <br/>
        /// <i>Uses <see cref="GoldenFruitCollision(int, bool, Vector2)"/></i>
        /// </summary>
        /// <param name="_GoldenFruitHashcode">The <see cref="HashCode"/> of the upgraded golden fruit</param>
        /// <param name="_FruitToDestroyHashCode">The <see cref="HashCode"/> of the other fruit</param>
        private void UpgradedGoldenFruitCollision(int _GoldenFruitHashcode, int _FruitToDestroyHashCode, Vector2 _CollisionPoint)
        {
            var _otherIsNotGoldenFruit = GoldenFruitCollision(_FruitToDestroyHashCode, false, _CollisionPoint);
            if (_otherIsNotGoldenFruit)
            {
                GoldenFruitCollision(_GoldenFruitHashcode, true, _CollisionPoint);
            }
        }

        /// <summary>
        /// Destroys the fruits with the given <see cref="HashCode"/>
        /// </summary>
        /// <param name="_FruitToDestroyHashCode"><see cref="HashCode"/> of the fruit to destroy</param>
        /// <param name="_CollisionPoint">The collision point in world coordinates</param>
        private void GoldenFruitCollision(int _FruitToDestroyHashCode, Vector2 _CollisionPoint)
        {
            this.GoldenFruitCollision(_FruitToDestroyHashCode, false, _CollisionPoint);
        }
        
        /// <summary>
        /// Destroys the fruit with the given <see cref="HashCode"/>
        /// </summary>
        /// <param name="_FruitToDestroyHashCode"><see cref="HashCode"/> of the fruit to destroy</param>
        /// <param name="_ForceDestroy">
        /// Force destroy the given fruit, even if it is a <see cref="FruitBehaviour.IsGoldenFruit"/> <br/>
        /// <i>For upgraded golden fruits (They are destroyed through this method)</i>
        /// </param>
        /// <param name="_CollisionPoint">The collision point in world coordinates</param>
        /// <returns>True if the other fruit was not a golden fruit, otherwise false</returns>
        private bool GoldenFruitCollision(int _FruitToDestroyHashCode, bool _ForceDestroy, Vector2 _CollisionPoint)
        {
            var _otherIsNotGoldenFruit = true;
            var _fruitToDestroy = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _FruitToDestroyHashCode).Value;

            if (_fruitToDestroy != null)
            {
                if (_fruitToDestroy.IsGoldenFruit && !_ForceDestroy)
                {
                    return false;
                }
                
                var _enumFruit = enumFruits.FirstOrDefault(_Fruit => _Fruit == (Fruit)_fruitToDestroy.Fruit.Value);

                if (!_ForceDestroy)
                {
                    OnGoldenFruitCollision?.Invoke(_enumFruit);
                    this.networkFruitController.CmdGoldenFruitCollision(_CollisionPoint);
                }
                
                _fruitToDestroy.DestroyFruit();
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return _otherIsNotGoldenFruit;
        }
        
        /// <summary>
        /// <see cref="EvolvingFruitTrigger.OnCanEvolve"/>
        /// </summary>
        /// <param name="_FruitBehaviourToEvolve">The <see cref="FruitBehaviour"/> to evolve</param>
        /// <param name="_NextFruitPosition">The position, the evolved fruit will spawn at</param>
        /// <param name="_Authority">Indicates if the local client has authority over this fruit</param>
        private void CanEvolve(FruitBehaviour _FruitBehaviourToEvolve, Vector2 _NextFruitPosition, bool _Authority)
        {
            foreach (var _evolvingFruits in evolvingFruits)
            {
                var _fruitBehaviour1 = _evolvingFruits.Fruit1;
                var _fruitBehaviour2 = _evolvingFruits.Fruit2;
                
                var _fruit1 = _fruitBehaviour1 == _FruitBehaviourToEvolve;
                var _fruit2 = _fruitBehaviour2 == _FruitBehaviourToEvolve;
                
                if (_fruit1 || _fruit2)
                {
                    Evolve(_Authority, _NextFruitPosition, _fruitBehaviour1, _fruitBehaviour2);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Evolves the given fruits
        /// </summary>
        /// <param name="_Authority">Indicates if the local client has authority over this fruit</param>
        /// <param name="_NextFruitPosition">Position where to spawn the evolved fruit</param>
        /// <param name="_FruitsToEvolve">The fruit/s to evolve (More than one = evolve with each other)</param>
        /// <returns>The <see cref="Fruit"/> type of the evolved fruit</returns>
        public static void Evolve(bool _Authority, Vector2 _NextFruitPosition, params FruitBehaviour[] _FruitsToEvolve)
        {
            var _enumFruit = enumFruits.FirstOrDefault(_Fruit => _Fruit == (Fruit)_FruitsToEvolve[0].Fruit.Value);
            
            OnEvolve?.Invoke(_enumFruit);

            foreach (var _fruit in _FruitsToEvolve)
            {
                evolvingFruits.RemoveAll(_EvolvingFruits => _EvolvingFruits.Contains(_fruit));
                _fruit.DestroyFruit();
            }
            
            AudioPool.PlayClip(AudioClipName.FruitDestroy, _Authority);
            
            if (_enumFruit != Fruit.Watermelon)
            {
                var _fruit = FruitPrefabSettings.FruitPrefabs[(int)_enumFruit + 1].Fruit;
                Instance.networkFruitController.Evolve(_NextFruitPosition, _fruit);
            }
        }
        
        /// <summary>
        /// Sets <see cref="FruitBehaviour.disableEvolving"/> to true, on all fruits in <see cref="fruits"/>
        /// </summary>
        private void DisableFruitEvolving()
        {
            foreach (var _fruitBehaviour in fruits.Values)
            {
                _fruitBehaviour.DisableEvolving();
            }
        }

        /// <summary>
        /// Clears <see cref="fruits"/> -> <see cref="GameController.OnResetGameFinished"/> <br/>
        /// <i>
        /// Also destroys all remaining child objects (fruits) of <see cref="FruitContainer"/> <br/>
        /// Shouldn't be needed, just a failsafe
        /// </i>
        /// </summary>
        /// <param name="_ResetReason">Not needed here</param>
        private void ClearFruits(ResetReason _ResetReason)
        {
            fruits.Clear();
            stoneFruits.Clear();
            
            var _fruitContainerTransform = FruitContainer.Transform;
            var _childCount = _fruitContainerTransform.childCount;
            
#if UNITY_EDITOR 
            // Can happen in multiplayer, because the "Fruit Container" contains the fruits of all clients
            // TODO: On GameOver, fruits have to be destroyed by the server, not by the individual clients
            // TODO: Remove "this.currentGameMode"
            if (_childCount > 0 && this.currentGameMode == GameMode.SinglePlayer && !GameController.IsApplicationQuitting && !GameController.IsEditorApplicationQuitting) // TODO: No idea why sometimes fruits all left in the container
            {
                Debug.LogError($"{FruitContainer.Transform.name} has still {_childCount} children, destroying now.");
            }
#endif
            // ReSharper disable once InconsistentNaming
            for (var i = _childCount - 1; i >= 0; i--)
            {
                var _fruit = _fruitContainerTransform.GetChild(i);
                Destroy(_fruit.gameObject);
            }
        }
        
#if DEBUG || DEVELOPMENT_BUILD
        /// <summary>
        /// <see cref="AddFruit"/> <br/>
        /// <b>Only for Development!</b>
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to add to <see cref="fruits"/></param>
        public static void AddFruit_DEVELOPMENT(FruitBehaviour _FruitBehaviour)
        {
            Instance.AddFruit(_FruitBehaviour);
        }
#endif
        
        /// <summary>
        /// Adds the given <see cref="FruitBehaviour"/> to <see cref="fruits"/>
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to add to <see cref="fruits"/></param>
        public void AddFruit(FruitBehaviour _FruitBehaviour)
        {
            if (_FruitBehaviour.HasBeenReleased || _FruitBehaviour.HasBeenEvolved)
            {
                var _hashCode = _FruitBehaviour.gameObject.GetHashCode();
                
                fruits.Add(_hashCode, _FruitBehaviour);   
            }
        }

        /// <summary>
        /// Adds teh given <see cref="StoneFruitBehaviour"/> to <see cref="stoneFruits"/>
        /// </summary>
        /// <param name="_StoneFruit">The <see cref="StoneFruitBehaviour"/> to add to <see cref="stoneFruits"/></param>
        public static void AddStoneFruit(StoneFruitBehaviour _StoneFruit)
        {
            stoneFruits.Add(_StoneFruit);
        }
        
        /// <summary>
        /// Returns the <see cref="FruitBehaviour"/> with the given <see cref="HashCode"/> from <see cref="fruits"/>
        /// </summary>
        /// <param name="_Hashcode">The <see cref="HashCode"/> to look for in <see cref="fruits"/></param>
        /// <returns>The <see cref="FruitBehaviour"/> with the given <see cref="HashCode"/> from <see cref="fruits"/></returns>
        [CanBeNull]
        public static FruitBehaviour GetFruit(int _Hashcode)
        {
            var _fruit = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _Hashcode).Value;

            return _fruit != null ? _fruit : null;
        }
        
        /// <summary>
        /// Removes the <see cref="FruitBehaviour"/> with the given <see cref="HashCode"/> from <see cref="fruits"/> <br/>
        /// <b>Should only be used from <see cref="FruitBehaviour"/>.<see cref="FruitBehaviour.DestroyFruit"/></b>
        /// </summary>
        /// <param name="_FruitHashcode">The <see cref="HashCode"/> of the fruit to remove</param>
        public static void RemoveFruit(int _FruitHashcode)
        {
            fruits.Remove(_FruitHashcode);
        }
        #endregion
    }
}