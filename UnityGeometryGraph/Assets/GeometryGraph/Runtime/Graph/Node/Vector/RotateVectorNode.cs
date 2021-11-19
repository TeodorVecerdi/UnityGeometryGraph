using System;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public class RotateVectorNode : RuntimeNode {
        private float3 vector;
        private float3 center;
        private float3 axis;
        private float3 eulerAngles;
        private float angle;

        private RotateVectorNode_Type type;

        public RuntimePort VectorPort { get; private set; }
        public RuntimePort CenterPort { get; private set; }
        public RuntimePort AxisPort { get; private set; }
        public RuntimePort EulerAnglesPort { get; private set; }
        public RuntimePort AnglePort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public RotateVectorNode(string guid) : base(guid) {
            VectorPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            CenterPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            AxisPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            EulerAnglesPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            AnglePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            ResultPort = RuntimePort.Create(PortType.Vector, PortDirection.Output, this);
        }

        public void UpdateType(RotateVectorNode_Type newType) {
            if (newType == type) return;
            type = newType;
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateValue(object value, RotateVectorNode_Which which) {
            switch (which) {
                case RotateVectorNode_Which.Vector: vector = (float3)value; break;
                case RotateVectorNode_Which.Center: center = (float3)value; break;
                case RotateVectorNode_Which.Axis: axis = (float3)value; break;
                case RotateVectorNode_Which.Euler: eulerAngles = (float3)value; break;
                case RotateVectorNode_Which.Angle: angle = (float)value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            NotifyPortValueChanged(ResultPort);
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            
            return Calculate();
        }

        private float3 Calculate() {
            return type switch {
                RotateVectorNode_Type.AxisAngle => math.rotate(quaternion.AxisAngle(axis, angle), vector - center) + center,
                RotateVectorNode_Type.Euler => math.rotate(quaternion.Euler(eulerAngles), vector - center) + center,
                RotateVectorNode_Type.X_Axis => math.rotate(quaternion.AxisAngle(float3_ext.right, angle), vector - center) + center,
                RotateVectorNode_Type.Y_Axis => math.rotate(quaternion.AxisAngle(float3_ext.up, angle), vector - center) + center,
                RotateVectorNode_Type.Z_Axis => math.rotate(quaternion.AxisAngle(float3_ext.forward, angle), vector - center) + center,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if(port == ResultPort) return;
            if (port == VectorPort) {
                vector = GetValue(VectorPort, vector);
                NotifyPortValueChanged(ResultPort);
            } else if (port == CenterPort) {
                center = GetValue(CenterPort, center);
                NotifyPortValueChanged(ResultPort);
            } else if (port == AxisPort) {
                axis = GetValue(AxisPort, axis);
                NotifyPortValueChanged(ResultPort);
            } else if (port == EulerAnglesPort) {
                eulerAngles = GetValue(EulerAnglesPort, eulerAngles);
                NotifyPortValueChanged(EulerAnglesPort);
            } else if (port == AnglePort) {
                var newAngle = GetValue(AnglePort, angle);
                if (Math.Abs(newAngle - angle) > 0.000001f) {
                    angle = newAngle;
                    NotifyPortValueChanged(ResultPort);
                }
            }
        }
        
        public override string GetCustomData() {
            var data = new JObject {
                ["t"] = (int)type,
                ["v"] = JsonConvert.SerializeObject(vector, float3Converter.Converter),
                ["c"] = JsonConvert.SerializeObject(center, float3Converter.Converter),
                ["x"] = JsonConvert.SerializeObject(axis, float3Converter.Converter),
                ["e"] = JsonConvert.SerializeObject(eulerAngles, float3Converter.Converter),
                ["a"] = angle,
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            type = (RotateVectorNode_Type)data.Value<int>("t");
            vector = JsonConvert.DeserializeObject<float3>(data.Value<string>("v"), float3Converter.Converter);
            center = JsonConvert.DeserializeObject<float3>(data.Value<string>("c"), float3Converter.Converter);
            axis = JsonConvert.DeserializeObject<float3>(data.Value<string>("x"), float3Converter.Converter);
            eulerAngles = JsonConvert.DeserializeObject<float3>(data.Value<string>("e"), float3Converter.Converter);
            angle = data.Value<float>("a");
            NotifyPortValueChanged(ResultPort);
        }

        public enum RotateVectorNode_Type {AxisAngle = 0, Euler = 1, X_Axis = 2, Y_Axis = 3, Z_Axis = 4}
        public enum RotateVectorNode_Which {Vector = 0, Center = 1, Axis = 2, Euler = 3, Angle = 4}
    }
}