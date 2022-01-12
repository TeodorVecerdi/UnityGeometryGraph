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
        protected override string Title => "Cylinder Primitive";
        protected override NodeCategory Category => NodeCategory.Geometry;

        private GraphFrameworkPort bottomRadiusPort;
        private GraphFrameworkPort topRadiusPort;
        private GraphFrameworkPort heightPort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort horizontalUVRepetitionsPort;
        private GraphFrameworkPort verticalUVRepetitionsPort;
        private GraphFrameworkPort resultPort;

        private ClampedFloatField bottomRadiusField;
        private ClampedFloatField topRadiusField;
        private ClampedFloatField heightField;
        private ClampedIntegerField pointsField;
        private ClampedIntegerField horizontalUVRepetitionsField;
        private ClampedIntegerField verticalUVRepetitionsField;

        private float bottomRadius = 1.0f;
        private float topRadius = 1.0f;
        private float height = 2.0f;
        private int points = 8;
        private int horizontalUVRepetitions = 2;
        private int verticalUVRepetitions = 1;

        protected override void CreateNode() {
            (bottomRadiusPort, bottomRadiusField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Bottom Radius", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateBottomRadius(bottomRadius));
            (topRadiusPort, topRadiusField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Top Radius", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateTopRadius(topRadius));
            (heightPort, heightField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Height", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateHeight(height));
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            (horizontalUVRepetitionsPort, horizontalUVRepetitionsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Horizontal Repetitions", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateHorizontalUVRepetitions(horizontalUVRepetitions));
            (verticalUVRepetitionsPort, verticalUVRepetitionsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Vertical Repetitions", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateVerticalUVRepetitions(verticalUVRepetitions));
            resultPort = GraphFrameworkPort.Create("Cylinder", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            topRadiusField.Min = Constants.MIN_CIRCULAR_GEOMETRY_RADIUS;
            topRadiusField.RegisterValueChangedCallback(evt => {
                float newValue = evt.newValue.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if (MathF.Abs(newValue - topRadius) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change cylinder top radius");
                topRadius = newValue;
                RuntimeNode.UpdateTopRadius(topRadius);
            });

            bottomRadiusField.Min = Constants.MIN_CIRCULAR_GEOMETRY_RADIUS;
            bottomRadiusField.RegisterValueChangedCallback(evt => {
                float newValue = evt.newValue.MinClamped(Constants.MIN_CIRCULAR_GEOMETRY_RADIUS);
                if (MathF.Abs(newValue - bottomRadius) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change cylinder bottom radius");
                bottomRadius = newValue;
                RuntimeNode.UpdateBottomRadius(bottomRadius);
            });

            heightField.Min = Constants.MIN_GEOMETRY_HEIGHT;
            heightField.RegisterValueChangedCallback(evt => {
                float newValue = evt.newValue.MinClamped(Constants.MIN_GEOMETRY_HEIGHT);
                if (MathF.Abs(newValue - height) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change cylinder height");
                height = newValue;
                RuntimeNode.UpdateHeight(height);
            });

            pointsField.Min = Constants.MIN_CIRCULAR_GEOMETRY_POINTS;
            pointsField.Max = Constants.MAX_CIRCULAR_GEOMETRY_POINTS;
            pointsField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.Clamped(Constants.MIN_CIRCULAR_GEOMETRY_POINTS, Constants.MAX_CIRCULAR_GEOMETRY_POINTS);
                if (newValue == points) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change cylinder points");
                points = newValue;
                RuntimeNode.UpdatePoints(points);
            });

            horizontalUVRepetitionsField.Min = 1;
            horizontalUVRepetitionsField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.MinClamped(1);
                if (newValue == horizontalUVRepetitions) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change horizontal UV repetitions");
                horizontalUVRepetitions = newValue;
                RuntimeNode.UpdateHorizontalUVRepetitions(horizontalUVRepetitions);
            });

            verticalUVRepetitionsField.Min = 1;
            verticalUVRepetitionsField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.MinClamped(1);
                if (newValue == verticalUVRepetitions) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vertical UV repetitions");
                verticalUVRepetitions = newValue;
                RuntimeNode.UpdateVerticalUVRepetitions(verticalUVRepetitions);
            });

            topRadiusField.SetValueWithoutNotify(1.0f);
            bottomRadiusField.SetValueWithoutNotify(1.0f);
            heightField.SetValueWithoutNotify(2.0f);
            pointsField.SetValueWithoutNotify(8);
            horizontalUVRepetitionsField.SetValueWithoutNotify(horizontalUVRepetitions);
            verticalUVRepetitionsField.SetValueWithoutNotify(verticalUVRepetitions);

            topRadiusPort.Add(topRadiusField);
            bottomRadiusPort.Add(bottomRadiusField);
            heightPort.Add(heightField);
            pointsPort.Add(pointsField);
            horizontalUVRepetitionsPort.Add(horizontalUVRepetitionsField);
            verticalUVRepetitionsPort.Add(verticalUVRepetitionsField);

            AddPort(topRadiusPort);
            AddPort(bottomRadiusPort);
            AddPort(heightPort);
            AddPort(pointsPort);

            inputContainer.Add(new Label("UV Settings") {name = "uvSettingsTitle"});
            AddPort(horizontalUVRepetitionsPort);
            AddPort(verticalUVRepetitionsPort);

            AddPort(resultPort);

            Refresh();
        }

        protected override void BindPorts() {
            BindPort(bottomRadiusPort, RuntimeNode.BottomRadiusPort);
            BindPort(topRadiusPort, RuntimeNode.TopRadiusPort);
            BindPort(heightPort, RuntimeNode.HeightPort);
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(horizontalUVRepetitionsPort, RuntimeNode.HorizontalUVRepetitionsPort);
            BindPort(verticalUVRepetitionsPort, RuntimeNode.VerticalUVRepetitionsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

            root["r"] = bottomRadius;
            root["R"] = topRadius;
            root["h"] = height;
            root["p"] = points;
            root["u"] = horizontalUVRepetitions;
            root["v"] = verticalUVRepetitions;

            return root;
        }

        protected internal override void Deserialize(JObject data) {
            bottomRadius = data.Value<float>("r");
            topRadius = data.Value<float>("R");
            height = data.Value<float>("h");
            points = data.Value<int>("p");
            horizontalUVRepetitions = data.Value<int>("u");
            verticalUVRepetitions = data.Value<int>("v");

            bottomRadiusField.SetValueWithoutNotify(bottomRadius);
            topRadiusField.SetValueWithoutNotify(topRadius);
            heightField.SetValueWithoutNotify(height);
            pointsField.SetValueWithoutNotify(points);
            horizontalUVRepetitionsField.SetValueWithoutNotify(horizontalUVRepetitions);
            verticalUVRepetitionsField.SetValueWithoutNotify(verticalUVRepetitions);

            RuntimeNode.UpdateBottomRadius(bottomRadius);
            RuntimeNode.UpdateTopRadius(topRadius);
            RuntimeNode.UpdateHeight(height);
            RuntimeNode.UpdatePoints(points);
            RuntimeNode.UpdateHorizontalUVRepetitions(horizontalUVRepetitions);
            RuntimeNode.UpdateVerticalUVRepetitions(verticalUVRepetitions);

            base.Deserialize(data);
        }
    }
}