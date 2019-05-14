using System;
using System.Collections.Generic;

namespace Clifton.Core.ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static string Parens(this string src)
        {
            return "(" + src + ")";
        }

        /// <summary>
        /// If the object is not null, performs the action and returns true.
        /// </summary>
        public static bool IfNotNull<T>(this T obj, Action<T> action)
        {
            bool ret = obj != null;

            if (ret) { action(obj); }

            return ret;
        }
        
        // ---------- ForEach iterators --------------

        /// <summary>
        /// Implements a ForEach for generic enumerators.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
        
        /// <summary>
        /// ForEach with an index.
        /// </summary>
        public static void ForEachWithIndex<T>(this IEnumerable<T> collection, Action<T, int> action)
        {
            int n = 0;

            foreach (var item in collection)
            {
                action(item, n++);
            }
        }
    }
}