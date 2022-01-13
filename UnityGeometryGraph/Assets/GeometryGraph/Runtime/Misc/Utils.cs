using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace GeometryGraph.Runtime {
    public static class Utils {
        public static T CallThenReturn<T>([NotNull] Action action, T value) {
            Assert.IsNotNull(action);
            action();
            return value;
        }

        public static T IfNotSerializing<T>([NotNull] Func<T> func, string display, T defaultValue = default) {
            if (RuntimeGraphObjectData.IsDuringSerialization) {
                Debug.LogWarning($"`<b>{display}</b>` is not allowed during serialization");
                return defaultValue;
            }

            try {
                return func();
            } catch (UnityException unityException) {
                Debug.LogWarning($"`<b>{display}</b>` is not allowed during serialization: {unityException.Message}");
                return defaultValue;
            } catch (Exception exception) {
                Debug.LogError($"<b>{display}</b> failed with exception: {exception.GetType()}. Returning default value: <b>{defaultValue}</b>\nException message and stack trace:\n{exception.Message}\n{exception.StackTrace}");
                return defaultValue;
            }
        }
    }
}