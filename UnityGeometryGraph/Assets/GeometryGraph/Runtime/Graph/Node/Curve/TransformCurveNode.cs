using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using Unity.Mathematics;

using IsClosedMode = GeometryGraph.Runtime.Graph.TransformCurveNode.TransformCurveNode_IsClosedMode;

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
        [Setting] public IsClosedMode ClosedMode { get; private set; } = TransformCurveNode_IsClosedMode.Unchanged;

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

                position.Add(math.mul(matrix, Input.Position[i].float4(1.0f)).xyz);
                tangent.Add(math.mul(matrix, Input.Tangent[i].float4()).xyz);
                normal.Add(math.mul(matrix, Input.Normal[i].float4()).xyz);
                binormal.Add(math.mul(matrix, Input.Binormal[i].float4()).xyz);
            }

            bool isClosed = ClosedMode switch {
                TransformCurveNode_IsClosedMode.Unchanged => Input.IsClosed,
                TransformCurveNode_IsClosedMode.Closed => true,
                TransformCurveNode_IsClosedMode.Open => false,
                TransformCurveNode_IsClosedMode.Variable => IsClosed,
                _ => throw new ArgumentOutOfRangeException()
            };

            Result = new CurveData(Input.Type, Input.Points, isClosed, position, tangent, normal, binormal);
        }

        public enum TransformCurveNode_IsClosedMode {
            Unchanged,
            Closed,
            Open,
            Variable
        }
    }
}