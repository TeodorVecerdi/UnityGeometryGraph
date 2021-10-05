using System;
using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    [Serializable]
    public class FloatProperty : AbstractProperty {
        public float Value;

        public FloatProperty() {
            DisplayName = "Float";
            Type = PropertyType.Float;
            Value = 0.0f;
        }

        public override object DefaultValue => Value;

        public override AbstractProperty Copy() {
            return new FloatProperty {
                DisplayName = DisplayName,
                Hidden = Hidden,
                Value = Value,
            };
        }
    }
}