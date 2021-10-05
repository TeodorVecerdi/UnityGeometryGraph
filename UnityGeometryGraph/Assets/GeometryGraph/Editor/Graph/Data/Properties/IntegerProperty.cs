using System;
using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    [Serializable]
    public class IntegerProperty : AbstractProperty {
        public int Value;

        public IntegerProperty() {
            DisplayName = "Integer";
            Type = PropertyType.Integer;
            Value = 0;
        }

        public override object DefaultValue => Value;

        public override AbstractProperty Copy() {
            return new IntegerProperty {
                DisplayName = DisplayName,
                Hidden = Hidden,
                Value = Value
            };
        }
    }
}