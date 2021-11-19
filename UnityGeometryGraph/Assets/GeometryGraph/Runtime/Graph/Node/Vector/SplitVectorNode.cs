using GeometryGraph.Runtime.Attributes;
using JetBrains.Annotations;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class SplitVectorNode {
        [In(GenerateEquality = false)] public float3 Vector { get; private set; }
        [Out] public float X { get; private set; }
        [Out] public float Y { get; private set; }
        [Out] public float Z { get; private set; }
        
        [GetterMethod(nameof(X), Inline = true), UsedImplicitly] private float GetX() => Vector.x;
        [GetterMethod(nameof(Y), Inline = true), UsedImplicitly] private float GetY() => Vector.y;
        [GetterMethod(nameof(Z), Inline = true), UsedImplicitly] private float GetZ() => Vector.z;
    }
}