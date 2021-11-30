using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.AttributeSystem;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    public static class GeometryPrimitive {
        public static GeometryData Circle(float radius, int points) {
            if (points < 3) points = 3;
            if (Math.Abs(radius) < 0.001f) radius = 0.001f;

            int faceCount = points;
            int edgeCount = 2 * points;
            List<float3> faceNormals = Enumerable.Range(0, faceCount).Select(_ => float3_ext.up).ToList();
            List<int> materialIndices = new int[faceCount].ToList();
            List<bool> smoothShaded = new bool[faceCount].ToList();
            List<float> creases = new float[edgeCount].ToList();
            
            List<float3> vertexPositions = new List<float3> { float3.zero };
            List<float2> vertexUvs = new List<float2> { float2_ext.one * 0.5f };
            
            for (int i = 0; i < points; i++) {
                float t = i / (float)(points);
                float angle = math_ext.TWO_PI * t;
                float3 circlePosition = new float3(math.cos(angle), 0.0f, math.sin(angle));
                vertexPositions.Add(radius * circlePosition);
                vertexUvs.Add(circlePosition.xz * 0.5f + new float2(0.5f));
            }

            List<GeometryData.Edge> edges = new List<GeometryData.Edge>();
            List<GeometryData.Face> faces = new List<GeometryData.Face>();
            List<GeometryData.FaceCorner> faceCorners = new List<GeometryData.FaceCorner>();
            
            // Inner Edges
            for (int i = 0; i < points; i++) {
                GeometryData.Edge edge = new GeometryData.Edge(0, i + 1, i) {
                    FaceA = i,
                    FaceB = (i - 1).mod(points)
                };
                edges.Add(edge);
            }
            // Outer edges
            for (int i = 0; i < points; i++) {
                int v = (i + 2) % (points + 1);
                if (v == 0) v = 1;
                GeometryData.Edge edge = new GeometryData.Edge(i + 1, v, i + points) {
                    FaceA = i
                };
                edges.Add(edge);
            }

            List<float2> uvs = new List<float2>();
            // Faces and FCs
            for (int i = 0; i < points; i++) {
                int v = (i + 2) % (points + 1);
                if (v == 0) v = 1;
                GeometryData.Face face = new GeometryData.Face(
                    v, i + 1, 0,
                    i * 3, i * 3 + 1, i * 3 + 2,
                    i, (i + 1) % points, i + points
                );
                uvs.Add(vertexUvs[v]);
                uvs.Add(vertexUvs[i+1]);
                uvs.Add(vertexUvs[0]);
                faces.Add(face);
                faceCorners.Add(new GeometryData.FaceCorner(i));
                faceCorners.Add(new GeometryData.FaceCorner(i));
                faceCorners.Add(new GeometryData.FaceCorner(i));
            }

            return new GeometryData(edges, faces, faceCorners, 1, vertexPositions, faceNormals, materialIndices, smoothShaded, creases, uvs);
        }

        public static GeometryData Plane(float2 size, int subdivisions) {
            subdivisions++;
            if (subdivisions < 1) subdivisions = 1;
            float3 step = (size / subdivisions).xyy;
            step.y = 0.0f;
            float3 halfSize = (size * 0.5f).xyy;
            halfSize.y = 0;

            int faceCount = 2 * subdivisions * subdivisions;
            int edgeCount = (subdivisions - 1) * (3 * subdivisions - 1);
            
            List<float3> faceNormals = Enumerable.Range(0, faceCount).Select(_ => float3_ext.up).ToList();
            List<int> materialIndices = new int[faceCount].ToList();
            List<bool> smoothShaded = new bool[faceCount].ToList();
            List<float> creases = new float[edgeCount].ToList();
            
            List<float3> vertexPositions = new List<float3>();
            List<float2> vertexUVs = new List<float2>();
            
            for (int y = 0; y <= subdivisions; y++) {
                for (int x = 0; x <= subdivisions; x++) {
                    vertexPositions.Add(step * new float3(x, 0, y) - halfSize);
                    vertexUVs.Add((step * new float3(x, 0, subdivisions - y)).xz);
                }
            }

            List<GeometryData.Edge> edges = new List<GeometryData.Edge>();
            List<GeometryData.Face> faces = new List<GeometryData.Face>();
            List<GeometryData.FaceCorner> faceCorners = new List<GeometryData.FaceCorner>();
            List<float2> uvs = new List<float2>();

            for (int y = 0; y < subdivisions; y++) {
                for (int x = 0; x < subdivisions; x++) {
                    int vertexIndex = y * (subdivisions + 1) + x;
                    int faceIndex = (y * subdivisions + x) * 2;
                    GeometryData.Edge edgeA = new GeometryData.Edge(vertexIndex, vertexIndex + 1, edges.Count) {
                        FaceA = faceIndex
                    };

                    GeometryData.Edge edgeB = new GeometryData.Edge(vertexIndex, vertexIndex + subdivisions + 1, edges.Count + 1) {
                        FaceA = faceIndex
                    };

                    GeometryData.Edge edgeC = new GeometryData.Edge(vertexIndex + 1, vertexIndex + subdivisions + 1, edges.Count + 2) {
                        FaceA = faceIndex,
                        FaceB = faceIndex + 1
                    };
                    int c = edges.Count;
                    edges.Add(edgeB);
                    edges.Add(edgeA);
                    edges.Add(edgeC);

                    GeometryData.Face faceA = new GeometryData.Face(vertexIndex + 1, vertexIndex, vertexIndex + subdivisions + 1,
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

                    GeometryData.Face faceB = new GeometryData.Face(
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
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 2));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 2));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 2));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));

                    if (x == subdivisions - 1) {
                        // Right
                        GeometryData.Edge edgeRight = new GeometryData.Edge(vertexIndex + 1, vertexIndex + subdivisions + 2, edges.Count) {
                            FaceA = faceIndex + 1
                        };
                        edges.Add(edgeRight);
                    }
                }
                
                if (y == subdivisions - 1) {
                    for (int x = 0; x < subdivisions; x++) {
                        int vertexIndex = y * (subdivisions + 1) + x;
                        int faceIndex = (y * subdivisions + x) * 2;
                        
                        // Top
                        GeometryData.Edge edgeTop = new GeometryData.Edge(vertexIndex + subdivisions + 1, vertexIndex + subdivisions + 2, edges.Count) {
                            FaceA = faceIndex + 1
                        };
                        edges.Add(edgeTop);
                    }
                }
            }
            
            return new GeometryData(edges, faces, faceCorners, 1, vertexPositions, faceNormals, materialIndices, smoothShaded, creases, uvs);
        }

        public static GeometryData Cube(float3 size) {
            const int faceCount = 12;
            const int edgeCount = 18;
            
            List<int> materialIndices = new int[faceCount].ToList();
            List<bool> smoothShaded = new bool[faceCount].ToList();
            List<float> creases = new float[edgeCount].ToList();
            
            List<float3> vertexPositions = new [] {
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
            
            float3 down = -float3_ext.up;
            float3 left = -float3_ext.right;
            float3 back = -float3_ext.forward;
            List<float3> faceNormals = new List<float3> {
                down, down,
                back, back,
                float3_ext.right, float3_ext.right, 
                float3_ext.forward, float3_ext.forward,
                left, left,
                float3_ext.up, float3_ext.up
            };

            float2 right = float2_ext.right;
            float2 zero = float2.zero;
            float2 up = float2_ext.up;
            float2 one = float2_ext.one;
            List<float2> uvs = new List<float2> {
                right, zero, one, up, one, zero,
                zero, up, right, one, right, up, 
                zero, up, right, one, right, up,
                zero, up, right, one, right, up,
                zero, up, right, one, right, up,
                zero, up, right, one, right, up,
            };

            int fcI = 0;
            List<GeometryData.Face> faces = new() {
                new GeometryData.Face(2, 3, 1, fcI++, fcI++, fcI++, 2, 4, 1), 
                new GeometryData.Face(0, 1, 3, fcI++, fcI++, fcI++, 4, 3, 0),
                new GeometryData.Face(0, 4, 1, fcI++, fcI++, fcI++, 5, 9, 0), 
                new GeometryData.Face(5, 1, 4, fcI++, fcI++, fcI++, 6, 9, 13),
                new GeometryData.Face(1, 5, 2, fcI++, fcI++, fcI++, 6, 10, 1), 
                new GeometryData.Face(6, 2, 5, fcI++, fcI++, fcI++, 7, 10, 14),
                new GeometryData.Face(2, 6, 3, fcI++, fcI++, fcI++, 7, 11, 2), 
                new GeometryData.Face(7, 3, 6, fcI++, fcI++, fcI++, 8, 11, 15),
                new GeometryData.Face(3, 7, 0, fcI++, fcI++, fcI++, 8, 12, 3), 
                new GeometryData.Face(4, 0, 7, fcI++, fcI++, fcI++, 5, 12, 16),
                new GeometryData.Face(4, 7, 5, fcI++, fcI++, fcI++, 16, 17, 13), 
                new GeometryData.Face(6, 5, 7, fcI++, fcI++, fcI++, 14, 17, 15),
            };

            int eI = 0;
            List<GeometryData.Edge> edges = new List<GeometryData.Edge> {
                new GeometryData.Edge(0, 1, eI++) { FaceA = 1, FaceB = 2 },
                new GeometryData.Edge(1, 2, eI++) { FaceA = 0, FaceB = 4 },
                new GeometryData.Edge(2, 3, eI++) { FaceA = 0, FaceB = 6 },
                new GeometryData.Edge(3, 0, eI++) { FaceA = 1, FaceB = 8 },
                new GeometryData.Edge(1, 3, eI++) { FaceA = 0, FaceB = 1 },
                new GeometryData.Edge(0, 4, eI++) { FaceA = 2, FaceB = 9 },
                new GeometryData.Edge(1, 5, eI++) { FaceA = 3, FaceB = 4 },
                new GeometryData.Edge(2, 6, eI++) { FaceA = 5, FaceB = 6 },
                new GeometryData.Edge(3, 7, eI++) { FaceA = 7, FaceB = 8 },
                new GeometryData.Edge(1, 4, eI++) { FaceA = 2, FaceB = 3 },
                new GeometryData.Edge(2, 5, eI++) { FaceA = 4, FaceB = 5 },
                new GeometryData.Edge(3, 6, eI++) { FaceA = 6, FaceB = 7 },
                new GeometryData.Edge(0, 7, eI++) { FaceA = 8, FaceB = 9 },
                new GeometryData.Edge(4, 5, eI++) { FaceA = 3, FaceB = 10 },
                new GeometryData.Edge(5, 6, eI++) { FaceA = 5, FaceB = 11 },
                new GeometryData.Edge(6, 7, eI++) { FaceA = 7, FaceB = 11 },
                new GeometryData.Edge(7, 4, eI++) { FaceA = 9, FaceB = 10 },
                new GeometryData.Edge(5, 7, eI++) { FaceA = 10, FaceB = 11 },
            };
            List<GeometryData.FaceCorner> faceCorners = Enumerable.Range(0, faceCount).SelectMany(i => new []{ new GeometryData.FaceCorner(i), new GeometryData.FaceCorner(i), new GeometryData.FaceCorner(i) }).ToList();

            return new GeometryData(edges, faces, faceCorners, 1, vertexPositions, faceNormals, materialIndices, smoothShaded, creases, uvs);
        }
        
        public static GeometryData Cone(float radius, float height, int points) {
            if (points < 3) points = 3;
            if (Math.Abs(radius) < 0.0f) radius = 0.0f;

            int faceCount = 2 * points;
            int edgeCount = 3 * points;
            List<int> materialIndices = new int[faceCount].ToList();
            List<bool> smoothShaded = Enumerable.Repeat((false, true), points).SelectMany(pair => new[] { pair.Item1, pair.Item2 }).ToList();
            List<float> creases = new float[edgeCount].ToList();
            
            List<float3> vertexPositions = new List<float3> { float3.zero, float3_ext.up * height };
            List<float2> vertexUvs = new List<float2> { float2_ext.one * 0.5f, float2_ext.one * 0.5f };
            
            for (int i = 0; i < points; i++) {
                float t = i / (float)points;
                float angle = math_ext.TWO_PI * t;
                float3 circlePosition = new float3(math.cos(angle), 0.0f, math.sin(angle));
                vertexPositions.Add(radius * circlePosition);
                vertexUvs.Add(circlePosition.xz * 0.5f + new float2(0.5f));
            }

            List<GeometryData.Edge> edges = new List<GeometryData.Edge>();
            List<GeometryData.Face> faces = new List<GeometryData.Face>();
            List<GeometryData.FaceCorner> faceCorners = new List<GeometryData.FaceCorner>();
            
            // Inner and Vertical Edges
            for (int i = 0; i < points; i++) {
                GeometryData.Edge edge = new GeometryData.Edge(0, i + 2, i) {
                    FaceA = i * 2,
                    FaceB = (i - 1).mod(points) * 2
                };
                GeometryData.Edge edgeV = new GeometryData.Edge(1, i + 2, i) {
                    FaceA = i * 2 + 1,
                    FaceB = (i - 1).mod(points) * 2 + 1
                };
                edges.Add(edge);
                edges.Add(edgeV);
            }
            // Outer edges
            for (int i = 0; i < points; i++) {
                int v = (i + 3) % (points + 2);
                if (v < 2) v = 2;
                GeometryData.Edge edge = new GeometryData.Edge(i + 2, v, i + points) {
                    FaceA = i * 2,
                    FaceB = i * 2 + 1
                };
                edges.Add(edge);
            }

            List<float2> uvs = new List<float2>();
            List<float3> faceNormals = new List<float3>();
            // Faces and FCs
            for (int i = 0; i < points; i++) {
                int v = (i + 3) % (points + 2);
                if (v < 2) v = 2;
                GeometryData.Face face = new GeometryData.Face(
                    v, 0, i + 2,
                    i * 6, i * 6 + 1, i * 6 + 2,
                    i * 2, i + 2 * points, ((i + 1) * 2) % (2 * points)
                );
                uvs.Add(vertexUvs[v]);
                uvs.Add(vertexUvs[0]);
                uvs.Add(vertexUvs[i+2]);
                faceNormals.Add(-float3_ext.up);
                faces.Add(face);
                
                GeometryData.Face faceV = new GeometryData.Face(
                    v, i + 2, 1,
                    i * 6 + 3, i * 6 + 4, i * 6 + 5,
                    i * 2 + 1, ((i + 1) * 2 + 1) % (2 * points), i + 2 * points
                );
                uvs.Add(vertexUvs[v]);
                uvs.Add(vertexUvs[i+2]);
                uvs.Add(vertexUvs[1]);
                faceNormals.Add(math.normalize(math.cross(vertexPositions[i + 2] - vertexPositions[v], vertexPositions[1] - vertexPositions[v])));
                faces.Add(faceV);
                
                faceCorners.Add(new GeometryData.FaceCorner(i * 2));
                faceCorners.Add(new GeometryData.FaceCorner(i * 2));
                faceCorners.Add(new GeometryData.FaceCorner(i * 2));
                faceCorners.Add(new GeometryData.FaceCorner(i * 2 + 1));
                faceCorners.Add(new GeometryData.FaceCorner(i * 2 + 1));
                faceCorners.Add(new GeometryData.FaceCorner(i * 2 + 1));
            }

            return new GeometryData(edges, faces, faceCorners, 1, vertexPositions, faceNormals, materialIndices, smoothShaded, creases, uvs);
        }

        public static GeometryData Cylinder(float bottomRadius, float topRadius, float height, int points, CylinderUVSettings uvSettings) {
            if (points < 3) points = 3;
            if (Math.Abs(bottomRadius) < 0.0f) bottomRadius = 0.0f;
            if (Math.Abs(topRadius) < 0.0f) topRadius = 0.0f;
            if (Math.Abs(height) < 0.0f) height = 0.0f;

            int faceCount = 4 * points;
            int edgeCount = 9 * points;
            List<int> materialIndices = new int[faceCount].ToList();
            List<bool> smoothShaded = Enumerable.Repeat(0, points).SelectMany(_ => new [] {false, false, true, true}).ToList();
            List<float> creases = new float[edgeCount].ToList();
            
            List<float3> vertexPositions = new List<float3> { float3.zero, float3_ext.up * height };
            List<float2> vertexUvs = new List<float2> { float2_ext.one * 0.5f, float2_ext.one * 0.5f };

            for (int i = 0; i < points; i++) {
                float t = i / (float)points;
                float angle = math_ext.TWO_PI * t;
                float3 circlePosition = new float3(math.cos(angle), 0.0f, math.sin(angle));
                vertexPositions.Add(bottomRadius * circlePosition);
                vertexPositions.Add(topRadius * circlePosition + float3_ext.up * height);
                vertexUvs.Add(circlePosition.xz * 0.5f + new float2(0.5f));
                vertexUvs.Add(circlePosition.xz * 0.5f + new float2(0.5f));
            }

            List<GeometryData.Edge> edges = new List<GeometryData.Edge>();
            List<GeometryData.Face> faces = new List<GeometryData.Face>();
            List<GeometryData.FaceCorner> faceCorners = new List<GeometryData.FaceCorner>();
            
            for (int i = 0; i < points; i++) {
                edges.Add(new GeometryData.Edge(0, (i + 1) * 2, i + 6 + 0));
                edges.Add(new GeometryData.Edge((i + 1) * 2, (i + 1) * 2 + 1, i + 6 + 1));
                edges.Add(new GeometryData.Edge(1, (i + 1) * 2 + 1, i + 6 + 2));
                edges.Add(new GeometryData.Edge((i+1) * 2 + 1, ((i + 1) % points + 1) * 2, i + 6 + 3));
                edges.Add(new GeometryData.Edge((i+1) * 2, ((i + 1) % points + 1) * 2, i + 6 + 4));
                edges.Add(new GeometryData.Edge((i+1) * 2 + 1, ((i + 1) % points + 1) * 2 + 1, i + 6 + 5));
            }

            List<float2> uvs = new List<float2>();
            List<float3> faceNormals = new List<float3>();
            int fcI = 0;
            
            float2 verticalUVOffset = float2_ext.up * (uvSettings.VerticalRepetitions - 1);
            float2 horizontalUVOffset = float2_ext.zero;
            float2 horizontalUVOffsetStep = float2_ext.right * ((float) uvSettings.HorizontalRepetitions / points);
            // Faces and FCs
            for (int i = 0; i < points; i++) {
                faces.Add(
                    new GeometryData.Face(
                        (i + 1) * 2, ((i + 1) % points + 1) * 2, 0,
                        fcI++, fcI++, fcI++,
                        i * 6, ((i + 1) % points) * 6, i * 6 + 4
                    )
                );
                faces.Add(
                    new GeometryData.Face(
                        1, ((i + 1) % points + 1) * 2 + 1, (i + 1) * 2 + 1, 
                        fcI++, fcI++, fcI++,
                        i * 6 + 2, i * 6 + 5, ((i + 1) % points) * 6 + 2
                    )
                );
                faces.Add(
                    new GeometryData.Face(
                        (i + 1) * 2, (i + 1) * 2 + 1, ((i + 1) % points + 1) * 2,
                        fcI++, fcI++, fcI++,
                        i * 6 + 4, i * 6 + 3, i * 6 + 1
                    )
                );
                faces.Add(
                    new GeometryData.Face(
                        (i + 1) * 2 + 1, ((i + 1) % points + 1) * 2 + 1, ((i + 1) % points + 1) * 2,
                        fcI++, fcI++, fcI++,
                        i * 6 + 3, ((i + i) % points) * 6 + 1, (i + 1) * 6 - 1
                    )
                );
                
                uvs.Add(vertexUvs[(i + 1) * 2]);
                uvs.Add(vertexUvs[((i + 1) % points + 1) * 2]);
                uvs.Add(vertexUvs[0]);
                uvs.Add(vertexUvs[1]);
                uvs.Add(vertexUvs[((i + 1) % points + 1) * 2 + 1]);
                uvs.Add(vertexUvs[(i + 1) * 2 + 1]);

                uvs.Add(horizontalUVOffset);
                uvs.Add(horizontalUVOffset + float2_ext.up + verticalUVOffset);
                uvs.Add(horizontalUVOffset + horizontalUVOffsetStep);
                uvs.Add(horizontalUVOffset + float2_ext.up + verticalUVOffset);
                uvs.Add(horizontalUVOffset + horizontalUVOffsetStep + float2_ext.up + verticalUVOffset);
                uvs.Add(horizontalUVOffset + horizontalUVOffsetStep);
                
                faceNormals.Add(-float3_ext.up);
                faceNormals.Add(float3_ext.up);
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
                
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 0));
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 0));
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 0));
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 1));
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 1));
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 1));
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 2));
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 2));
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 2));
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 3));
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 3));
                faceCorners.Add(new GeometryData.FaceCorner(i * 4 + 3));
                
                horizontalUVOffset += horizontalUVOffsetStep;
            }

            return new GeometryData(edges, faces, faceCorners, 1, vertexPositions, faceNormals, materialIndices, smoothShaded, creases, uvs);
        }

        public static GeometryData IcosahedronSphericalUV() {
            const int edgeCount = 30;
            const int faceCount = 20;
            const float x = 0.525731112119133606f;
            const float z = 0.850650808352039932f;
            
            List<int> materialIndices = Enumerable.Repeat(0, faceCount).ToList();
            List<bool> shadeSmooth = Enumerable.Repeat(false, faceCount).ToList();
            List<float> crease = Enumerable.Repeat(0.0f, edgeCount).ToList();
            List<float3> vertexPositions = new List<float3> {
                new float3(-x, 0.0f, z),
                new float3(x, 0.0f, z),
                new float3(-x, 0.0f, -z),
                new float3(x, 0.0f, -z),
                new float3(0.0f, z, x),
                new float3(0.0f, z, -x),
                new float3(0.0f, -z, x),
                new float3(0.0f, -z, -x),
                new float3(z, x, 0.0f),
                new float3(-z, x, 0.0f),
                new float3(z, -x, 0.0f),
                new float3(-z, -x, 0.0f)
            };

            
            List<float2> vertexUvs = new List<float2>();
            foreach(float3 pos in vertexPositions) {
                float3 n = math.normalize(pos);
                float u = 0.5f + math.atan2(n.z, n.x) / (math_ext.TWO_PI);
                float v = 0.5f - math.asin(n.y) / math.PI;
                vertexUvs.Add(new float2(u, v));
            }

            int eI = 0;
            List<GeometryData.Edge> edges = new List<GeometryData.Edge> {
                new GeometryData.Edge(0, 4, eI++) { FaceA = 0, FaceB = 1 },
                new GeometryData.Edge(4, 1, eI++) { FaceA = 0, FaceB = 4 },
                new GeometryData.Edge(1, 0, eI++) { FaceA = 0, FaceB = 14 },
                new GeometryData.Edge(0, 9, eI++) { FaceA = 1, FaceB = 16 },
                new GeometryData.Edge(9, 4, eI++) { FaceA = 1, FaceB = 2 },
                new GeometryData.Edge(9, 5, eI++) { FaceA = 2, FaceB = 18 },
                new GeometryData.Edge(5, 4, eI++) { FaceA = 2, FaceB = 3 },
                new GeometryData.Edge(5, 8, eI++) { FaceA = 3, FaceB = 7 },
                new GeometryData.Edge(8, 4, eI++) { FaceA = 3, FaceB = 4 },
                new GeometryData.Edge(8, 1, eI++) { FaceA = 4, FaceB = 5 },
                new GeometryData.Edge(8, 10, eI++) { FaceA = 5, FaceB = 6 },
                new GeometryData.Edge(10, 1, eI++) { FaceA = 5, FaceB = 15 },
                new GeometryData.Edge(8, 3, eI++) { FaceA = 6, FaceB = 7 },
                new GeometryData.Edge(3, 10, eI++) { FaceA = 6, FaceB = 10 },
                new GeometryData.Edge(5, 3, eI++) { FaceA = 7, FaceB = 8 },
                new GeometryData.Edge(5, 2, eI++) { FaceA = 8, FaceB = 18 },
                new GeometryData.Edge(2, 3, eI++) { FaceA = 8, FaceB = 9 },
                new GeometryData.Edge(2, 7, eI++) { FaceA = 9, FaceB = 19 },
                new GeometryData.Edge(7, 3, eI++) { FaceA = 9, FaceB = 10 },
                new GeometryData.Edge(7, 10, eI++) { FaceA = 10, FaceB = 11 },
                new GeometryData.Edge(7, 6, eI++) { FaceA = 11, FaceB = 12 },
                new GeometryData.Edge(6, 10, eI++) { FaceA = 11, FaceB = 15 },
                new GeometryData.Edge(7, 11, eI++) { FaceA = 12, FaceB = 19 },
                new GeometryData.Edge(11, 6, eI++) { FaceA = 12, FaceB = 13 },
                new GeometryData.Edge(11, 0, eI++) { FaceA = 13, FaceB = 16 },
                new GeometryData.Edge(0, 6, eI++) { FaceA = 13, FaceB = 14 },
                new GeometryData.Edge(1, 6, eI++) { FaceA = 14, FaceB = 15 },
                new GeometryData.Edge(11, 9, eI++) { FaceA = 16, FaceB = 17 },
                new GeometryData.Edge(11, 2, eI++) { FaceA = 17, FaceB = 19 },
                new GeometryData.Edge(2, 9, eI++) { FaceA = 17, FaceB = 18 },
            };

            int fcIdx = 0;
            List<GeometryData.Face> faces = new List<GeometryData.Face> {
                new GeometryData.Face(4, 0, 1, fcIdx++, fcIdx++, fcIdx++, 0, 1, 2),
                new GeometryData.Face(9, 0, 4, fcIdx++, fcIdx++, fcIdx++, 3, 4, 0),
                new GeometryData.Face(5, 9, 4, fcIdx++, fcIdx++, fcIdx++, 5, 6, 4),
                new GeometryData.Face(5, 4, 8, fcIdx++, fcIdx++, fcIdx++, 6, 7, 8),
                new GeometryData.Face(8, 4, 1, fcIdx++, fcIdx++, fcIdx++, 8, 9, 1),
                new GeometryData.Face(10, 8, 1, fcIdx++, fcIdx++, fcIdx++, 10, 11, 9),
                new GeometryData.Face(3, 8, 10, fcIdx++, fcIdx++, fcIdx++, 12, 13, 10),
                new GeometryData.Face(3, 5, 8, fcIdx++, fcIdx++, fcIdx++, 14, 12, 7),
                new GeometryData.Face(2, 5, 3, fcIdx++, fcIdx++, fcIdx++, 15, 16, 14),
                new GeometryData.Face(7, 2, 3, fcIdx++, fcIdx++, fcIdx++, 17, 18, 16),
                new GeometryData.Face(10, 7, 3, fcIdx++, fcIdx++, fcIdx++, 19, 13, 18),
                new GeometryData.Face(6, 7, 10, fcIdx++, fcIdx++, fcIdx++, 20, 21, 19),
                new GeometryData.Face(11, 7, 6, fcIdx++, fcIdx++, fcIdx++, 22, 23, 20),
                new GeometryData.Face(0, 11, 6, fcIdx++, fcIdx++, fcIdx++, 24, 25, 23),
                new GeometryData.Face(1, 0, 6, fcIdx++, fcIdx++, fcIdx++, 2, 26, 25),
                new GeometryData.Face(1, 6, 10, fcIdx++, fcIdx++, fcIdx++, 26, 11, 21),
                new GeometryData.Face(0, 9, 11, fcIdx++, fcIdx++, fcIdx++, 3, 24, 27),
                new GeometryData.Face(11, 9, 2, fcIdx++, fcIdx++, fcIdx++, 27, 28, 29),
                new GeometryData.Face(2, 9, 5, fcIdx++, fcIdx++, fcIdx++, 29, 15, 5),
                new GeometryData.Face(2, 7, 11, fcIdx++, fcIdx++, fcIdx++, 17, 28, 22),
            };

            List<float3> normals = new List<float3>();
            List<float2> uvs = new List<float2>();
            List<GeometryData.FaceCorner> faceCorners = new List<GeometryData.FaceCorner>();
            for (int i = 0; i < faces.Count; i++) {
                GeometryData.Face face = faces[i];
                normals.Add(math.normalize(math.cross(vertexPositions[face.VertB] - vertexPositions[face.VertA], vertexPositions[face.VertC] - vertexPositions[face.VertA])));
                uvs.Add(vertexUvs[face.VertA]);
                uvs.Add(vertexUvs[face.VertB]);
                uvs.Add(vertexUvs[face.VertC]);
                faceCorners.Add(new GeometryData.FaceCorner(i) {Vert = face.VertA});
                faceCorners.Add(new GeometryData.FaceCorner(i) {Vert = face.VertB});
                faceCorners.Add(new GeometryData.FaceCorner(i) {Vert = face.VertC});
            }
            
            GeometryData geometry = new GeometryData(edges, faces, faceCorners, 1, vertexPositions, normals, materialIndices, shadeSmooth, crease, uvs);

            quaternion rotQuaternion = quaternion.Euler(math.radians(-30), 0, 0);
            float4x4 trs = float4x4.TRS(float3.zero, rotQuaternion, float3_ext.one);
            Vector3Attribute positionIcosahedron = geometry.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            positionIcosahedron.Yield(pos => math.mul(trs, new float4(pos, 1.0f)).xyz).Into(positionIcosahedron);
            Vector3Attribute normalIcosahedron = geometry.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face);
            normalIcosahedron.Yield(normal => math.normalize(math.mul(trs, new float4(normal, 1.0f)).xyz)).Into(normalIcosahedron);
            
            geometry.StoreAttribute(positionIcosahedron, AttributeDomain.Vertex);
            geometry.StoreAttribute(normalIcosahedron, AttributeDomain.Face);

            return geometry;
        }

        public static GeometryData Icosahedron() {
            const int edgeCount = 30;
            const int faceCount = 20;
            const float x = 0.525731112119133606f;
            const float z = 0.850650808352039932f;
            
            List<int> materialIndices = Enumerable.Repeat(0, faceCount).ToList();
            List<bool> shadeSmooth = Enumerable.Repeat(false, faceCount).ToList();
            List<float> crease = Enumerable.Repeat(0.0f, edgeCount).ToList();
            List<float3> vertexPositions = new List<float3> {
                new float3(-x, 0.0f, z),
                new float3(x, 0.0f, z),
                new float3(-x, 0.0f, -z),
                new float3(x, 0.0f, -z),
                new float3(0.0f, z, x),
                new float3(0.0f, z, -x),
                new float3(0.0f, -z, x),
                new float3(0.0f, -z, -x),
                new float3(z, x, 0.0f),
                new float3(-z, x, 0.0f),
                new float3(z, -x, 0.0f),
                new float3(-z, -x, 0.0f)
            };
            
            int eI = 0;
            List<GeometryData.Edge> edges = new List<GeometryData.Edge> {
                new GeometryData.Edge(0, 4, eI++) { FaceA = 0, FaceB = 1 },
                new GeometryData.Edge(4, 1, eI++) { FaceA = 0, FaceB = 4 },
                new GeometryData.Edge(1, 0, eI++) { FaceA = 0, FaceB = 14 },
                new GeometryData.Edge(0, 9, eI++) { FaceA = 1, FaceB = 16 },
                new GeometryData.Edge(9, 4, eI++) { FaceA = 1, FaceB = 2 },
                new GeometryData.Edge(9, 5, eI++) { FaceA = 2, FaceB = 18 },
                new GeometryData.Edge(5, 4, eI++) { FaceA = 2, FaceB = 3 },
                new GeometryData.Edge(5, 8, eI++) { FaceA = 3, FaceB = 7 },
                new GeometryData.Edge(8, 4, eI++) { FaceA = 3, FaceB = 4 },
                new GeometryData.Edge(8, 1, eI++) { FaceA = 4, FaceB = 5 },
                new GeometryData.Edge(8, 10, eI++) { FaceA = 5, FaceB = 6 },
                new GeometryData.Edge(10, 1, eI++) { FaceA = 5, FaceB = 15 },
                new GeometryData.Edge(8, 3, eI++) { FaceA = 6, FaceB = 7 },
                new GeometryData.Edge(3, 10, eI++) { FaceA = 6, FaceB = 10 },
                new GeometryData.Edge(5, 3, eI++) { FaceA = 7, FaceB = 8 },
                new GeometryData.Edge(5, 2, eI++) { FaceA = 8, FaceB = 18 },
                new GeometryData.Edge(2, 3, eI++) { FaceA = 8, FaceB = 9 },
                new GeometryData.Edge(2, 7, eI++) { FaceA = 9, FaceB = 19 },
                new GeometryData.Edge(7, 3, eI++) { FaceA = 9, FaceB = 10 },
                new GeometryData.Edge(7, 10, eI++) { FaceA = 10, FaceB = 11 },
                new GeometryData.Edge(7, 6, eI++) { FaceA = 11, FaceB = 12 },
                new GeometryData.Edge(6, 10, eI++) { FaceA = 11, FaceB = 15 },
                new GeometryData.Edge(7, 11, eI++) { FaceA = 12, FaceB = 19 },
                new GeometryData.Edge(11, 6, eI++) { FaceA = 12, FaceB = 13 },
                new GeometryData.Edge(11, 0, eI++) { FaceA = 13, FaceB = 16 },
                new GeometryData.Edge(0, 6, eI++) { FaceA = 13, FaceB = 14 },
                new GeometryData.Edge(1, 6, eI++) { FaceA = 14, FaceB = 15 },
                new GeometryData.Edge(11, 9, eI++) { FaceA = 16, FaceB = 17 },
                new GeometryData.Edge(11, 2, eI++) { FaceA = 17, FaceB = 19 },
                new GeometryData.Edge(2, 9, eI++) { FaceA = 17, FaceB = 18 },
            };

            int fcIdx = 0;
            List<GeometryData.Face> faces = new List<GeometryData.Face> {
                new GeometryData.Face(4, 0, 1, fcIdx++, fcIdx++, fcIdx++, 0, 1, 2),
                new GeometryData.Face(4, 9, 0, fcIdx++, fcIdx++, fcIdx++, 3, 4, 0),
                new GeometryData.Face(4, 5, 9, fcIdx++, fcIdx++, fcIdx++, 5, 6, 4),
                new GeometryData.Face(4, 8, 5, fcIdx++, fcIdx++, fcIdx++, 6, 7, 8),
                new GeometryData.Face(4, 1, 8, fcIdx++, fcIdx++, fcIdx++, 8, 9, 1),

                new GeometryData.Face(6, 1, 0, fcIdx++, fcIdx++, fcIdx++, 2, 26, 25),
                new GeometryData.Face(0, 11, 6, fcIdx++, fcIdx++, fcIdx++, 24, 25, 23),
                new GeometryData.Face(11, 0, 9, fcIdx++, fcIdx++, fcIdx++, 3, 24, 27),
                new GeometryData.Face(9, 2, 11, fcIdx++, fcIdx++, fcIdx++, 27, 28, 29),
                new GeometryData.Face(2, 9, 5, fcIdx++, fcIdx++, fcIdx++, 29, 15, 5),
                new GeometryData.Face(5, 3, 2, fcIdx++, fcIdx++, fcIdx++, 15, 16, 14),
                new GeometryData.Face(3, 5, 8, fcIdx++, fcIdx++, fcIdx++, 14, 12, 7),
                new GeometryData.Face(8, 10, 3, fcIdx++, fcIdx++, fcIdx++, 12, 13, 10),
                new GeometryData.Face(10, 8, 1, fcIdx++, fcIdx++, fcIdx++, 10, 11, 9),
                new GeometryData.Face(1, 6, 10, fcIdx++, fcIdx++, fcIdx++, 26, 11, 21),

                new GeometryData.Face(7, 6, 11, fcIdx++, fcIdx++, fcIdx++, 22, 23, 20),
                new GeometryData.Face(7, 11, 2, fcIdx++, fcIdx++, fcIdx++, 17, 28, 22),
                new GeometryData.Face(7, 2, 3, fcIdx++, fcIdx++, fcIdx++, 17, 18, 16),
                new GeometryData.Face(7, 3, 10, fcIdx++, fcIdx++, fcIdx++, 19, 13, 18),
                new GeometryData.Face(7, 10, 6, fcIdx++, fcIdx++, fcIdx++, 20, 21, 19),
            };

            List<float3> normals = new List<float3>();
            List<GeometryData.FaceCorner> faceCorners = new List<GeometryData.FaceCorner>();
            for (int i = 0; i < faces.Count; i++) {
                GeometryData.Face face = faces[i];
                normals.Add(math.normalize(math.cross(vertexPositions[face.VertB] - vertexPositions[face.VertA], vertexPositions[face.VertC] - vertexPositions[face.VertA])));
                faceCorners.Add(new GeometryData.FaceCorner(i) {Vert = face.VertA});
                faceCorners.Add(new GeometryData.FaceCorner(i) {Vert = face.VertB});
                faceCorners.Add(new GeometryData.FaceCorner(i) {Vert = face.VertC});
            }
            
            // UV unwrapping
            const float sideLength = 0.1818182F; // 1 / 5.5
            const float height = 0.1574592F; // sqrt(3) / 11
            List<float2> uvs = new List<float2>();
            
            // Bottom faces
            for (int i = 0; i <= 4; i++) {
                uvs.Add(new float2(sideLength * 0.5f + sideLength * i, 0.0f));
                uvs.Add(new float2(sideLength * (i + 1), 1.0f * height));
                uvs.Add(new float2(sideLength * i, 1.0f * height));
            }

            // Middle faces
            for (int i = 0; i <= 4; i++) {
                // Mid bottom
                uvs.Add(new float2(sideLength * 0.5f + sideLength * i, 2.0f * height));
                uvs.Add(new float2(sideLength * i, 1.0f * height));
                uvs.Add(new float2(sideLength * (i+1), 1.0f * height));
                
                // Mid top
                uvs.Add(new float2(sideLength * (i + 1), 1.0f * height));
                uvs.Add(new float2(sideLength * 0.5f + sideLength * (i + 1), 2.0f * height));
                uvs.Add(new float2(sideLength * 0.5f + sideLength * i, 2.0f * height));
            }
            
            // Top faces
            for (int i = 0; i <= 4; i++) {
                uvs.Add(new float2(sideLength * (i + 1), 3.0f * height));
                uvs.Add(new float2(sideLength * 0.5f + sideLength * i, 2.0f * height));
                uvs.Add(new float2(sideLength * 0.5f + sideLength * (i + 1), 2.0f * height));
            }
            
            GeometryData geometry = new GeometryData(edges, faces, faceCorners, 1, vertexPositions, normals, materialIndices, shadeSmooth, crease, uvs);

            quaternion rotQuaternion = quaternion.Euler(math.radians(-30), 0, 0);
            float4x4 trs = float4x4.TRS(float3.zero, rotQuaternion, float3_ext.one);
            Vector3Attribute positionIcosahedron = geometry.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            positionIcosahedron.Yield(pos => math.mul(trs, new float4(pos, 1.0f)).xyz).Into(positionIcosahedron);
            Vector3Attribute normalIcosahedron = geometry.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face);
            normalIcosahedron.Yield(normal => math.normalize(math.mul(trs, new float4(normal, 1.0f)).xyz)).Into(normalIcosahedron);
            
            geometry.StoreAttribute(positionIcosahedron, AttributeDomain.Vertex);
            geometry.StoreAttribute(normalIcosahedron, AttributeDomain.Face);
            
            return geometry;
        }

        public static GeometryData IcosahedronHardcoded() {
            const float x = 0.525731112119133606f;
            const float z = 0.850650808352039932f;
            
            List<float3> vertexPositions = new List<float3> {
                new float3(-x, 0.0f, z),
                new float3(x, 0.0f, z),
                new float3(-x, 0.0f, -z),
                new float3(x, 0.0f, -z),
                new float3(0.0f, z, x),
                new float3(0.0f, z, -x),
                new float3(0.0f, -z, x),
                new float3(0.0f, -z, -x),
                new float3(z, x, 0.0f),
                new float3(-z, x, 0.0f),
                new float3(z, -x, 0.0f),
                new float3(-z, -x, 0.0f)
            };

            List<int> triangles = new List<int> {
                0, 4, 1, 0, 9, 4, 9, 5, 4, 4, 5, 8, 4, 8, 1,
                8, 10, 1, 8, 3, 10, 5, 3, 8, 5, 2, 3, 2, 7, 3,
                7, 10, 3, 7, 6, 10, 7, 11, 6, 11, 0, 6, 0, 1, 6,
                6, 1, 10, 9, 0, 11, 9, 11, 2, 9, 2, 5, 7, 2, 11
            };

            Mesh mesh = new Mesh();
            mesh.vertices = vertexPositions.Select(v => (Vector3)v).ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            return new GeometryData(mesh, 179.9f);
        }

        public static GeometryData Icosphere(float radius, int subdivisions) {
            if (radius < 0.01f) radius = 0.01f;
            if (subdivisions < 0) subdivisions = 0;

            GeometryData icosphere = Icosahedron();
         
            icosphere.StoreAttribute(Enumerable.Repeat(true, icosphere.Faces.Count).Into<BoolAttribute>("shade_smooth", AttributeDomain.Face));
            
            for (int i = 0; i < subdivisions; i++) {
                icosphere = SimpleSubdivision.Subdivide(icosphere);
            }
            
            // Project on sphere
            Vector3Attribute positionAttribute = icosphere.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex);
            positionAttribute.Yield(pos => math.normalize(pos) * radius).Into(positionAttribute);
                
            // Recalculate normals
            List<float3> newNormals = new List<float3>();
            foreach (GeometryData.Face face in icosphere.Faces) {
                newNormals.Add(math.normalize(math.cross(positionAttribute[face.VertB] - positionAttribute[face.VertA], positionAttribute[face.VertC] - positionAttribute[face.VertA])));
            }

            icosphere.StoreAttribute(positionAttribute, AttributeDomain.Vertex);
            icosphere.StoreAttribute(newNormals.Into<Vector3Attribute>("normal", AttributeDomain.Face), AttributeDomain.Face);

            return icosphere;
        }
    }
}