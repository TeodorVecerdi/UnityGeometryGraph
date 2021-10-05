using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    public class GeometryObjectProperty : AbstractProperty {

        public GeometryObjectProperty() {
            DisplayName = "Geometry";
            Type = PropertyType.GeometryObject;
        }

        public override object DefaultValue => null;

        public override AbstractProperty Copy() {
            return new GeometryObjectProperty {
                DisplayName = DisplayName,
                Hidden = Hidden
            };
        }
    }
}