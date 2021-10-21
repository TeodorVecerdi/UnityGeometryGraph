using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Curve.Primitive {
    public sealed class CircleCurve : Curve {
        protected internal override CurveType Type => CurveType.Circle;
        private readonly float radius;
        
        public CircleCurve(int resolution, float radius) : base(resolution.Clamped(Constants.MIN_CIRCLE_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION)) {
            this.radius = radius.Min(Constants.MIN_CIRCULAR_CURVE_RADIUS);
            IsClosed = true;
        }

        protected internal override int PointCount() {
            return Resolution;
        }

        internal override void Generate() {
            var pointCount = PointCount();
            var points = new NativeArray<float3>(pointCount, Allocator.Persistent);
            var tangents = new NativeArray<float3>(pointCount, Allocator.Persistent);
            var normals = new NativeArray<float3>(pointCount, Allocator.Persistent);
            var binormals = new NativeArray<float3>(pointCount, Allocator.Persistent);
            var job = new CircleJob(points, tangents, normals, binormals, Resolution, radius);
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
        private struct CircleJob : ICurveJob {
            [WriteOnly] private NativeArray<float3> points;
            [WriteOnly] private NativeArray<float3> tangents;
            [WriteOnly] private NativeArray<float3> normals;
            [WriteOnly] private NativeArray<float3> binormals;

            private readonly int resolution;
            private readonly float radius;

            public CircleJob(NativeArray<float3> points, NativeArray<float3> tangents, NativeArray<float3> normals, NativeArray<float3> binormals, int resolution, float radius) {
                this.points = points;
                this.tangents = tangents;
                this.normals = normals;
                this.binormals = binormals;
                this.resolution = resolution;
                this.radius = radius;
            }

            public void Execute(int index) {
                var t = index / (float)resolution;
                points[index] = Position(t);
                tangents[index] = Tangent(t);
                normals[index] = Normal(t);
                binormals[index] = float3_util.up;
            }

            public float3 Position(float t) {
                var angle = math_util.TWO_PI * t;
                return new float3(radius * math.cos(angle), 0.0f, radius * math.sin(angle));
            }

            private float Slope(float t) {
                var angle = math_util.TWO_PI * t;
                var x = math.cos(angle);
                var z = math.sin(angle);
                if (z == 0.0f) return t > 0.5f ? float.PositiveInfinity : float.NegativeInfinity;
                return -x / z;
            }

            public float3 Tangent(float t) {
                var slope = Slope(t);
                if (float.IsPositiveInfinity(slope)) return -float3_util.forward;
                if (float.IsNegativeInfinity(slope)) return float3_util.forward;

                var tangent = math.normalize(new float3(1.0f, 0.0f, slope));
                if (t >= 0.5) return tangent;
                return -tangent;
            }

            public float3 Normal(float t) {
                var tangent = Tangent(t);
                return new float3(tangent.z, 0.0f, -tangent.x);
            }

            [BurstDiscard]
            public float3 Binormal(float t) {
                throw new NotImplementedException("The binormal is constant for a circle. Use float3_util.up instead.");
            }
        }
    }
}