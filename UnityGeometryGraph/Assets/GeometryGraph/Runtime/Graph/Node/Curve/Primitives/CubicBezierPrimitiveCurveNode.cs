using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    public class CubicBezierPrimitiveCurveNode : RuntimeNode {
        private MinMaxInt points = new MinMaxInt(32, Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
        private bool isClosed;
        private float3 start = float3.zero;
        private float3 controlA = float3_ext.forward;
        private float3 controlB = float3_ext.right + float3_ext.forward;
        private float3 end = float3_ext.right;

        private CurveData curve;

        public RuntimePort PointsPort { get; private set; }
        public RuntimePort ClosedPort { get; private set; }
        public RuntimePort StartPort { get; private set; }
        public RuntimePort ControlAPort { get; private set; }
        public RuntimePort ControlBPort { get; private set; }
        public RuntimePort EndPort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public CubicBezierPrimitiveCurveNode(string guid) : base(guid) {
            PointsPort = RuntimePort.Create(PortType.Integer, PortDirection.Input, this);
            ClosedPort = RuntimePort.Create(PortType.Boolean, PortDirection.Input, this);
            StartPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ControlAPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ControlBPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            EndPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Curve, PortDirection.Output, this);
        }

        private void CalculateResult() {
            if (RuntimeGraphObjectData.IsDuringSerialization) {
                DebugUtility.Log("Attempting to generate curve during serialization. Aborting.");
                curve = null;
                return;
            }

            curve = CurvePrimitive.CubicBezier(points - 1, isClosed, start, controlA, controlB, end);
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultPort) return;
            if (port == PointsPort) {
                int newValue = GetValue(connection, (int)points);
                if (newValue == points) return;
                points.Value = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == ClosedPort) {
                bool newValue = GetValue(connection, isClosed);
                if (newValue == isClosed) return;
                isClosed = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == StartPort) {
                float3 newValue = GetValue(connection, start);
                if (newValue.Equals(start)) return;
                start = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == ControlAPort) {
                float3 newValue = GetValue(connection, controlA);
                if (newValue.Equals(controlA)) return;
                controlA = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == ControlBPort) {
                float3 newValue = GetValue(connection, controlB);
                if (newValue.Equals(controlB)) return;
                controlB = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            } else if (port == EndPort) {
                float3 newValue = GetValue(connection, end);
                if (newValue.Equals(end)) return;
                end = newValue;
                CalculateResult();
                NotifyPortValueChanged(ResultPort);
            }
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            if (curve == null) CalculateResult();
            return curve == null ? CurveData.Empty : curve.Clone();
        }

        public override string Serialize() {
            JArray array = new JArray {
                (int)points,
                isClosed ? 1 : 0,
                JsonConvert.SerializeObject(start, float3Converter.Converter),
                JsonConvert.SerializeObject(controlA, float3Converter.Converter),
                JsonConvert.SerializeObject(controlB, float3Converter.Converter),
                JsonConvert.SerializeObject(end, float3Converter.Converter),
            };
            return array.ToString(Formatting.None);
        }

        public override void Deserialize(string json) {
            if (string.IsNullOrEmpty(json)) return;

            JArray data = JArray.Parse(json);
            points = new MinMaxInt(data.Value<int>(0), Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
            isClosed = data.Value<int>(1) == 1;
            start = JsonConvert.DeserializeObject<float3>(data.Value<string>(2)!, float3Converter.Converter);
            controlA = JsonConvert.DeserializeObject<float3>(data.Value<string>(3)!, float3Converter.Converter);
            controlB = JsonConvert.DeserializeObject<float3>(data.Value<string>(4)!, float3Converter.Converter);
            end = JsonConvert.DeserializeObject<float3>(data.Value<string>(5)!, float3Converter.Converter);
        }

        public override void OnAfterDeserialize() {
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdatePoints(int newValue) {
            newValue = newValue.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION + 1, Constants.MAX_CURVE_RESOLUTION + 1);
            if (newValue == points) return;

            points.Value = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateClosed(bool newValue) {
            if (newValue == isClosed) return;
            isClosed = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateStart(float3 newValue) {
            if (start.Equals(newValue)) return;
            start = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateControlA(float3 newValue) {
            if (controlA.Equals(newValue)) return;
            controlA = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateControlB(float3 newValue) {
            if (controlB.Equals(newValue)) return;
            controlB = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateEnd(float3 newValue) {
            if (end.Equals(newValue)) return;
            end = newValue;
            CalculateResult();
            NotifyPortValueChanged(ResultPort);
        }
    }
}