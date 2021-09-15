using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class CollectionExtensions {
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable) {
        foreach (var value in enumerable) {
            collection.Add(value);
        }
    }
    
    public static IEnumerable<T> Convert<T>(this IEnumerable source, Func<object, T> converter) {
        foreach (var obj in source)
            yield return converter(obj);
    }

    public static T FirstOrGivenDefault<T>(this IEnumerable<T> values, Func<T, bool> predicate, T defaultValue) {
        foreach (var value in values) {
            if (predicate(value)) return value;
        }

        return defaultValue;
    }

    public static IEnumerable<(T, T)> SubSets2<T>(this IList<T> list) {
        for (var i = 0; i < list.Count - 1; i++) {
            for (var j = i + 1; j < list.Count; j++) {
                yield return (list[i], list[j]);
            }
        }
    }

    public static IEnumerable<IEnumerable<T>> KSubSets<T>(this IEnumerable<T> list, int length) where T : IComparable {
        if (length == 1) return list.Select(t => new[] { t });
        return KSubSets(list, length - 1).SelectMany(t => list.Where(e => t.All(g => g.CompareTo(e) == -1)), (t1, t2) => t1.Concat(new T[] { t2 }));
    }
}