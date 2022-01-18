using System;
using System.Collections.Generic;
using GeometryGraph.Runtime.Curve;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityCommons;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GeometryGraph.Runtime.Testing {
    public class IncrementalRotationCurveTest : SerializedMonoBehaviour {
        [Title("References")]
        [Required] public ICurveProvider Circle;
        [Required] public CurveVisualizer VisualizerA;
        [Required] public CurveVisualizer VisualizerB;

        [Title("Join Settings")]
        public bool ShowLines;
        public bool FillFaces;
        public float LineWidth = 2.0f;
        public Color LineColor;
        public bool SmartJoin;
        public bool SmartJoinLine;
        [Range(0.0f, 2.0f)] public float WiggleRoom = 0.0f;
        public bool AutoWiggleRoom;
        [Range(-2.0f, 32.0f)] public float AutoWiggleRoomDelta = 0.0f;

        [Title("Settings")]
        [OnValueChanged(nameof(__OnIncrementalRotationOffsetChanged))]
        public float IncrementalRotationOffset;
        [OnValueChanged(nameof(__OnIncrementalRotationOffsetChanged))]
        public Vector3 Normal;
        [OnValueChanged(nameof(__OnIncrementalRotationOffsetChanged))]
        public Vector3 Tangent;
        [OnValueChanged(nameof(__OnIncrementalRotationOffsetChanged))]
        public Vector3 Binormal;
        [Space]
        [OnValueChanged(nameof(__OnIncrementalRotationOffsetChanged))]
        public Vector3 Offset;
        [OnValueChanged(nameof(__OnIncrementalRotationOffsetChanged))]
        public Vector3 OffsetB;

        [SerializeField] private CurveData a;
        [SerializeField] private CurveData b;

        [Button]
        public void Generate() {
            if (Circle == null || VisualizerA == null || VisualizerB == null) return;

            a = AlignCurve(Circle.Curve, Offset, 0);
            b = AlignCurve(Circle.Curve, Offset + OffsetB, IncrementalRotationOffset);

            VisualizerA.Load(a);
            VisualizerB.Load(b);
        }

        private void __OnIncrementalRotationOffsetChanged() {
            Generate();
        }

        private CurveData AlignCurve(CurveData source, float3 offset, float rotation) {
            var alignMat = new float4x4(((float3)Normal).float4(), ((float3)Tangent).float4(), ((float3)Binormal).float4(), offset.float4(1.0f));
            var matrix = math.mul(alignMat, float4x4.RotateY(math.radians(rotation)));

            var position = new List<float3>();
            var tangent = new List<float3>();
            var normal = new List<float3>();
            var binormal = new List<float3>();

            for (var i = 0; i < source.Points; i++) {
                position.Add(math.mul(matrix, source.Position[i].float4(1.0f)).xyz);
                tangent.Add(math.mul(matrix, source.Tangent[i].float4()).xyz);
                normal.Add(math.mul(matrix, source.Normal[i].float4()).xyz);
                binormal.Add(math.mul(matrix, source.Binormal[i].float4()).xyz);
            }

            return new CurveData(source.Type, source.Points, source.IsClosed, position, tangent, normal, binormal);
        }

        private void OnDrawGizmos() {
            if((!ShowLines && !FillFaces) || a == null || b == null) return;

            Handles.matrix = transform.localToWorldMatrix;
            Random.InitState(0);
            if (SmartJoin) {
                for (var i = 0; i < a.Points; i++) {
                    int bIndex = i;
                    int bIndex2 = (i + 1).mod(b.Points);
                    if (SmartJoinLine) {
                        if (FlipLineSmartJoin()) {
                            bIndex =  (b.Points - i - 1).mod(b.Points);
                            bIndex2 = (b.Points - i - 2).mod(b.Points);
                        }
                    } else {
                        bIndex = (i + CalculateIndexOffset()).mod(b.Points);
                        bIndex2 = (bIndex + 1).mod(b.Points);
                    }
                    if (ShowLines) {
                        Handles.color = LineColor;
                        Handles.DrawAAPolyLine(LineWidth, a.Position[i], b.Position[bIndex]);
                    }
                    if (FillFaces) {
                        if (!a.IsClosed && i == a.Points - 1) continue;
                        Handles.color = Random.ColorHSV(0.0f, 1.0f, 0.75f, 1.0f, 0.75f, 1.0f);
                        Handles.DrawAAConvexPolygon(a.Position[i], a.Position[(i + 1).mod(a.Points)], b.Position[bIndex]);
                        Handles.DrawAAConvexPolygon(b.Position[bIndex], a.Position[(i + 1).mod(a.Points)], b.Position[bIndex2]);
                    }
                }
            } else {
                for (var i = 0; i < a.Points; i++) {
                    if (ShowLines) {
                        Handles.color = LineColor;
                        Handles.DrawAAPolyLine(LineWidth, a.Position[i], b.Position[i]);
                    }
                    if (FillFaces) {
                        if (!a.IsClosed && i == a.Points - 1) continue;
                        Handles.color = Random.ColorHSV(0.0f, 1.0f, 0.75f, 1.0f, 0.75f, 1.0f);
                        Handles.DrawAAConvexPolygon(a.Position[i], a.Position[(i + 1).mod(a.Points)], b.Position[i]);
                        Handles.DrawAAConvexPolygon(b.Position[i], a.Position[(i + 1).mod(a.Points)], b.Position[(i + 1).mod(b.Points)]);
                    }
                }
            }
        }

        private bool FlipLineSmartJoin() {
            return math.fmod(IncrementalRotationOffset, 360.0f).Between(90.0f, 270.0f);
        }

        private int CalculateIndexOffset() {
            float wiggle = WiggleRoom + 1.0f;
            if (AutoWiggleRoom) wiggle = MathF.Sqrt(a.Points) + AutoWiggleRoomDelta;
            float degreesPerPoint = wiggle * 360.0f / (a.Points);
            float nearest = IncrementalRotationOffset.RoundedTo(degreesPerPoint);
            return ((int)(wiggle * nearest / degreesPerPoint)).mod(a.Points);
        }
    }
}