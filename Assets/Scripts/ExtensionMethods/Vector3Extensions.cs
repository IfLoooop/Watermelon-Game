using UnityEngine;

namespace Watermelon_Game.ExtensionMethods
{
    internal static class Vector3Extensions
    {
        #region Methods
        public static Vector3 WithZ(this Vector3 _Vector3, float _Z)
        {
            return new Vector3(_Vector3.x, _Vector3.y, _Z);
        }
        #endregion
    }
}