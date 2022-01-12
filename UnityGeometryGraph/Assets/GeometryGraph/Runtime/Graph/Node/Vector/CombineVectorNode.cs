using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class CombineVectorNode {
        [In] public float X { get; private set; }
        [In] public float Y { get; private set; }
        [In] public float Z { get; private set; }
        [Out] public float3 Vector { get; private set; }

        private readonly List<float3> results = new();
        private bool resultsDirty = true;

        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;

        [CalculatesProperty(nameof(Vector))]
        private void CalculateVector() {
            Vector = new float3(X, Y, Z);
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != VectorPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }

                yield break;
            }

            List<float> xs = GetValues(XPort, count, X).ToList();
            List<float> ys = GetValues(YPort, count, Y).ToList();
            List<float> zs = GetValues(ZPort, count, Z).ToList();
            results.Clear();

            for (int i = 0; i < count; i++) {
                float3 result = new(xs[i], ys[i], zs[i]);
                results.Add(result);
                yield return result;
            }

            resultsDirty = false;
        }
    }
}