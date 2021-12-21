using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class ReverseCurveNode {
        [In] public CurveData Input { get; private set; }
        [Out] public CurveData Result { get; private set; }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port == InputPort) {
                Input = CurveData.Empty;
                Result = CurveData.Empty;
                NotifyPortValueChanged(ResultPort);
            }
        }

        [CalculatesProperty(nameof(Result))]
        private void Calculate() {
            if (Input == null || Input.Points == 0) {
                Result = CurveData.Empty;
                return;
            }

            List<float3> newPosition = new();
            List<float3> newTangent = new();
            List<float3> newNormal = new();
            
            for (int i = Input.Points - 1; i >= 0; --i) {
                newPosition.Add(Input.Position[i]);
                newTangent.Add(-Input.Tangent[i]);
                newNormal.Add(-Input.Normal[i]);
            }
            
            Result = new CurveData(Input.Type, Input.Points, Input.IsClosed, newPosition, newTangent, newNormal, new List<float3>(Input.Binormal));
        }
    }
}