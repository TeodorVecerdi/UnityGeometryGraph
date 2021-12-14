using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class GetPositionNode {
        [In] public GeometryData Geometry { get; private set; }
        [Out] public float3 Position { get; private set; }
        
        private readonly List<float3> results = new List<float3>();
        private bool resultsDirty = true;
        
        [CalculatesAllProperties] private void MarkResultsDirty() => resultsDirty = true;

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port == GeometryPort) {
                Geometry = GeometryData.Empty;
                Position = float3.zero;
                NotifyPortValueChanged(GeometryPort);
            }
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (port != PositionPort || count <= 0) yield break;
            if (!resultsDirty && results.Count == count) {
                for (int i = 0; i < count; i++) {
                    yield return results[i];
                }
                yield break;
            }

            resultsDirty = false;
            results.Clear();
            
            if (Geometry == null || Geometry.SubmeshCount == 0) {
                for (int i = 0; i < count; i++) {
                    results.Add(float3.zero);
                    yield return float3.zero;
                }
                yield break;
            }
            
            Vector3Attribute positionAttribute = Geometry.GetAttribute<Vector3Attribute>(AttributeId.Position, AttributeDomain.Vertex);
            for (int i = 0; i < positionAttribute!.Count; i++) {
                results.Add(positionAttribute[i]);
                yield return positionAttribute[i];
            }
            if (positionAttribute.Count < count) {
                for (int i = positionAttribute.Count; i < count; i++) {
                    results.Add(float3.zero);
                    yield return float3.zero;
                }
            }
        }
    }
}