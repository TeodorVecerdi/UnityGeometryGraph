using System;
using GeometryGraph.Runtime.Graph;
using Unity.Mathematics;

namespace GeometryGraph.Editor {
    [Serializable]
    public class VectorProperty : AbstractProperty {
        public float3 Value;

        public VectorProperty() {
            DisplayName = "Vector";
            Type = PropertyType.Vector;
            Value = float3.zero;
        }

        public override object DefaultValue => Value;

        public override AbstractProperty Copy() {
            return new VectorProperty {
                DisplayName = DisplayName,
                Hidden = Hidden,
                Value = Value
            };
        }
    }
}