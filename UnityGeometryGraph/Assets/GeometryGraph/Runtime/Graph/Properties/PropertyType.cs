using System;
using GeometryGraph.Runtime.Data;

namespace GeometryGraph.Runtime.Graph {
    public enum PropertyType {
        GeometryObject,
        GeometryCollection,
    }

    public static class PropertyUtils {
        public static Type GetBackingValueType(PropertyType type) {
            return type switch {
                PropertyType.GeometryObject => typeof(GeometryObject),
                PropertyType.GeometryCollection => typeof(GeometryCollection),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static bool IsUnityObjectType(PropertyType type) {
            return type switch {
                PropertyType.GeometryObject => true,
                PropertyType.GeometryCollection => true,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}