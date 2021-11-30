using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Float", "Clamp")]
    public class ClampFloatNode : AbstractNode<GeometryGraph.Runtime.Graph.ClampFloatNode> {
        
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort minPort;
        private GraphFrameworkPort maxPort;
        private GraphFrameworkPort resultPort;

        private FloatField inputField;
        private FloatField minField;
        private FloatField maxField;

        private float inputValue;
        private float minValue = 0.0f;
        private float maxValue = 1.0f;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Clamp (Float)", NodeCategory.Float);

            (inputPort, inputField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Input", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateInput(inputValue));
            (minPort, minField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Min", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateMin(minValue));
            (maxPort, maxField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Max", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateMax(maxValue));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Float, this);

            inputField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                inputValue = evt.newValue;
                RuntimeNode.UpdateInput(inputValue);
            });
            
            minField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                minValue = evt.newValue;
                RuntimeNode.UpdateMin(minValue);
            });
            
            maxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                maxValue = evt.newValue;
                RuntimeNode.UpdateMax(maxValue);
            });
            
            minField.SetValueWithoutNotify(0.0f);
            maxField.SetValueWithoutNotify(1.0f);

            inputPort.Add(inputField);
            minPort.Add(minField);
            maxPort.Add(maxField);
            
            AddPort(inputPort);
            AddPort(minPort);
            AddPort(maxPort);
            AddPort(resultPort);
            
            Refresh();
        }
        
        public override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(minPort, RuntimeNode.MinPort);
            BindPort(maxPort, RuntimeNode.MaxPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();

            root["i"] = inputValue;
            root["m"] = minValue;
            root["M"] = maxValue;
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            inputValue = jsonData.Value<float>("i");
            minValue = jsonData.Value<float>("m");
            maxValue = jsonData.Value<float>("M");
            
            inputField.SetValueWithoutNotify(inputValue);
            minField.SetValueWithoutNotify(minValue);
            maxField.SetValueWithoutNotify(maxValue);
            
            RuntimeNode.UpdateInput(inputValue);
            RuntimeNode.UpdateMin(minValue);
            RuntimeNode.UpdateMax(maxValue);

            base.SetNodeData(jsonData);
        }
    }
}