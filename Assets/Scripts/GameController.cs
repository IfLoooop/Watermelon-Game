using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon_Game.Fruit;
using Watermelon_Game.Fruit_Spawn;
using Watermelon_Game.Menu;
using Watermelon_Game.Skills;

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
        private static readonly Dictionary<int, FruitBehaviour> fruits = new();
        #endregion

        #region Properties
        public static GameController Instance { get; private set; }
        
        public FruitCollection FruitCollection => this.fruitCollection;
        public float CurrentGameTimeStamp { get; private set; }
        #endregion
        
        #region Methods
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            StartGame();
        }
        
        /// <summary>
        /// Adds a <see cref="FruitBehaviour"/> to <see cref="fruits"/>
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to add to <see cref="fruits"/></param>
        public static void AddFruit(FruitBehaviour _FruitBehaviour)
        {
            fruits.Add(_FruitBehaviour.gameObject.GetHashCode(), _FruitBehaviour);
        }

        public static void GoldenFruitCollision(int _Fruit)
        {
            var _fruit = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _Fruit).Value;

            if (_fruit != null)
            {
                fruits.Remove(_Fruit);
                
                var _fruitIndex = (int)Enum.GetValues(typeof(Fruit.Fruit)).Cast<Fruit.Fruit>().FirstOrDefault(_Fruit => _Fruit == _fruit.Fruit);
                    
                PointsController.Instance.AddPoints((Fruit.Fruit)_fruitIndex);
                Instance.FruitCollection.PlayEvolveSound();
                
                _fruit.Destroy();
            }
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
                
                if (_fruit1.Fruit == _fruit2.Fruit)
                {
                    fruits.Remove(_Fruit1);
                    fruits.Remove(_Fruit2);

                    var _position = (_fruit1.transform.position + _fruit2.transform.position) / 2;
                    var _fruitIndex = (int)Enum.GetValues(typeof(Fruit.Fruit)).Cast<Fruit.Fruit>().FirstOrDefault(_Fruit => _Fruit == _fruit1.Fruit);
                    
                    PointsController.Instance.AddPoints((Fruit.Fruit)_fruitIndex);
                    GameOverMenu.Instance.AddFruitCount(_fruit1.Fruit);
                    Instance.FruitCollection.PlayEvolveSound();
                    
                    //TODO: Move towards each other before destroying
                    _fruit1.Destroy();
                    _fruit2.Destroy();
                    
                    if (_fruitIndex != (int)Fruit.Fruit.Melon)
                    {
                        var _fruit = Instance.FruitCollection.Fruits[_fruitIndex + 1].Fruit;
                        var _fruitBehaviour = FruitBehaviour.SpawnFruit(_position, _fruit, true);
                        _fruitBehaviour.Evolve();
                    }
                    // When two Watermelons are evolved
                    else
                    {
                        NextFruit.Instance.ShowNextNextFruit();
                    }
                }
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

        public static void StartGame()
        {
            FruitSpawner.Instance.ResetFruitSpawner(true);
            FruitSpawnerAim.Enable(true);
            PointsController.Instance.ResetPoints();
            StatsMenu.Instance.GamesPlayed++;
            Instance.CurrentGameTimeStamp = Time.time;
        }
        
        public static void GameOver()
        {
            FruitSpawner.Instance.BlockInput = true;
            FruitSpawnerAim.Enable(false);

            // TODO: Destroy all fruits in a coroutine over time
            foreach (var _fruitBehaviour in fruits.Values)
            {
                _fruitBehaviour.Destroy();
            }
            
            fruits.Clear();
            
            PointsController.Instance.SavePoints();
            PointsController.Instance.ResetPoints();
            // Needed for the SkillController.PointsChanged() method to be called
            PointsController.Instance.SubtractPoints(0);
            MenuController.Instance.GameOver();
            NextFruit.Instance.GameOVer();
        }
        
        /// <summary>
        /// Gets the current count of all fruits on the map
        /// </summary>
        /// <param name="_SubtractNextFruits">Subtracts this value from the count (for the next fruits)</param>
        /// <returns>Returns the current count of all fruits on the map</returns>
        public static int GetFruitCount(int _SubtractNextFruits = 3)
        {
            return fruits.Count - _SubtractNextFruits;
        }
        #endregion
    }
}