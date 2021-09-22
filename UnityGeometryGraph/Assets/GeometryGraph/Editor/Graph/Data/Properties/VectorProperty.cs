using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    public class VectorProperty : AbstractProperty {

        public VectorProperty() {
            DisplayName = "Vector";
            Type = PropertyType.Vector;
        }

        public override AbstractProperty Copy() {
            return new VectorProperty {
                DisplayName = DisplayName,
                Hidden = Hidden
            };
        }
    }
}