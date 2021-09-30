using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityCommons;
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
            var creases = new float[edgeCount].ToList();
            
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
            var creases = new float[edgeCount].ToList();
            
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

        public static GeometryData MakeCube(float3 size) {
            const int faceCount = 12;
            const int edgeCount = 18;
            
            
            var materialIndices = new int[faceCount].ToList();
            var smoothShaded = new bool[faceCount].ToList();
            var creases = new float[edgeCount].ToList();
            
            var vertexPositions = new [] {
                // Bottom
                float3.zero,
                new float3(size.x, 0.0f, 0.0f),
                new float3(size.x, 0.0f, size.z),
                new float3(0.0f, 0.0f, size.z),
                // Top
                new float3(0.0f, size.y, 0.0f),
                new float3(size.x, size.y, 0.0f),
                new float3(size.x, size.y, size.z),
                new float3(0.0f, size.y, size.z),
            } // Offset center
            .Select(p => p - size * 0.5f).ToList();
            
            var down = -float3_util.up;
            var left = -float3_util.right;
            var back = -float3_util.forward;
            var faceNormals = new List<float3> {
                down, down,
                back, back,
                float3_util.right, float3_util.right, 
                float3_util.forward, float3_util.forward,
                left, left,
                float3_util.up, float3_util.up
            };

            var right = float2_util.right;
            var zero = float2.zero;
            var up = float2_util.up;
            var one = float2_util.one;
            var uvs = new List<float2> {
                zero, right, up, one, up, right,
                zero, right, up, one, up, right, 
                zero, right, up, one, up, right,
                zero, right, up, one, up, right,
                zero, right, up, one, up, right,
                zero, right, up, one, up, right
            };

            var fcI = 0;
            var faces = new List<Face> {
                new Face(2, 1, 3, fcI++, fcI++, fcI++, 1, 4, 2), 
                new Face(1, 0, 3, fcI++, fcI++, fcI++, 0, 3, 4),
                new Face(0, 1, 4, fcI++, fcI++, fcI++, 0, 9, 5), 
                new Face(5, 4, 1, fcI++, fcI++, fcI++, 13, 9, 6),
                new Face(1, 2, 5, fcI++, fcI++, fcI++, 1, 10, 6), 
                new Face(6, 5, 2, fcI++, fcI++, fcI++, 14, 10, 7),
                new Face(2, 3, 6, fcI++, fcI++, fcI++, 2, 11, 7), 
                new Face(7, 6, 3, fcI++, fcI++, fcI++, 15, 11, 8),
                new Face(3, 0, 7, fcI++, fcI++, fcI++, 3, 12, 8), 
                new Face(4, 7, 0, fcI++, fcI++, fcI++, 16, 12, 5),
                new Face(4, 5, 7, fcI++, fcI++, fcI++, 13, 17, 16), 
                new Face(6, 7, 5, fcI++, fcI++, fcI++, 15, 17, 14),
            };

            var eI = 0;
            var edges = new List<Edge> {
                new Edge(0, 1, eI++) { FaceA = 1, FaceB = 2 },
                new Edge(1, 2, eI++) { FaceA = 0, FaceB = 4 },
                new Edge(2, 3, eI++) { FaceA = 0, FaceB = 6 },
                new Edge(3, 0, eI++) { FaceA = 1, FaceB = 8 },
                new Edge(1, 3, eI++) { FaceA = 0, FaceB = 1 },
                new Edge(0, 4, eI++) { FaceA = 2, FaceB = 9 },
                new Edge(1, 5, eI++) { FaceA = 3, FaceB = 4 },
                new Edge(2, 6, eI++) { FaceA = 5, FaceB = 6 },
                new Edge(3, 7, eI++) { FaceA = 7, FaceB = 8 },
                new Edge(1, 4, eI++) { FaceA = 2, FaceB = 3 },
                new Edge(2, 5, eI++) { FaceA = 4, FaceB = 5 },
                new Edge(3, 6, eI++) { FaceA = 6, FaceB = 7 },
                new Edge(0, 7, eI++) { FaceA = 8, FaceB = 9 },
                new Edge(4, 5, eI++) { FaceA = 3, FaceB = 10 },
                new Edge(5, 6, eI++) { FaceA = 5, FaceB = 11 },
                new Edge(6, 7, eI++) { FaceA = 7, FaceB = 11 },
                new Edge(7, 4, eI++) { FaceA = 9, FaceB = 10 },
                new Edge(5, 7, eI++) { FaceA = 10, FaceB = 11 },
            };
            var faceCorners = Enumerable.Range(0, faceCount).SelectMany(i => new []{ new FaceCorner(i), new FaceCorner(i), new FaceCorner(i) }).ToList();

            return new GeometryData(edges, faces, faceCorners, 1, vertexPositions, faceNormals, materialIndices, smoothShaded, creases, uvs);
        }
        
        public static GeometryData MakeCone(float radius, float height, int points) {
            if (points < 3) points = 3;
            if (Math.Abs(radius) < 0.0f) radius = 0.0f;

            var faceCount = 2 * points;
            var edgeCount = 3 * points;
            var materialIndices = new int[faceCount].ToList();
            var smoothShaded = Enumerable.Repeat((false, true), points).SelectMany(pair => new[] { pair.Item1, pair.Item2 }).ToList();
            var creases = new float[edgeCount].ToList();
            
            var vertexPositions = new List<float3> { float3.zero, float3_util.up * height };
            var vertexUvs = new List<float2> { float2_util.one * 0.5f, float2_util.one * 0.5f };
            
            for (var i = 0; i < points; i++) {
                var t = i / (float)points;
                var angle = 2.0f * Mathf.PI * t;
                var circlePosition = new float3(math.cos(angle), 0.0f, math.sin(angle));
                vertexPositions.Add(radius * circlePosition);
                vertexUvs.Add(circlePosition.xz * 0.5f + new float2(0.5f));
            }

            var edges = new List<Edge>();
            var faces = new List<Face>();
            var faceCorners = new List<FaceCorner>();
            
            // Inner and Vertical Edges
            for (var i = 0; i < points; i++) {
                var edge = new Edge(0, i + 2, i) {
                    FaceA = i * 2,
                    FaceB = (i - 1).Mod(points) * 2
                };
                var edgeV = new Edge(1, i + 2, i) {
                    FaceA = i * 2 + 1,
                    FaceB = (i - 1).Mod(points) * 2 + 1
                };
                edges.Add(edge);
                edges.Add(edgeV);
            }
            // Outer edges
            for (var i = 0; i < points; i++) {
                var v = (i + 3) % (points + 2);
                if (v < 2) v = 2;
                var edge = new Edge(i + 2, v, i + points) {
                    FaceA = i * 2,
                    FaceB = i * 2 + 1
                };
                edges.Add(edge);
            }

            var uvs = new List<float2>();
            var faceNormals = new List<float3>();
            // Faces and FCs
            for (var i = 0; i < points; i++) {
                var v = (i + 3) % (points + 2);
                if (v < 2) v = 2;
                var face = new Face(
                    v, 0, i + 2,
                    i * 6, i * 6 + 1, i * 6 + 2,
                    i * 2, i + 2 * points, ((i + 1) * 2) % (2 * points)
                );
                uvs.Add(vertexUvs[v]);
                uvs.Add(vertexUvs[0]);
                uvs.Add(vertexUvs[i+2]);
                faceNormals.Add(-float3_util.up);
                faces.Add(face);
                
                var faceV = new Face(
                    v, i + 2, 1,
                    i * 6 + 3, i * 6 + 4, i * 6 + 5,
                    i * 2 + 1, ((i + 1) * 2 + 1) % (2 * points), i + 2 * points
                );
                uvs.Add(vertexUvs[v]);
                uvs.Add(vertexUvs[i+2]);
                uvs.Add(vertexUvs[1]);
                faceNormals.Add(math.normalize(math.cross(vertexPositions[i + 2] - vertexPositions[v], vertexPositions[1] - vertexPositions[v])));
                faces.Add(faceV);
                
                faceCorners.Add(new FaceCorner(i * 2));
                faceCorners.Add(new FaceCorner(i * 2));
                faceCorners.Add(new FaceCorner(i * 2));
                faceCorners.Add(new FaceCorner(i * 2 + 1));
                faceCorners.Add(new FaceCorner(i * 2 + 1));
                faceCorners.Add(new FaceCorner(i * 2 + 1));
            }

            return new GeometryData(edges, faces, faceCorners, 1, vertexPositions, faceNormals, materialIndices, smoothShaded, creases, uvs);
        }

        public static GeometryData MakeCylinder(float bottomRadius, float topRadius, float height, int points) {
            if (points < 3) points = 3;
            if (Math.Abs(bottomRadius) < 0.0f) bottomRadius = 0.0f;
            if (Math.Abs(topRadius) < 0.0f) topRadius = 0.0f;
            if (Math.Abs(height) < 0.0f) height = 0.0f;

            var faceCount = 4 * points;
            var edgeCount = 9 * points;
            var materialIndices = new int[faceCount].ToList();
            var smoothShaded = Enumerable.Repeat(0, points).SelectMany(_ => new [] {false, false, true, true}).ToList();
            var creases = new float[edgeCount].ToList();
            
            var vertexPositions = new List<float3> { float3.zero, float3_util.up * height };
            var vertexUvs = new List<float2> { float2_util.one * 0.5f, float2_util.one * 0.5f };
            
            for (var i = 0; i < points; i++) {
                var t = i / (float)points;
                var angle = 2.0f * Mathf.PI * t;
                var circlePosition = new float3(math.cos(angle), 0.0f, math.sin(angle));
                vertexPositions.Add(bottomRadius * circlePosition);
                vertexPositions.Add(topRadius * circlePosition + float3_util.up * height);
                vertexUvs.Add(circlePosition.xz * 0.5f + new float2(0.5f));
                vertexUvs.Add(circlePosition.xz * 0.5f + new float2(0.5f));
            }

            var edges = new List<Edge>();
            var faces = new List<Face>();
            var faceCorners = new List<FaceCorner>();
            
            for (var i = 0; i < points; i++) {
                edges.Add(new Edge(0, (i + 1) * 2, i + 6 + 0));
                edges.Add(new Edge((i + 1) * 2, (i + 1) * 2 + 1, i + 6 + 1));
                edges.Add(new Edge(1, (i + 1) * 2 + 1, i + 6 + 2));
                edges.Add(new Edge((i+1) * 2 + 1, ((i + 1) % points + 1) * 2, i + 6 + 3));
                edges.Add(new Edge((i+1) * 2, ((i + 1) % points + 1) * 2, i + 6 + 4));
                edges.Add(new Edge((i+1) * 2 + 1, ((i + 1) % points + 1) * 2 + 1, i + 6 + 5));
            }

            var uvs = new List<float2>();
            var faceNormals = new List<float3>();
            var fcI = 0;
            // Faces and FCs
            for (var i = 0; i < points; i++) {
                faces.Add(
                    new Face(
                        (i + 1) * 2, 0, ((i + 1) % points + 1) * 2,
                        fcI++, fcI++, fcI++,
                        i * 6, ((i + 1) % points) * 6, i * 6 + 4
                    )
                );
                faces.Add(
                    new Face(
                        1, (i + 1) * 2 + 1, ((i + 1) % points + 1) * 2 + 1,
                        fcI++, fcI++, fcI++,
                        i * 6 + 2, i * 6 + 5, ((i + 1) % points) * 6 + 2
                    )
                );
                faces.Add(
                    new Face(
                        (i + 1) * 2, (i + 1) * 2 + 1, ((i + 1) % points + 1) * 2,
                        fcI++, fcI++, fcI++,
                        i * 6 + 4, i * 6 + 3, i * 6 + 1
                    )
                );
                faces.Add(
                    new Face(
                        (i + 1) * 2 + 1, ((i + 1) % points + 1) * 2 + 1, ((i + 1) % points + 1) * 2,
                        fcI++, fcI++, fcI++,
                        i * 6 + 3, ((i + i) % points) * 6 + 1, (i + 1) * 6 - 1
                    )
                );
                
                uvs.Add(vertexUvs[(i + 1) * 2]);
                uvs.Add(vertexUvs[0]);
                uvs.Add(vertexUvs[((i + 1) % points + 1) * 2]);
                uvs.Add(vertexUvs[1]);
                uvs.Add(vertexUvs[(i + 1) * 2 + 1]);
                uvs.Add(vertexUvs[((i + 1) % points + 1) * 2 + 1]);
                
                uvs.Add(float2.zero);
                uvs.Add(float2_util.up);
                uvs.Add(float2_util.right);
                uvs.Add(float2_util.up);
                uvs.Add(float2_util.one);
                uvs.Add(float2_util.right);
                
                faceNormals.Add(-float3_util.up);
                faceNormals.Add(float3_util.up);
                faceNormals.Add(
                    math.normalize(
                        math.cross(
                            vertexPositions[(i + 1) * 2 + 1] - vertexPositions[(i + 1) * 2],
                            vertexPositions[((i + 1) % points + 1) * 2] - vertexPositions[(i + 1) * 2]
                        )
                    )
                );
                faceNormals.Add(
                    math.normalize(
                        math.cross(
                            vertexPositions[((i + 1) % points + 1) * 2 + 1] - vertexPositions[(i + 1) * 2 + 1], 
                            vertexPositions[((i + 1) % points + 1) * 2] - vertexPositions[(i + 1) * 2 + 1]
                        )
                    )
                );
                
                faceCorners.Add(new FaceCorner(i * 4 + 0));
                faceCorners.Add(new FaceCorner(i * 4 + 0));
                faceCorners.Add(new FaceCorner(i * 4 + 0));
                faceCorners.Add(new FaceCorner(i * 4 + 1));
                faceCorners.Add(new FaceCorner(i * 4 + 1));
                faceCorners.Add(new FaceCorner(i * 4 + 1));
                faceCorners.Add(new FaceCorner(i * 4 + 2));
                faceCorners.Add(new FaceCorner(i * 4 + 2));
                faceCorners.Add(new FaceCorner(i * 4 + 2));
                faceCorners.Add(new FaceCorner(i * 4 + 3));
                faceCorners.Add(new FaceCorner(i * 4 + 3));
                faceCorners.Add(new FaceCorner(i * 4 + 3));
            }

            return new GeometryData(edges, faces, faceCorners, 1, vertexPositions, faceNormals, materialIndices, smoothShaded, creases, uvs);
        }

    }
}