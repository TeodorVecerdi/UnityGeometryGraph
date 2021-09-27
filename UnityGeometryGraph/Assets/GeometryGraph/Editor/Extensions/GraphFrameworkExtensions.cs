using Newtonsoft.Json;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public static class GraphFrameworkExtensions {
        public static AbstractProperty Deserialize(this SerializedProperty property) {
            var type = System.Type.GetType(property.Type);
            return (AbstractProperty) JsonConvert.DeserializeObject(property.Data, type, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
        }
    }
}