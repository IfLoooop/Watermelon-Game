using System;
using System.Collections;

namespace Watermelon_Game.ExtensionMethods
{
    // ReSharper disable once InconsistentNaming
    internal static class IEnumerableExtensions
    {
        #region Methods
        public static void ForEach<T>(this IEnumerable _IEnumerable, Action<T> _Action)
        {
            foreach (var _object in _IEnumerable)
            {
                _Action.Invoke((T)_object);
            }
        }
        #endregion
    }
}