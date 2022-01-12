using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [AdditionalUsingStatements("UnityCommons")]
    public partial class SetMaterialNode {
        [In] public GeometryData Input { get; private set; }

        [AdditionalValueChangedCode("{other} = {other}.MinClamped(0);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        [In] public int MaterialIndex { get; private set; }

        [Out] public GeometryData Result { get; private set; }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port == InputPort) {
                Input = GeometryData.Empty;
                Result = GeometryData.Empty;
                NotifyPortValueChanged(ResultPort);
            }
        }

        [CalculatesProperty(nameof(Result))]
        private void Calculate() {
            if (Input == null) return;
            Result = Input.Clone();
            IEnumerable<int> materialIndices = GetValues(MaterialIndexPort, Input.Faces.Count, MaterialIndex);
            Result.StoreAttribute(materialIndices.Into<IntAttribute>(AttributeId.Material, AttributeDomain.Face));
        }
    }
}