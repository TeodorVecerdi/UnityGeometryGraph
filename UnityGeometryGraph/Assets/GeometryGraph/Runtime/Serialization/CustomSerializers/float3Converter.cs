using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Serialization {
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class float3Converter : PartialConverter<float3> {
        public static float3Converter Converter = new float3Converter();
        
        protected override void ReadValue(ref float3 value, string name, JsonReader reader, JsonSerializer serializer) {
            switch (name) {
                case nameof(value.x):
                    value.x = reader.ReadAsFloat() ?? 0f;
                    break;
                case nameof(value.y):
                    value.y = reader.ReadAsFloat() ?? 0f;
                    break;
                case nameof(value.z):
                    value.z = reader.ReadAsFloat() ?? 0f;
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, float3 value, JsonSerializer serializer) {
            writer.WritePropertyName(nameof(value.x));
            writer.WriteValue(value.x);
            writer.WritePropertyName(nameof(value.y));
            writer.WriteValue(value.y);
            writer.WritePropertyName(nameof(value.z));
            writer.WriteValue(value.z);
        }
    }
}