using System.Collections.Generic;

namespace Watermelon_Game.ExtensionMethods
{
    /// <summary>
    /// Contains extension methods for <see cref="List{T}"/>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal static class IListExtensions
    {
        #region Methods
        /// <summary>
        /// Removes and returns the object at the beginning of the <see cref="List{T}"/>
        /// </summary>
        /// <param name="_List">The <see cref="List{T}"/> to remove and return the object from</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>The object at the beginning of the <see cref="List{T}"/></returns>
        public static T Dequeue<T>(this IList<T> _List)
        {
            var _firstEntry = _List[0];
            _List.RemoveAt(0);

            return _firstEntry;
        }
        #endregion
    }
}