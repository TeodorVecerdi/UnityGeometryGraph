using System;
using GeometryGraph.Runtime.Data;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public enum PropertyType {
        GeometryObject,
        GeometryCollection,
        Integer,
        Float,
        Vector
    }

    public static class PropertyUtils {
        public static Type GetBackingValueType(PropertyType type) {
            return type switch {
                PropertyType.GeometryObject => typeof(GeometryObject),
                PropertyType.GeometryCollection => typeof(GeometryCollection),
                PropertyType.Integer => typeof(int),
                PropertyType.Float => typeof(float),
                PropertyType.Vector => typeof(float3),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static bool IsUnityObjectType(PropertyType type) {
            return type switch {
                PropertyType.GeometryObject => true,
                PropertyType.GeometryCollection => true,
                
                PropertyType.Integer => false,
                PropertyType.Float => false,
                PropertyType.Vector => false,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}