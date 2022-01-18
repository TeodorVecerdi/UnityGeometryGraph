using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class SetCurvePositionNode {
        [In(
            DefaultValue = "CurveData.Empty",
            GetValueCode = "{self} = GetValue(connection, {default})",
            UpdateValueCode = ""
        )]
        public CurveData Input { get; private set; }

        [In(IsSerialized = false)]
        public float3 Position { get; private set; }

        [Out] public CurveData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true)]
        private CurveData GetResult() => Result ?? CurveData.Empty;

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != InputPort) return;
            Result = CurveData.Empty;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Input == null || Input.Points == 0 || Input.Type == CurveType.None) {
                Result = CurveData.Empty;
                return;
            }

            List<float3> position = GetValues(PositionPort, Input.Points, Position).ToList();
            Result = new CurveData(Input.Type, Input.Points, Input.IsClosed, position, Input.Tangent.ToList(), Input.Normal.ToList(), Input.Binormal.ToList());
        }
    }
}