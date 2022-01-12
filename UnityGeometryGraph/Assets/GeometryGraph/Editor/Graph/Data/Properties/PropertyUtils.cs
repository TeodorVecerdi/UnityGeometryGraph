using System;
using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    public static class PropertyUtils {
        public static Type PropertyTypeToNodeType(PropertyType propertyType) {
            return propertyType switch {
                PropertyType.GeometryObject => typeof(GeometryObjectPropertyNode),
                PropertyType.GeometryCollection => typeof(GeometryCollectionPropertyNode),
                PropertyType.Integer => typeof(IntegerPropertyNode),
                PropertyType.Float => typeof(FloatPropertyNode),
                PropertyType.Vector => typeof(VectorPropertyNode),
                PropertyType.String => typeof(StringPropertyNode),
                _ => throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null)
            };
        }
    }
}