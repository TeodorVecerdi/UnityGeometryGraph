using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using Which = GeometryGraph.Runtime.Graph.CombineVectorNode.CombineVectorNode_Which;

namespace GeometryGraph.Editor {
    [Title("Vector", "Combine")]
    public class CombineVectorNode : AbstractNode<GeometryGraph.Runtime.Graph.CombineVectorNode> {
        private GraphFrameworkPort xPort;
        private GraphFrameworkPort yPort;
        private GraphFrameworkPort zPort;
        private GraphFrameworkPort vectorPort;

        private FloatField xField;
        private FloatField yField;
        private FloatField zField;

        private float3 vector;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Combine", EditorView.DefaultNodePosition);

            (xPort, xField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("X", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this);
            (yPort, yField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Y", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this);
            (zPort, zField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Z", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this);
            vectorPort = GraphFrameworkPort.Create("Vector", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Vector, edgeConnectorListener, this);

            xField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                vector.x = evt.newValue;
                RuntimeNode.UpdateValue(vector.x, Which.X);
            });
            
            yField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                vector.y = evt.newValue;
                RuntimeNode.UpdateValue(vector.y, Which.Y);
            });
            
            zField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                vector.z = evt.newValue;
                RuntimeNode.UpdateValue(vector.z, Which.Z);
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
        
        public override void BindPorts() {
            BindPort(xPort, RuntimeNode.XPort);
            BindPort(yPort, RuntimeNode.YPort);
            BindPort(zPort, RuntimeNode.ZPort);
            BindPort(vectorPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();
            root["v"] = JsonConvert.SerializeObject(vector, float3Converter.Converter);
            
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            vector = JsonConvert.DeserializeObject<float3>(jsonData.Value<string>("v"), float3Converter.Converter);
            
            xField.SetValueWithoutNotify(vector.x);
            yField.SetValueWithoutNotify(vector.y);
            zField.SetValueWithoutNotify(vector.z);
            RuntimeNode.UpdateValue(vector.x, Which.X);
            RuntimeNode.UpdateValue(vector.y, Which.Y);
            RuntimeNode.UpdateValue(vector.z, Which.Z);

            base.SetNodeData(jsonData);
        }
    }
}