using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GeometryGraph.Runtime {
    public static class CollectionExtensions {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable) {
            foreach (T value in enumerable) {
                collection.Add(value);
            }
        }

        public static IEnumerable<T> Convert<T>(this IEnumerable source, Func<object, T> converter) {
            foreach (object obj in source)
                yield return converter(obj);
        }

        public static T FirstOrGivenDefault<T>(this IEnumerable<T> values, Func<T, bool> predicate, T defaultValue) {
            foreach (T value in values) {
                if (predicate(value)) return value;
            }

            return defaultValue;
        }

        public static IEnumerable<(T, T)> SubSets2<T>(this IList<T> list) {
            for (int i = 0; i < list.Count - 1; i++) {
                for (int j = i + 1; j < list.Count; j++) {
                    yield return (list[i], list[j]);
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> SubSetsN<T>(this IList<T> list, int length) where T : IComparable {
            if (length == 1) return list.Select(t => new[] { t });
            return SubSetsN(list, length - 1).SelectMany(t => list.Where(e => t.All(g => g.CompareTo(e) == -1)), (t1, t2) => t1.Concat(new T[] { t2 }));
        }
        
        public static List<T> Flatten<T>(this IEnumerable<IEnumerable<T>> list) {
            return list.SelectMany(t => t).ToList();
        }
    }
}