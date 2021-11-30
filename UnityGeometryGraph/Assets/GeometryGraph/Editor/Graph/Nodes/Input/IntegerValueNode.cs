using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Input", "Integer Value")]
    public class IntegerValueNode : AbstractNode<GeometryGraph.Runtime.Graph.IntegerValueNode> {
        private int value;
        private IntegerField valueField;
        private GraphFrameworkPort valuePort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Integer Value", NodeCategory.Input);

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
        
        public override void BindPorts() {
            BindPort(valuePort, RuntimeNode.ValuePort);
        }

        public override JObject GetNodeData() {
            JObject root =  base.GetNodeData();
            root["v"] = value;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            value = jsonData.Value<int>("v");
            valueField.SetValueWithoutNotify(value);
            RuntimeNode.UpdateValue(value);
            
            base.SetNodeData(jsonData);
        }
    }
}