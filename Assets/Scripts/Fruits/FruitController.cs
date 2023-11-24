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
using Watermelon_Game.Menus;
using Watermelon_Game.Menus.MainMenus;
using Watermelon_Game.Networking;
using Watermelon_Game.Utility;
using Watermelon_Game.Web;

namespace Watermelon_Game.Fruits
{
    /// <summary>
    /// Contains logic for <see cref="FruitBehaviour"/>
    /// </summary>
    internal sealed class FruitController : MonoBehaviour
    {
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to a FruitSettings asset")]
        [SerializeField] private FruitSettings fruitSettings;
        [Tooltip("Reference to a Fruits asset")]
        [SerializeField] private FruitPrefabSettings fruitPrefabSettings;
        [Tooltip("Reference to the NetworkFruitController child")]
        [SerializeField] private NetworkFruitController networkFruitController;
        [Tooltip("Contains all released fruits in the scene")]
        [SerializeField] private GameObject fruitContainer;
        #endregion
        
        #region Fields
        /// <summary>
        /// Singleton of <see cref="FruitController"/>
        /// </summary>
        private static FruitController instance;
        
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
        /// <see cref="Transform"/> component of the <see cref="fruitContainer"/>
        /// </summary>
        public static Transform FruitContainerTransform => instance.fruitContainer.transform;
        /// <summary>
        /// <see cref="fruits"/> <br/>
        /// <i>Only contains the <see cref="FruitBehaviour"/></i>
        /// </summary> // TODO: Maybe change "ReadOnlyCollection" see -> "GameController.ResetGame()"
        public static ReadOnlyCollection<FruitBehaviour> Fruits => new(fruits.Values.ToList());
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
        /// <summary>
        /// Needs to be called with <see cref="RuntimeInitializeLoadType.BeforeSplashScreen"/>
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void SubscribeToWebSettings()
        {
            WebSettings.OnApplyWebSettings += FruitSettings.ApplyWebSettings;
        }

        private void OnDestroy()
        {
            WebSettings.OnApplyWebSettings -= FruitSettings.ApplyWebSettings;
        }
        
        private void Awake()
        {
            instance = this;
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

            MainMenuBase.OnGameModeTransition += _Mode => this.currentGameMode = _Mode; // TODO: Temporary
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
        /// <i>Uses <see cref="GoldenFruitCollision"/></i>
        /// </summary>
        /// <param name="_GoldenFruitHashcode">The <see cref="HashCode"/> of the upgraded golden fruit</param>
        /// <param name="_FruitToDestroyHashCode">The <see cref="HashCode"/> of the other fruit</param>
        private void UpgradedGoldenFruitCollision(int _GoldenFruitHashcode, int _FruitToDestroyHashCode)
        {
            var _otherIsNotGoldenFruit = GoldenFruitCollision(_FruitToDestroyHashCode, false);
            if (_otherIsNotGoldenFruit)
            {
                GoldenFruitCollision(_GoldenFruitHashcode, true);
            }
        }
        
        /// <summary>
        /// Destroys the fruit with the given <see cref="HashCode"/>
        /// </summary>
        /// <param name="_FruitToDestroyHashCode"><see cref="HashCode"/> of the fruit to destroy</param>
        /// <param name="_ForceDestroy">
        /// Force destroy the given fruit, even if it is a <see cref="FruitBehaviour.IsGoldenFruit"/> <br/>
        /// <i>For upgraded golden fruits (They are destroyed through this method)</i>
        /// </param>
        /// <returns>True if the other fruit was not a golden fruit, otherwise false</returns>
        private bool GoldenFruitCollision(int _FruitToDestroyHashCode, bool _ForceDestroy)
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
                var _fruitBehaviour1 = _evolvingFruits.Fruit1.FruitBehaviour;
                var _fruitBehaviour2 = _evolvingFruits.Fruit2.FruitBehaviour;
                
                var _fruit1 = _fruitBehaviour1 == _FruitBehaviourToEvolve;
                var _fruit2 = _fruitBehaviour2 == _FruitBehaviourToEvolve;
                
                // TODO: Maybe combine into one method
                if (_fruit1)
                {
                    _evolvingFruits.Fruit1HasReachedTarget();
                    if (_evolvingFruits.HaveBothFinished())
                    {
                        Evolve(_Authority, _NextFruitPosition, _fruitBehaviour1, _fruitBehaviour2);
                        break;
                    }
                }
                else if (_fruit2)
                {
                    _evolvingFruits.Fruit2HasReachedTarget();
                    if (_evolvingFruits.HaveBothFinished())
                    {
                        Evolve(_Authority, _NextFruitPosition, _fruitBehaviour1, _fruitBehaviour2);
                        break;
                    }
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
                instance.networkFruitController.Evolve(_NextFruitPosition, _fruit);
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
        /// Also destroys all remaining child objects (fruits) of <see cref="fruitContainer"/> <br/>
        /// Shouldn't be needed, just a failsafe
        /// </i>
        /// </summary>
        /// <param name="_ResetReason">Not needed here</param>
        private void ClearFruits(ResetReason _ResetReason)
        {
            fruits.Clear();

            var _fruitContainerTransform = this.fruitContainer.transform;
            var _childCount = _fruitContainerTransform.childCount;

#if UNITY_EDITOR 
            // Can happen in multiplayer, because the "Fruit Container" contains the fruits of all clients
            // TODO: On GameOver, fruits have to be destroyed by the server, not by the individual clients
            // TODO: Remove "this.currentGameMode"
            if (_childCount > 0 && this.currentGameMode == GameMode.SinglePlayer) // TODO: No idea why sometimes fruits all left in the container
            {
                Debug.LogError($"{fruitContainer.name} has still {_childCount} children, destroying now.");
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
            instance.AddFruit(_FruitBehaviour);
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