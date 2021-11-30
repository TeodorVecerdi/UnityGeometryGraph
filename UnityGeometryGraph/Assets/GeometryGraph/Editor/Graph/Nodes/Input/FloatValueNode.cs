using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Input", "Float Value")]
    public class FloatValueNode : AbstractNode<GeometryGraph.Runtime.Graph.FloatValueNode> {
        private float value;
        private FloatField valueField;
        private GraphFrameworkPort valuePort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Float Value", NodeCategory.Input);

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
        
        public override void BindPorts() {
            BindPort(valuePort, RuntimeNode.ValuePort);
        }

        public override JObject GetNodeData() {
            JObject root =  base.GetNodeData();
            root["v"] = value;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            value = jsonData.Value<float>("v");
            valueField.SetValueWithoutNotify(value);
            RuntimeNode.UpdateValue(value);
            
            base.SetNodeData(jsonData);
        }
    }
}