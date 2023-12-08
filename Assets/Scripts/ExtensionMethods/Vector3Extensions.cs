using UnityEngine;

namespace Watermelon_Game.ExtensionMethods
{
    /// <summary>
    /// Contains extension methods for <see cref="Vector3"/>
    /// </summary>
    internal static class Vector3Extensions
    {
        #region Methods
        /// <summary>
        /// Returns this <see cref="Vector3"/> with the given <see cref="Vector3.y"/> value
        /// </summary>
        /// <param name="_Vector3">The <see cref="Vector3"/> to set the <see cref="Vector3.y"/> value of</param>
        /// <param name="_Y">The <see cref="Vector3.y"/> value to set the <see cref="Vector3"/> to</param>
        /// <returns>A new <see cref="Vector3"/> with the previous vectors values and <see cref="Vector3.y"/> set to the given value</returns>
        public static Vector3 WithY(this Vector3 _Vector3, float _Y)
        {
            return new Vector3(_Vector3.x, _Y, _Vector3.z);
        }
        
        /// <summary>
        /// Returns this <see cref="Vector3"/> with the given <see cref="Vector3.z"/> value
        /// </summary>
        /// <param name="_Vector3">The <see cref="Vector3"/> to set the <see cref="Vector3.z"/> value of</param>
        /// <param name="_Z">The <see cref="Vector3.z"/> value to set the <see cref="Vector3"/> to</param>
        /// <returns>A new <see cref="Vector3"/> with the previous vectors values and <see cref="Vector3.z"/> set to the given value</returns>
        public static Vector3 WithZ(this Vector3 _Vector3, float _Z)
        {
            return new Vector3(_Vector3.x, _Vector3.y, _Z);
        }

        /// <summary>
        /// Returns the largest value along a single axis in this <see cref="Vector3"/>
        /// </summary>
        /// <param name="_Vector3">The <see cref="Vector3"/> to get the largest value of</param>
        /// <returns>The largest value along a single axis in this &lt;see cref="Vector3"/&gt;</returns>
        public static float Max(this Vector3 _Vector3)
        {
            var _value = _Vector3.x;
            
            if (_Vector3.y > _value)
            {
                _value = _Vector3.x;
            }
            if (_Vector3.z > _value)
            {
                _value = _Vector3.x;
            }

            return _value;
        }
        #endregion
    }
}