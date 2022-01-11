using System;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GenerationMode = GeometryGraph.Runtime.Graph.DistributePointsSimpleNode.DistributePointsSimpleNode_GenerationMode;

namespace GeometryGraph.Editor {
    [Title("Point", "Distribute Points (Simple)")]
    public class DistributePointsSimpleNode : AbstractNode<GeometryGraph.Runtime.Graph.DistributePointsSimpleNode> {
        protected override string Title => "Distribute Points (Simple)";
        protected override NodeCategory Category => NodeCategory.Point;

        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort seedPort;
        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort pointsRatioPort;
        private GraphFrameworkPort resultPort;

        private IntegerField seedField;
        private ClampedIntegerField pointsField;
        private ClampedFloatField pointsRatioField;
        private EnumSelectionToggle<GenerationMode> modeToggle;

        private int seed = 0;
        private int points = 4;
        private float pointsRatio = 4.0f;
        private GenerationMode mode = GenerationMode.Constant;

        protected override void CreateNode() {
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (seedPort, seedField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Seed", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateSeed(seed));
            (pointsPort, pointsField) = GraphFrameworkPort.CreateWithBackingField<ClampedIntegerField, int>("Points", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdatePoints(points));
            (pointsRatioPort, pointsRatioField) = GraphFrameworkPort.CreateWithBackingField<ClampedFloatField, float>("Points Ratio", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdatePointsRatio(pointsRatio));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            seedField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == seed) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Seed");
                seed = evt.newValue;
                RuntimeNode.UpdateSeed(seed);
            });

            pointsField.Min = 1;
            pointsField.Max = Constants.MAX_POINT_DISTRIBUTION_POINTS;
            pointsField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == points) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Points");
                points = evt.newValue;
                RuntimeNode.UpdatePoints(points);
            });

            pointsRatioField.Min = 1.0f;
            pointsRatioField.Max = Constants.MAX_POINT_DISTRIBUTION_RATIO;
            pointsRatioField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - pointsRatio) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Points Ratio");
                pointsRatio = evt.newValue;
                RuntimeNode.UpdatePointsRatio(pointsRatio);
            });


            modeToggle = new EnumSelectionToggle<GenerationMode>(mode);
            modeToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == mode) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Generation Mode");
                mode = evt.newValue;
                RuntimeNode.UpdateMode(mode);
                OnModeChanged();
            });

            modeToggle.SetValueWithoutNotify(mode);
            seedField.SetValueWithoutNotify(seed);
            pointsField.SetValueWithoutNotify(points);
            pointsRatioField.SetValueWithoutNotify(pointsRatio);

            seedPort.Add(seedField);
            pointsPort.Add(pointsField);
            pointsRatioPort.Add(pointsRatioField);

            inputContainer.Add(modeToggle);
            AddPort(geometryPort);
            AddPort(seedPort);
            AddPort(pointsPort);
            AddPort(pointsRatioPort);
            AddPort(resultPort);

            OnModeChanged();
        }

        private void OnModeChanged() {
            if (mode == GenerationMode.Constant) {
                pointsPort.Show();
                pointsRatioPort.HideAndDisconnect();
            } else {
                pointsPort.HideAndDisconnect();
                pointsRatioPort.Show();
            }
        }

        protected override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(seedPort, RuntimeNode.SeedPort);
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(pointsRatioPort, RuntimeNode.PointsRatioPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray data = new() {
                (int)mode,
                seed,
                points,
                pointsRatio
            };
            root["d"] = data;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;
            mode = (GenerationMode)array!.Value<int>(0);
            seed = array.Value<int>(1);
            points = array.Value<int>(2);
            pointsRatio = array.Value<float>(3);

            modeToggle.SetValueWithoutNotify(mode);
            seedField.SetValueWithoutNotify(seed);
            pointsField.SetValueWithoutNotify(points);
            pointsRatioField.SetValueWithoutNotify(pointsRatio);

            RuntimeNode.UpdateMode(mode);
            RuntimeNode.UpdateSeed(seed);
            RuntimeNode.UpdatePoints(points);
            RuntimeNode.UpdatePointsRatio(pointsRatio);

            OnModeChanged();

            base.Deserialize(data);
        }
    }
}