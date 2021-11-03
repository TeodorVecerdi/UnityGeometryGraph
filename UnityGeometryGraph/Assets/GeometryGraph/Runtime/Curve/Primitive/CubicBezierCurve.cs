using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Curve.Primitive {
    public sealed class CubicBezierCurve : Curve {
        protected internal override CurveType Type => CurveType.CubicBezier;
        private float3 start;
        private float3 controlA;
        private float3 controlB;
        private float3 end;

        public CubicBezierCurve(int resolution, bool isClosed, float3 start, float3 controlA, float3 controlB, float3 end) : base(
            resolution.Clamped(Constants.MIN_BEZIER_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION)) {
            IsClosed = isClosed;
            this.start = start;
            this.controlA = controlA;
            this.controlB = controlB;
            this.end = end;
        }

        protected internal override int PointCount() {
            return Resolution + 1;
        }

        internal override void Generate() {
            var pointCount = PointCount();
            var points = new NativeArray<float3>(pointCount, Allocator.Persistent);
            var tangents = new NativeArray<float3>(pointCount, Allocator.Persistent);
            var normals = new NativeArray<float3>(pointCount, Allocator.Persistent);
            var binormals = new NativeArray<float3>(pointCount, Allocator.Persistent);
            var job = new CubicBezierJob(points, tangents, normals, binormals, Resolution, start, controlA, controlB, end);
            job.Schedule(pointCount, Environment.ProcessorCount).Complete();

            Points = new List<float3>(points);
            Tangents = new List<float3>(tangents);
            Normals = new List<float3>(normals);
            Binormals = new List<float3>(binormals);

            points.Dispose();
            tangents.Dispose();
            normals.Dispose();
            binormals.Dispose();

            IsInitialized = true;
        }

        private struct CubicBezierJob : ICurveJob {
            [WriteOnly] private NativeArray<float3> points;
            [WriteOnly] private NativeArray<float3> tangents;
            [WriteOnly] private NativeArray<float3> normals;
            [WriteOnly] private NativeArray<float3> binormals;

            private readonly int resolution;
            private float3 start;
            private float3 controlA;
            private float3 controlB;
            private float3 end;

            public CubicBezierJob(NativeArray<float3> points, NativeArray<float3> tangents, NativeArray<float3> normals, NativeArray<float3> binormals, int resolution,
                                  float3 start, float3 controlA, float3 controlB, float3 end) {
                this.points = points;
                this.tangents = tangents;
                this.normals = normals;
                this.binormals = binormals;
                this.resolution = resolution;
                this.start = start;
                this.controlA = controlA;
                this.controlB = controlB;
                this.end = end;
            }

            public void Execute(int index) {
                var t = index / (float)resolution;
                var tangent = Tangent(t);
                var binormal = Binormal(t);
                
                points[index] = Position(t);
                tangents[index] = tangent;
                normals[index] = binormal;
                binormals[index] = math.cross(tangent, binormal);
            }

            public float3 Position(float t) {
                var t0 = 1.0f - t;
                var t1 = t0 * t0;
                var t2 = t1 * t0;
                return t2 * start +
                       3.0f * t1 * t * controlA +
                       3.0f * t0 * t * t * controlB +
                       t * t * t * end;
            }

            public float3 Tangent(float t) {
                var t0 = 1.0f - t;
                var t1 = t0 * t0;
                var tangent = 3.0f * t1 * (controlA - start) +
                              6.0f * t0 * t * (controlB - controlA) +
                              3.0f * t * t * (end - controlB);
                return math.normalizesafe(tangent, float3_ext.forward);
            }

            public float3 Normal(float t) {
                var normal = 6.0f * (1.0f - t) * (controlB - 2.0f * controlA + start) + 
                             6.0f * t * (end - 2.0f * controlB + controlA);
                return math.normalizesafe(normal, float3_ext.right);
            }

            public float3 Binormal(float t) {
                return math.normalizesafe(math.cross(Tangent(t), Normal(t)), float3_ext.up);
            }
        }
    }
}