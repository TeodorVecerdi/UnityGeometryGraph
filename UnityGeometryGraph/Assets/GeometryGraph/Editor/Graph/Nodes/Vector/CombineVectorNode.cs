using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Vector", "Combine")]
    public class CombineVectorNode : AbstractNode<GeometryGraph.Runtime.Graph.CombineVectorNode> {
        protected override string Title => "Combine";
        protected override NodeCategory Category => NodeCategory.Vector;

        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort zPort;
        private GraphFrameworkPort vectorPort;

        private FloatField xField;
        private FloatField yField;
        private FloatField zField;

        private float3 vector;

        protected override void CreateNode() {
            (xPort, xField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("X", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateX(vector.x));
            (yPort, yField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Y", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateY(vector.y));
            (zPort, zField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Z", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateZ(vector.z));
            vectorPort = GraphFrameworkPort.Create("Vector", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);

            xField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector x component");
                vector.x = evt.newValue;
                RuntimeNode.UpdateX(vector.x);
            });
            
            yField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector y component");
                vector.y = evt.newValue;
                RuntimeNode.UpdateY(vector.y);
            });
            
            zField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector z component");
                vector.z = evt.newValue;
                RuntimeNode.UpdateZ(vector.z);
            });

            xPort.Add(xField);
            yPort.Add(yField);
            zPort.Add(zField);
            
            AddPort(xPort);
            AddPort(yPort);
            AddPort(zPort);
            AddPort(vectorPort);
            
            Refresh();
        }

        protected override void BindPorts() {
            BindPort(xPort, RuntimeNode.XPort);
            BindPort(yPort, RuntimeNode.YPort);
            BindPort(zPort, RuntimeNode.ZPort);
            BindPort(vectorPort, RuntimeNode.VectorPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            root["v"] = JsonConvert.SerializeObject(vector, float3Converter.Converter);
            
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            vector = JsonConvert.DeserializeObject<float3>(data.Value<string>("v"), float3Converter.Converter);
            
            xField.SetValueWithoutNotify(vector.x);
            yField.SetValueWithoutNotify(vector.y);
            zField.SetValueWithoutNotify(vector.z);
            RuntimeNode.UpdateX(vector.x);
            RuntimeNode.UpdateY(vector.y);
            RuntimeNode.UpdateZ(vector.z);

            base.Deserialize(data);
        }
    }
}