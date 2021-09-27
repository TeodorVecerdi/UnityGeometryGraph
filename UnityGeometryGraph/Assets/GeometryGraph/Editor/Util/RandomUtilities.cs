using System.Text;

namespace GeometryGraph.Editor {
    public static class RandomUtilities {
        /// <summary>
        /// Splits <paramref name="value"/> into words where the case changes.
        /// </summary>
        /// <example><c>"someStringValue"</c> becomes <c>"Some String Value"</c></example>
        public static string DisplayNameString(string value) {
            var result = new StringBuilder();
            var currentWordStart = 0;
            for (var i = 1; i < value.Length - 1; i++) {
                if (value[i] == '_') {
                    result.Append($"{Capitalize(value.Substring(currentWordStart, i - currentWordStart))} ");
                    currentWordStart = i + 1;
                    i += 2;
                }
                if (char.IsUpper(value[i]) != char.IsUpper(value[i + 1]) || char.IsLetter(value[i]) != char.IsLetter(value[i + 1])) {
                    result.Append($"{Capitalize(value.Substring(currentWordStart, i - currentWordStart + 1))} ");
                    i++;
                    currentWordStart = i;
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