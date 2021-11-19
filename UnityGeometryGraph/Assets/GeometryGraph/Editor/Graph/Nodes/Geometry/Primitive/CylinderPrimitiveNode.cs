using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Cylinder")]
    public class CylinderPrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.CylinderPrimitiveNode> {
        private GraphFrameworkPort bottomRadiusPort;
        private GraphFrameworkPort topRadiusPort;
        private GraphFrameworkPort heightPort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort resultPort;

        private ClampedFloatField bottomRadiusField;
        private ClampedFloatField topRadiusField;
        private ClampedFloatField heightField;
        private ClampedIntegerField pointsField;

        private float bottomRadius = 1.0f;
        private float topRadius = 1.0f;
        private float height = 2.0f;
        private int points = 8;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Cylinder Primitive");

            (bottomRadiusPort, bottomRadiusField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Bottom Radius", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateBottomRadius(bottomRadius));
            (topRadiusPort, topRadiusField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Top Radius", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateTopRadius(topRadius));
            (heightPort, heightField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Height", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateHeight(height));
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            resultPort = GraphFrameworkPort.Create("Cylinder", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            topRadiusField.Min = Constants.MIN_CIRCULAR_GEOMETRY_RADIUS;
            topRadiusField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if (MathF.Abs(newValue - topRadius) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                topRadius = newValue;
                RuntimeNode.UpdateTopRadius(topRadius);
            });

            bottomRadiusField.Min = Constants.MIN_CIRCULAR_GEOMETRY_RADIUS;
            bottomRadiusField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if (MathF.Abs(newValue - bottomRadius) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                bottomRadius = newValue;
                RuntimeNode.UpdateBottomRadius(bottomRadius);
            });

            heightField.Min = Constants.MIN_GEOMETRY_HEIGHT;
            heightField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.MinClamped(Constants.MIN_GEOMETRY_HEIGHT);
                if (MathF.Abs(newValue - height) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                height = newValue;
                RuntimeNode.UpdateHeight(height);
            });

            pointsField.Min = Constants.MIN_CIRCULAR_GEOMETRY_POINTS;
            pointsField.Max = Constants.MAX_CIRCULAR_GEOMETRY_POINTS;
            pointsField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.Clamped(Constants.MIN_CIRCULAR_GEOMETRY_POINTS, Constants.MAX_CIRCULAR_GEOMETRY_POINTS);
                if (newValue == points) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                points = newValue;
                RuntimeNode.UpdatePoints(points);
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

            RuntimeNode.UpdateBottomRadius(bottomRadius);
            RuntimeNode.UpdateTopRadius(topRadius);
            RuntimeNode.UpdateHeight(height);
            RuntimeNode.UpdatePoints(points);

            base.SetNodeData(jsonData);
        }
    }
}