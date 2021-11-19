using System;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class HelixPrimitiveCurveNode : RuntimeNode {
        private MinMaxInt points = new (64, Constants.MIN_HELIX_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
        private MinMaxFloat topRadius = new (1.0f, Constants.MIN_CIRCULAR_CURVE_RADIUS);
        private MinMaxFloat bottomRadius = new (1.0f, Constants.MIN_CIRCULAR_CURVE_RADIUS);
        private float rotations = 2.0f;
        private float pitch = 1.0f;
        private CurveData curve;

        public RuntimePort PointsPort { get; private set; }
        public RuntimePort RotationsPort { get; private set; }
        public RuntimePort PitchPort { get; private set; }
        public RuntimePort TopRadiusPort { get; private set; }
        public RuntimePort BottomRadiusPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public HelixPrimitiveCurveNode(string guid) : base(guid) {
            PointsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            RotationsPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            PitchPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            TopRadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            BottomRadiusPort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Curve, PortDirection.Output, this);
        }

        private void CalculateResult() {
            if (RuntimeGraphObjectData.IsDuringSerialization) {
                DebugUtility.Log("Attempting to generate curve during serialization. Aborting.");
                curve = null;
                return;
            }
            curve = CurvePrimitive.Helix(points - 1, rotations, pitch, topRadius, bottomRadius);
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == PointsPort) {
                var newValue = GetValue(connection, (int)points);
                if (newValue == points) return;
                points.Value = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == PitchPort) {
                var newValue = GetValue(connection, pitch);
                if (Math.Abs(newValue - pitch) < Constants.FLOAT_TOLERANCE) return;
                pitch = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == RotationsPort) {
                var newValue = GetValue(connection, rotations);
                if (Math.Abs(newValue - rotations) < Constants.FLOAT_TOLERANCE) return;
                rotations = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == TopRadiusPort) {
                var newValue = GetValue(connection, (float)topRadius);
                if (Math.Abs(newValue - topRadius) < Constants.FLOAT_TOLERANCE) return;
                topRadius.Value = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == BottomRadiusPort) {
                var newValue = GetValue(connection, (float)bottomRadius);
                if (Math.Abs(newValue - bottomRadius) < Constants.FLOAT_TOLERANCE) return;
                bottomRadius.Value = newValue;
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
            var array = new JArray {
                (int)points,
                rotations,
                pitch,
                (float)topRadius,
                (float)bottomRadius,
            };
            return array.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if (string.IsNullOrEmpty(json)) return;

            var data = JArray.Parse(json);
            points = new MinMaxInt(data.Value<int>(0), Constants.MIN_HELIX_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
            rotations = data.Value<float>(1);
            pitch = data.Value<float>(2);
            topRadius = new MinMaxFloat(data.Value<float>(3), Constants.MIN_CIRCULAR_CURVE_RADIUS);
            bottomRadius = new MinMaxFloat(data.Value<float>(4), Constants.MIN_CIRCULAR_CURVE_RADIUS);
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdatePoints(int newValue) {
            newValue = newValue.Clamped(Constants.MIN_HELIX_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
            if (newValue == points) return;
            
            points.Value = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateRotations(float newValue) {
            if (Math.Abs(newValue - rotations) < Constants.FLOAT_TOLERANCE) return;
            rotations = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
        
        public void UpdatePitch(float newValue) {
            if (Math.Abs(newValue - pitch) < Constants.FLOAT_TOLERANCE) return;
            pitch = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
        
        public void UpdateTopRadius(float newValue) {
            newValue = newValue.Min(Constants.MIN_CIRCULAR_CURVE_RADIUS);
            if (Math.Abs(newValue - topRadius) < Constants.FLOAT_TOLERANCE) return;
            topRadius.Value = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
        
        public void UpdateBottomRadius(float newValue) {
            newValue = newValue.Min(Constants.MIN_CIRCULAR_CURVE_RADIUS);
            if (Math.Abs(newValue - bottomRadius) < Constants.FLOAT_TOLERANCE) return;
            bottomRadius.Value = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
        
        }
}