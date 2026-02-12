using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DualGrid.Editor.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Checks if the values of a specific field differ among items in the collection.
        /// </summary>
        /// <param name="collection">The collection we want to iterate over</param>
        /// <param name="selector">A way to access the current enumerable element in the collection</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <returns></returns>
        public static bool HasDifferentValues<T, TField>(this IEnumerable<T> collection, Func<T, TField> selector)
        {
            if (collection == null)
                return false;
            
            //'using' calls dispose on the enumerator once we're finished, the same as 'try, finally' where we would call dispose in the 'finally' block
            using var enumerator = collection.GetEnumerator();
            
            if(!enumerator.MoveNext())
                return false;
            
            var firstValue = selector(enumerator.Current);
            var comparer = EqualityComparer<TField>.Default;

            while (enumerator.MoveNext())
            {
                var currentValue = selector(enumerator.Current);

                if (!comparer.Equals(currentValue, firstValue))
                {
                    //a difference has been found
                    return true; 
                }
            }

            //All values in the collection were the same
            return false;
        }
    }
}
