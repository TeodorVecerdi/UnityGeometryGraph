using System.Collections.Generic;

public static class CollectionExtensions {
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable) {
        foreach (var value in enumerable) {
            collection.Add(value);
        }
    }
}