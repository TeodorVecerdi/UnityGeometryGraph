using System;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace GeometryGraph.Runtime {
    public static class MiscUtilities {
        public static T CallThenReturn<T>([NotNull] Action action, T value) {
            Assert.IsNotNull(action);
            action();
            return value;
        }
    }
}