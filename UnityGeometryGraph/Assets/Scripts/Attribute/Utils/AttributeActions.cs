using System;

namespace Attribute {
    public static class AttributeActions {
        public static Func<T, T> NoOp<T>() => arg => arg;
        public static Func<T, T, T> NoOp2<T>() => (arg, _) => arg;
    }
}