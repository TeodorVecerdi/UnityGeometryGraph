using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Input", "String Value")]
    public class StringValueNode : AbstractNode<GeometryGraph.Runtime.Graph.StringValueNode> {
        protected override string Title => "String Value";
        protected override NodeCategory Category => NodeCategory.Input;

        private string value;
        private TextField valueField;
        private GraphFrameworkPort valuePort;

        protected override void CreateNode() {
            valuePort = GraphFrameworkPort.Create("Value", Direction.Output, Port.Capacity.Multi, PortType.String, this);
            valueField = new TextField("Value");
            valueField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed float value");
                value = evt.newValue;
                RuntimeNode.UpdateValue(value);
            });
            AddPort(valuePort);
            extensionContainer.Add(valueField);

            Refresh();
        }

        protected override void BindPorts() {
            BindPort(valuePort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root =  base.Serialize();
            root["v"] = value;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            value = data.Value<string>("v");
            valueField.SetValueWithoutNotify(value);
            RuntimeNode.UpdateValue(value);

            base.Deserialize(data);
        }
    }
}