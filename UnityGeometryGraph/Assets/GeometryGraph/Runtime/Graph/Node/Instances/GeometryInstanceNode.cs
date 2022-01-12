using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [AdditionalUsingStatements("System.Linq")]
    public partial class GeometryInstanceNode {
        [In(DefaultValue = "GeometryData.Empty")] public GeometryData Points { get; private set; }
        [In(DefaultValue = "GeometryData.Empty")] public GeometryData Geometry { get; private set; }
        [In(
            DefaultValue = "Enumerable.Empty<GeometryData>()",
            UpdateValueCode = "{self} = new List<GeometryData>({other})")
        ] public List<GeometryData> Collection { get; private set; } = new ();
        [In] public int CollectionSamplingSeed { get; private set; } = 0;

        [Setting] public GeometryInstanceNode_Mode Mode { get; private set; } = GeometryInstanceNode_Mode.Geometry;
        [Out] public InstancedGeometryData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true)]
        private InstancedGeometryData GetResult() {
            if (Result == null || Result.IsEmpty) CalculateResult();
            return Result;
        }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port == PointsPort
             || port == GeometryPort   && Mode == GeometryInstanceNode_Mode.Geometry
             || port == CollectionPort && Mode == GeometryInstanceNode_Mode.Collection)
            {
                Result = InstancedGeometryData.Empty;
            }

            if (port == GeometryPort) {
                Geometry = GeometryData.Empty;
            }

            if (port == CollectionPort) {
                Collection.Clear();
            }
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Points == null || Points.SubmeshCount == 0) {
                Result = InstancedGeometryData.Empty;
                return;
            }

            Vector3Attribute positionAttribute = Points.GetAttributeOrDefault<Vector3Attribute, float3>(AttributeId.Position, AttributeDomain.Vertex, float3.zero);
            Vector3Attribute rotationAttribute = Points.GetAttributeOrDefault<Vector3Attribute, float3>("rotation", AttributeDomain.Vertex, float3.zero);
            Vector3Attribute scaleAttribute = Points.GetAttributeOrDefault<Vector3Attribute, float3>("scale", AttributeDomain.Vertex, float3_ext.one);

            if (Mode == GeometryInstanceNode_Mode.Geometry) {
                if (Geometry == null || Geometry.SubmeshCount == 0) {
                    Result = InstancedGeometryData.Empty;
                    return;
                }

                List<InstancedTransformData> transforms = positionAttribute
                                                          .Zip(rotationAttribute, (pos, rot) => (pos, rot))
                                                          .Zip(scaleAttribute, (tuple, scale) => new InstancedTransformData(tuple.pos, tuple.rot, scale))
                                                          .ToList();
                Result = new InstancedGeometryData(Geometry, transforms);
            } else if (Mode == GeometryInstanceNode_Mode.Collection) {
                if (Collection == null || Collection.Count == 0) {
                    Result = InstancedGeometryData.Empty;
                    return;
                }
                Dictionary<int, int> instanceIndices = new();
                List<GeometryData> instances = new();
                List<List<InstancedTransformData>> transforms = new();

                Rand.PushState(CollectionSamplingSeed);
                for (int i = 0; i < positionAttribute.Count; i++) {
                    int index = Rand.Range(0, Collection.Count);
                    if (!instanceIndices.ContainsKey(index)) {
                        instanceIndices.Add(index, instances.Count);
                        instances.Add(Collection[index]);
                        transforms.Add(new List<InstancedTransformData>());
                    }

                    transforms[instanceIndices[index]].Add(new InstancedTransformData(positionAttribute[i], rotationAttribute[i], scaleAttribute[i]));
                }
                Rand.PopState();

                Result = new InstancedGeometryData(instances, transforms);
            }
        }

        public enum GeometryInstanceNode_Mode { Geometry = 0, Collection }
    }
}