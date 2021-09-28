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
                vertexUvs.Add(circlePosition.xz * 0.5f + new float2(0.5f));
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
                uvs.Add(vertexUvs[v]);
                uvs.Add(vertexUvs[i+1]);
                uvs.Add(vertexUvs[0]);
                faces.Add(face);
                faceCorners.Add(new FaceCorner(i));
                faceCorners.Add(new FaceCorner(i));
                faceCorners.Add(new FaceCorner(i));
            }

            return new GeometryData(edges, faces, faceCorners, 1, vertexPositions, faceNormals, materialIndices, smoothShaded, creases, uvs);
        }

        public static GeometryData MakePlane(float2 size, int subdivisions) {
            subdivisions++;
            if (subdivisions < 1) subdivisions = 1;
            var step = (size / subdivisions).xyy;
            step.y = 0.0f;
            var halfSize = (size * 0.5f).xyy;
            halfSize.y = 0;

            var faceCount = 2 * subdivisions * subdivisions;
            var edgeCount = (subdivisions - 1) * (3 * subdivisions - 1);
            
            var faceNormals = Enumerable.Range(0, faceCount).Select(_ => float3_util.up).ToList();
            var materialIndices = new int[faceCount].ToList();
            var smoothShaded = new bool[faceCount].ToList();
            var creases = Enumerable.Range(0, edgeCount).Select(_ => 0.0f).ToList();
            
            var vertexPositions = new List<float3>();
            var vertexUVs = new List<float2>();
            
            for (var y = 0; y <= subdivisions; y++) {
                for (var x = 0; x <= subdivisions; x++) {
                    vertexPositions.Add(step * new float3(x, 0, y) - halfSize);
                    vertexUVs.Add((step * new float3(x, 0, subdivisions - y)).xz);
                }
            }

            var edges = new List<Edge>();
            var faces = new List<Face>();
            var faceCorners = new List<FaceCorner>();
            var uvs = new List<float2>();

            for (var y = 0; y < subdivisions; y++) {
                for (var x = 0; x < subdivisions; x++) {
                    var vertexIndex = y * (subdivisions + 1) + x;
                    var faceIndex = (y * subdivisions + x) * 2;
                    var edgeA = new Edge(vertexIndex, vertexIndex + 1, edges.Count) {
                        FaceA = faceIndex
                    };

                    var edgeB = new Edge(vertexIndex, vertexIndex + subdivisions + 1, edges.Count + 1) {
                        FaceA = faceIndex
                    };

                    var edgeC = new Edge(vertexIndex + 1, vertexIndex + subdivisions + 1, edges.Count + 2) {
                        FaceA = faceIndex,
                        FaceB = faceIndex + 1
                    };
                    var c = edges.Count;
                    edges.Add(edgeB);
                    edges.Add(edgeA);
                    edges.Add(edgeC);

                    var faceA = new Face(vertexIndex + 1, vertexIndex, vertexIndex + subdivisions + 1,
                                         faceCorners.Count, faceCorners.Count + 1, faceCorners.Count + 2, 
                                         c, c + 1, c + 2
                    );

                    int eA, eC;
                    if (x < subdivisions - 1 && y < subdivisions - 1) {
                        eA = c + subdivisions + 3;
                        eC = c + subdivisions * 3 + 2;
                    } else if (x == subdivisions - 1 && y < subdivisions - 1) {
                        eA = c + 3; 
                        eC = c + subdivisions * 3 + 2;
                    } else if (x < subdivisions - 1 && y == subdivisions - 1) {
                        eA = c + subdivisions + 3;
                        eC = c + (subdivisions - x) * 3 + x + 1;
                    } else {
                        eA = c + 3;
                        eC = c + (subdivisions - x) * 3 + x + 1;
                    }

                    var faceB = new Face(
                        vertexIndex + 1, vertexIndex + subdivisions + 1, vertexIndex + subdivisions + 2, 
                        faceCorners.Count + 3, faceCorners.Count + 4, faceCorners.Count + 5, 
                        eA, c, eC
                    );
                    
                    uvs.Add(vertexUVs[vertexIndex + 1]);
                    uvs.Add(vertexUVs[vertexIndex]);
                    uvs.Add(vertexUVs[vertexIndex + subdivisions + 1]);
                    uvs.Add(vertexUVs[vertexIndex + 1]);
                    uvs.Add(vertexUVs[vertexIndex + subdivisions + 1]);
                    uvs.Add(vertexUVs[vertexIndex + subdivisions + 2]);
                    
                    faces.Add(faceA);
                    faces.Add(faceB);
                    faceCorners.Add(new FaceCorner(faces.Count - 2));
                    faceCorners.Add(new FaceCorner(faces.Count - 2));
                    faceCorners.Add(new FaceCorner(faces.Count - 2));
                    faceCorners.Add(new FaceCorner(faces.Count - 1));
                    faceCorners.Add(new FaceCorner(faces.Count - 1));
                    faceCorners.Add(new FaceCorner(faces.Count - 1));

                    if (x == subdivisions - 1) {
                        // Right
                        var edgeRight = new Edge(vertexIndex + 1, vertexIndex + subdivisions + 2, edges.Count) {
                            FaceA = faceIndex + 1
                        };
                        edges.Add(edgeRight);
                    }
                }
                
                if (y == subdivisions - 1) {
                    for (var x = 0; x < subdivisions; x++) {
                        var vertexIndex = y * (subdivisions + 1) + x;
                        var faceIndex = (y * subdivisions + x) * 2;
                        
                        // Top
                        var edgeTop = new Edge(vertexIndex + subdivisions + 1, vertexIndex + subdivisions + 2, edges.Count) {
                            FaceA = faceIndex + 1
                        };
                        edges.Add(edgeTop);
                    }
                }
            }
            
            return new GeometryData(edges, faces, faceCorners, 1, vertexPositions, faceNormals, materialIndices, smoothShaded, creases, uvs);
        }
    }
}