using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityCommons;

namespace GeometryGraph.Runtime.Curve.Primitive {
    public sealed class LineCurve : Curve {
        protected internal override CurveType Type => CurveType.Line;
        private readonly float3 start;
        private readonly float3 end;

        public LineCurve(int resolution, float3 start, float3 end) : base(resolution.Clamped(Constants.MIN_LINE_CURVE_RESOLUTION, Constants.MAX_CURVE_RESOLUTION)) {
            this.start = start;
            this.end = end;
        }


        protected internal override int PointCount() {
            return Resolution + 1;
        }

        internal override void Generate() {
            int pointCount = PointCount();
            NativeArray<float3> points = new(pointCount, Allocator.Persistent);
            NativeArray<float3> tangents = new(pointCount, Allocator.Persistent);
            NativeArray<float3> normals = new(pointCount, Allocator.Persistent);
            NativeArray<float3> binormals = new(pointCount, Allocator.Persistent);
            LineJob job = new(points, tangents, normals, binormals, Resolution, start, end);
            job.Schedule(pointCount, Environment.ProcessorCount).Complete();

            Points = new List<float3>(points);
            Tangents = new List<float3>(tangents);
            Normals = new List<float3>(normals);
            Binormals = new List<float3>(binormals);
            
            points.Dispose();
            tangents.Dispose();
            normals.Dispose();
            binormals.Dispose();
            
            IsClosed = false;
            IsInitialized = true;
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct LineJob : ICurveJob {
            [WriteOnly] private NativeArray<float3> points;
            [WriteOnly] private NativeArray<float3> tangents;
            [WriteOnly] private NativeArray<float3> normals;
            [WriteOnly] private NativeArray<float3> binormals;

            private readonly int resolution;
            private readonly float3 start; 
            private readonly float3 end;
            
            private readonly float3 tangent;
            private readonly float3 normal;
            private readonly float3 binormal; 

            public LineJob(NativeArray<float3> points, NativeArray<float3> tangents, NativeArray<float3> normals, NativeArray<float3> binormals, int resolution, float3 start, float3 end) {
                this.points = points;
                this.tangents = tangents;
                this.normals = normals;
                this.binormals = binormals;
                this.resolution = resolution;
                this.start = start;
                this.end = end;
                
                tangent = math.normalizesafe(end - start, float3_ext.forward);
                binormal = math.normalizesafe(math.cross(math.cross(tangent, float3_ext.up), tangent), float3_ext.up);
                normal = math.normalizesafe(math.cross(binormal, tangent), float3_ext.right);
            }

            public void Execute(int index) {
                float t = index / (float)resolution;
                points[index] = Position(t);
                tangents[index] = Tangent(t);
                normals[index] = Normal(t);
                binormals[index] = Binormal(t);
            }

            public float3 Position(float t) {
                return math.lerp(start, end, t);
            }

            public float3 Tangent(float t) {
                return tangent;
            }

            public float3 Normal(float t) {
                return normal;
            }

            public float3 Binormal(float t) {
                return binormal;
            }
        }
    }
}