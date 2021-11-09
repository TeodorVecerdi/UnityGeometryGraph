using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attribute;
using GeometryGraph.Runtime.Geometry;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;
using UnityEngine.Assertions;

namespace GeometryGraph.Runtime.Curve {
    internal static class CurveToGeometry {
        private static readonly HashSet<CurveType> closeCapsSupportedTypes = new() {
            CurveType.Circle,
        };

        private static readonly HashSet<CurveType> fixIncrementalRotationEndsSupportedTypes = new() {
            CurveType.Circle,
        };

        // Returns a `GeometryData` object composed of only vertices and edges, representing the curve
        internal static GeometryData WithoutProfile(CurveData curve) {
            if (curve.Points == 0) return GeometryData.Empty;
            var vertexPositions = new List<float3>();
            var crease = new float[curve.Points - 1].ToList();

            for (var i = 0; i < curve.Points; i++) {
                vertexPositions.Add(curve.Position[i]);
            }

            var edges = new List<GeometryData.Edge>();
            for (var i = 0; i < curve.Points - 1; i++) {
                edges.Add(new GeometryData.Edge(i, i + 1, i));
            }

            if (curve.IsClosed) {
                edges.Add(new GeometryData.Edge(curve.Points - 1, 0, edges.Count));
            }

            var geometry = new GeometryData(edges, new List<GeometryData.Face>(), new List<GeometryData.FaceCorner>(), 1, vertexPositions,
                                            new List<float3>(), new List<int>(), new List<bool>(), crease, new List<float2>());

            geometry.StoreAttribute(curve.Tangent.Into("tangent", AttributeType.Vector3, AttributeDomain.Vertex));
            geometry.StoreAttribute(curve.Normal.Into("normal", AttributeType.Vector3, AttributeDomain.Vertex));
            geometry.StoreAttribute(curve.Binormal.Into("binormal", AttributeType.Vector3, AttributeDomain.Vertex));
            return geometry;
        }

        internal static GeometryData WithProfile(CurveData curve, CurveData profile, CurveToGeometrySettings settings) {
            bool closeCaps = settings.CloseCaps;
            if (curve.IsClosed || !profile.IsClosed || !closeCapsSupportedTypes.Contains(profile.Type) || profile.Points < 3) closeCaps = false;

            int edgeCount = (profile.Points - (profile.IsClosed ? 0 : 1)) * curve.Points
                          + profile.Points * (curve.Points - 1)
                          + (profile.Points - (curve.IsClosed ? 0 : 1)) * (curve.Points - 1)
                          + (closeCaps ? 2 * GetCapEdgeCount(profile) : 0) 
                            // extra edges if the curve is closed
                          + (2 * profile.Points - (profile.IsClosed ? 0 : 1)) * (curve.IsClosed ? 1 : 0); 

            int faceCount = 2 * (profile.Points - (profile.IsClosed ? 0 : 1)) * (curve.Points - 1) 
                          + (closeCaps ? 2 * GetCapFaceCount(profile) : 0) 
                            // extra faces if the curve is closed
                          + 2 * (profile.Points - (profile.IsClosed ? 0 : 1)) * (curve.IsClosed ? 1 : 0);

            // Prepare burst job native arrays
            if (profile.Points > kBurstAlignPointsThreshold) {
                burstAlign_positionSrc = new NativeArray<float4>(profile.Points, Allocator.Persistent);
                burstAlign_tangentSrc = new NativeArray<float4>(profile.Points, Allocator.Persistent);
                burstAlign_normalSrc = new NativeArray<float4>(profile.Points, Allocator.Persistent);
                burstAlign_binormalSrc = new NativeArray<float4>(profile.Points, Allocator.Persistent);
                burstAlign_positionDst = new NativeArray<float3>(profile.Points, Allocator.Persistent);
                burstAlign_tangentDst = new NativeArray<float3>(profile.Points, Allocator.Persistent);
                burstAlign_normalDst = new NativeArray<float3>(profile.Points, Allocator.Persistent);
                burstAlign_binormalDst = new NativeArray<float3>(profile.Points, Allocator.Persistent);

                for (var i = 0; i < profile.Points; i++) {
                    burstAlign_positionSrc[i] = profile.Position[i].float4(1.0f);
                    burstAlign_tangentSrc[i] = profile.Tangent[i].float4();
                    burstAlign_normalSrc[i] = profile.Normal[i].float4();
                    burstAlign_binormalSrc[i] = profile.Binormal[i].float4();
                }
            }

            List<int> materialIndices;
            List<bool> shadeSmooth;
            int capsFaceCount = 2 * GetCapFaceCount(profile);
            int submeshCount = closeCaps && settings.SeparateMaterialForCaps ? 2 : 1;
            
            if (closeCaps && settings.SeparateMaterialForCaps) {
                materialIndices = Enumerable.Repeat(0, faceCount - capsFaceCount).ToList();
                for (var i = 0; i < capsFaceCount; i++) {
                    materialIndices.Add(1);
                }
            } else {
                materialIndices = Enumerable.Repeat(0, faceCount).ToList();
            }

            if (closeCaps) {
                shadeSmooth = Enumerable.Repeat(settings.ShadeSmoothCurve, faceCount - capsFaceCount).ToList();
                for (var i = 0; i < capsFaceCount; i++) {
                    shadeSmooth.Add(settings.ShadeSmoothCaps);
                }
            } else {
                shadeSmooth = Enumerable.Repeat(settings.ShadeSmoothCurve, faceCount).ToList();
            }
            
            List<float> creases = new float[edgeCount].ToList();
            List<float2> uvs = new List<float2>();
            List<float3> vertexPositions = new List<float3>();
            List<float3> faceNormals = new List<float3>();
            
            List<GeometryData.Edge> edges = new List<GeometryData.Edge>();
            List<GeometryData.Face> faces = new List<GeometryData.Face>();
            List<GeometryData.FaceCorner> faceCorners = new List<GeometryData.FaceCorner>();
            
            float rotationOffset = settings.RotationOffset;
            float incrementalRotationOffset = settings.IncrementalRotationOffset;

            // First iteration done outside of loop
            CurveData current = Align(curve, profile, 0, rotationOffset);
            // Add points and edges
            for (var i = 0; i < current.Points; i++) {
                vertexPositions.Add(current.Position[i]);
            }

            for (var i = 0; i < current.Points - 1; i++) {
                edges.Add(new GeometryData.Edge(i, i + 1, i) {
                    FaceA = i * 2 + 1
                });
            }

            if (current.IsClosed) {
                edges.Add(new GeometryData.Edge(vertexPositions.Count - 1, 0, edges.Count) {
                    FaceA = (current.Points - 1) * 2 + 1
                });
            }

            int fcIdx = 0;
            int facesPerIteration = 2 * current.Points - (current.IsClosed ? 0 : 1);
            int middleEdgeCount = current.Points - (current.IsClosed ? 0 : 1);
            int verticalEdgeCount = current.Points;
            int profileEdgeCount = middleEdgeCount;
            int faceIterations = middleEdgeCount;
            for (var index = 1; index < curve.Points; index++) {
                rotationOffset += incrementalRotationOffset;
                current = Align(curve, profile, index, rotationOffset);
                int faceOffset = (index - 1) * facesPerIteration;
                int edgeOffset = edges.Count;
                int vertexOffset = vertexPositions.Count;

                // Middle edges
                for (var i = 0; i < middleEdgeCount; i++) {
                    int fromVertex = vertexOffset - current.Points + i;
                    int toVertex = vertexOffset + (i + 1).mod(current.Points);
                    edges.Add(new GeometryData.Edge(fromVertex, toVertex, edges.Count) {
                        FaceA = faceOffset + i * 2,
                        FaceB = faceOffset + i * 2 + 1
                    });
                }

                // Vertical edges (from profile to profile)
                for (var i = 0; i < verticalEdgeCount; i++) {
                    int fromVertex = vertexOffset - current.Points + i;
                    int toVertex = vertexOffset + i;
                    int faceA, faceB;
                    if (i > 0 && i < verticalEdgeCount - 1) {
                        faceA = faceOffset + 2 * (i - 1) + 1;
                        faceB = faceOffset + i * 2;
                    } else {
                        if (i == 0) {
                            faceB = faceOffset + i * 2;
                            if (current.IsClosed) {
                                faceA = faceOffset + 2 * (i - 1).mod(current.Points) + 1;
                            } else {
                                faceA = -1;
                            }
                        } else {
                            faceA = faceOffset + 2 * (i - 1) + 1;
                            if (current.IsClosed) {
                                faceB = faceOffset + i * 2;
                            } else {
                                faceB = -1;
                            }
                        }
                    }

                    edges.Add(new GeometryData.Edge(fromVertex, toVertex, edges.Count) {
                        FaceA = faceA,
                        FaceB = faceB
                    });
                }

                // Profile edges
                for (var i = 0; i < profileEdgeCount; i++) {
                    int fromVertex = vertexOffset + i;
                    int toVertex = vertexOffset + (i + 1).mod(current.Points);
                    edges.Add(new GeometryData.Edge(fromVertex, toVertex, edges.Count) {
                        FaceA = faceOffset + facesPerIteration + i * 2 + 1,
                        FaceB = faceOffset + i * 2
                    });
                }

                // Vertices
                for (var i = 0; i < current.Points; i++) {
                    vertexPositions.Add(current.Position[i]);
                }

                // Faces
                for (var i = 0; i < faceIterations; i++) {
                    // First face
                    faces.Add(new GeometryData.Face(
                                  vertexOffset + i,
                                  vertexOffset - current.Points + i,
                                  vertexOffset + (i + 1).mod(current.Points),
                                  fcIdx++,
                                  fcIdx++,
                                  fcIdx++,
                                  edgeOffset + middleEdgeCount + verticalEdgeCount + i,
                                  edgeOffset + i,
                                  edgeOffset + middleEdgeCount + i
                              )
                    );
                    // Second face
                    faces.Add(new GeometryData.Face(
                                  vertexOffset - current.Points + i,
                                  vertexOffset - current.Points + (i + 1).mod(current.Points),
                                  vertexOffset + (i + 1).mod(current.Points),
                                  fcIdx++,
                                  fcIdx++,
                                  fcIdx++,
                                  edgeOffset + i,
                                  edgeOffset + middleEdgeCount + (i + 1).mod(current.Points),
                                  edgeOffset - profileEdgeCount + i
                              )
                    );

                    faceNormals.Add(
                        math.normalizesafe(
                            math.cross(
                                vertexPositions[vertexOffset - current.Points + i] - vertexPositions[vertexOffset + i],
                                vertexPositions[vertexOffset + (i + 1).mod(current.Points)] - vertexPositions[vertexOffset + i]
                            ), float3_ext.up
                        )
                    );
                    faceNormals.Add(
                        math.normalizesafe(
                            math.cross(
                                vertexPositions[vertexOffset - current.Points + (i + 1).mod(current.Points)] - vertexPositions[vertexOffset - current.Points + i],
                                vertexPositions[vertexOffset + (i + 1).mod(current.Points)] - vertexPositions[vertexOffset - current.Points + i]
                            ), float3_ext.up
                        )
                    );

                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 2));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 2));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 2));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                    uvs.Add(float2_ext.zero);
                    uvs.Add(float2_ext.right);
                    uvs.Add(float2_ext.up);
                    uvs.Add(float2_ext.right);
                    uvs.Add(float2_ext.one);
                    uvs.Add(float2_ext.up);
                }
            }

            if (curve.IsClosed) {
                int edgeOffset = edges.Count;

                // Add edges for diagonals
                for (var i = 0; i < middleEdgeCount; i++) {
                    int fromVertex = (i + 1).mod(profile.Points);
                    int toVertex = vertexPositions.Count - profile.Points + i;
                    edges.Add(new GeometryData.Edge(fromVertex, toVertex, edges.Count) {
                        FaceA = faces.Count + i * 2,
                        FaceB = faces.Count + i * 2 + 1
                    });
                }
                
                // Add edges from start to end
                for (var i = 0; i < verticalEdgeCount; i++) {
                    int fromVertex = i;
                    int toVertex = vertexPositions.Count - profile.Points + i;
                    int faceA, faceB;
                    if (i > 0 && i < verticalEdgeCount - 1) {
                        faceA = faces.Count + 2 * (i - 1) + 1;
                        faceB = faces.Count + i * 2;
                    } else {
                        if (i == 0) {
                            faceB = faces.Count + i * 2;
                            if (profile.IsClosed) {
                                faceA = faces.Count + 2 * (i - 1).mod(profile.Points) + 1;
                            } else {
                                faceA = -1;
                            }
                        } else {
                            faceA = faces.Count + 2 * (i - 1) + 1;
                            if (profile.IsClosed) {
                                faceB = faces.Count + i * 2;
                            } else {
                                faceB = -1;
                            }
                        }
                    }
                    
                    edges.Add(new GeometryData.Edge(fromVertex, toVertex, edges.Count) {
                        FaceA = faceA,
                        FaceB = faceB
                    });
                }
                
                // Faces
                for (var i = 0; i < faceIterations; i++) {
                    // First face
                    faces.Add(new GeometryData.Face(
                                  vertexPositions.Count - profile.Points + i,
                                  (i + 1).mod(profile.Points),
                                  i,
                                  fcIdx++,
                                  fcIdx++,
                                  fcIdx++,
                                  edgeOffset + middleEdgeCount + i,
                                  i,
                                  edgeOffset + i
                              )
                    );
                    // Second face
                    faces.Add(new GeometryData.Face(
                                  vertexPositions.Count - profile.Points + i,
                                  vertexPositions.Count - profile.Points + (i + 1).mod(profile.Points),
                                  (i + 1).mod(profile.Points),
                                  fcIdx++,
                                  fcIdx++,
                                  fcIdx++,
                                  edgeOffset + i,
                                  edgeOffset + middleEdgeCount + (i + 1).mod(profile.Points),
                                  edgeOffset - profileEdgeCount + i
                              )
                    );
                    
                    Assert.IsTrue(vertexPositions.Count - profile.Points + i >= 0 && vertexPositions.Count - profile.Points + i < vertexPositions.Count);
                    Assert.IsTrue(i >= 0 && i < vertexPositions.Count);
                    Assert.IsTrue((i + 1).mod(profile.Points) >= 0 && (i + 1).mod(profile.Points) < vertexPositions.Count);
                    Assert.IsTrue(vertexPositions.Count - profile.Points + i >= 0 && vertexPositions.Count - profile.Points + i < vertexPositions.Count);
                    Assert.IsTrue((i + 1).mod(profile.Points) >= 0 && (i + 1).mod(profile.Points) < vertexPositions.Count);
                    Assert.IsTrue(vertexPositions.Count - profile.Points + (i + 1).mod(profile.Points) >= 0 && vertexPositions.Count - profile.Points + (i + 1).mod(profile.Points) < vertexPositions.Count);
                    
                    
                    faceNormals.Add(
                        math.normalizesafe(
                            math.cross(
                                vertexPositions[(i + 1).mod(profile.Points)] - vertexPositions[vertexPositions.Count - profile.Points + i],
                                vertexPositions[i] - vertexPositions[vertexPositions.Count - profile.Points + i]
                            ), float3_ext.up
                        )
                    );
                    faceNormals.Add(
                        math.normalizesafe(
                            math.cross(
                                vertexPositions[vertexPositions.Count - profile.Points + (i + 1).mod(profile.Points)] - vertexPositions[vertexPositions.Count - profile.Points + i],
                                vertexPositions[(i + 1).mod(profile.Points)] - vertexPositions[vertexPositions.Count - profile.Points + i]
                            ), float3_ext.up
                        )
                    );

                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 2));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 2));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 2));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                    faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                    uvs.Add(float2_ext.right);
                    uvs.Add(float2_ext.up);
                    uvs.Add(float2_ext.zero);
                    uvs.Add(float2_ext.right);
                    uvs.Add(float2_ext.one);
                    uvs.Add(float2_ext.up);
                }

            }

            if (closeCaps) {
                CloseCapsEnd(curve, current, settings.CapUvType, edges, faces, ref fcIdx, faceNormals, vertexPositions, faceCorners, uvs);

                current = Align(curve, profile, 0, settings.RotationOffset);
                CloseCapsStart(curve, current, settings.CapUvType, edges, faces, ref fcIdx, faceNormals, vertexPositions, faceCorners, uvs);
            } else if (!curve.IsClosed) {
                for (var i = edges.Count - 1; i >= edges.Count - profileEdgeCount; i--) {
                    edges[i].FaceA = -1;
                }
            }

            // Dispose burst native arrays
            if (profile.Points > kBurstAlignPointsThreshold) {
                burstAlign_positionSrc.Dispose();
                burstAlign_tangentSrc.Dispose();
                burstAlign_normalSrc.Dispose();
                burstAlign_binormalSrc.Dispose();
                burstAlign_positionDst.Dispose();
                burstAlign_tangentDst.Dispose();
                burstAlign_normalDst.Dispose();
                burstAlign_binormalDst.Dispose();
            }

            return new GeometryData(edges, faces, faceCorners, submeshCount, vertexPositions, faceNormals, materialIndices, shadeSmooth, creases, uvs);
        }

        private static void CloseCapsStart(
            CurveData curve, CurveData profile, CurveToGeometrySettings.CapUVType uvType, 
            List<GeometryData.Edge> edges, List<GeometryData.Face> faces, 
            ref int fcIdx, 
            List<float3> faceNormals, List<float3> vertexPositions, List<GeometryData.FaceCorner> faceCorners, List<float2> uvs
        ) {
            int currentEdgeOffset = edges.Count;
            int currentFaceOffset = faces.Count;

            for (var i = 2; i <= profile.Points - 2; i++) {
                var edge = new GeometryData.Edge(0, i, edges.Count) {
                    FaceA = currentFaceOffset + i - 2,
                    FaceB = currentFaceOffset + i - 1,
                };
                edges.Add(edge);
            }
            
            var vertexUVs = new List<float2>();
            float2 minUV = new float2(float.MaxValue, float.MaxValue);
            float2 maxUV = new float2(float.MinValue, float.MinValue);
            for (var i = 0; i < profile.Points; i++) {
                float uvX = math.dot(profile.Position[i], curve.Normal[0]); // right vector
                float uvY = math.dot(profile.Position[i], curve.Binormal[0]); // forward vector
                var uv = new float2(uvX, uvY);
                vertexUVs.Add(uv);
                
                minUV = math.min(minUV, uv);
                maxUV = math.max(maxUV, uv);
            }

            if (uvType != CurveToGeometrySettings.CapUVType.WorldSpace) {
                for (var i = 0; i < vertexUVs.Count; i++) {
                    switch (uvType) {
                        case CurveToGeometrySettings.CapUVType.LocalSpace:
                            float2 uv = vertexUVs[i];
                            float2 normalizedUV = (uv - minUV) / (maxUV - minUV);
                            vertexUVs[i] = normalizedUV;
                            break;
                        case CurveToGeometrySettings.CapUVType.WorldSpaceAligned:
                            vertexUVs[i] -= minUV;
                            break;
                        case CurveToGeometrySettings.CapUVType.WorldSpace:
                        default:
                            throw new ArgumentOutOfRangeException(nameof(uvType), uvType, null);
                    }
                }
            }

            // First face
            var firstFace = new GeometryData.Face(
                0, 2, 1,
                fcIdx++, fcIdx++, fcIdx++,
                0, profile.Points == 3 ? 2 : currentEdgeOffset, 1 
            );
            faces.Add(firstFace);
            faceNormals.Add(
                math.normalizesafe(
                    math.cross(
                        vertexPositions[2] - vertexPositions[0],
                        vertexPositions[1] - vertexPositions[0]
                    ), float3_ext.up
                )
            );
            faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
            faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
            faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
            uvs.Add(vertexUVs[0]);
            uvs.Add(vertexUVs[2]);
            uvs.Add(vertexUVs[1]);

            edges[0].FaceB = faces.Count - 1;
            edges[1].FaceB = faces.Count - 1;
            if (profile.Points == 3)
                edges[2].FaceB = faces.Count - 1;

            int capFaceCount = profile.Points - 2;
            for (var i = 1; i < capFaceCount - 1; i++) {
                var face = new GeometryData.Face(
                    0, i + 2, i + 1,
                    fcIdx++, fcIdx++, fcIdx++,
                    currentEdgeOffset + i - 1, currentEdgeOffset + i, i + 1
                );
                faces.Add(face);
                faceNormals.Add(
                    math.normalizesafe(
                        math.cross(
                            vertexPositions[i + 2] - vertexPositions[0],
                            vertexPositions[i + 1] - vertexPositions[0]
                        ), float3_ext.up
                    )
                );
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                uvs.Add(vertexUVs[0]);
                uvs.Add(vertexUVs[i + 2]);
                uvs.Add(vertexUVs[i + 1]);

                edges[i + 1].FaceB = faces.Count - 1;
            }

            // last face
            if (profile.Points > 3) {
                var face = new GeometryData.Face(
                    profile.Points - 2, 0, profile.Points - 1,
                    fcIdx++, fcIdx++, fcIdx++,
                    edges.Count - 1, profile.Points - 1, profile.Points - 2
                );
                faces.Add(face);
                faceNormals.Add(
                    math.normalizesafe(
                        math.cross(
                            vertexPositions[0] - vertexPositions[profile.Points - 2],
                            vertexPositions[profile.Points - 1] - vertexPositions[profile.Points - 2]
                        ), float3_ext.up
                    )
                );
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                uvs.Add(vertexUVs[profile.Points - 2]);
                uvs.Add(vertexUVs[0]);
                uvs.Add(vertexUVs[profile.Points - 1]);

                edges[profile.Points - 2].FaceB = faces.Count - 1;
                edges[profile.Points - 1].FaceB = faces.Count - 1;
            }
        }

        private static void CloseCapsEnd(
            CurveData curve, CurveData profile, CurveToGeometrySettings.CapUVType uvType,
            List<GeometryData.Edge> edges, List<GeometryData.Face> faces,
            ref int fcIdx,
            List<float3> faceNormals, List<float3> vertexPositions, List<GeometryData.FaceCorner> faceCorners, List<float2> uvs
        ) {
            int currentEdgeOffset = edges.Count;
            int currentFaceOffset = faces.Count;
            int vertexStart = vertexPositions.Count - profile.Points;
            int edgeStart = edges.Count - profile.Points - profile.Points + 3;
            
            var vertexUVs = new List<float2>();
            float2 minUV = new float2(float.MaxValue, float.MaxValue);
            float2 maxUV = new float2(float.MinValue, float.MinValue);
            for (var i = 0; i < profile.Points; i++) {
                float uvX = math.dot(profile.Position[i], -curve.Normal[curve.Points - 1]); // right vector
                float uvY = math.dot(profile.Position[i], curve.Binormal[curve.Points - 1]); // forward vector
                var uv = new float2(uvX, uvY);
                vertexUVs.Add(uv);
                
                minUV = math.min(minUV, uv);
                maxUV = math.max(maxUV, uv);
            }
            
            if (uvType != CurveToGeometrySettings.CapUVType.WorldSpace) {
                for (var i = 0; i < vertexUVs.Count; i++) {
                    switch (uvType) {
                        case CurveToGeometrySettings.CapUVType.LocalSpace:
                            float2 uv = vertexUVs[i];
                            float2 normalizedUV = (uv - minUV) / (maxUV - minUV);
                            vertexUVs[i] = normalizedUV;
                            break;
                        case CurveToGeometrySettings.CapUVType.WorldSpaceAligned:
                            vertexUVs[i] -= minUV;
                            break;
                        case CurveToGeometrySettings.CapUVType.WorldSpace:
                        default:
                            throw new ArgumentOutOfRangeException(nameof(uvType), uvType, null);
                    }
                }
            }
            
            for (var i = 2; i <= profile.Points - 2; i++) {
                var edge = new GeometryData.Edge(vertexStart, vertexStart + i, edges.Count) {
                    FaceA = currentFaceOffset + i - 2,
                    FaceB = currentFaceOffset + i - 1,
                };
                edges.Add(edge);
            }
            
            // First face
            var firstFace = new GeometryData.Face(
                vertexStart + 2, vertexStart, vertexStart + 1,
                fcIdx++, fcIdx++, fcIdx++,
                profile.Points == 3 ? edgeStart + 2 : currentEdgeOffset, edgeStart + 0, edgeStart + 1
            );
            faces.Add(firstFace);
            faceNormals.Add(
                math.normalizesafe(
                    math.cross(
                        vertexPositions[vertexStart + 0] - vertexPositions[vertexStart + 2],
                        vertexPositions[vertexStart + 1] - vertexPositions[vertexStart + 2]
                    ), float3_ext.up
                )
            );
            faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
            faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
            faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
            uvs.Add(vertexUVs[(vertexStart + 2).mod(profile.Points)]);
            uvs.Add(vertexUVs[vertexStart.mod(profile.Points)]);
            uvs.Add(vertexUVs[(vertexStart + 1).mod(profile.Points)]);
            edges[edgeStart].FaceA = faces.Count - 1;
            edges[edgeStart + 1].FaceA = faces.Count - 1;
            if (profile.Points == 3)
                edges[edgeStart + 2].FaceA = faces.Count - 1;
            
            int capFaceCount = profile.Points - 2;
            for (var i = 1; i < capFaceCount - 1; i++) {
                var face = new GeometryData.Face(
                    vertexStart + i + 2, vertexStart, vertexStart + i + 1,
                    fcIdx++, fcIdx++, fcIdx++,
                    currentEdgeOffset + i, currentEdgeOffset + i - 1, edgeStart + i + 1
                );
                faces.Add(face);
                faceNormals.Add(
                    math.normalizesafe(
                        math.cross(
                            vertexPositions[vertexStart] - vertexPositions[vertexStart + i + 2],
                            vertexPositions[vertexStart + i + 1] - vertexPositions[vertexStart + i + 2]
                        ), float3_ext.up
                    )
                );
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                uvs.Add(vertexUVs[(vertexStart + i + 2).mod(profile.Points)]);
                uvs.Add(vertexUVs[vertexStart.mod(profile.Points)]);
                uvs.Add(vertexUVs[(vertexStart + i + 1).mod(profile.Points)]);
                edges[edgeStart + i + 1].FaceA = faces.Count - 1;
            }
            
            // Last face
            if (profile.Points > 3) {
                var lastFace = new GeometryData.Face(
                    vertexStart + profile.Points - 1, vertexStart, vertexStart + profile.Points - 2,
                    fcIdx++, fcIdx++, fcIdx++,
                    edgeStart + profile.Points - 1, currentEdgeOffset + profile.Points - 4, edgeStart + profile.Points - 2
                );
                faces.Add(lastFace);
                faceNormals.Add(
                    math.normalizesafe(
                        math.cross(
                            vertexPositions[vertexStart] - vertexPositions[vertexStart + profile.Points - 1],
                            vertexPositions[vertexStart + profile.Points - 2] - vertexPositions[vertexStart + profile.Points - 1]
                        ), float3_ext.up
                    )
                );
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count - 1));
                uvs.Add(vertexUVs[(vertexStart + profile.Points - 1).mod(profile.Points)]);
                uvs.Add(vertexUVs[vertexStart.mod(profile.Points)]);
                uvs.Add(vertexUVs[(vertexStart + profile.Points - 2).mod(profile.Points)]);
                edges[edgeStart + profile.Points - 1].FaceA = faces.Count - 1;
                edges[edgeStart + profile.Points - 2].FaceA = faces.Count - 1;
            }
        }


        private static CurveData Align(CurveData alignOn, CurveData toAlign, int index, float rotationOffset) {
            Assert.IsTrue(index.InRange(..alignOn.Points));

            var rotation = float4x4.RotateY(math.radians(rotationOffset));
            var align = new float4x4(alignOn.Normal[index].float4(), alignOn.Tangent[index].float4(), alignOn.Binormal[index].float4(), alignOn.Position[index].float4(1.0f));
            var matrix = math.mul(align, rotation);

            if (toAlign.Points > kBurstAlignPointsThreshold) {
                var job = new AlignCurveJob(burstAlign_positionDst, burstAlign_tangentDst, burstAlign_normalDst, burstAlign_binormalDst,
                                            burstAlign_positionSrc, burstAlign_tangentSrc, burstAlign_normalSrc, burstAlign_binormalSrc,
                                            matrix);
                job.Schedule(toAlign.Points, Environment.ProcessorCount).Complete();
                return new CurveData(toAlign.Type, toAlign.Points, toAlign.IsClosed, burstAlign_positionDst.ToList(), burstAlign_tangentDst.ToList(),
                                     burstAlign_normalDst.ToList(), burstAlign_binormalDst.ToList());
            }

            var position = new List<float3>();
            var tangent = new List<float3>();
            var normal = new List<float3>();
            var binormal = new List<float3>();
            for (var i = 0; i < toAlign.Points; i++) {
                position.Add(math.mul(matrix, toAlign.Position[i].float4(1.0f)).xyz);
                tangent.Add(math.mul(matrix, toAlign.Tangent[i].float4()).xyz);
                normal.Add(math.mul(matrix, toAlign.Normal[i].float4()).xyz);
                binormal.Add(math.mul(matrix, toAlign.Binormal[i].float4()).xyz);
            }

            return new CurveData(toAlign.Type, toAlign.Points, toAlign.IsClosed, position, tangent, normal, binormal);
        }

        private static int GetCapEdgeCount(CurveData profile) {
            return profile.Points - 3;
        }

        private static int GetCapFaceCount(CurveData profile) {
            return profile.Points - 2;
        }


        private const int kBurstAlignPointsThreshold = 8;
        private static NativeArray<float4> burstAlign_positionSrc;
        private static NativeArray<float4> burstAlign_tangentSrc;
        private static NativeArray<float4> burstAlign_normalSrc;
        private static NativeArray<float4> burstAlign_binormalSrc;
        private static NativeArray<float3> burstAlign_positionDst;
        private static NativeArray<float3> burstAlign_tangentDst;
        private static NativeArray<float3> burstAlign_normalDst;
        private static NativeArray<float3> burstAlign_binormalDst;

        [BurstCompile(CompileSynchronously = true)]
        private struct AlignCurveJob : IJobParallelFor {
            [ReadOnly] private NativeArray<float4> positionSrc;
            [ReadOnly] private NativeArray<float4> tangentSrc;
            [ReadOnly] private NativeArray<float4> normalSrc;
            [ReadOnly] private NativeArray<float4> binormalSrc;

            [WriteOnly] private NativeArray<float3> positionDst;
            [WriteOnly] private NativeArray<float3> tangentDst;
            [WriteOnly] private NativeArray<float3> normalDst;
            [WriteOnly] private NativeArray<float3> binormalDst;

            private readonly float4x4 matrix;

            public AlignCurveJob(
                NativeArray<float3> positionDst, NativeArray<float3> tangentDst, NativeArray<float3> normalDst, NativeArray<float3> binormalDst,
                NativeArray<float4> positionSrc, NativeArray<float4> tangentSrc, NativeArray<float4> normalSrc, NativeArray<float4> binormalSrc,
                float4x4 matrix) {
                this.positionDst = positionDst;
                this.tangentDst = tangentDst;
                this.normalDst = normalDst;
                this.binormalDst = binormalDst;

                this.positionSrc = positionSrc;
                this.tangentSrc = tangentSrc;
                this.normalSrc = normalSrc;
                this.binormalSrc = binormalSrc;

                this.matrix = matrix;
            }

            public void Execute(int index) {
                positionDst[index] = math.mul(matrix, positionSrc[index]).xyz;
                tangentDst[index] = math.mul(matrix, tangentSrc[index]).xyz;
                normalDst[index] = math.mul(matrix, normalSrc[index]).xyz;
                binormalDst[index] = math.mul(matrix, binormalSrc[index]).xyz;
            }
        }
    }
}