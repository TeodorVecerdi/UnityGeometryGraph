using System;
using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    public static class PropertyUtils {
        public static Type PropertyTypeToSystemType(PropertyType propertyType) {
            switch (propertyType) {
                case PropertyType.GeometryObject: return typeof(GeometryObjectPropertyNode);
                case PropertyType.GeometryCollection: return typeof(GeometryCollectionPropertyNode);
                default: throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
            }
        } 
    }
}