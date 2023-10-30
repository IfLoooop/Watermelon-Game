using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon_Game.ExtensionMethods;
using Object = UnityEngine.Object;

namespace Watermelon_Game.Utility
{
    /// <summary>
    /// Generic object pool for <see cref="Component"/>s
    /// </summary>
    /// <typeparam name="T">Must be <see cref="Type"/> of <see cref="Component"/></typeparam>
    [Serializable]
    internal sealed class ObjectPool<T> where T : Component
    {
        #region Fields
        /// <summary>
        /// Holds all <see cref="Component"/>s
        /// </summary>
        [ListDrawerSettings(DefaultExpandedState = true)]
        [ReadOnly] [ShowInInspector] private readonly List<T> objectPool = new();
        /// <summary>
        /// All <see cref="Component"/>s in <see cref="objectPool"/> will be clones of this
        /// </summary>
        private readonly T componentBlueprint;
        /// <summary>
        /// Parent that is used during instantiation
        /// </summary>
        private readonly Transform parent;
        /// <summary>
        /// The initial active state of the <see cref="GameObject"/> when it's instantiated and returned to the <see cref="ObjectPool{T}"/>
        /// </summary>
        private readonly bool initialActiveState;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="ObjectPool{T}"/>
        /// </summary>
        /// <param name="_ComponentBlueprint"><see cref="componentBlueprint"/></param>
        /// <param name="_Parent"><see cref="parent"/></param>
        /// <param name="_StartAmount">Amount to instantiate when this <see cref="ObjectPool{T}"/> is created</param>
        /// <param name="_InitialActiveState"><see cref="initialActiveState"/></param>
        public ObjectPool(T _ComponentBlueprint, Transform _Parent, uint _StartAmount = 0, bool _InitialActiveState = false)
        {
            this.componentBlueprint = _ComponentBlueprint;
            this.parent = _Parent;
            this.initialActiveState = _InitialActiveState;
            this.Add(_StartAmount);
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Returns the <see cref="Component"/> at the beginning of <see cref="objectPool"/>
        /// </summary>
        /// <param name="_NewParent">The <see cref="Transform"/> to use as a <see cref="Transform.parent"/></param>
        /// <returns></returns>
        public T Get(Transform _NewParent)
        {
            return this.Get(true, _NewParent, null, null);
        }
        
        /// <summary>
        /// Returns the <see cref="Component"/> at the beginning of <see cref="objectPool"/>
        /// </summary>
        /// <param name="_NewPosition">The <see cref="Transform.position"/> of the dequeued <see cref="Component"/></param>
        /// <returns></returns>
        public T Get(Vector3 _NewPosition)
        {
            return this.Get(true, null, _NewPosition, null);
        }
        
        /// <summary>
        /// Returns the <see cref="Component"/> at the beginning of <see cref="objectPool"/>
        /// </summary>
        /// <param name="_ActiveState">Whether the <see cref="GameObject"/> is active or inactive</param>
        /// <param name="_NewParent">The <see cref="Transform"/> to use as a <see cref="Transform.parent"/></param>
        /// <param name="_NewPosition">The <see cref="Transform.position"/> of the dequeued <see cref="Component"/></param>
        /// <param name="_NewRotation">The <see cref="Transform.rotation"/> of the dequeued <see cref="Component"/></param>
        /// <returns>The <see cref="Component"/> at the beginning of <see cref="objectPool"/></returns>
        public T Get(bool _ActiveState, [CanBeNull] Transform _NewParent, Vector3? _NewPosition, Quaternion? _NewRotation)
        {
            var _component = this.InternalGet();

            if (_NewParent is not null)
            {
                _component.transform.SetParent(_NewParent);
            }
            if (_NewPosition != null)
            {
                _component.transform.position = _NewPosition.Value;
            }
            if (_NewRotation != null)
            {
                _component.transform.rotation = _NewRotation.Value;
            }
            
            _component.gameObject.SetActive(_ActiveState);
            
            return _component;
        }

        /// <summary>
        /// Returns the <see cref="Component"/> at the beginning of <see cref="objectPool"/> or instantiates a new one and return it
        /// </summary>
        /// <returns>The <see cref="Component"/> at the beginning of <see cref="objectPool"/> or instantiates a new one and return it</returns>
        private T InternalGet()
        {
            var _isNotEmpty = this.objectPool.Count != 0;
            var _component = _isNotEmpty ? this.objectPool.Dequeue() : this.AddAndReturn();
            
            return _component;
        }
        
        /// <summary>
        /// Instantiates a new <see cref="Component"/>, adds it to <see cref="objectPool"/> and returns it
        /// </summary>
        /// <param name="_Amount">How many <see cref="componentBlueprint"/>s to instantiate</param>
        /// <returns>
        /// The instantiated <see cref="componentBlueprint"/> <br/>
        /// <i>Can only return null of <see cref="_Amount"/> is less than 0</i>
        /// </returns>
        [CanBeNull]
        private T AddAndReturn(uint _Amount = 1)
        {
            this.Add(_Amount);

            return this.objectPool.Count > 0 ? this.objectPool.Dequeue() : null;
        }
        
        /// <summary>
        /// Instantiates a new <see cref="Component"/> and adds it to <see cref="objectPool"/>
        /// </summary>
        /// <param name="_Amount">How many <see cref="componentBlueprint"/>s to instantiate</param>
        private void Add(uint _Amount = 1)
        {
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < _Amount; i++)
            {
                var _component = Object.Instantiate(this.componentBlueprint, this.parent);
                _component.gameObject.SetActive(this.initialActiveState);
                this.objectPool.Add(_component);
            }
        }
        
        /// <summary>
        /// Returns the given <see cref="Component"/> back to <see cref="objectPool"/>
        /// </summary>
        /// <param name="_Component">The <see cref="Component"/> to return to <see cref="objectPool"/></param>
        public void Return(T _Component)
        {
            _Component.transform.SetParent(this.parent);
            _Component.gameObject.SetActive(this.initialActiveState);
            this.objectPool.Add(_Component);
        }
        
        /// <summary>
        /// Removes the given <see cref="Component"/> from <see cref="objectPool"/>
        /// </summary>
        /// <param name="_Component">The <see cref="Component"/> to remove</param>
        public void Remove(T _Component)
        {
            this.objectPool.Remove(_Component);
        }
        #endregion
    }
}