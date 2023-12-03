using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Watermelon_Game.Utility
{
    /// <summary>
    /// Can be used to display a value as a readonly field in the inspector <br/>
    /// <i>The inspector value is typeof <see cref="object"/>, so this class can even be used on types that can not properly be displayed in the inspector</i>
    /// </summary>
    /// <typeparam name="T">Can be any type</typeparam>
    [Serializable]
    [InlineProperty(LabelWidth = 50)]
    internal sealed class DebugField<T>
    {
#if UNITY_EDITOR
        #region Inspector Fields
        [Tooltip("The value of this DebugField")]
        [ShowInInspector][ReadOnly] private object value;
        #endregion  

        #region Fields
        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private T backingField;
        #endregion
#endif
        
        #region Properties
        /// <summary>
        /// Can be displayed in the inspector as a readonly value <br/>
        /// <i>Use <see cref="Set"/> to set the value, or create a new <see cref="DebugField{T}"/> in order to set the value</i>
        /// </summary>
        public T Value
        {
#if UNITY_EDITOR
            get => this.backingField;
            private set
            {
                this.backingField = value;
                this.value = this.backingField;
            }
#else
            get; private set;
#endif
        }
        #endregion

        #region Constreuctor
        /// <summary>
        /// <see cref="DebugField{T}"/>
        /// </summary>
        /// <param name="_Value"><see cref="Value"/></param>
        public DebugField(T _Value)
        {
            this.Value = _Value;
        }
        #endregion

        #region Operators
        /// <summary>
        /// <see cref="DebugField{T}"/>
        /// </summary>
        /// <param name="_Value"><see cref="Value"/></param>
        /// <returns><see cref="DebugField{T}"/></returns>
        public static implicit operator DebugField<T> (T _Value)
        {
            return new DebugField<T>(_Value);
        }
        
        /// <summary>
        /// <see cref="DebugField{T}"/>
        /// </summary>
        /// <param name="_DebugField"><see cref="DebugField{T}"/></param>
        /// <returns><see cref="Value"/></returns>
        public static implicit operator T (DebugField<T> _DebugField)
        {
            return _DebugField.Value;
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Sets the value of <see cref="Value"/>
        /// </summary>
        /// <param name="_Value">The value to set <see cref="Value"/> to</param>
        public void Set(T _Value)
        {
            this.Value = _Value;
        }
        
        /// <summary>
        /// Will return <see cref="Value"/>.<see cref="ToString"/>
        /// </summary>
        /// <returns><see cref="Value"/>.<see cref="ToString"/></returns>
        public override string ToString()
        {
            return this.Value.ToString();
        }
        #endregion
    }
}