using System;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;

namespace GeometryGraph.Editor {
    [Serializable]
    public class SerializedProperty {
        public string Type;
        public string Data;

        public SerializedProperty(AbstractProperty property) {
            Data = JsonConvert.SerializeObject(property, Formatting.None, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All, Converters = new [] { float3Converter.Converter }});
            Type = property.GetType().FullName;
        }
    }
}