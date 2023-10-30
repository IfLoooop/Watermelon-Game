using System;
using System.Collections.Generic;

namespace Watermelon_Game.ExtensionMethods
{
    /// <summary>
    /// Contains extension methods for <see cref="IEnumerable{T}"/>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal static class IEnumerableExtensions
    {
        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire
        /// </summary>
        /// <param name="_Enumerable">The <see cref="IEnumerable{T}"/> to search on</param>
        /// <param name="_Match">The condition to search for</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1</returns>
        public static int FindIndex<T>(this IEnumerable<T> _Enumerable, Predicate<T> _Match)
        {
            var _count = 0;
            using var _enumerator = _Enumerable.GetEnumerator();
            
            while (_enumerator.MoveNext())
            {
                if (_Match(_enumerator.Current))
                {
                    return _count;
                }

                _count++;
            }
            
            return -1;
        }
    }
}