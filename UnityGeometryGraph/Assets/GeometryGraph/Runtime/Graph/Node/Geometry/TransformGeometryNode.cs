using System;
using GeometryGraph.Runtime.Attribute;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

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
            var rotQuaternion = quaternion.Euler(math.radians(rotation));
            var trs = float4x4.TRS(translation, rotQuaternion, scale);
            var trsNormal = float4x4.TRS(float3.zero, rotQuaternion, scale);
            result = (GeometryData) GetValue(InputGeometryPort, GeometryData.Empty).Clone();
            
            var positionAttribute = result.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            positionAttribute.Yield(pos => math.mul(trs, new float4(pos, 1.0f)).xyz).Into(positionAttribute);
            var normalAttribute = result.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face);
            normalAttribute.Yield(normal => math.normalize(math.mul(trsNormal, new float4(normal, 1.0f)).xyz)).Into(normalAttribute);
        }

        public override string GetCustomData() {
            var data = new JObject {
                ["t"] = JsonConvert.SerializeObject(defaultTranslation, Formatting.None, float3Converter.Converter),
                ["r"] = JsonConvert.SerializeObject(defaultRotation, Formatting.None, float3Converter.Converter),
                ["s"] = JsonConvert.SerializeObject(defaultScale, Formatting.None, float3Converter.Converter),
            };
            return data.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            if(string.IsNullOrEmpty(json)) return;
            
            var data = JObject.Parse(json);
            defaultTranslation = JsonConvert.DeserializeObject<float3>(data.Value<string>("t"), float3Converter.Converter);
            defaultRotation = JsonConvert.DeserializeObject<float3>(data.Value<string>("r"), float3Converter.Converter);
            defaultScale = JsonConvert.DeserializeObject<float3>(data.Value<string>("s"), float3Converter.Converter);
            NotifyPortValueChanged(OutputGeometryPort);
        }

        public enum WhichDefaultValue {Translation = 0, Rotation = 1, Scale = 2}
    }
}