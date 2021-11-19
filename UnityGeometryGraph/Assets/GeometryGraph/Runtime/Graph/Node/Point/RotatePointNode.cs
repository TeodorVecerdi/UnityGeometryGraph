using System;
using System.Linq;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public class RotatePointNode : RuntimeNode {
        private GeometryData geometry;
        private float3 rotation;
        private string rotationAttribute;
        private float3 axis;
        private string axisAttribute;
        private float angle;
        private string angleAttribute;
        
        private RotatePointNode_RotationMode rotationMode;
        private RotatePointNode_AxisMode axisMode;
        private RotatePointNode_AngleMode angleMode;
        private RotatePointNode_RotationType rotationType = RotatePointNode_RotationType.Euler;
        
        private GeometryData result;
        
        public RuntimePort InputPort { get; private set; }
        public RuntimePort RotationPort { get; private set; }
        public RuntimePort AxisPort { get; private set; }
        public RuntimePort AnglePort { get; private set; }
        public RuntimePort RotationAttributePort { get; private set; }
        public RuntimePort AxisAttributePort { get; private set; }
        public RuntimePort AngleAttributePort { get; private set; }
        public RuntimePort ResultPort { get; private set; }

        public RotatePointNode(string guid) : base(guid) {
            InputPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            
            RotationPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            AxisPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            AnglePort = RuntimePort.Create(PortType.Float, PortDirection.Input, this);
            
            RotationAttributePort = RuntimePort.Create(PortType.String, PortDirection.Input, this);
            AxisAttributePort = RuntimePort.Create(PortType.String, PortDirection.Input, this);
            AngleAttributePort = RuntimePort.Create(PortType.String, PortDirection.Input, this);
            
            ResultPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateRotationType(RotatePointNode_RotationType newType) {
            if(rotationType == newType) return;

            rotationType = newType;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }
        public void UpdateRotationMode(RotatePointNode_RotationMode newMode) {
            if(rotationMode == newMode) return;

            rotationMode = newMode;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }
        
        public void UpdateAxisMode(RotatePointNode_AxisMode newMode) {
            if(axisMode == newMode) return;

            axisMode = newMode;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }
        
        public void UpdateAngleMode(RotatePointNode_AngleMode newMode) {
            if(angleMode == newMode) return;

            angleMode = newMode;
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public void UpdateValue(object value, RotatePointNode_Which which) {
            switch (which) {
                case RotatePointNode_Which.RotationVector: rotation = (float3)value; break;
                case RotatePointNode_Which.RotationAttribute: rotationAttribute = (string)value; break;
                case RotatePointNode_Which.AxisVector: axis = (float3)value; break;
                case RotatePointNode_Which.AxisAttribute: axisAttribute = (string)value; break;
                case RotatePointNode_Which.AngleFloat: angle = (float)value; break;
                case RotatePointNode_Which.AngleAttribute: angleAttribute = (string)value; break;
                default: throw new ArgumentOutOfRangeException(nameof(which), which, null);
            }

            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != InputPort) return;
            geometry = GeometryData.Empty;
            result = GeometryData.Empty;
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultPort) return null;
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if(port == ResultPort) return;
            
            if (port == InputPort) {
                geometry = GetValue(connection, geometry);
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == RotationPort) {
                rotation = GetValue(connection, rotation);
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == AxisPort) {
                axis = GetValue(connection, axis);
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == AnglePort) {
                var newValue = GetValue(connection, angle);
                if (Math.Abs(newValue - angle) < 0.000001f) return;
                
                angle = newValue;
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == RotationAttributePort) {
                var newValue = GetValue(connection, rotationAttribute);
                if (string.Equals(newValue, rotationAttribute, StringComparison.InvariantCulture)) return;
                
                rotationAttribute = newValue;
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == AxisAttributePort) {
                var newValue = GetValue(connection, axisAttribute);
                if (string.Equals(newValue, axisAttribute, StringComparison.InvariantCulture)) return;
                
                axisAttribute = newValue;
                Calculate();
                NotifyPortValueChanged(ResultPort);
            } else if (port == AngleAttributePort) {
                var newValue = GetValue(connection, angleAttribute);
                if (string.Equals(newValue, angleAttribute, StringComparison.InvariantCulture)) return;
                
                angleAttribute = newValue;
                Calculate();
                NotifyPortValueChanged(ResultPort);
            }
        }
        
        public override void RebindPorts() {
            InputPort = Ports[0];
            RotationPort = Ports[1];
            AxisPort = Ports[2];
            AnglePort = Ports[3];
            RotationAttributePort = Ports[4];
            AxisAttributePort = Ports[5];
            AngleAttributePort = Ports[6];
            ResultPort = Ports[7];
        }

        public void Calculate() {
            if (geometry == null) return;
            result = geometry.Clone();
            var rotAttribute = result.GetAttributeOrDefault<Vector3Attribute, float3>("rotation", AttributeDomain.Vertex, float3.zero);
            if (rotationType == RotatePointNode_RotationType.Euler) {
                var tmpAttribute = rotationMode == RotatePointNode_RotationMode.Vector 
                    ? Enumerable.Repeat(rotation, rotAttribute.Count).Into<Vector3Attribute>("tmp", AttributeDomain.Vertex) 
                    : result.GetAttributeOrDefault<Vector3Attribute, float3>(rotationAttribute, AttributeDomain.Vertex, float3.zero);
                
                rotAttribute.YieldWithAttribute(tmpAttribute, (rot, euler) => math.rotate(quaternion.Euler(euler), rot)).Into(rotAttribute);
            } else {
                var axisAttribute = axisMode == RotatePointNode_AxisMode.Vector 
                    ? Enumerable.Repeat(axis, rotAttribute.Count).Into<Vector3Attribute>("axisAttribute", AttributeDomain.Vertex) 
                    : result.GetAttributeOrDefault<Vector3Attribute, float3>(this.axisAttribute, AttributeDomain.Vertex, float3_ext.up);
                var angleAttribute = angleMode == RotatePointNode_AngleMode.Float
                    ? Enumerable.Repeat(angle, rotAttribute.Count).Into<FloatAttribute>("angleAttribute", AttributeDomain.Vertex)
                    : result.GetAttributeOrDefault<FloatAttribute, float>(this.angleAttribute, AttributeDomain.Vertex, 0.0f);
                rotAttribute.YieldWithAttribute(axisAttribute, angleAttribute,
                                                     (rot, axis, angle) => math.rotate(quaternion.AxisAngle(axis, angle), rot))
                                 .Into(rotAttribute);
            }
            result.StoreAttribute(rotAttribute);
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["0"] = JsonConvert.SerializeObject(rotation, Formatting.None, float3Converter.Converter),
                ["1"] = JsonConvert.SerializeObject(axis, Formatting.None, float3Converter.Converter),
                ["2"] = angle,
                ["3"] = rotationAttribute,
                ["4"] = axisAttribute,
                ["5"] = angleAttribute,
                ["6"] = (int)rotationMode,
                ["7"] = (int)axisMode,
                ["8"] = (int)angleMode,
                ["9"] = (int)rotationType,
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            rotation = JsonConvert.DeserializeObject<float3>(data.Value<string>("0")!, float3Converter.Converter);
            axis = JsonConvert.DeserializeObject<float3>(data.Value<string>("1")!, float3Converter.Converter);
            angle = data.Value<float>("2");
            rotationAttribute = data.Value<string>("3");
            axisAttribute = data.Value<string>("4");
            angleAttribute = data.Value<string>("5");
            rotationType = (RotatePointNode_RotationType) data.Value<int>("6");
            rotationMode = (RotatePointNode_RotationMode) data.Value<int>("7");
            axisMode = (RotatePointNode_AxisMode) data.Value<int>("8");
            angleMode = (RotatePointNode_AngleMode) data.Value<int>("9");
           
            Calculate();
            NotifyPortValueChanged(ResultPort);
        }

        public enum RotatePointNode_Which {RotationVector = 0, RotationAttribute = 1, AxisVector = 2, AxisAttribute = 3, AngleFloat = 4, AngleAttribute = 5}
        public enum RotatePointNode_RotationType {AxisAngle = 0, Euler = 1}
        public enum RotatePointNode_RotationMode {Vector = 0, Attribute = 1}
        public enum RotatePointNode_AxisMode {Vector = 0, Attribute = 1}
        public enum RotatePointNode_AngleMode {Float = 0, Attribute = 1}
    }
}