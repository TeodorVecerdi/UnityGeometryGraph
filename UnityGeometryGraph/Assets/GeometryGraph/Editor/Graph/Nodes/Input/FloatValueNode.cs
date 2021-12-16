using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Input", "Float Value")]
    public class FloatValueNode : AbstractNode<GeometryGraph.Runtime.Graph.FloatValueNode> {
        protected override string Title => "Float Value";
        protected override NodeCategory Category => NodeCategory.Input;

        private float value;
        private FloatField valueField;
        private GraphFrameworkPort valuePort;

        protected override void CreateNode() {
            valuePort = GraphFrameworkPort.Create("Value", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            valueField = new FloatField("Value");
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
            BindPort(valuePort, RuntimeNode.ValuePort);
        }

        protected internal override JObject GetNodeData() {
            JObject root =  base.GetNodeData();
            root["v"] = value;
            return root;
        }

        protected internal override void SetNodeData(JObject jsonData) {
            value = jsonData.Value<float>("v");
            valueField.SetValueWithoutNotify(value);
            RuntimeNode.UpdateValue(value);
            
            base.SetNodeData(jsonData);
        }
    }
}