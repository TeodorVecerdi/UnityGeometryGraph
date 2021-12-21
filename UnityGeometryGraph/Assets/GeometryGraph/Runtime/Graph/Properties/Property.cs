using System;
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

        public static T GetValueOrDefault<T>(Property property, T @default) {
            return property?.Value is T tValue ? tValue : property?.DefaultValue != null ? property.DefaultValue.Value<T>() : @default;
        }
    }

    [Serializable]
    public class DefaultPropertyValue {
        public int IntValue;
        public float FloatValue;
        public float3 VectorValue;
        public string StringValue;

        public T Value<T>() {
            Type tType = typeof(T);
            if (tType == typeof(int)) return (T)(object)IntValue;
            if (tType == typeof(float)) return (T)(object)FloatValue;
            if (tType == typeof(float3)) return (T)(object)VectorValue;
            if (tType == typeof(string)) return (T)(object)StringValue;
            return (T)(object)null;
        }

        public DefaultPropertyValue(PropertyType propertyType, object value){
            switch (propertyType) {
                case PropertyType.GeometryObject:
                case PropertyType.GeometryCollection:
                    return;

                case PropertyType.Integer: IntValue = (int)value; break;
                case PropertyType.Float: FloatValue = (float)value; break;
                case PropertyType.Vector: VectorValue = (float3)value; break;
                case PropertyType.String: StringValue = (string)value; break;
                default: throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null);
            }
        }

        public override string ToString() {
            return $"Int: `{IntValue}` -- Float: `{FloatValue}` -- Vector: `{VectorValue}` -- String: `{StringValue}`";
        }
    }
}