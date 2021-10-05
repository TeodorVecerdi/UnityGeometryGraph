using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.CylinderPrimitiveNode.CylinderPrimitiveNode_Which;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Cylinder")]
    public class CylinderPrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.CylinderPrimitiveNode> {
        private GraphFrameworkPort bottomRadiusPort;
        private GraphFrameworkPort topRadiusPort;
        private GraphFrameworkPort heightPort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort resultPort;

        private FloatField bottomRadiusField;
        private FloatField topRadiusField;
        private FloatField heightField;
        private IntegerField pointsField;

        private float bottomRadius = 1.0f;
        private float topRadius = 1.0f;
        private float height = 2.0f;
        private int points = 8;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Cylinder Primitive", EditorView.DefaultNodePosition);

            (bottomRadiusPort, bottomRadiusField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Bottom Radius", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(bottomRadius, Which.BottomRadius));
            (topRadiusPort, topRadiusField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Top Radius", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(topRadius, Which.TopRadius));
            (heightPort, heightField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Height", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(height, Which.Height));
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Points", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(points, Which.Points));
            resultPort = GraphFrameworkPort.Create("Cylinder", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            topRadiusField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                if (evt.newValue < 0.01f) {
                    topRadius = 0.01f;
                } else topRadius = evt.newValue;

                RuntimeNode.UpdateValue(topRadius, Which.TopRadius);
            });
            
            topRadiusField.RegisterCallback<FocusOutEvent>(_ => {
                topRadiusField.SetValueWithoutNotify(topRadius);
            });
            
            bottomRadiusField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                if (evt.newValue < 0.01f) {
                    bottomRadius = 0.01f;
                } else bottomRadius = evt.newValue;

                RuntimeNode.UpdateValue(bottomRadius, Which.BottomRadius);
            });
            
            bottomRadiusField.RegisterCallback<FocusOutEvent>(_ => {
                bottomRadiusField.SetValueWithoutNotify(bottomRadius);
            });

            heightField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                if (evt.newValue < 0.01f) {
                    height = 0.01f;
                    heightField.SetValueWithoutNotify(0.01f);
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

            topRadiusField.SetValueWithoutNotify(1.0f);
            bottomRadiusField.SetValueWithoutNotify(1.0f);
            heightField.SetValueWithoutNotify(2.0f);
            pointsField.SetValueWithoutNotify(8);

            topRadiusPort.Add(topRadiusField);
            bottomRadiusPort.Add(bottomRadiusField);
            heightPort.Add(heightField);
            pointsPort.Add(pointsField);

            AddPort(topRadiusPort);
            AddPort(bottomRadiusPort);
            AddPort(heightPort);
            AddPort(pointsPort);
            AddPort(resultPort);

            Refresh();
        }

        public override void BindPorts() {
            BindPort(bottomRadiusPort, RuntimeNode.BottomRadiusPort);
            BindPort(topRadiusPort, RuntimeNode.TopRadiusPort);
            BindPort(heightPort, RuntimeNode.HeightPort);
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["r"] = bottomRadius;
            root["R"] = topRadius;
            root["h"] = height;
            root["p"] = points;

            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            bottomRadius = jsonData.Value<float>("r");
            topRadius = jsonData.Value<float>("R");
            height = jsonData.Value<float>("h");
            points = jsonData.Value<int>("p");

            bottomRadiusField.SetValueWithoutNotify(bottomRadius);
            topRadiusField.SetValueWithoutNotify(topRadius);
            heightField.SetValueWithoutNotify(height);
            pointsField.SetValueWithoutNotify(points);

            RuntimeNode.UpdateValue(bottomRadius, Which.BottomRadius);
            RuntimeNode.UpdateValue(topRadius, Which.TopRadius);
            RuntimeNode.UpdateValue(height, Which.Height);
            RuntimeNode.UpdateValue(points, Which.Points);

            base.SetNodeData(jsonData);
        }
    }
}