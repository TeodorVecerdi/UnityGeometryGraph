using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Cone")]
    public class ConePrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.ConePrimitiveNode> {
        private GraphFrameworkPort radiusPort;
        private GraphFrameworkPort heightPort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort resultPort;

        private ClampedFloatField radiusField;
        private ClampedFloatField heightField;
        private ClampedIntegerField pointsField;

        private float radius = 1.0f;
        private float height = 2.0f;
        private int points = 8;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Cone Primitive");

            (radiusPort, radiusField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Radius", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateRadius(radius));
            (heightPort, heightField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Height", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateHeight(height));
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            resultPort = GraphFrameworkPort.Create("Cone", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            radiusField.Min = Constants.MIN_CIRCULAR_GEOMETRY_RADIUS;
            radiusField.RegisterValueChangedCallback(evt => {
                float newValue = evt.newValue.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if (MathF.Abs(newValue - radius) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                radius = newValue;
                RuntimeNode.UpdateRadius(radius);
            });

            heightField.Min = Constants.MIN_GEOMETRY_HEIGHT;
            heightField.RegisterValueChangedCallback(evt => {
                float newValue = evt.newValue.MinClamped(Constants.MIN_GEOMETRY_HEIGHT);
                if (MathF.Abs(newValue - height) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                height = newValue;
                RuntimeNode.UpdateHeight(height);
            });

            pointsField.Min = Constants.MIN_CIRCULAR_GEOMETRY_POINTS;
            pointsField.Max = Constants.MAX_CIRCULAR_GEOMETRY_POINTS;
            pointsField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.Clamped(Constants.MIN_CIRCULAR_GEOMETRY_POINTS, Constants.MAX_CIRCULAR_GEOMETRY_POINTS);
                if (newValue == points) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                points = newValue;
                RuntimeNode.UpdatePoints(points);
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
            JObject root = base.GetNodeData();

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

            RuntimeNode.UpdateRadius(radius);
            RuntimeNode.UpdateHeight(height);
            RuntimeNode.UpdatePoints(points);

            base.SetNodeData(jsonData);
        }
    }
}