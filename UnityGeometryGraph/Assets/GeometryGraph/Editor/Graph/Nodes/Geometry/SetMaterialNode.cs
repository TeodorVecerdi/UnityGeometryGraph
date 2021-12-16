using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Set Material")]
    public class SetMaterialNode : AbstractNode<GeometryGraph.Runtime.Graph.SetMaterialNode> {
        protected override string Title => "Set Material";
        protected override NodeCategory Category => NodeCategory.Geometry;

        private int materialIndex;
        
        private ClampedIntegerField materialIndexField;
        
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort materialIndexPort;
        private GraphFrameworkPort resultPort;

        protected override void CreateNode() {
            inputPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (materialIndexPort, materialIndexField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Material", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateMaterialIndex(materialIndex));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            materialIndexField.Min = 0;
            materialIndexField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.MinClamped(0);
                if (newValue == materialIndex) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Material Index");
                materialIndex = newValue;
                RuntimeNode.UpdateMaterialIndex(materialIndex);
            });
            
            materialIndexPort.Add(materialIndexField);
            
            AddPort(inputPort);
            AddPort(materialIndexPort);
            AddPort(resultPort);
            
            Refresh();
        }

        protected override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(materialIndexPort, RuntimeNode.MaterialIndexPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();
            root["0"] = materialIndex;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            materialIndex = jsonData.Value<int>("0");
            
            materialIndexField.SetValueWithoutNotify(materialIndex);
            
            RuntimeNode.UpdateMaterialIndex(materialIndex);
            
            base.SetNodeData(jsonData);
        }
    }
}