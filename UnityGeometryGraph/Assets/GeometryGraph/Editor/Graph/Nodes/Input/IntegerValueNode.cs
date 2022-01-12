using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Input", "Integer Value")]
    public class IntegerValueNode : AbstractNode<GeometryGraph.Runtime.Graph.IntegerValueNode> {
        protected override string Title => "Integer Value";
        protected override NodeCategory Category => NodeCategory.Input;

        private int value;
        private IntegerField valueField;
        private GraphFrameworkPort valuePort;

        protected override void CreateNode() {
            valuePort = GraphFrameworkPort.Create("Value", Direction.Output, Port.Capacity.Multi, PortType.Integer, this);
            valueField = new IntegerField("Value");
            valueField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed integer value");
                value = evt.newValue;
                RuntimeNode.UpdateValue(value);
            });
            AddPort(valuePort);
            extensionContainer.Add(valueField);
            
            Refresh();
        }

        protected override void BindPorts() {
            BindPort(valuePort, RuntimeNode.ValuePort);
        }

        protected internal override JObject Serialize() {
            JObject root =  base.Serialize();
            root["v"] = value;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            value = data.Value<int>("v");
            valueField.SetValueWithoutNotify(value);
            RuntimeNode.UpdateValue(value);
            
            base.Deserialize(data);
        }
    }
}