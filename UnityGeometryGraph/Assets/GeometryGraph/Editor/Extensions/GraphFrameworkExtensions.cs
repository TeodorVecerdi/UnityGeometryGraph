using System;
using Newtonsoft.Json;

namespace GeometryGraph.Editor {
    public static class GraphFrameworkExtensions {
        public static AbstractProperty Deserialize(this SerializedProperty property) {
            Type type = System.Type.GetType(property.Type);
            return (AbstractProperty) JsonConvert.DeserializeObject(property.Data, type, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
        }
    }
}