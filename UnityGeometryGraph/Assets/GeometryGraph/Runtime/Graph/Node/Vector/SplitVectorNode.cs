using System.Collections.Generic;
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

        private readonly List<float3> vectors = new List<float3>();
        private bool vectorsDirty = true;

        [CalculatesAllProperties] private void MarkVectorsDirty() => vectorsDirty = true;

        [GetterMethod(nameof(X), Inline = true)] private float GetX() => Vector.x;
        [GetterMethod(nameof(Y), Inline = true)] private float GetY() => Vector.y;
        [GetterMethod(nameof(Z), Inline = true)] private float GetZ() => Vector.z;

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (count <= 0) yield break;
            if (vectorsDirty || vectors.Count != count) {
                vectors.Clear();
                vectors.AddRange(GetValues(VectorPort, count, Vector));
                vectorsDirty = false;
            }

            for (int i = 0; i < count; i++) {
                if (port == XPort) yield return vectors[i].x;
                else if (port == YPort) yield return vectors[i].y;
                else if (port == ZPort) yield return vectors[i].z;
            }
        }
    }
}