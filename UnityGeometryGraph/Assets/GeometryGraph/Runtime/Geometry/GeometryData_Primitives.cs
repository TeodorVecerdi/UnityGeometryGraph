using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    public partial class GeometryData {
        public static GeometryData MakeCircle(float radius, int points) {
            if (points < 3) points = 3;
            if (Math.Abs(radius) < 0.001f) radius = 0.001f;

            var faceCount = points;
            var edgeCount = 2 * points;
            var faceNormals = Enumerable.Range(0, faceCount).Select(_ => float3_util.up).ToList();
            var materialIndices = new int[faceCount].ToList();
            var smoothShaded = new bool[faceCount].ToList();
            var creases = Enumerable.Range(0, edgeCount).Select(_ => 0.0f).ToList();
            
            var vertexPositions = new List<float3> { float3.zero };
            var vertexUvs = new List<float2> { float2_util.one * 0.5f };
            
            for (var i = 0; i < points; i++) {
                var t = i / (float)(points);
                var angle = 2.0f * Mathf.PI * t;
                var circlePosition = new float3(math.cos(angle), 0.0f, math.sin(angle));
                vertexPositions.Add(radius * circlePosition);
                vertexUvs.Add((circlePosition.xz + float2_util.one) / 2.0f);
            }

            var edges = new List<Edge>();
            var faces = new List<Face>();
            var faceCorners = new List<FaceCorner>();
            
            // Inner Edges
            for (var i = 0; i < points; i++) {
                var edge = new Edge(0, i + 1, i) {
                    FaceA = i,
                    FaceB = (i - 1).Mod(points)
                };
                edges.Add(edge);
            }
            // Outer edges
            for (var i = 0; i < points; i++) {
                var v = (i + 2) % (points + 1);
                if (v == 0) v = 1;
                var edge = new Edge(i + 1, v, i + points) {
                    FaceA = i
                };
                edges.Add(edge);
            }

            var uvs = new List<float2>();
            // Faces and FCs
            for (var i = 0; i < points; i++) {
                var v = (i + 2) % (points + 1);
                if (v == 0) v = 1;
                var face = new Face(
                    v, i + 1, 0,
                    i * 3, i * 3 + 1, i * 3 + 2,
                    i, (i + 1) % points, i + points
                );
                uvs.Add(vertexUvs[0]);
                uvs.Add(vertexUvs[i+1]);
                uvs.Add(vertexUvs[v]);
                faces.Add(face);
                faceCorners.Add(new FaceCorner(i));
                faceCorners.Add(new FaceCorner(i));
                faceCorners.Add(new FaceCorner(i));
            }

            return new GeometryData(edges, faces, faceCorners, 1, vertexPositions, faceNormals, materialIndices, smoothShaded, creases, uvs);
        }
    }
}