using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class TranslatePointNode {
        [In] public GeometryData Input { get; private set; }
        [In] public float3 Translation { get; private set; }
        [In] public string AttributeName { get; private set; }
        [Setting] public TranslatePointNode_Mode Mode { get; private set; }
        [Out] public GeometryData Result { get; private set; }
        

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != InputPort) return;
            Input = GeometryData.Empty;
            Result = GeometryData.Empty;
        }

        [CalculatesProperty(nameof(Result))]
        private void Calculate() {
            if (Input == null) return;
            Result = Input.Clone();
            var positionAttr = Result.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            if (Mode == TranslatePointNode_Mode.Vector) {
                positionAttr.Yield(position => position + Translation).Into(positionAttr);
                
                /*
                Storing even though I'm writing to the same attribute (with the .Into call) because AttributeManager
                makes no guarantee that it will return the original attribute and not a clone of the attribute.
                
                Cloning happens when there is a domain or type mismatch and the original attribute gets converted.
                
                Also, storing the same attribute twice has no side effects and it's safe. The StoreAttribute method 
                returns a boolean indicating whether an attribute was overwritten by the Store operation in case you 
                care about that.
                */
                Result.StoreAttribute(positionAttr);
            } else {
                if (!Result.HasAttribute(AttributeName)) {
                    Debug.LogWarning($"Couldn't find attribute [{AttributeName}]");
                    return;
                }
                
                var otherAttribute = Result.GetAttribute<Vector3Attribute>(AttributeName, AttributeDomain.Vertex);
                positionAttr.YieldWithAttribute(otherAttribute, (position, translation) => position + translation).Into(positionAttr);
                Result.StoreAttribute(positionAttr);
            }
        }

        public enum TranslatePointNode_Mode {Vector = 0, Attribute = 1}
    }
}