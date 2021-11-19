using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Curve.Primitive {
    public sealed class HelixCurve : Curve {
        protected internal override CurveType Type => CurveType.Helix;
        private float time;
        private float topRadius;
        private float bottomRadius;
        private float b;
        
        public HelixCurve(int resolution, float rotations, float pitch, float topRadius, float bottomRadius) : base(resolution.Clamped(Constants.MIN_HELIX_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION)) {
            time = rotations * math_ext.TWO_PI;
            b = pitch / math_ext.TWO_PI;
            this.topRadius = topRadius.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS);
            this.bottomRadius = bottomRadius.MinClamped(Constants.MIN_CIRCULAR_CURVE_RADIUS);
            
            IsClosed = false;
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
            var job = new HelixCurveJob(points, tangents, normals, binormals, Resolution, topRadius, bottomRadius, time, b);
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
        private struct HelixCurveJob : ICurveJob {
            [WriteOnly] private NativeArray<float3> points;
            [WriteOnly] private NativeArray<float3> tangents;
            [WriteOnly] private NativeArray<float3> normals;
            [WriteOnly] private NativeArray<float3> binormals;

            private readonly int resolution;
            private readonly float topRadius;
            private readonly float bottomRadius;
            private readonly float time;
            private readonly float b;
            
            public HelixCurveJob(NativeArray<float3> points, NativeArray<float3> tangents, NativeArray<float3> normals, NativeArray<float3> binormals, int resolution, float topRadius, float bottomRadius, float time, float b) {
                this.points = points;
                this.tangents = tangents;
                this.normals = normals;
                this.binormals = binormals;
                this.resolution = resolution;
                this.topRadius = topRadius;
                this.bottomRadius = bottomRadius;
                this.time = time;
                this.b = b;
            }

            public void Execute(int index) {
                var t = index / (float)resolution;
                
                points[index] = Position(t * time);
                tangents[index] = Tangent(t * time);
                normals[index] = Normal(t * time);
                binormals[index] = Binormal(t * time);
            }

            public float3 Position(float t) {
                var radius = math.lerp(bottomRadius, topRadius, t / time);
                math.sincos(t, out var sin, out var cos);
                return new float3(radius * cos, b * t, radius * sin);
            }

            public float3 Tangent(float t) {
                var radius = math.lerp(bottomRadius, topRadius, t / time);
                math.sincos(t, out var sin, out var cos);
                return math.normalizesafe(new float3(-radius * sin, b, radius * cos), float3_ext.forward);
            }

            public float3 Normal(float t) {
                var radius = math.lerp(bottomRadius, topRadius, t / time);
                math.sincos(t, out var sin, out var cos);
                return math.normalizesafe(new float3(radius * cos, 0.0f, radius * sin), float3_ext.right);
            }

            public float3 Binormal(float t) {
                return math.cross(Tangent(t), Normal(t));
            }
        }
    }
}