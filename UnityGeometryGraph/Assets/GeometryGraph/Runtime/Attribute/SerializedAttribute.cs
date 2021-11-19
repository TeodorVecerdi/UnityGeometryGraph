using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.AttributeSystem {
    [Serializable]
    public class SerializedAttribute {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
        private static readonly Type listType = typeof(List<>);
        
        public string Type;
        public string ValueType;
        public string Data;

        public static SerializedAttribute Serialize(BaseAttribute attribute) {
            var type = attribute.GetType();
            var valueType = attribute.ElementType;
            var listValueType = listType.MakeGenericType(valueType);

            var json = new JObject {
                ["n"] = attribute.Name,
                ["d"] = (int)attribute.Domain,
                ["v"] = JsonConvert.SerializeObject(attribute.Values, listValueType, Formatting.None, settings),
            };

            return new SerializedAttribute {
                Type = type.FullName,
                ValueType = valueType.AssemblyQualifiedName,
                Data = json.ToString(Formatting.None),
            };
        }

        public static BaseAttribute Deserialize(SerializedAttribute serializedAttribute) {
            var attributeType = System.Type.GetType(serializedAttribute.Type);
            var valueType = System.Type.GetType(serializedAttribute.ValueType);
            var listValueType = listType.MakeGenericType(valueType);
            
            var json = JObject.Parse(serializedAttribute.Data);
            
            var name = json.Value<string>("n");
            var attribute = (BaseAttribute) Activator.CreateInstance(attributeType!, name);
            var values = (IEnumerable) JsonConvert.DeserializeObject(json.Value<string>("v")!, listValueType, settings);
            
            attribute.Domain = (AttributeDomain) json.Value<int>("d");
            attribute.Values = new List<object>();
            foreach (var value in values!) {
                attribute.Values.Add(value);
            }
            
            return attribute;
        } 
    }
}