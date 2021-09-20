using System;

namespace GeometryGraph.Runtime.Attribute {
    public static class AttributeActions {
        public static Func<T, T> NoOp<T>() => arg => arg;
        public static Func<T, _, T> NoOp<T, _>() where _ : T => (arg, _) => arg;
    }
}