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
        protected override string Title => "Vector Value";
        protected override NodeCategory Category => NodeCategory.Input;

        private float3 value;
        private Vector3Field valueField;
        private GraphFrameworkPort valuePort;

        protected override void CreateNode() {
            valuePort = GraphFrameworkPort.Create("Value", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
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
        
        protected override void BindPorts() {
            BindPort(valuePort, RuntimeNode.ValuePort);
        }

        protected internal override JObject Serialize() {
            JObject root =  base.Serialize();
            root["v"] = JsonConvert.SerializeObject(value, Formatting.None, float3Converter.Converter);
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            value = JsonConvert.DeserializeObject<float3>(data.Value<string>("v")!, float3Converter.Converter);
            valueField.SetValueWithoutNotify(value);
            RuntimeNode.UpdateValue(value);
            
            base.Deserialize(data);
        }
    }
}