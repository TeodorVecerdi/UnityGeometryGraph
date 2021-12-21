using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class RotateVectorNode {
        [In] public float3 Vector { get; private set; }
        [In] public float3 Center { get; private set; }
        [In] public float3 Axis { get; private set; }
        [In] public float3 EulerAngles { get; private set; }
        [In] public float Angle { get; private set; }
        [Setting] public RotateVectorNode_Mode Mode { get; private set; }
        [Out] public float3 Result { get; private set; }

        private readonly List<float3> results = new();
        private bool resultsDirty = true;
        
        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;

        [GetterMethod(nameof(Result), Inline = true)]
        private float3 GetResult() {
            return CalculateResult(Vector, Center, Axis, EulerAngles, Angle);
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != ResultPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }
            }

            resultsDirty = false;
            results.Clear();
            List<float3> vector = GetValues(VectorPort, count, Vector).ToList();
            List<float3> center = GetValues(CenterPort, count, Center).ToList();
            
            if (Mode == RotateVectorNode_Mode.Euler) {
                List<float3> eulerAngles = GetValues(EulerAnglesPort, count, EulerAngles).ToList();
                for (int i = 0; i < count; i++) {
                    float3 result = CalculateResult(vector[i], center[i], float3.zero, eulerAngles[i], 0);
                    results.Add(result);
                    yield return result;
                }
                
                yield break;
            }

            List<float> angle = GetValues(AnglePort, count, Angle).ToList();
            if (Mode == RotateVectorNode_Mode.AxisAngle) {
                List<float3> axis = GetValues(AxisPort, count, Axis).ToList();
                for (int i = 0; i < count; i++) {
                    float3 result = CalculateResult(vector[i], center[i], axis[i], float3.zero, angle[i]);
                    results.Add(result);
                    yield return result;
                }
                    
                yield break;
            }

            // X/Y/Z Axis
            for (int i = 0; i < count; i++) {
                float3 result = CalculateResult(vector[i], center[i], float3.zero, float3.zero, angle[i]);
                results.Add(result);
                yield return result;
            }
        }

        private float3 CalculateResult(float3 vector, float3 center, float3 axis, float3 eulerAngles, float angle) {
            return Mode switch {
                RotateVectorNode_Mode.AxisAngle => math.rotate(quaternion.AxisAngle(axis, angle), vector - center) + center,
                RotateVectorNode_Mode.Euler => math.rotate(quaternion.Euler(eulerAngles), vector - center) + center,
                RotateVectorNode_Mode.X_Axis => math.rotate(quaternion.AxisAngle(float3_ext.right, angle), vector - center) + center,
                RotateVectorNode_Mode.Y_Axis => math.rotate(quaternion.AxisAngle(float3_ext.up, angle), vector - center) + center,
                RotateVectorNode_Mode.Z_Axis => math.rotate(quaternion.AxisAngle(float3_ext.forward, angle), vector - center) + center,
                _ => throw new ArgumentOutOfRangeException(nameof(Mode), Mode, null)
            };
        }
        
        public enum RotateVectorNode_Mode {AxisAngle = 0, Euler = 1, X_Axis = 2, Y_Axis = 3, Z_Axis = 4}
    }
}