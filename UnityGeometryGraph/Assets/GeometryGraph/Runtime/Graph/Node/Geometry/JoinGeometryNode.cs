using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;
using JetBrains.Annotations;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class JoinGeometryNode {
        [In(
            PortName = "InputPort",
            UpdateValueCode = "",
            GetValueCode = ""
        )]
        public GeometryData Input { get; private set; }

        [Out] public GeometryData Result { get; private set; }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            List<GeometryData> values = GetValues(InputPort, GeometryData.Empty).ToList();
            Result = GeometryData.Empty;
            foreach (GeometryData geometryData in values) {
                if (geometryData == null) continue;
                Result.MergeWith(geometryData);
            }
        }
    }
}