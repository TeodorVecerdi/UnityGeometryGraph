using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    public class FloatProperty : AbstractProperty {

        public FloatProperty() {
            DisplayName = "Float";
            Type = PropertyType.Float;
        }

        public override AbstractProperty Copy() {
            return new FloatProperty {
                DisplayName = DisplayName,
                Hidden = Hidden
            };
        }
    }
}