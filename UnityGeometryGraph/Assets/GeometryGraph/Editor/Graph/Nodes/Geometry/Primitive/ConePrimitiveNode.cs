using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.ConePrimitiveNode.ConePrimitiveNode_Which;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Cone")]
    public class ConePrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.ConePrimitiveNode> {
        private GraphFrameworkPort radiusPort;
        private GraphFrameworkPort heightPort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort resultPort;

        private FloatField radiusField;
        private FloatField heightField;
        private IntegerField pointsField;

        private float radius = 1.0f;
        private float height = 2.0f;
        private int points = 8;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Cone Primitive");

            (radiusPort, radiusField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Radius", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(radius, Which.Radius));
            (heightPort, heightField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Height", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(height, Which.Height));
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Points", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(points, Which.Points));
            resultPort = GraphFrameworkPort.Create("Cone", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            radiusField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                if (evt.newValue < 0.0f) {
                    radius = 0.0f;
                    radiusField.SetValueWithoutNotify(0.0f);
                } else radius = evt.newValue;

                RuntimeNode.UpdateValue(radius, Which.Radius);
            });
            
            heightField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                if (evt.newValue < 0.0f) {
                    height = 0.0f;
                    heightField.SetValueWithoutNotify(0.0f);
                } else height = evt.newValue;

                RuntimeNode.UpdateValue(height, Which.Height);
            });

            pointsField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                points = evt.newValue < 3 ? 3 : evt.newValue;

                RuntimeNode.UpdateValue(points, Which.Points);
            });
            pointsField.RegisterCallback<BlurEvent>(_ => {
                if (pointsField.value >= 3) return;
                pointsField.SetValueWithoutNotify(3);
            });

            radiusField.SetValueWithoutNotify(1.0f);
            heightField.SetValueWithoutNotify(2.0f);
            pointsField.SetValueWithoutNotify(8);

            radiusPort.Add(radiusField);
            heightPort.Add(heightField);
            pointsPort.Add(pointsField);

            AddPort(radiusPort);
            AddPort(heightPort);
            AddPort(pointsPort);
            AddPort(resultPort);

            Refresh();
        }

        public override void BindPorts() {
            BindPort(radiusPort, RuntimeNode.RadiusPort);
            BindPort(heightPort, RuntimeNode.HeightPort);
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["r"] = radius;
            root["h"] = height;
            root["p"] = points;

            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            radius = jsonData.Value<float>("r");
            height = jsonData.Value<float>("h");
            points = jsonData.Value<int>("p");

            radiusField.SetValueWithoutNotify(radius);
            heightField.SetValueWithoutNotify(height);
            pointsField.SetValueWithoutNotify(points);

            RuntimeNode.UpdateValue(radius, Which.Radius);
            RuntimeNode.UpdateValue(height, Which.Height);
            RuntimeNode.UpdateValue(points, Which.Points);

            base.SetNodeData(jsonData);
        }
    }
}