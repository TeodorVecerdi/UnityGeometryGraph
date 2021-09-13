using System;
using System.Collections;
using System.Collections.Generic;

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
}