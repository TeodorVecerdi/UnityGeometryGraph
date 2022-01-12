using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeometryGraph.Runtime.AttributeSystem {
    [Serializable]
    public class SerializedAttribute {
        private static readonly JsonSerializerSettings settings = new() { TypeNameHandling = TypeNameHandling.None };
        private static readonly Type listType = typeof(List<>);

        public string Type;
        public string ValueType;
        public string Data;

        public static SerializedAttribute Serialize(BaseAttribute attribute) {
            Type type = attribute.GetType();
            Type valueType = attribute.ElementType;
            Type listValueType = listType.MakeGenericType(valueType);

            JObject json = new() {
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
            Type attributeType = System.Type.GetType(serializedAttribute.Type);
            Type valueType = System.Type.GetType(serializedAttribute.ValueType);
            Type listValueType = listType.MakeGenericType(valueType);

            JObject json = JObject.Parse(serializedAttribute.Data);

            string name = json.Value<string>("n");
            BaseAttribute attribute = (BaseAttribute) Activator.CreateInstance(attributeType!, name);
            IEnumerable values = (IEnumerable) JsonConvert.DeserializeObject(json.Value<string>("v")!, listValueType, settings);

            attribute.Domain = (AttributeDomain) json.Value<int>("d");
            attribute.Values = new List<object>();
            foreach (object value in values!) {
                attribute.Values.Add(value);
            }

            return attribute;
        }
    }
}