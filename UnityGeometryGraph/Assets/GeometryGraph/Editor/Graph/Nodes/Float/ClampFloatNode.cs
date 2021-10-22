using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.ClampFloatNode.ClampFloatNode_Which;

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
            Initialize("Clamp");

            (inputPort, inputField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Input", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(inputValue, Which.Input));
            (minPort, minField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Min", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(minValue, Which.Min));
            (maxPort, maxField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Max", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(maxValue, Which.Max));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener, this);

            inputField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                inputValue = evt.newValue;
                RuntimeNode.UpdateValue(inputValue, Which.Input);
            });
            
            minField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                minValue = evt.newValue;
                RuntimeNode.UpdateValue(minValue, Which.Min);
            });
            
            maxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                maxValue = evt.newValue;
                RuntimeNode.UpdateValue(maxValue, Which.Max);
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
            var root = base.GetNodeData();

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
            
            RuntimeNode.UpdateValue(inputValue, Which.Input);
            RuntimeNode.UpdateValue(minValue, Which.Min);
            RuntimeNode.UpdateValue(maxValue, Which.Max);

            base.SetNodeData(jsonData);
        }
    }
}