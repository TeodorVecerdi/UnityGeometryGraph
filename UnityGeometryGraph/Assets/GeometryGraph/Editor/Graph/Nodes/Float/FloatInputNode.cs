using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Float", "Input")]
    public class FloatInputNode : AbstractNode {
        private float value;
        private GraphFrameworkPort valuePort;

        private FloatField field;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            
            Initialize("Float", EditorView.DefaultNodePosition);
            field = new FloatField("Value");
            field.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed Float Input Node value");
                value = evt.newValue;
                NotifyPortValueChanged(valuePort);
            });
            
            extensionContainer.Add(field);
            valuePort = GraphFrameworkPort.Create("Value", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Float, edgeConnectorListener);
            AddPort(valuePort, false);
            titleButtonContainer.Add(valuePort);
            
            RefreshExpandedState();
        }

        public override object GetValueForPort(GraphFrameworkPort port) {
            if (port == valuePort) return value;
            return null;
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();
            root["value"] = value;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            value = jsonData.Value<float>("value");
            field.SetValueWithoutNotify(value);
            NotifyPortValueChanged(valuePort);
            base.SetNodeData(jsonData);
        }
    }
}