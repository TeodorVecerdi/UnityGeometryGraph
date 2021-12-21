using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    public static class RandomUtilities {
        private static readonly Dictionary<Type, Dictionary<ulong, string>> displayNameOverrides = CollectDisplayNameOverrides();

        /// <inheritdoc cref="DisplayNameString"/>
        public static string DisplayNameEnum(object enumValue) {
            Type enumType = enumValue.GetType();
            
            if (displayNameOverrides.ContainsKey(enumType) && displayNameOverrides[enumType].ContainsKey(ToUInt64(enumValue))) return displayNameOverrides[enumType][ToUInt64(enumValue)];
            
            return DisplayNameString(Enum.GetName(enumType, enumValue));
        }

        /// <summary>
        /// Splits <paramref name="value"/> into words where the case changes.
        /// </summary>
        /// <example><c>"someStringValue"</c> becomes <c>"Some String Value"</c></example>
        public static string DisplayNameString(string value) {
            StringBuilder result = new StringBuilder();
            int currentWordStart = 0;
            for (int i = 1; i < value.Length - 1; i++) {
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

        private static Dictionary<Type, Dictionary<ulong, string>> CollectDisplayNameOverrides() {
            Dictionary<Type, Dictionary<ulong, string>> dict = new Dictionary<Type, Dictionary<ulong, string>>();
            Type enumType = typeof(Enum);
            List<Type> allEnumTypes = typeof(RandomUtilities).Assembly.GetTypes().Where(type => type.IsSubclassOf(enumType))
                                                             .Union(typeof(RuntimeGraphObject).Assembly.GetTypes().Where(type => type.IsSubclassOf(enumType))).ToList();
            
            foreach (Type type in allEnumTypes) {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                foreach (FieldInfo field in fields) {
                    DisplayNameAttribute displayNameAttribute = field.GetCustomAttribute<DisplayNameAttribute>();
                    if(displayNameAttribute == null) continue;
                    ulong enumValue = ToUInt64(field.GetRawConstantValue());

                    if (!dict.ContainsKey(type)) dict[type] = new Dictionary<ulong, string>();
                    dict[type][enumValue] = displayNameAttribute.Name;
                }
            }

            return dict;
        }

        public static ulong ToUInt64<T>(this T value) where T : Enum {
            return ToUInt64((object)value);
        }
        
        private static ulong ToUInt64(object value)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return unchecked((ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture));

                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Char:
                case TypeCode.Boolean:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture);

                default: throw new InvalidOperationException("Unknown Enum Type");
            }
        }
    }
}