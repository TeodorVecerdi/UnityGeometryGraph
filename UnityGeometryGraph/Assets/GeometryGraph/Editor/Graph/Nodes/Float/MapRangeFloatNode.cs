using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Float", "Map Range")]
    public class MapRangeFloatNode : AbstractNode<GeometryGraph.Runtime.Graph.MapRangeFloatNode> {
        
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort fromMinPort;
        private GraphFrameworkPort fromMaxPort;
        private GraphFrameworkPort toMinPort;
        private GraphFrameworkPort toMaxPort;
        private GraphFrameworkPort resultPort;

        private Toggle clampField;
        private FloatField inputField;
        private FloatField fromMinField;
        private FloatField toMinField;
        private FloatField fromMaxField;
        private FloatField toMaxField;

        private bool clamp;
        private float inputValue;
        private float fromMin = 0.0f;
        private float fromMax = 1.0f;
        private float toMin = 0.0f;
        private float toMax = 1.0f;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Map Range");

            (inputPort, inputField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Input", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(inputValue));
            (fromMinPort, fromMinField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("From Min", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateFromMin(fromMin));
            (fromMaxPort, fromMaxField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("From Max", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateFromMax(fromMax));
            (toMinPort, toMinField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("To Min", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateToMin(toMin));
            (toMaxPort, toMaxField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("To Max", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateToMax(toMax));
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener, this);

            clampField = new Toggle("Clamp");
            clampField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change clamp");
                clamp = evt.newValue;
                RuntimeNode.UpdateClamp(clamp);
            });
            
            inputField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                inputValue = evt.newValue;
                RuntimeNode.UpdateValue(inputValue);
            });
            
            fromMinField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                fromMin = evt.newValue;
                RuntimeNode.UpdateFromMin(fromMin);
            });
            fromMaxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                fromMax = evt.newValue;
                RuntimeNode.UpdateFromMax(fromMax);
            });
            toMinField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                toMin = evt.newValue;
                RuntimeNode.UpdateToMin(toMin);
            });
            toMaxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                toMax = evt.newValue;
                RuntimeNode.UpdateToMax(toMax);
            });
            
            fromMinField.SetValueWithoutNotify(0.0f);
            fromMaxField.SetValueWithoutNotify(1.0f);
            toMinField.SetValueWithoutNotify(0.0f);
            toMaxField.SetValueWithoutNotify(1.0f);

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
            BindPort(inputPort, RuntimeNode.ValuePort);
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
            inputValue = jsonData.Value<float>("i");
            fromMin = jsonData.Value<float>("f");
            fromMax = jsonData.Value<float>("F");
            toMin = jsonData.Value<float>("t");
            toMax = jsonData.Value<float>("T");
            
            inputField.SetValueWithoutNotify(inputValue);
            fromMinField.SetValueWithoutNotify(fromMin);
            fromMaxField.SetValueWithoutNotify(fromMax);
            toMinField.SetValueWithoutNotify(toMin);
            toMaxField.SetValueWithoutNotify(toMax);
            
            RuntimeNode.UpdateClamp(clamp);
            RuntimeNode.UpdateValue(inputValue);
            RuntimeNode.UpdateFromMin(fromMin);
            RuntimeNode.UpdateFromMax(fromMax);
            RuntimeNode.UpdateToMin(toMin);
            RuntimeNode.UpdateToMax(toMax);

            base.SetNodeData(jsonData);
        }
    }
}