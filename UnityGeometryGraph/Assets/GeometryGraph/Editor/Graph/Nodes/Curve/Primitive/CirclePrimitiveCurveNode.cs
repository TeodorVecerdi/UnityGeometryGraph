using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Curve", "Primitive", "Circle")]
    public class CirclePrimitiveCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.CirclePrimitiveCurveNode> {
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort radiusPort;
        private GraphFrameworkPort resultPort;

        private ClampedIntegerField pointsField;
        private ClampedFloatField radiusField;

        private int points = 32;
        private float radius = 1.0f;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Circle Primitive Curve");

            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", Orientation.Horizontal, PortType.Integer, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            (radiusPort, radiusField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Radius", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this, onDisconnect: (_, _) => RuntimeNode.UpdateRadius(radius));
            resultPort = GraphFrameworkPort.Create("Curve", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Curve, edgeConnectorListener, this);

            pointsField.Min = Constants.MIN_CIRCLE_CURVE_RESOLUTION;
            pointsField.Max = Constants.MAX_CURVE_RESOLUTION;
            pointsField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.Clamped(Constants.MIN_CIRCLE_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION);
                if (newValue == points) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change circle curve points");
                points = newValue;
                RuntimeNode.UpdatePoints(points);
            });

            radiusField.Min = Constants.MIN_CIRCULAR_CURVE_RADIUS;
            radiusField.RegisterValueChangedCallback(evt => {
                var newValue = evt.newValue.Min(Constants.MIN_CIRCULAR_CURVE_RADIUS);
                if (Math.Abs(newValue - radius) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change circle curve radius");
                radius = newValue;
                RuntimeNode.UpdateRadius(radius);
            });
            
            
            pointsField.SetValueWithoutNotify(points);
            radiusField.SetValueWithoutNotify(radius);
            
            pointsPort.Add(pointsField);
            radiusPort.Add(radiusField);
            AddPort(pointsPort);
            AddPort(radiusPort);
            AddPort(resultPort);
            
            Refresh();
        }

        public override void BindPorts() {
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(radiusPort, RuntimeNode.RadiusPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root =  base.GetNodeData();
            var array = new JArray {
                points,
                radius
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            var array = jsonData["d"] as JArray;

            points = array!.Value<int>(0);
            radius = array!.Value<float>(1);
            
            pointsField.SetValueWithoutNotify(points);
            radiusField.SetValueWithoutNotify(radius);
            
            RuntimeNode.UpdatePoints(points);
            RuntimeNode.UpdateRadius(radius);
            
            base.SetNodeData(jsonData);
        }
    }
}