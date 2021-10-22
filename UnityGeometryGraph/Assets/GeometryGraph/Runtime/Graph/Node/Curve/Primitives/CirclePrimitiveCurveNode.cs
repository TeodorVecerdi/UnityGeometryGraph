using System;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class CirclePrimitiveCurveNode : RuntimeNode {
        private MinMaxInt points = new (32, Constants.MIN_CIRCLE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
        private MinMaxFloat radius = new (1.0f, Constants.MIN_CIRCULAR_CURVE_RADIUS);
        private CurveData circleCurve;

        public RuntimePort PointsPort { get; private set; }
        public RuntimePort RadiusPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public CirclePrimitiveCurveNode(string guid) : base(guid) {
            PointsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            RadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Curve, PortDirection.Output, this);
        }

        private void CalculateResult() {
            circleCurve = CurvePrimitive.Circle(points - 1, radius);
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == PointsPort) {
                var newValue = GetValue(connection, (int)points);
                if (newValue == points) return;
                points.Value = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == RadiusPort) {
                var newValue = GetValue(connection, (float)radius);
                if (Math.Abs(newValue - radius) < Constants.FLOAT_TOLERANCE) return;
                radius.Value = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            }
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            if (circleCurve == null) CalculateResult();
            return circleCurve.Clone();
        }

        public override string GetCustomData() {
            var array = new JArray {
                (int)points,
                (float)radius,
            };
            return array.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if (string.IsNullOrEmpty(json)) return;

            var data = JArray.Parse(json);
            points = new MinMaxInt(data.Value<int>(0), Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
            radius = new MinMaxFloat(data.Value<float>(1), Constants.MIN_CIRCULAR_CURVE_RADIUS);
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdatePoints(int newPoints) {
            newPoints = newPoints.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
            if (newPoints == points) return;
            
            points.Value = newPoints;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateRadius(float newRadius) {
            newRadius = newRadius.Min(Constants.MIN_CIRCULAR_CURVE_RADIUS);
            if (Math.Abs(newRadius - radius) < Constants.FLOAT_TOLERANCE) return;
            radius.Value = newRadius;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
        
        public override void RebindPorts() {
            throw new System.NotImplementedException();
        }
    }
}