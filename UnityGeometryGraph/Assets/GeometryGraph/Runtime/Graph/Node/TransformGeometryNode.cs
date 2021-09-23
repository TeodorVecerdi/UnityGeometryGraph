using System;
using System.Linq;
using GeometryGraph.Runtime.Attribute;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public class TransformGeometryNode : RuntimeNode {
        private GeometryData result;
        private float3 defaultTranslation = float3.zero;
        private float3 defaultRotation = float3.zero;
        private float3 defaultScale = float3_util.one;

        public RuntimePort InputGeometryPort { get; private set; }
        public RuntimePort TranslationPort { get; private set; }
        public RuntimePort RotationPort { get; private set; }
        public RuntimePort ScalePort { get; private set; }
        public RuntimePort OutputGeometryPort { get; private set; }

        public TransformGeometryNode(string guid) : base(guid) {
            InputGeometryPort = RuntimePort.Create(PortType.Geometry, PortDirection.Input, this);
            
            TranslationPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            RotationPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ScalePort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            
            OutputGeometryPort = RuntimePort.Create(PortType.Geometry, PortDirection.Output, this);
        }

        public void UpdateDefaultValue(float3 value, WhichDefaultValue whichDefaultValue) {
            switch (whichDefaultValue) {
                case WhichDefaultValue.Translation: defaultTranslation = value; break;
                case WhichDefaultValue.Rotation: defaultRotation = value; break;
                case WhichDefaultValue.Scale: defaultScale = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(whichDefaultValue), whichDefaultValue, null);
            }

            NotifyPortValueChanged(OutputGeometryPort);
        }

        public override object GetValueForPort(RuntimePort port) {
            if (port != OutputGeometryPort) return null;
            CalculateResult();
            return result;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            NotifyPortValueChanged(OutputGeometryPort);
        }
        
        public override void RebindPorts() {
            InputGeometryPort = Ports[0];
            TranslationPort = Ports[1];
            RotationPort = Ports[2];
            ScalePort = Ports[3];
            OutputGeometryPort = Ports[4];
        }

        private void CalculateResult() {
            var translation = GetValue(TranslationPort, defaultTranslation);
            var rotation = GetValue(RotationPort, defaultRotation);
            var scale = GetValue(ScalePort, defaultScale);
            var trs = float4x4.TRS(translation, quaternion.EulerXYZ(math.radians(rotation)), scale);
            result = GetValue(InputGeometryPort, GeometryData.Empty);
            
            var positionAttribute = result.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            positionAttribute.Yield(pos => math.mul(trs, new float4(pos, 1.0f)).xyz).Into(positionAttribute);
        }
        
        public enum WhichDefaultValue {Translation = 0, Rotation = 1, Scale = 2}
    }
}