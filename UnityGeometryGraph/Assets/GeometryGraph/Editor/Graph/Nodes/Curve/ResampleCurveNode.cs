using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using ResampleMode = GeometryGraph.Runtime.Graph.ResampleCurveNode.ResampleCurveNode_Mode;

namespace GeometryGraph.Editor {
    [Title("Curve", "Resample Curve")]
    public class ResampleCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.ResampleCurveNode> {
        protected override string Title => "Resample Curve";
        protected override NodeCategory Category => NodeCategory.Curve;

        private GraphFrameworkPort curvePort;
        private GraphFrameworkPort distancePort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort resultPort;

        private ClampedFloatField distanceField;
        private ClampedIntegerField pointsField;
        private EnumSelectionToggle<ResampleMode> modeToggle;

        private float distance = 0.1f;
        private int points = 32;
        private ResampleMode mode = ResampleMode.Points;

        protected override void CreateNode() {
            curvePort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            (distancePort, distanceField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Distance", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateDistance(distance));
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Curve, this);

            distanceField.Min = 0.001f;
            pointsField.Min = 2;
            pointsField.Max = Constants.MAX_CURVE_RESOLUTION;

            modeToggle = new EnumSelectionToggle<ResampleMode>(mode);
            modeToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == mode) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Resample Mode");
                mode = evt.newValue;
                RuntimeNode.UpdateMode(mode);
                OnModeChanged();
            });

            distanceField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - distance) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Resample Distance");
                distance = evt.newValue;
                RuntimeNode.UpdateDistance(distance);
            });

            pointsField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == points) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Resample Points");
                points = evt.newValue;
                RuntimeNode.UpdatePoints(points);
            });

            modeToggle.SetValueWithoutNotify(mode);
            distanceField.SetValueWithoutNotify(distance);
            pointsField.SetValueWithoutNotify(points);

            distancePort.Add(distanceField);
            pointsPort.Add(pointsField);

            inputContainer.Add(modeToggle);
            AddPort(curvePort);
            AddPort(distancePort);
            AddPort(pointsPort);
            AddPort(resultPort);

            OnModeChanged();
        }

        protected override void BindPorts() {
            BindPort(curvePort, RuntimeNode.CurvePort);
            BindPort(distancePort, RuntimeNode.DistancePort);
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        private void OnModeChanged() {
            if (mode == ResampleMode.Distance) {
                pointsPort.HideAndDisconnect();
                distancePort.Show();
            } else {
                pointsPort.Show();
                distancePort.HideAndDisconnect();
            }
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray data = new() {
                (int)mode,
                distance,
                points
            };

            root["d"] = data;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            mode = (ResampleMode)array!.Value<int>(0);
            distance = array.Value<float>(1);
            points = array.Value<int>(2);

            modeToggle.SetValueWithoutNotify(mode);
            distanceField.SetValueWithoutNotify(distance);
            pointsField.SetValueWithoutNotify(points);

            RuntimeNode.UpdateMode(mode);
            RuntimeNode.UpdateDistance(distance);
            RuntimeNode.UpdatePoints(points);

            OnModeChanged();

            base.Deserialize(data);
        }
    }
}