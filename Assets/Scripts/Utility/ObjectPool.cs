using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Watermelon_Game.Utility
{
    [Serializable]
    internal sealed class ObjectPool<T> where T : Component
    {
        #region Fields
        private readonly Queue<T> objectPool = new();
        private readonly T component;
        private readonly Transform parent;
        #endregion

        #region Constructors
        public ObjectPool(T _Component, Transform _Parent, uint _StartAmount = 0)
        {
            this.component = _Component;
            this.parent = _Parent;
            this.Add(_StartAmount);
        }
        #endregion
        
        #region Methods
        public T Get([CanBeNull] Transform _NewParent = null, Vector3? _Position = null, Quaternion? _Rotation = null)
        {
            var _component = this.InternalGet();

            if (_NewParent is not null)
            {
                _component.transform.SetParent(_NewParent);
            }
            if (_Position != null)
            {
                _component.transform.position = _Position.Value;
            }
            if (_Rotation != null)
            {
                _component.transform.rotation = _Rotation.Value;
            }
            
            _component.gameObject.SetActive(true);
            
            return _component;
        }

        public void Return(T _Component)
        {
            _Component.gameObject.SetActive(false);
            this.objectPool.Enqueue(_Component);
        }

        private T InternalGet()
        {
            var _isNotEmpty = this.objectPool.Count != 0;
            var _component = _isNotEmpty ? this.objectPool.Dequeue() : this.Add();
            
            return _component;
        }

        [CanBeNull]
        private T Add(uint _Amount = 1)
        {
            // ReSharper disable once InconsistentNaming
            for (var i = 0; i < _Amount; i++)
            {
                var _component = Object.Instantiate(this.component, this.parent);
                _component.gameObject.SetActive(false);
                this.objectPool.Enqueue(_component);
            }

            return this.objectPool.Count > 0 ? this.objectPool.Dequeue() : null;
        }
        #endregion
    }
}