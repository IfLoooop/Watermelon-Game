using UnityEngine;

namespace Watermelon_Game.ExtensionMethods
{
    internal static class ColorExtensions
    {
        #region Methods
        /// <summary>
        /// Returns this <see cref="Color"/> with the <see cref="Color.a"/> value set to the given value
        /// </summary>
        /// <param name="_Color">The <see cref="Color"/> object to set the alpha value of</param>
        /// <param name="_AlphaValue">Value must be in a range from 0 - 1</param>
        /// <returns>This <see cref="Color"/> with the <see cref="Color.a"/> value set to the given value</returns>
        public static Color WithAlpha(this Color _Color, float _AlphaValue)
        {
            _Color.a = _AlphaValue;
            return _Color;
        }
        #endregion
    }
}