using System;
using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    public static class PropertyUtils {
        public static Type PropertyTypeToNodeType(PropertyType propertyType) {
            switch (propertyType) {
                case PropertyType.GeometryObject: return typeof(GeometryObjectPropertyNode);
                case PropertyType.GeometryCollection: return typeof(GeometryCollectionPropertyNode);
                case PropertyType.Integer: return typeof(IntegerPropertyNode);
                case PropertyType.Float: return typeof(FloatPropertyNode);
                case PropertyType.Vector: return typeof(VectorPropertyNode);
                default: throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
            }
        } 
    }
}