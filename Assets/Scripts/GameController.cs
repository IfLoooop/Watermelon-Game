using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon_Game.Fruit;

namespace Watermelon_Game
{
    internal sealed class GameController : MonoBehaviour
    {
        #region Constants
        public const int FRUIT_LAYER = 8;
        #endregion

        #region Fields
        /// <summary>
        /// Contains all instantiated fruits in the scene <br/>
        /// <b>Key:</b> Hashcode of the fruit <see cref="GameObject"/> <br/>
        /// <b>Value:</b> The <see cref="FruitBehaviour"/>
        /// </summary>
        private static readonly Dictionary<int, FruitBehaviour> fruits = new();
        #endregion
        
        #region Events
        /// <summary>
        /// Adds a <see cref="FruitBehaviour"/> to <see cref="fruits"/>
        /// </summary>
        /// <param name="_FruitBehaviour">The <see cref="FruitBehaviour"/> to add to <see cref="fruits"/></param>
        public static void AddFruit(FruitBehaviour _FruitBehaviour)
        {
            fruits.Add(_FruitBehaviour.gameObject.GetHashCode(), _FruitBehaviour);
        }
        
        /// <summary>
        /// Determines what happens when two fruits collide with each other
        /// </summary>
        /// <param name="_Fruit1">HashCode of the first fruit</param>
        /// <param name="_Fruit2">HashCode of the second fruit</param>
        public static void FruitCollision(int _Fruit1, int _Fruit2)
        {
            var _fruit1 = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _Fruit1).Value;
            var _fruit2 = fruits.FirstOrDefault(_Kvp => _Kvp.Key == _Fruit2).Value;

            if (_fruit1 != null && _fruit2 != null)
            {
                if (_fruit1.Fruit == _fruit2.Fruit)
                {
                    fruits.Remove(_Fruit1);
                    fruits.Remove(_Fruit2);

                    var _position = (_fruit1.transform.position + _fruit2.transform.position) / 2;
                    var _fruitIndex = (int)Enum.GetValues(typeof(Fruit.Fruit)).Cast<Fruit.Fruit>().FirstOrDefault(_Fruit => _Fruit == _fruit1.Fruit + 1);
                    
                    //TODO: Move towards each other before destroying
                    Destroy(_fruit1.gameObject);
                    Destroy(_fruit2.gameObject);
                    
                    if (_fruitIndex != (int)Fruit.Fruit.Grape)
                    {
                        var _fruit = FruitCollection.Instance.Fruits[_fruitIndex].Fruit;
                        FruitBehaviour.SpawnFruit(_position, _fruit);
                    }
                }
            }
        }
        #endregion
    }
}