using System;
using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    [Serializable]
    public class StringProperty : AbstractProperty {
        public string Value;

        public StringProperty() {
            DisplayName = "String";
            Type = PropertyType.String;
            Value = "";
        }

        public override object DefaultValue => Value;

        public override AbstractProperty Copy() {
            return new StringProperty {
                DisplayName = DisplayName,
                Hidden = Hidden,
                Value = Value,
            };
        }
    }
}