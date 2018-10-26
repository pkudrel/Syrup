using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Linq
{
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Executes function for each sequence item
        /// </summary>
        /// <typeparam name="TSource">Sequence</typeparam>
        /// <param name="source">Function to execute</param>
        /// <returns />
        [DebuggerStepThrough]
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (action == null)
                throw new ArgumentNullException("action");
            foreach (TSource source1 in source)
                action(source1);
        }

        /// <summary>
        ///     Tries to cast each item to the specified type. If it fails,  it just ignores the item
        /// </summary>
        /// <typeparam name="T" />
        /// <param name="source" />
        /// <returns />
        public static IEnumerable<T> CastSilentlyTo<T>(this IEnumerable source) where T : class
        {
            T res = default(T);
            foreach (object obj in source)
            {
                res = obj as T;
                if (res != null)
                    yield return res;
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}