using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.ClampIntegerNode.ClampIntegerNode_Which;

namespace GeometryGraph.Editor {
    [Title("Integer", "Clamp")]
    public class ClampIntegerNode : AbstractNode<GeometryGraph.Runtime.Graph.ClampIntegerNode> {
        
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort minPort;
        private GraphFrameworkPort maxPort;
        private GraphFrameworkPort resultPort;

        private IntegerField inputField;
        private IntegerField minField;
        private IntegerField maxField;

        private int inputValue;
        private int minValue = 0;
        private int maxValue = 1;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Clamp", EditorView.DefaultNodePosition);

            (inputPort, inputField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Input", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this);
            (minPort, minField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Min", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this);
            (maxPort, maxField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Max", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this);
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Integer, edgeConnectorListener, this);

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
            
            minField.SetValueWithoutNotify(0);
            maxField.SetValueWithoutNotify(1);

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
            inputValue = jsonData.Value<int>("i");
            minValue = jsonData.Value<int>("m");
            maxValue = jsonData.Value<int>("M");
            
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