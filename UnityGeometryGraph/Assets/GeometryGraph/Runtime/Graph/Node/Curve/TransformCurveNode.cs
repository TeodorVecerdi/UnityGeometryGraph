using System.Collections.Generic;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    public class TransformCurveNode : RuntimeNode{
        private CurveData inputCurve;
        private float3 translation;
        private float3 rotation;
        private float3 scale;
        private bool isClosed;
        private CurveData resultCurve;
        private bool changeClosed;

        public RuntimePort InputCurvePort { get; private set; }
        public RuntimePort TranslationPort { get; private set; }
        public RuntimePort RotationPort { get; private set; }
        public RuntimePort ScalePort { get; private set; }
        public RuntimePort IsClosedPort { get; private set; }
        public RuntimePort ResultCurvePort { get; private set; }

        public TransformCurveNode(string guid) : base(guid) {
            InputCurvePort = RuntimePort.Create(PortType.Curve, PortDirection.Input, this);
            
            TranslationPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            RotationPort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            ScalePort = RuntimePort.Create(PortType.Vector, PortDirection.Input, this);
            IsClosedPort = RuntimePort.Create(PortType.Boolean, PortDirection.Input, this);
            
            ResultCurvePort = RuntimePort.Create(PortType.Curve, PortDirection.Output, this);
        }

        public void UpdateTranslation(float3 newTranslation) {
            translation = newTranslation;
            CalculateResult();
            NotifyPortValueChanged(ResultCurvePort);
        }
        
        public void UpdateRotation(float3 newRotation) {
            rotation = newRotation;
            CalculateResult();
            NotifyPortValueChanged(ResultCurvePort);
        }
        
        public void UpdateScale(float3 newScale) {
            scale = newScale;
            CalculateResult();
            NotifyPortValueChanged(ResultCurvePort);
        }
        
        public void UpdateIsClosed(bool newIsClosed) {
            if (newIsClosed == isClosed) return;
            
            isClosed = newIsClosed;
            if (resultCurve != null) {
                resultCurve.IsClosed = isClosed;
            } else {
                CalculateResult();
            }
            NotifyPortValueChanged(ResultCurvePort);
        }

        public void UpdateChangeClosed(bool newChangeClosed) {
            if (newChangeClosed == changeClosed) return;
            
            changeClosed = newChangeClosed;
            CalculateResult();
            NotifyPortValueChanged(ResultCurvePort);
        }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != InputCurvePort) return;
            resultCurve = null;
        }

        protected override object GetValueForPort(RuntimePort port) {
            if (port != ResultCurvePort) return null;
            return resultCurve ?? CurveData.Empty;
        }

        protected override void OnPortValueChanged(Connection connection, RuntimePort port) {
            if (port == ResultCurvePort) return;
            if (port == InputCurvePort) {
                inputCurve = GetValue(connection, (CurveData)null);
                CalculateResult();
                NotifyPortValueChanged(ResultCurvePort);
            } else if (port == TranslationPort) {
                translation = GetValue(connection, translation);
                CalculateResult();
                NotifyPortValueChanged(ResultCurvePort);
            } else if (port == RotationPort) {
                rotation = GetValue(connection, rotation);
                CalculateResult();
                NotifyPortValueChanged(ResultCurvePort);
            } else if (port == ScalePort) {
                scale = GetValue(connection, scale);
                CalculateResult();
                NotifyPortValueChanged(ResultCurvePort);
            } else if (port == IsClosedPort) {
                bool newIsClosed = GetValue(connection, isClosed);
                if (newIsClosed == isClosed) return;
                isClosed = newIsClosed;
                if (resultCurve != null) {
                    resultCurve.IsClosed = isClosed;
                } else {
                    CalculateResult();
                }
                NotifyPortValueChanged(ResultCurvePort);
            }
        }

        private void CalculateResult() {
            if (inputCurve == null) {
                resultCurve = null;
                return;
            }

            var rotationQuat = quaternion.Euler(math.radians(rotation));
            var matrix = float4x4.TRS(translation, rotationQuat, scale);
            var matrixNormal = float4x4.TRS(float3.zero, rotationQuat, scale);

            var position = new List<float3>(inputCurve.Points);
            var tangent = new List<float3>(inputCurve.Points);
            var normal = new List<float3>(inputCurve.Points);
            var binormal = new List<float3>(inputCurve.Points);
            
            for (var i = 0; i < inputCurve.Points; i++) {
                position.Add(math.mul(matrix, inputCurve.Position[i].float4(1.0f)).xyx);
                tangent.Add(math.mul(matrixNormal, inputCurve.Tangent[i].float4(1.0f)).xyx);
                normal.Add(math.mul(matrixNormal, inputCurve.Normal[i].float4(1.0f)).xyx);
                binormal.Add(math.mul(matrixNormal, inputCurve.Binormal[i].float4(1.0f)).xyx);
            }
            
            resultCurve = new CurveData(inputCurve.Type, inputCurve.Points, changeClosed ? isClosed : inputCurve.IsClosed, position, tangent, normal, binormal);
        }

        public override void RebindPorts() {
            throw new System.NotImplementedException();
        }

        public override string GetCustomData() {
            return new JArray {
                JsonConvert.SerializeObject(translation, Formatting.None, float3Converter.Converter),
                JsonConvert.SerializeObject(rotation, Formatting.None, float3Converter.Converter),
                JsonConvert.SerializeObject(scale, Formatting.None, float3Converter.Converter),
                isClosed ? 1 : 0
            }.ToString(Formatting.None);
        }

        public override void SetCustomData(string json) {
            JArray array = JArray.Parse(json);
            translation = JsonConvert.DeserializeObject<float3>(array.Value<string>(0), float3Converter.Converter);
            rotation = JsonConvert.DeserializeObject<float3>(array.Value<string>(1), float3Converter.Converter);
            scale = JsonConvert.DeserializeObject<float3>(array.Value<string>(2), float3Converter.Converter);
            isClosed = array.Value<int>(3) == 1;
            
            CalculateResult();
            NotifyPortValueChanged(ResultCurvePort);
        }
    }
}