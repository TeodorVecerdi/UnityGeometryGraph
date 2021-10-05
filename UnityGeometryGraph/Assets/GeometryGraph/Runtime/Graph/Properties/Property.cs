using System;
using Sirenix.OdinInspector;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class Property {
        public string Guid;
        public string ReferenceName;
        public string DisplayName;
        public PropertyType Type;
        public object Value;
        public DefaultPropertyValue DefaultValue;
    }

    [Serializable]
    public class DefaultPropertyValue {
        public int IntValue;
        public float FloatValue;
        public float3 VectorValue;

        public DefaultPropertyValue(PropertyType propertyType, object value){
            switch (propertyType) {
                case PropertyType.GeometryObject:
                case PropertyType.GeometryCollection:
                    return;

                case PropertyType.Integer: IntValue = (int)value; break;
                case PropertyType.Float: FloatValue = (float)value; break;
                case PropertyType.Vector: VectorValue = (float3)value; break;
                default: throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
            }
        }
    }
}