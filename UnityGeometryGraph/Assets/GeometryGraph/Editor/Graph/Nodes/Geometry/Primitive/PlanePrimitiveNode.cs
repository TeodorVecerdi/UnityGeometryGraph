using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.PlanePrimitiveNode.PlanePrimitiveNode_Which;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Plane")]
    public class PlanePrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.PlanePrimitiveNode> {
        private GraphFrameworkPort widthPort;
        private GraphFrameworkPort heightPort;
        private GraphFrameworkPort subdivisionsPort;
        private GraphFrameworkPort resultPort;

        private FloatField widthField;
        private FloatField heightField;
        private IntegerField subdivisionsField;

        private float2 size = float2_util.one;
        private int subdivisions;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Plane Primitive", EditorView.DefaultNodePosition);

            (widthPort, widthField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Width", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(size.x, Which.Width));
            (heightPort, heightField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Height", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(size.y, Which.Height));
            (subdivisionsPort, subdivisionsField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Subdivisions", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(subdivisions, Which.Subdivisions));
            resultPort = GraphFrameworkPort.Create("Plane", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            widthField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                if (evt.newValue < 0.0f) {
                    size.x = 0.0f;
                    widthField.SetValueWithoutNotify(0.0f);
                } else size.x = evt.newValue;

                RuntimeNode.UpdateValue(size.x, Which.Width);
            });
            
            heightField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                if (evt.newValue < 0.0f) {
                    size.y = 0.0f;
                    heightField.SetValueWithoutNotify(0.0f);
                } else size.y = evt.newValue;

                RuntimeNode.UpdateValue(size.y, Which.Height);
            });

            subdivisionsField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                subdivisions = evt.newValue < 0 ? 0 : evt.newValue;

                RuntimeNode.UpdateValue(subdivisions, Which.Subdivisions);
            });
            
            subdivisionsField.RegisterCallback<BlurEvent>(_ => {
                if (subdivisionsField.value >= 0) return;
                subdivisionsField.SetValueWithoutNotify(0);
            });

            widthField.SetValueWithoutNotify(1.0f);
            heightField.SetValueWithoutNotify(1.0f);
            subdivisionsField.SetValueWithoutNotify(0);

            widthPort.Add(widthField);
            heightPort.Add(heightField);
            subdivisionsPort.Add(subdivisionsField);

            AddPort(widthPort);
            AddPort(heightPort);
            AddPort(subdivisionsPort);
            AddPort(resultPort);

            Refresh();
        }

        public override void BindPorts() {
            BindPort(widthPort, RuntimeNode.WidthPort);
            BindPort(heightPort, RuntimeNode.HeightPort);
            BindPort(subdivisionsPort, RuntimeNode.SubdivisionsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["w"] = size.x;
            root["h"] = size.y;
            root["s"] = subdivisions;

            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            size.x = jsonData.Value<float>("w");
            size.y = jsonData.Value<float>("h");
            subdivisions = jsonData.Value<int>("s");

            widthField.SetValueWithoutNotify(size.x);
            heightField.SetValueWithoutNotify(size.y);
            subdivisionsField.SetValueWithoutNotify(subdivisions);

            RuntimeNode.UpdateValue(size.x, Which.Width);
            RuntimeNode.UpdateValue(size.y, Which.Height);
            RuntimeNode.UpdateValue(subdivisions, Which.Subdivisions);

            base.SetNodeData(jsonData);
        }
    }
}