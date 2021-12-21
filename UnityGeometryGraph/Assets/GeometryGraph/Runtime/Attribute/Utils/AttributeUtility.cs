using System;
using System.Collections.Generic;
using System.Linq;

namespace GeometryGraph.Runtime.AttributeSystem {
    public static class AttributeUtility {
        public static AttributeType SystemTypeToAttributeType(Type type) {
            return attributeTypeDictionary[type];
        }
        
        public static Type AttributeTypeToSystemType(AttributeType type) {
            return systemTypeDictionary[type];
        }

        private static readonly Dictionary<Type, AttributeType> attributeTypeDictionary = new() {
            { typeof(BoolAttribute), AttributeType.Boolean },
            { typeof(IntAttribute), AttributeType.Integer },
            { typeof(FloatAttribute), AttributeType.Float },
            { typeof(ClampedFloatAttribute), AttributeType.ClampedFloat },
            { typeof(Vector2Attribute), AttributeType.Vector2 },
            { typeof(Vector3Attribute), AttributeType.Vector3 }
        };

        private static readonly Dictionary<AttributeType, Type> systemTypeDictionary = attributeTypeDictionary.ToDictionary(pair => pair.Value, pair => pair.Key);
    }
}