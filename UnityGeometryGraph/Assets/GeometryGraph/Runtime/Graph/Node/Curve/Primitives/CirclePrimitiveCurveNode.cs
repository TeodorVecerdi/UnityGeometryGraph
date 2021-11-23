using System;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class CirclePrimitiveCurveNode : RuntimeNode {
        private MinMaxInt points = new(32, Constants.MIN_CIRCLE_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION);
        private MinMaxFloat radius = new(1.0f, Constants.MIN_CIRCULAR_CURVE_RADIUS);
        private CurveData curve;

        public RuntimePort PointsPort { get; private set; }
        public RuntimePort RadiusPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public CirclePrimitiveCurveNode(string guid) : base(guid) {
            PointsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            RadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Curve, PortDirection.Output, this);
        }

        private void CalculateResult() {
            if (RuntimeGraphObjectData.IsDuringSerialization) {
                DebugUtility.Log("Attempting to generate curve during serialization. Aborting.");
                curve = null;
                return;
            }

            curve = CurvePrimitive.Circle(points, radius);
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == PointsPort) {
                int newValue = GetValue(connection, (int)points);
                if (newValue == points) return;
                points.Value = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == RadiusPort) {
                float newValue = GetValue(connection, (float)radius);
                if (Math.Abs(newValue - radius) < Constants.FLOAT_TOLERANCE) return;
                radius.Value = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            }
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            if (curve == null) CalculateResult();
            return curve == null ? CurveData.Empty : curve.Clone();
        }

        public override string GetCustomData() {
            JArray array = new JArray {
                (int)points,
                (float)radius,
            };
            return array.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if (string.IsNullOrEmpty(json)) return;

            JArray data = JArray.Parse(json);
            points = new MinMaxInt(data.Value<int>(0), Constants.MIN_CIRCLE_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION);
            radius = new MinMaxFloat(data.Value<float>(1), Constants.MIN_CIRCULAR_CURVE_RADIUS);
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdatePoints(int newPoints) {
            newPoints = newPoints.Clamped(Constants.MIN_CIRCLE_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION);
            if (newPoints == points) return;

            points.Value = newPoints;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateRadius(float newRadius) {
            newRadius = newRadius.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS);
            if (Math.Abs(newRadius - radius) < Constants.FLOAT_TOLERANCE) return;
            radius.Value = newRadius;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
    }
}