using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters;
using Unity.Mathematics;

namespace Serialization.JsonConverters {
    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public class float2Converter : PartialConverter<float2> {
        protected override void ReadValue(ref float2 value, string name, JsonReader reader, JsonSerializer serializer) {
            switch (name) {
                case nameof(value.x):
                    value.x = reader.ReadAsFloat() ?? 0f;
                    break;
                case nameof(value.y):
                    value.y = reader.ReadAsFloat() ?? 0f;
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, float2 value, JsonSerializer serializer) {
            writer.WritePropertyName(nameof(value.x));
            writer.WriteValue(value.x);
            writer.WritePropertyName(nameof(value.y));
            writer.WriteValue(value.y);
        }
    }
}