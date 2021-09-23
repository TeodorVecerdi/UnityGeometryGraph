using System.Text;

namespace GeometryGraph.Editor {
    public static class RandomUtilities {
        /// <summary>
        /// Splits <paramref name="value"/> into words where the case changes.
        /// </summary>
        /// <example><c>"someStringValue"</c> becomes <c>"Some String Value"</c></example>
        public static string WordBreakString(string value) {
            var result = new StringBuilder();
            var currentWordStart = 0;
            for (var i = 1; i < value.Length - 1; i++) {
                if (char.IsUpper(value[i]) != char.IsUpper(value[i + 1])) {
                    result.Append($"{Capitalize(value.Substring(currentWordStart, i - currentWordStart + 1))} ");
                    currentWordStart = i + 1;
                    i++;
                }
            }

            result.Append(Capitalize(value.Substring(currentWordStart)));

            return result.ToString();
        }

        private static string Capitalize(string value) {
            return $"{char.ToUpper(value[0])}{value.Substring(1)}";
        }
    }
}