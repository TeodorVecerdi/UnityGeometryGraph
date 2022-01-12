using System;
using System.Collections;

namespace GeometryGraph.Runtime {
    internal static class MiscExtensions {
        internal static bool InRange(this Index index, Range range) {
            if (range.Start.IsFromEnd || range.End.IsFromEnd) return false;
            return index.Value >= range.Start.Value && index.Value < range.End.Value;
        }

        internal static bool InRange(this int index, Range range) {
            return index >= range.Start.Value && index < range.End.Value;
        }

        internal static bool InRange(this Index index, ICollection collection) {
            int actualIndex = index.IsFromEnd ? collection.Count - index.Value : index.Value; 
            return actualIndex >= 0 && actualIndex < collection.Count;
        }
        internal static bool InRange(this int index, ICollection collection) {
            return index >= 0 && index < collection.Count;
        }
    }
}