using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Curve.TEMP {
    public class AlignCurveTest : SerializedMonoBehaviour {
        public CurveVisualizer Visualizer;
        public ICurveProvider Target;
        public ICurveProvider Source;
        public bool AutoUpdate;
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public static double Duration;

        public CurveData Aligned;
        [OnValueChanged(nameof(Align))] public float RotationOffset;
        [OnValueChanged(nameof(IndexChanged))] public int Index = 0;

        private void IndexChanged() {
            if (Index >= Target.Curve.Points) Index = 0;
            if (Index < 0) Index = Target.Curve.Points - 1;
            Align();
        }

        private void OnDrawGizmosSelected() {
            if (AutoUpdate) Align();
            Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
        }

        [Button]
        private void Align() {
            if (Target?.Curve == null || Source?.Curve == null) return;
            Aligned = AlignCurve(Target.Curve, Source.Curve, Index, RotationOffset);
            if (Visualizer != null) {
                Visualizer.Load(Aligned);
            }
        }

        private static CurveData AlignCurve(CurveData alignOn, CurveData toAlign, int index, float rotationOffset) {
            var rotation = float4x4.RotateY(math.radians(rotationOffset));
            var align = new float4x4(alignOn.Normal[index].float4(), alignOn.Tangent[index].float4(), alignOn.Binormal[index].float4(), alignOn.Position[index].float4(1.0f));
            var matrix = math.mul(align, rotation);

            var position = new List<float3>();
            var tangent = new List<float3>();
            var normal = new List<float3>();
            var binormal = new List<float3>();

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < toAlign.Points; i++) {
                position.Add(math.mul(matrix, toAlign.Position[i].float4(1.0f)).xyz);
                tangent.Add(math.mul(matrix, toAlign.Tangent[i].float4()).xyz);
                normal.Add(math.mul(matrix, toAlign.Normal[i].float4()).xyz);
                binormal.Add(math.mul(matrix, toAlign.Binormal[i].float4()).xyz);
            }

            Duration = sw.Elapsed.TotalMilliseconds;
            return new CurveData(toAlign.Type, toAlign.Points, toAlign.IsClosed, position, tangent, normal, binormal);
        }
    }
}