using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Curve.Primitive {
    public sealed class QuadraticBezierCurve : Curve {
        protected internal override CurveType Type => CurveType.QuadraticBezier;
        private readonly float3 start;
        private readonly float3 control;
        private readonly float3 end;

        public QuadraticBezierCurve(int resolution, bool isClosed, float3 start, float3 control, float3 end) : base(resolution.Clamped(Constants.MIN_BEZIER_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION)) {
            IsClosed = isClosed;
            this.start = start;
            this.control = control;
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
            var job = new QuadraticBezierJob(points, tangents, normals, binormals, Resolution, start, control, end);
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

        [BurstCompile(CompileSynchronously = true)]
        private struct QuadraticBezierJob : ICurveJob {
            [WriteOnly] private NativeArray<float3> points;
            [WriteOnly] private NativeArray<float3> tangents;
            [WriteOnly] private NativeArray<float3> normals;
            [WriteOnly] private NativeArray<float3> binormals;

            private readonly int resolution;
            private readonly float3 start;
            private readonly float3 control;
            private readonly float3 end;

            public QuadraticBezierJob(NativeArray<float3> points, NativeArray<float3> tangents, NativeArray<float3> normals, NativeArray<float3> binormals, int resolution, float3 start, float3 control, float3 end) {
                this.points = points;
                this.tangents = tangents;
                this.normals = normals;
                this.binormals = binormals;
                this.resolution = resolution;
                this.start = start;
                this.control = control;
                this.end = end;
            }

            public void Execute(int index) {
                var t = index / (float)resolution;
                points[index] = Position(t);
                var tangent = Tangent(t);
                var binormal = Binormal(t);
                var normal = math.cross(tangent, binormal);
                tangents[index] = tangent;
                normals[index] = binormal;
                binormals[index] = normal;
            }

            public float3 Position(float t) {
                var t0 = (1.0f - t) * (1.0f - t);
                var t1 = t * t;
                var p0 = start - control;
                var p1 = end - control;
            
                return control + t0 * p0 + t1 * p1;
            }

            public float3 Tangent(float t) {
                // first derivative with respect to `t`
                return math.normalizesafe(2.0f * (1.0f - t) * (control - start) + 2.0f * t * (end - control), float3_util.forward);
            }

            public float3 Normal(float t) {
                // second derivative with respect to `t`
                return math.normalizesafe(2.0f * (end - 2.0f * control + start), float3_util.right);
            }

            public float3 Binormal(float t) {
                return math.normalizesafe(math.cross(Tangent(t), Normal(t)), float3_util.up);
            }
        }
    }
}