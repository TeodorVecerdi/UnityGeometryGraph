using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Vector", "Split")]
    public class SplitVectorNode : AbstractNode<GeometryGraph.Runtime.Graph.SplitVectorNode> {
        protected override string Title => "Split";
        protected override NodeCategory Category => NodeCategory.Vector;

        private GraphFrameworkPort vectorPort;
        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort zPort;

        private Vector3Field vectorField;

        private float3 vector;

        protected override void CreateNode() {
            (vectorPort, vectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Vector", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateVector(vector));
            xPort = GraphFrameworkPort.Create("X", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            yPort = GraphFrameworkPort.Create("Y", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            zPort = GraphFrameworkPort.Create("Z", Direction.Output, Port.Capacity.Multi, PortType.Float, this);

            vectorField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector value");
                vector = evt.newValue;
                RuntimeNode.UpdateVector(vector);
            });

            
            AddPort(vectorPort);
            inputContainer.Add(vectorField);
            AddPort(xPort);
            AddPort(yPort);
            AddPort(zPort);
            
            Refresh();
        }
        
        protected override void BindPorts() {
            BindPort(vectorPort, RuntimeNode.VectorPort);
            BindPort(xPort, RuntimeNode.XPort);
            BindPort(yPort, RuntimeNode.YPort);
            BindPort(zPort, RuntimeNode.ZPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            root["v"] = JsonConvert.SerializeObject(vector, float3Converter.Converter);
            
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            vector = JsonConvert.DeserializeObject<float3>(data.Value<string>("v")!, float3Converter.Converter);
            
            vectorField.SetValueWithoutNotify(vector);
            RuntimeNode.UpdateVector(vector);

            base.Deserialize(data);
        }
    }
}