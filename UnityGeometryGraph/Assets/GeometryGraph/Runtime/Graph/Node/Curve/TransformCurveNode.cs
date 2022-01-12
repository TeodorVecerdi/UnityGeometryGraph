using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class TransformCurveNode {
        [In(
            DefaultValue = "(CurveData)null",
            GetValueCode = "{self} = GetValue(connection, {default})",
            UpdateValueCode = ""
        )]
        public CurveData Input { get; private set; }

        [AdditionalValueChangedCode(
            @"if (Result != null) {
{indent}    Result.IsClosed = IsClosed;
{indent}} else {
{indent}    CalculateResult();
{indent}}",
            Where = AdditionalValueChangedCodeAttribute.Location.AfterUpdate
        )]
        [In(CallCalculateMethodsIfChanged = false)]
        public bool IsClosed { get; private set; }

        [In] public float3 Translation { get; private set; }
        [In] public float3 Rotation { get; private set; }
        [In] public float3 Scale { get; private set; }
        [Setting] public bool ChangeClosed { get; private set; }

        [Out] public CurveData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true)]
        private CurveData GetResult() => Result ?? CurveData.Empty;

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != InputPort) return;
            Result = null;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Input == null) {
                Result = null;
                return;
            }
            
            List<float3> translation = GetValues(TranslationPort, Input.Points, Translation).ToList();
            List<float3> rotation = GetValues(RotationPort, Input.Points, Rotation).ToList();
            List<float3> scale = GetValues(ScalePort, Input.Points, Scale).ToList();

            List<float3> position = new(Input.Points);
            List<float3> tangent = new(Input.Points);
            List<float3> normal = new(Input.Points);
            List<float3> binormal = new(Input.Points);

            for (int i = 0; i < Input.Points; i++) {
                quaternion rotationQuat = quaternion.Euler(math.radians(rotation[i]));
                float4x4 matrix = float4x4.TRS(translation[i], rotationQuat, scale[i]);
                float4x4 matrixNormal = float4x4.TRS(float3.zero, rotationQuat, scale[i]);

                position.Add(math.mul(matrix, Input.Position[i].float4(1.0f)).xyz);
                tangent.Add(math.mul(matrixNormal, Input.Tangent[i].float4(1.0f)).xyz);
                normal.Add(math.mul(matrixNormal, Input.Normal[i].float4(1.0f)).xyz);
                binormal.Add(math.mul(matrixNormal, Input.Binormal[i].float4(1.0f)).xyz);
            }

            Result = new CurveData(Input.Type, Input.Points, ChangeClosed ? IsClosed : Input.IsClosed, position, tangent, normal, binormal);
        }
    }
}