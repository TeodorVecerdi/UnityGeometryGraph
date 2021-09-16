using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor.Vector {
    [Title("Vector", "Split XYZ")]
    public class SplitVectorNode : AbstractNode{
        private Vector3 input;
        private float x;
        private float y;
        private float z;
        
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort zPort;

        private Vector3Field inputField;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Split XYZ", EditorView.DefaultNodePosition);

            (inputPort, inputField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>(
                "In", Orientation.Horizontal, PortType.Vector, edgeConnectorListener, showLabelOnField: false, onDisconnect: (edge, port) => {
                    input = inputField.value;
                    NotifyPortValueChanged(xPort);
                    NotifyPortValueChanged(yPort);
                    NotifyPortValueChanged(zPort);
                });
            xPort = GraphFrameworkPort.Create("x", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener);
            yPort = GraphFrameworkPort.Create("y", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener);
            zPort = GraphFrameworkPort.Create("z", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener);

            inputField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Vector Math input (A)");
                input = evt.newValue;
                NotifyPortValueChanged(xPort);
                NotifyPortValueChanged(yPort);
                NotifyPortValueChanged(zPort);
            });
            
            AddPort(inputPort);
            inputContainer.Add(inputField);
            AddPort(xPort);
            AddPort(yPort);
            AddPort(zPort);
            
            RefreshExpandedState();
        }

        protected override void OnPortValueChanged(Edge edge, GraphFrameworkPort port) {
            if (port != inputPort) return;

            var newInput = GetValueFromEdge(edge, input);
            if (input == newInput) return;

            input = newInput;
            
            NotifyPortValueChanged(xPort);
            NotifyPortValueChanged(yPort);
            NotifyPortValueChanged(zPort);
        }

        public override object GetValueForPort(GraphFrameworkPort port) {
            if (port == xPort) return input.x;
            if (port == yPort) return input.y;
            if (port == zPort) return input.z;
            return null;
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();
            root["i"] = JsonConvert.SerializeObject(input, Formatting.None);
            root["if"] =JsonConvert.SerializeObject(inputField.enabledSelf ? input : inputField.value, Formatting.None);
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            input = JsonConvert.DeserializeObject<Vector3>(jsonData.Value<string>("i"));
            inputField.SetValueWithoutNotify(JsonConvert.DeserializeObject<Vector3>(jsonData.Value<string>("if")));
            
            base.SetNodeData(jsonData);
        }
    }
}