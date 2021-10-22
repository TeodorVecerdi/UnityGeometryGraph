using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Input", "Vector Value")]
    public class VectorValueNode : AbstractNode<GeometryGraph.Runtime.Graph.VectorValueNode> {
        private float3 value;
        private Vector3Field valueField;
        private GraphFrameworkPort valuePort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Vector Value");

            valuePort = GraphFrameworkPort.Create("Value", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Vector, edgeConnectorListener, this);
            valueField = new Vector3Field("Value");
            valueField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Changed vector value");
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
            var root =  base.GetNodeData();
            root["v"] = JsonConvert.SerializeObject(value, Formatting.None, float3Converter.Converter);
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            value = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("v")!, float3Converter.Converter);
            valueField.SetValueWithoutNotify(value);
            RuntimeNode.UpdateValue(value);
            
            base.SetNodeData(jsonData);
        }
    }
}