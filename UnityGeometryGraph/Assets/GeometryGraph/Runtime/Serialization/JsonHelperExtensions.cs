using System.Globalization;
using Newtonsoft.Json;

namespace Serialization {
    internal static class JsonHelperExtensions {
        public static float? ReadAsFloat(this JsonReader reader) {
            // https://github.com/jilleJr/Newtonsoft.Json-for-Unity.Converters/issues/46

            var str = reader.ReadAsString();

            if (string.IsNullOrEmpty(str))
                return null;

            return float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var valueParsed) ? valueParsed : 0f;
        }
    }
}