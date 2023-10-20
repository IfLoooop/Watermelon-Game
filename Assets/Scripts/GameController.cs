using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon_Game.ExtensionMethods;
using Watermelon_Game.Fruit;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Menu;
using Watermelon_Game.Points;
using Watermelon_Game.Skills;
using Watermelon_Game.Web;

namespace Watermelon_Game
{
    internal sealed class GameController : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private FruitCollection fruitCollection;
        #endregion
        
        #region Fields
        /// <summary>
        /// Contains all instantiated fruits in the scene <br/>
        /// <b>Key:</b> Hashcode of the fruit <see cref="GameObject"/> <br/>
        /// <b>Value:</b> The <see cref="FruitBehaviour"/>
        /// </summary>
        private static readonly Dictionary<int, FruitBehaviour> fruits = new(); // TODO: Change to "List<FruitBehaviour>"
        /// <summary>
        /// Fruits that are currently evolving <br/>
        /// <b>Key:</b> Hashcode of the fruit <br/>
        /// <b>Value:</b> Whether the fruit has reached the target position 
        /// </summary>
        private static readonly List<EvolvingFruits> evolvingFruits = new();
        #endregion

        #region Properties
        public static GameController Instance { get; private set; }
        
        public FruitCollection FruitCollection => this.fruitCollection;
        public float CurrentGameTimeStamp { get; private set; }
        #endregion
        
        #region Methods
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SetScreen()
        {
            var _workArea = Screen.mainWindowDisplayInfo.workArea;
            Screen.SetResolution(_workArea.width, _workArea.height, FullScreenMode.MaximizedWindow);
        }
        
        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            StartGame();
        }
        
        public static void StartGame()
        {
            VersionControl.Instance.CheckLatestVersion();
            FruitSpawner.Instance.ResetFruitSpawner(true);
            FruitSpawnerAim.Enable(true);
            //PointsController.Instance.ResetPoints();
            StatsMenu.Instance.GamesPlayed++;
            GameOverMenu.Instance.Reset();
            Instance.CurrentGameTimeStamp = Time.time;
        }
        
        public static void GameOver()
        {
            Instance.StartCoroutine(ResetGame(MenuController.Instance.GameOver));
        }

        public static void Restart()
        {
            Instance.StartCoroutine(ResetGame(StartGame));
        }
        
        private static IEnumerator ResetGame(Action _Action)
        {
            var _waitTime = new WaitForSeconds(.1f);

            MenuController.Instance.CloseCurrentlyActiveMenu();
            MenuController.Instance.BlockInput = true;
            FruitSpawner.Instance.BlockInput = true;
            FruitSpawnerAim.Enable(false);
            
            fruits.Values.ForEach<FruitBehaviour>(_FruitBehaviour => _FruitBehaviour.DisableEvolving());
            
            // ReSharper disable once InconsistentNaming
            for (var i = fruits.Values.Count - 1; i >= 0; i--)
            {
                Instance.FruitCollection.PlayEvolveSound();
                fruits.Values.ElementAt(i).Destroy();
                
                yield return _waitTime;
            }
            
            fruits.Clear();
            
            PointsController.Instance.SavePoints();
            PointsController.Instance.ResetPoints();
            // Needed for the SkillController.PointsChanged() method to be called
            PointsController.Instance.SubtractPoints(0);
            FruitSpawner.GameOver();
            NextFruit.Instance.GameOVer();

            _Action();
            MenuController.Instance.BlockInput = false;
        }
        
        /// <summary>
        /// Determines what happens when two fruits collide with each other
        /// </summary>
        /// <param name="_Fruit1">HashCode of the first fruit</param>
        /// <param name="_Fruit2">HashCode of the second fruit</param>
        public static void FruitCollision(int _Fruit1, int _Fruit2)
        {
            // TODO: Combine this method with the "Skill_Evolve()"-method in "SkillController.cs"
            
            var _fruit1 = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _Fruit1).Value;
            var _fruit2 = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _Fruit2).Value;

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

        public static void Evolve(FruitBehaviour _FruitBehaviour, Vector3 _Position)
        {
            foreach (var _evolvingFruits in evolvingFruits)
            {
                var _fruitBehaviour1 = _evolvingFruits.Fruit1.FruitBehaviour;
                var _fruitBehaviour2 = _evolvingFruits.Fruit2.FruitBehaviour;
                
                var _fruit1 = _fruitBehaviour1 == _FruitBehaviour;
                var _fruit2 = _fruitBehaviour2 == _FruitBehaviour;
                
                if (_fruit1)
                {
                    _evolvingFruits.Fruit1Finished();
                    if (_evolvingFruits.HaveBothFinished())
                    {
                        Evolve(_fruitBehaviour1, _fruitBehaviour2, _Position);
                        break;
                    }
                }
                else if (_fruit2)
                {
                    _evolvingFruits.Fruit2Finished();
                    if (_evolvingFruits.HaveBothFinished())
                    {
                        Evolve(_fruitBehaviour1, _fruitBehaviour2, _Position);
                        break;
                    }
                }
            }
        }
        
        private static void Evolve(FruitBehaviour _Fruit1, FruitBehaviour _Fruit2, Vector3 _Position)
        {
            var _fruitIndex = (int)Enum.GetValues(typeof(Fruit.Fruit)).Cast<Fruit.Fruit>().FirstOrDefault(_Fruit => _Fruit == _Fruit1.Fruit);
                    
            PointsController.Instance.AddPoints((Fruit.Fruit)_fruitIndex);
            GameOverMenu.Instance.AddFruitCount(_Fruit1.Fruit);
            StatsMenu.Instance.AddFruitCount(_Fruit1.Fruit);
            Instance.FruitCollection.PlayEvolveSound();

            evolvingFruits.RemoveAll(_EvolvingFruits => _EvolvingFruits.Contains(_Fruit1, _Fruit2));
            fruits.Remove(_Fruit1.GetHashCode());
            fruits.Remove(_Fruit2.GetHashCode());
            
            _Fruit1.Destroy();
            _Fruit2.Destroy();
                    
            if (_fruitIndex != (int)Fruit.Fruit.Melon)
            {
                var _fruit = Instance.FruitCollection.Fruits[_fruitIndex + 1].Fruit;
                var _fruitBehaviour = FruitBehaviour.SpawnFruit(_Position, _fruit, true);
                _fruitBehaviour.Evolve();
            }
            // When two Watermelons are evolved
            else
            {
                NextFruit.Instance.ShowNextNextFruit();
            }
        }
        
        public static void GoldenFruitCollision(GameObject _FruitToDestroy)
        {
            var _fruitToDestroyHashCode = _FruitToDestroy.GetHashCode();
            var _fruitToDestroy = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _fruitToDestroyHashCode).Value;

            if (_fruitToDestroy != null)
            {
                fruits.Remove(_fruitToDestroyHashCode);
                
                var _fruitIndex = (int)Enum.GetValues(typeof(Fruit.Fruit)).Cast<Fruit.Fruit>().FirstOrDefault(_Fruit => _Fruit == _fruitToDestroy.Fruit);
                    
                PointsController.Instance.AddPoints((Fruit.Fruit)_fruitIndex);
                Instance.FruitCollection.PlayEvolveSound();
                
                _fruitToDestroy.Destroy();
            }
        }

        public static void UpgradedGoldenFruitCollision(GameObject _UpgradedGoldenFruit, GameObject _FruitToDestroy)
        {
            var _fruitToDestroyHashCode = _FruitToDestroy.GetHashCode();
            var _fruitToDestroy = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _fruitToDestroyHashCode).Value;

            if (_fruitToDestroy != null)
            {
                if (_fruitToDestroy.IsGoldenFruit)
                {
                    return;
                }
                
                GoldenFruitCollision(_FruitToDestroy);
                GoldenFruitCollision(_UpgradedGoldenFruit);
            }
        }
        
        /// <summary>
        /// Tries to evolve the fruit with the given HashCode
        /// </summary>
        /// <param name="_Fruit">The HashCode of the fruit to evolve</param>
        public static void EvolveFruit(int _Fruit)
        {
            var _fruit = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _Fruit).Value;

            if (_fruit != null)
            {
                fruits.Remove(_Fruit);
                SkillController.Instance.Skill_Evolve(_fruit);
            }
        }
        
        /// <summary>
        /// Tries to destroy the fruit with the given HashCode
        /// </summary>
        /// <param name="_Fruit">The HashCode of the fruit to destroy</param>
        public static void DestroyFruit(int _Fruit)
        {
            var _fruit = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _Fruit).Value;

            if (_fruit != null)
            {
                fruits.Remove(_Fruit);
                SkillController.Instance.Skill_Destroy(_fruit);
            }
        }
        
        /// <summary>
        /// Adds a <see cref="FruitBehaviour"/> to <see cref="fruits"/>
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to add to <see cref="fruits"/></param>
        public static void AddFruit(FruitBehaviour _FruitBehaviour)
        {
            if (_FruitBehaviour.CanBeAddedToFruitCollection)
            {
                fruits.Add(_FruitBehaviour.gameObject.GetHashCode(), _FruitBehaviour);
            }
        }
        
        /// <summary>
        /// Gets the current count of all fruits on the map
        /// </summary>
        /// <returns>Returns the current count of all fruits on the map</returns>
        public static int GetFruitCount()
        {
            // Subtracts "-3" for the "FruitSpawner", "NextFruit" and "NextNextFruit"
            return fruits.Count - 3;
        }

        // TODO: Temporary
        public static void RemoveFruit(int _FruitHashcode)
        {
            fruits.Remove(_FruitHashcode);
        }
        #endregion
    }
}