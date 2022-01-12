using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Curve.TEMP {
    [RequireComponent(typeof(MeshFilter), typeof(CurveVisualizer))]
    public class CurveWorldSpaceUVs : SerializedMonoBehaviour {
        [Required] public ICurveProvider Curve;
        [Required] public ICurveProvider Profile;
        [Required] public CurveVisualizer Visualizer;

        [Space]
        public bool OffsetUVs;
        public bool NormalizeUVs;
        [Space]
        public bool EndingCap;
        public float Rotation;

        private CurveData aligned;
        private MeshFilter meshFilter;
        private Mesh mesh;

        [Button]
        private void Generate() {
            if (Curve?.Curve == null || Profile?.Curve == null) return;

            aligned = AlignCurve(Curve.Curve, Profile.Curve, EndingCap ? Curve.Curve.Points-1 : 0, Rotation);
            Visualizer.Load(aligned);

            GenerateMesh();
        }

        [Button]
        private void SetMeshToNull() {
            meshFilter.sharedMesh = null;
            DestroyImmediate(mesh);
            mesh = null;
        }

        private void GenerateMesh() {
            if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
            if (mesh == null) {
                mesh = new Mesh { name = "Generated" };
                if (meshFilter.sharedMesh != null) DestroyImmediate(meshFilter.sharedMesh);
                meshFilter.sharedMesh = mesh;
            }
            if (aligned == null) return;

            mesh.Clear();

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            float2 minUV = new float2(float.MaxValue, float.MaxValue);
            float2 maxUV = new float2(float.MinValue, float.MinValue);
            for (var i = 0; i < aligned.Position.Count; i++) {
                float3 vertex = aligned.Position[i];
                vertices.Add(vertex);

                float uvX = math.dot(vertex, Curve.Curve.Normal[0]); // right vector
                float uvY = math.dot(vertex, Curve.Curve.Binormal[0]); // forward vector
                if (EndingCap) {
                    uvX = math.dot(vertex, -Curve.Curve.Normal[Curve.Curve.Points - 1]); // right vector
                    uvY = math.dot(vertex, Curve.Curve.Binormal[Curve.Curve.Points - 1]); // forward vector
                }

                float2 uv = new float2(uvX, uvY);
                uvs.Add(uv);

                minUV = math.min(minUV, uv);
                maxUV = math.max(maxUV, uv);
            }

            for (var i = 0; i < uvs.Count; i++) {
                if (NormalizeUVs) {
                    float2 currentUV = uvs[i];
                    float2 normalizedUV = (currentUV - minUV) / (maxUV - minUV);
                    uvs[i] = normalizedUV;
                } else if (OffsetUVs) {
                    uvs[i] -= (Vector2) minUV;
                }
            }

            var triangles = new List<int>();

            // First face
            if (EndingCap) {
                triangles.Add(0);
                triangles.Add(1);
                triangles.Add(2);
            } else {
                triangles.Add(0);
                triangles.Add(2);
                triangles.Add(1);
            }

            int capFaceCount = aligned.Points - 2;
            for (var i = 1; i < capFaceCount - 1; i++) {
                if (EndingCap) {
                    triangles.Add(0);
                    triangles.Add(i + 1);
                    triangles.Add(i + 2);
                } else {
                    triangles.Add(0);
                    triangles.Add(i + 2);
                    triangles.Add(i + 1);
                }
            }

            // Last face
            if (aligned.Points > 3) {
                if (EndingCap) {
                    triangles.Add(aligned.Points - 2);
                    triangles.Add(aligned.Points - 1);
                    triangles.Add(0);
                } else {
                    triangles.Add(aligned.Points - 2);
                    triangles.Add(0);
                    triangles.Add(aligned.Points - 1);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
        }

        private static CurveData AlignCurve(CurveData alignOn, CurveData toAlign, int index, float rotationOffset) {
            var rotation = float4x4.RotateY(math.radians(rotationOffset));
            var align = new float4x4(alignOn.Normal[index].float4(), alignOn.Tangent[index].float4(), alignOn.Binormal[index].float4(), alignOn.Position[index].float4(1.0f));
            var matrix = math.mul(align, rotation);

            var position = new List<float3>();
            var tangent = new List<float3>();
            var normal = new List<float3>();
            var binormal = new List<float3>();

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < toAlign.Points; i++) {
                position.Add(math.mul(matrix, toAlign.Position[i].float4(1.0f)).xyz);
                tangent.Add(math.mul(matrix, toAlign.Tangent[i].float4()).xyz);
                normal.Add(math.mul(matrix, toAlign.Normal[i].float4()).xyz);
                binormal.Add(math.mul(matrix, toAlign.Binormal[i].float4()).xyz);
            }

            return new CurveData(toAlign.Type, toAlign.Points, toAlign.IsClosed, position, tangent, normal, binormal);
        }

    }
}