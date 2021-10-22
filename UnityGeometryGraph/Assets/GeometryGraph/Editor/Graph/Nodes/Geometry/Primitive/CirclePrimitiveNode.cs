using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.CirclePrimitiveNode.CirclePrimitiveNode_Which;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Primitive", "Circle")]
    public class CirclePrimitiveNode : AbstractNode<GeometryGraph.Runtime.Graph.CirclePrimitiveNode> {
        private GraphFrameworkPort radiusPort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort resultPort;

        private FloatField radiusField;
        private ClampedIntegerField pointsField;

        private float radius = 1.0f;
        private int points = 8;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Circle Primitive");

            (radiusPort, radiusField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Radius", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(radius, Which.Radius));
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(points, Which.Points));
            resultPort = GraphFrameworkPort.Create("Circle", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);

            radiusField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.Min(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if (MathF.Abs(newValue - radius) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                radius = newValue;
                RuntimeNode.UpdateValue(radius, Which.Radius);
            });

            pointsField.Min = Constants.MIN_CIRCULAR_GEOMETRY_POINTS;
            pointsField.Max = Constants.MAX_CIRCULAR_GEOMETRY_POINTS;
            pointsField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.Clamped(Constants.MIN_CIRCULAR_GEOMETRY_POINTS, Constants.MAX_CIRCULAR_GEOMETRY_POINTS);
                if (newValue == points) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                points = newValue;
                RuntimeNode.UpdateValue(points, Which.Points);
            });

            radiusField.SetValueWithoutNotify(1.0f);
            pointsField.SetValueWithoutNotify(8);

            radiusPort.Add(radiusField);
            pointsPort.Add(pointsField);

            AddPort(radiusPort);
            AddPort(pointsPort);
            AddPort(resultPort);

            Refresh();
        }

        public override void BindPorts() {
            BindPort(radiusPort, RuntimeNode.RadiusPort);
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["r"] = radius;
            root["p"] = points;

            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            radius = jsonData.Value<float>("r");
            points = jsonData.Value<int>("p");

            radiusField.SetValueWithoutNotify(radius);
            pointsField.SetValueWithoutNotify(points);

            RuntimeNode.UpdateValue(radius, Which.Radius);
            RuntimeNode.UpdateValue(points, Which.Points);

            base.SetNodeData(jsonData);
        }
    }
}