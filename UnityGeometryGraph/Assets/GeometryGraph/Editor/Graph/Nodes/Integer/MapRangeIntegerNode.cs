using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.MapRangeIntegerNode.MapRangeIntegerNode_Which;

namespace GeometryGraph.Editor {
    [Title("Integer", "Map Range")]
    public class MapRangeIntegerNode : AbstractNode<GeometryGraph.Runtime.Graph.MapRangeIntegerNode> {
        
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort fromMinPort;
        private GraphFrameworkPort fromMaxPort;
        private GraphFrameworkPort toMinPort;
        private GraphFrameworkPort toMaxPort;
        private GraphFrameworkPort resultPort;

        private Toggle clampField;
        private IntegerField inputField;
        private IntegerField fromMinField;
        private IntegerField toMinField;
        private IntegerField fromMaxField;
        private IntegerField toMaxField;

        private bool clamp;
        private int inputValue;
        private int fromMin = 0;
        private int fromMax = 1;
        private int toMin = 0;
        private int toMax = 1;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Map Range");

            (inputPort, inputField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Input", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(inputValue, Which.Input));
            (fromMinPort, fromMinField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("From Min", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(fromMin, Which.FromMin));
            (fromMaxPort, fromMaxField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("From Max", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(fromMax, Which.FromMax));
            (toMinPort, toMinField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("To Min", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(toMin, Which.ToMin));
            (toMaxPort, toMaxField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("To Max", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(toMax, Which.ToMax));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Integer, edgeConnectorListener, this);

            clampField = new Toggle("Clamp");
            clampField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change clamp");
                clamp = evt.newValue;
                RuntimeNode.UpdateClamped(clamp);
            });
            
            inputField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                inputValue = evt.newValue;
                RuntimeNode.UpdateValue(inputValue, Which.Input);
            });
            
            fromMinField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                fromMin = evt.newValue;
                RuntimeNode.UpdateValue(fromMin, Which.FromMin);
            });
            fromMaxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                fromMax = evt.newValue;
                RuntimeNode.UpdateValue(fromMax, Which.FromMax);
            });
            toMinField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                toMin = evt.newValue;
                RuntimeNode.UpdateValue(toMin, Which.ToMin);
            });
            toMaxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                toMax = evt.newValue;
                RuntimeNode.UpdateValue(toMax, Which.ToMax);
            });
            
            fromMinField.SetValueWithoutNotify(0);
            fromMaxField.SetValueWithoutNotify(1);
            toMinField.SetValueWithoutNotify(0);
            toMaxField.SetValueWithoutNotify(1);

            inputPort.Add(inputField);
            fromMinPort.Add(fromMinField);
            fromMaxPort.Add(fromMaxField);
            toMinPort.Add(toMinField);
            toMaxPort.Add(toMaxField);

            inputContainer.Add(clampField);
            AddPort(inputPort);
            AddPort(fromMinPort);
            AddPort(fromMaxPort);
            AddPort(toMinPort);
            AddPort(toMaxPort);
            AddPort(resultPort);
            
            Refresh();
        }
        
        public override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(fromMinPort, RuntimeNode.FromMinPort);
            BindPort(fromMaxPort, RuntimeNode.FromMaxPort);
            BindPort(toMinPort, RuntimeNode.ToMinPort);
            BindPort(toMaxPort, RuntimeNode.ToMaxPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["c"] = clamp ? 1 : 0;
            root["i"] = inputValue;
            root["f"] = fromMin;
            root["F"] = fromMax;
            root["t"] = toMin;
            root["T"] = toMax;
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            clamp = jsonData.Value<int>("c") == 1;
            inputValue = jsonData.Value<int>("i");
            fromMin = jsonData.Value<int>("f");
            fromMax = jsonData.Value<int>("F");
            toMin = jsonData.Value<int>("t");
            toMax = jsonData.Value<int>("T");
            
            inputField.SetValueWithoutNotify(inputValue);
            fromMinField.SetValueWithoutNotify(fromMin);
            fromMaxField.SetValueWithoutNotify(fromMax);
            toMinField.SetValueWithoutNotify(toMin);
            toMaxField.SetValueWithoutNotify(toMax);
            
            RuntimeNode.UpdateClamped(clamp);
            RuntimeNode.UpdateValue(inputValue, Which.Input);
            RuntimeNode.UpdateValue(fromMin, Which.FromMin);
            RuntimeNode.UpdateValue(fromMax, Which.FromMax);
            RuntimeNode.UpdateValue(toMin, Which.ToMin);
            RuntimeNode.UpdateValue(toMax, Which.ToMax);

            base.SetNodeData(jsonData);
        }
    }
}