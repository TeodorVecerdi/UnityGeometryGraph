﻿using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    public class GeometryCollectionProperty : AbstractProperty {
        public GeometryCollectionProperty() {
            DisplayName = "Geometry Collection";
            Type = PropertyType.GeometryCollection;
        }

        public override AbstractProperty Copy() {
            return new GeometryCollectionProperty {
                DisplayName = DisplayName,
                Hidden = Hidden
            };
        }
    }
}