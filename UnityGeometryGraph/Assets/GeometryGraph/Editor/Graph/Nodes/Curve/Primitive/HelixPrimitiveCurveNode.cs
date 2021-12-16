using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityCommons;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Curve", "Primitive", "Helix")]
    public class HelixPrimitiveCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.HelixPrimitiveCurveNode> {
        protected override string Title => "Helix Primitive Curve";
        protected override NodeCategory Category => NodeCategory.Curve;

        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort rotationsPort;
        private GraphFrameworkPort pitchPort;
        private GraphFrameworkPort topRadiusPort;
        private GraphFrameworkPort bottomRadiusPort;
        private GraphFrameworkPort resultPort;

        private ClampedIntegerField pointsField;
        private FloatField rotationsField;
        private FloatField pitchField;
        private ClampedFloatField topRadiusField;
        private ClampedFloatField bottomRadiusField;

        private int points = 64;
        private float rotations = 2.0f;
        private float pitch = 1.0f;
        private float topRadius = 1.0f;
        private float bottomRadius = 1.0f;

        public override void CreateNode() {
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            (rotationsPort, rotationsField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Rotations", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateRotations(rotations));
            (pitchPort, pitchField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Pitch", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdatePitch(pitch));
            (topRadiusPort, topRadiusField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Top Radius", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateTopRadius(topRadius));
            (bottomRadiusPort, bottomRadiusField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Bottom Radius", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateBottomRadius(bottomRadius));
            resultPort = GraphFrameworkPort.Create("Curve", Direction.Output, Port.Capacity.Multi, PortType.Curve, this);

            pointsField.Min = Constants.MIN_HELIX_CURVE_RESOLUTION + 1;
            pointsField.Max = Constants.MAX_CURVE_RESOLUTION + 1;
            pointsField.RegisterValueChangedCallback(evt => {
                int newValue = evt.newValue.Clamped(Constants.MIN_HELIX_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
                if (newValue == points) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change helix curve points");
                points = newValue;
                RuntimeNode.UpdatePoints(points);
            });
            
            rotationsField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - rotations) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change helix curve rotations");
                rotations = evt.newValue;
                RuntimeNode.UpdateRotations(rotations);
            });
            
            pitchField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - pitch) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change helix curve pitch");
                pitch = evt.newValue;
                RuntimeNode.UpdatePitch(pitch);
            });

            topRadiusField.Min = Constants.MIN_CIRCULAR_CURVE_RADIUS;
            topRadiusField.RegisterValueChangedCallback(evt => {
                float newValue = evt.newValue.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS);
                if (Math.Abs(newValue - topRadius) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change helix curve top radius");
                topRadius = newValue;
                RuntimeNode.UpdateTopRadius(topRadius);
            });
            
            bottomRadiusField.Min = Constants.MIN_CIRCULAR_CURVE_RADIUS;
            bottomRadiusField.RegisterValueChangedCallback(evt => {
                float newValue = evt.newValue.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS);
                if (Math.Abs(newValue - bottomRadius) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change helix curve bottom radius");
                bottomRadius = newValue;
                RuntimeNode.UpdateBottomRadius(bottomRadius);
            });
            
            
            pointsField.SetValueWithoutNotify(points);
            rotationsField.SetValueWithoutNotify(rotations);
            pitchField.SetValueWithoutNotify(pitch);
            topRadiusField.SetValueWithoutNotify(topRadius);
            bottomRadiusField.SetValueWithoutNotify(bottomRadius);
            
            pointsPort.Add(pointsField);
            rotationsPort.Add(rotationsField);
            pitchPort.Add(pitchField);
            topRadiusPort.Add(topRadiusField);
            bottomRadiusPort.Add(bottomRadiusField);
            AddPort(pointsPort);
            AddPort(rotationsPort);
            AddPort(pitchPort);
            AddPort(topRadiusPort);
            AddPort(bottomRadiusPort);
            AddPort(resultPort);
            
            Refresh();
        }

        public override void BindPorts() {
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(rotationsPort, RuntimeNode.RotationsPort);
            BindPort(pitchPort, RuntimeNode.PitchPort);
            BindPort(topRadiusPort, RuntimeNode.TopRadiusPort);
            BindPort(bottomRadiusPort, RuntimeNode.BottomRadiusPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            JObject root =  base.GetNodeData();
            JArray array = new JArray {
                points,
                rotations,
                pitch,
                topRadius,
                bottomRadius,
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            JArray array = jsonData["d"] as JArray;

            points = array!.Value<int>(0);
            rotations = array!.Value<float>(1);
            pitch = array!.Value<float>(2);
            topRadius = array!.Value<float>(3);
            bottomRadius = array!.Value<float>(4);
            
            pointsField.SetValueWithoutNotify(points);
            rotationsField.SetValueWithoutNotify(rotations);
            pitchField.SetValueWithoutNotify(pitch);
            topRadiusField.SetValueWithoutNotify(topRadius);
            bottomRadiusField.SetValueWithoutNotify(bottomRadius);
            
            RuntimeNode.UpdatePoints(points);
            RuntimeNode.UpdateRotations(rotations);
            RuntimeNode.UpdatePitch(pitch);
            RuntimeNode.UpdateTopRadius(topRadius);
            RuntimeNode.UpdateBottomRadius(bottomRadius);
            
            base.SetNodeData(jsonData);
        }
    }
}