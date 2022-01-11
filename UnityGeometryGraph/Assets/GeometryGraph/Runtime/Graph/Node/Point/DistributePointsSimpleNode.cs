using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using GenerationMode = GeometryGraph.Runtime.Graph.DistributePointsSimpleNode.DistributePointsSimpleNode_GenerationMode;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class DistributePointsSimpleNode {
        [In] public GeometryData Geometry { get; private set; }
        [In] public int Seed { get; private set; } = 0;
        [In]
        [AdditionalValueChangedCode("{other} = {other}.Clamped(1, Constants.MAX_POINT_DISTRIBUTION_POINTS);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        public int Points { get; private set; } = 4;
        [In]
        [AdditionalValueChangedCode("{other} = {other}.Clamped(0.01f, Constants.MAX_POINT_DISTRIBUTION_RATIO);", Where = AdditionalValueChangedCodeAttribute.Location.AfterGetValue)]
        public float PointsRatio { get; private set; } = 4.0f;
        [Setting] public GenerationMode Mode { get; private set; } = GenerationMode.Constant;
        [Out] public GeometryData Result { get; private set; }

        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != GeometryPort) return;
            Geometry = GeometryData.Empty;
            Result = GeometryData.Empty;
        }

        [GetterMethod(nameof(Result), Inline = true)]
        private GeometryData GetResult() {
            if (Result == null) CalculateResult();
            return Result ??= GeometryData.Empty;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Geometry == null) {
                Result = GeometryData.Empty;
                return;
            }

            Result = GeometryData.Empty;
            Vector3Attribute positionAttribute = Geometry.GetAttribute<Vector3Attribute>(AttributeId.Position, AttributeDomain.Vertex);
            Debug.Assert(positionAttribute != null, nameof(positionAttribute) + " != null");

            Rand.PushState(Seed);
            List<float3> points = new();
            foreach (GeometryData.Face face in Geometry.Faces) {
                float3 p0 = positionAttribute[face.VertA];
                float3 p1 = positionAttribute[face.VertB];
                float3 p2 = positionAttribute[face.VertC];
                float3 v1 = p1 - p0;
                float3 v2 = p2 - p0;

                int pointCount = Points.Clamped(1, Constants.MAX_POINT_DISTRIBUTION_POINTS);
                if (Mode == GenerationMode.Ratio) {
                    pointCount = Mathf.CeilToInt(PointsRatio.Clamped(0.01f, Constants.MAX_POINT_DISTRIBUTION_RATIO) * CalculateArea(v1, v2)).Clamped(1, Constants.MAX_POINT_DISTRIBUTION_POINTS);
                }

                for (int i = 0; i < pointCount; i++) {
                    points.Add(GeneratePoint(p0, v1, v2));
                }
            }
            Rand.PopState();

            Result = new GeometryData(
                Enumerable.Empty<GeometryData.Edge>(), Enumerable.Empty<GeometryData.Face>(), Enumerable.Empty<GeometryData.FaceCorner>(),
                1, points, Enumerable.Empty<float3>(), Enumerable.Empty<int>(),
                Enumerable.Empty<bool>(), Enumerable.Empty<float2>()
            );
        }

        private float CalculateArea(float3 v1, float3 v2) {
            return 0.5f * math.length(math.cross(v1, v2));
        }

        private float3 GeneratePoint(float3 p0, float3 v1, float3 v2) {
            float a = Rand.Float;
            float b = Rand.Float;

            if (a + b >= 1.0f) {
                a = 1.0f - a;
                b = 1.0f - b;
            }

            return p0 + a * v1 + b * v2;
        }

        public enum DistributePointsSimpleNode_GenerationMode {
            Constant,
            Ratio,
        }
    }
}