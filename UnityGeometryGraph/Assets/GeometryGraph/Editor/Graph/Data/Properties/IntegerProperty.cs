using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    public class IntegerProperty : AbstractProperty {

        public IntegerProperty() {
            DisplayName = "Integer";
            Type = PropertyType.Integer;
        }

        public override AbstractProperty Copy() {
            return new IntegerProperty {
                DisplayName = DisplayName,
                Hidden = Hidden
            };
        }
    }
}