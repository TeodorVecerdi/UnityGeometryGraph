using System;
using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode(OutputPath = "_Generated")]
    public partial class RotateVectorNode {
        [In] public float3 Vector { get; private set; }
        [In] public float3 Center { get; private set; }
        [In] public float3 Axis { get; private set; }
        [In] public float3 EulerAngles { get; private set; }
        [In] public float Angle { get; private set; }
        [Setting] public RotateVectorNode_Mode Mode { get; private set; }
        [Out] public float3 Result { get; private set; }


        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private float3 GetResult() {
            return Mode switch {
                RotateVectorNode_Mode.AxisAngle => math.rotate(quaternion.AxisAngle(Axis, Angle), Vector - Center) + Center,
                RotateVectorNode_Mode.Euler => math.rotate(quaternion.Euler(EulerAngles), Vector - Center) + Center,
                RotateVectorNode_Mode.X_Axis => math.rotate(quaternion.AxisAngle(float3_ext.right, Angle), Vector - Center) + Center,
                RotateVectorNode_Mode.Y_Axis => math.rotate(quaternion.AxisAngle(float3_ext.up, Angle), Vector - Center) + Center,
                RotateVectorNode_Mode.Z_Axis => math.rotate(quaternion.AxisAngle(float3_ext.forward, Angle), Vector - Center) + Center,
                _ => throw new ArgumentOutOfRangeException(nameof(Mode), Mode, null)
            };
        }
        
        public enum RotateVectorNode_Mode {AxisAngle = 0, Euler = 1, X_Axis = 2, Y_Axis = 3, Z_Axis = 4}
    }
}