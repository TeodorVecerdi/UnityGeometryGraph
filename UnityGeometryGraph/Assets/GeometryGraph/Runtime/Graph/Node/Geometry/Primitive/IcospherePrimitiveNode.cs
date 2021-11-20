using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using JetBrains.Annotations;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [AdditionalUsingStatements("UnityCommons")]
    [GenerateRuntimeNode]
    public partial class IcospherePrimitiveNode {
        [AdditionalValueChangedCode("{other} = {other}.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public float Radius { get; private set; } = 1.0f;
        
        [AdditionalValueChangedCode("GeometryDirty = true", Where = AdditionalValueChangedCodeAttribute.Location.AfterUpdate)]
        [AdditionalValueChangedCode("{other} = {other}.Clamped(0, Constants.MAX_ICOSPHERE_SUBDIVISIONS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] 
        public int Subdivisions { get; private set; } = 2;

        [CustomSerialization("{self} ? 1 : 0", "{self} = {storage}.Value<int>({index}) == 1 || Result == null")]
        [Setting(UpdatedFromEditorNode = false)] 
        public bool GeometryDirty { get; private set; } = true;
        
        [Out] 
        public GeometryData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true), UsedImplicitly]
        private GeometryData GetResult() {
            if (Result == null) CalculateResult();
            return Result;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            DebugUtility.Log($"Calculating result with Radius:`{Radius}` Subdiv:`{Subdivisions}`");

            if (GeometryDirty || Result == null) {
                DebugUtility.Log("Regenerated geometry");
                // Recalculate new geometry
                Result = GeometryPrimitive.Icosphere(Radius, Subdivisions);
                GeometryDirty = false;
            } else {
                DebugUtility.Log("Recalculated radius on existing geometry");
                // Re-project on sphere with new radius
                var positionAttribute = Result.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
                positionAttribute!.Yield(pos => math.normalize(pos) * Radius).Into(positionAttribute);
                Result.StoreAttribute(positionAttribute);
            }
        }
    }
}