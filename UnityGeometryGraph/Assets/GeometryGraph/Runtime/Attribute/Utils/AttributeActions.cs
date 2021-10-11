using System;

namespace GeometryGraph.Runtime.Attribute {
    public static class AttributeActions {
        public static Func<T, T> NoOp<T>() => arg => arg;
        public static Func<T, T0, T> NoOp<T, T0>() => (arg, _) => arg;
        public static Func<T, T0, T1, T> NoOp<T, T0, T1>() => (arg, _, _) => arg;
    }
}