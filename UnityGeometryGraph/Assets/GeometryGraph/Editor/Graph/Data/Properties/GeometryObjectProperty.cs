using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    public class GeometryObjectProperty : AbstractProperty {

        public GeometryObjectProperty() {
            DisplayName = "Geometry Object";
            Type = PropertyType.GeometryObject;
        }

        public override AbstractProperty Copy() {
            return new GeometryObjectProperty {
                DisplayName = DisplayName,
                Hidden = Hidden
            };
        }
    }
}