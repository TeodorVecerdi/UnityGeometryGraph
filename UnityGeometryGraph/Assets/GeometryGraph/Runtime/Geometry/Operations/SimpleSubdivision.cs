using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.AttributeSystem;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    public static class SimpleSubdivision {
        public static GeometryData Subdivide(GeometryData geometryData, int levels = 1) {
            if (levels <= 0) return geometryData.Clone();

            var subdivided = geometryData.Clone();
            for (var i = 0; i < levels; i++) {
                subdivided = Subdivide_Impl(subdivided);
            }

            return subdivided;
        }

        private static GeometryData Subdivide_Impl(GeometryData geometry) {
            var edgeDict = new Dictionary<int, (int, int)>();
            var midPointDict = new Dictionary<int, int>();
            var vertexPositions = geometry.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex)!.ToList();
            
            List<float2> uvsOriginal;
            if (geometry.HasAttribute("uv", AttributeDomain.FaceCorner))
                uvsOriginal = geometry.GetAttribute<Vector2Attribute>("uv", AttributeDomain.FaceCorner)!.ToList();
            else 
                uvsOriginal = new float2[geometry.FaceCorners.Count].ToList();
            var uvs = new List<float2>();
            
            var creaseOriginal = geometry.GetAttribute<ClampedFloatAttribute>("crease", AttributeDomain.Edge)!;
            var crease = new List<float>();
            var faceNormalsOriginal = geometry.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face)!;
            var faceNormals = new List<float3>();
            var materialIndicesOriginal = geometry.GetAttribute<IntAttribute>("material_index", AttributeDomain.Face)!;
            var materialIndices = new List<int>();
            var shadeSmoothOriginal = geometry.GetAttribute<BoolAttribute>("shade_smooth", AttributeDomain.Face)!;
            var shadeSmooth = new List<bool>();

            var edges = new List<GeometryData.Edge>();
            var faces = new List<GeometryData.Face>();
            var faceCorners = new List<GeometryData.FaceCorner>();

            var builtinFaceAttributeNames = new HashSet<string> { "normal", "material_index", "shade_smooth" };
            var allVertexAttributes = geometry.GetAttributes(AttributeDomain.Vertex)
                                              .Where(attribute => attribute.Name != "position")
                                              .Select(attribute => (attribute.Name, attribute.Type, attribute.Values)).ToList();
            var allEdgeAttributes = geometry.GetAttributes(AttributeDomain.Edge)
                                            .Where(attribute => attribute.Name != "crease")
                                            .Select(attribute => (attribute.Name, attribute.Type, attribute.Values, new List<object>())).ToList();
            var allFaceAttributes = geometry.GetAttributes(AttributeDomain.Face)
                                            .Where(attribute => !builtinFaceAttributeNames.Contains(attribute.Name))
                                            .Select(attribute => (attribute.Name, attribute.Type, attribute.Values, new List<object>())).ToList();
            var allFaceCornerAttributes = geometry.GetAttributes(AttributeDomain.FaceCorner)
                                                  .Where(attribute => attribute.Name != "uv")
                                                  .Select(attribute => (attribute.Name, attribute.Type, attribute.Values, new List<object>())).ToList();

            for (var i = 0; i < geometry.Edges.Count; i++) {
                var edge = geometry.Edges[i];
                
                var midPoint = (vertexPositions[edge.VertA] + vertexPositions[edge.VertB]) * 0.5f;
                var edgeA = new GeometryData.Edge(edge.VertA, vertexPositions.Count, edges.Count);
                var edgeB = new GeometryData.Edge(vertexPositions.Count, edge.VertB, edges.Count + 1);
                foreach (var edgeAttribute in allEdgeAttributes) {
                    edgeAttribute.Item4.Add(edgeAttribute.Values[edge.SelfIndex]);
                    edgeAttribute.Item4.Add(edgeAttribute.Values[edge.SelfIndex]);
                }
                
                foreach (var vertexAttribute in allVertexAttributes) {
                    vertexAttribute.Values.Add(AttributeConvert.Average(vertexAttribute.Type, vertexAttribute.Values[edge.VertA], vertexAttribute.Values[edge.VertB]));
                }
                
                edgeDict.Add(i, (edges.Count, edges.Count + 1));
                midPointDict.Add(i, vertexPositions.Count);
                crease.Add(creaseOriginal![i]);
                crease.Add(creaseOriginal[i]);
                vertexPositions.Add(midPoint);
                edges.Add(edgeA);
                edges.Add(edgeB);
            }

            var fcIdx = 0;
            
            for (var i = 0; i < geometry.Faces.Count; i++) {
                var face = geometry.Faces[i];
                var (vX, vY, vZ) = (face.VertA, face.VertB, face.VertC);
                //!! uvs
                var uX = uvsOriginal[face.FaceCornerA];
                var uY = uvsOriginal[face.FaceCornerB];
                var uZ = uvsOriginal[face.FaceCornerC];
                var uXm = math.lerp(uvsOriginal[face.FaceCornerA], uvsOriginal[face.FaceCornerB], 0.5f);
                var uYm = math.lerp(uvsOriginal[face.FaceCornerB], uvsOriginal[face.FaceCornerC], 0.5f);
                var uZm = math.lerp(uvsOriginal[face.FaceCornerA], uvsOriginal[face.FaceCornerC], 0.5f);
                
                //!! edges in order x->y, y->z, x->z
                var (e_xy, e_yz, e_xz) = SortEdges(vX, vY, vZ, geometry.Edges[face.EdgeA], geometry.Edges[face.EdgeB], geometry.Edges[face.EdgeC]);
                //!! midpoint vertex idx
                var (vXm, vYm, vZm) = (midPointDict[e_xy], midPointDict[e_yz], midPointDict[e_xz]);
                //!! split edges
                var (e_xxm, e_xmy) = GetSplitEdges(vX, vY, geometry.Edges[e_xy], edgeDict);
                var (e_yym, e_ymz) = GetSplitEdges(vY, vZ, geometry.Edges[e_yz], edgeDict);
                var (e_xzm, e_zmz) = GetSplitEdges(vX, vZ, geometry.Edges[e_xz], edgeDict);
                //!! middle edges
                var (e_xmym, e_ymzm, e_xmzm) = (edges.Count, edges.Count + 1, edges.Count + 2); // will add them later

                var face0 = new GeometryData.Face(vXm, vYm, vZm, fcIdx++, fcIdx++, fcIdx++, e_xmym, e_ymzm, e_xmzm);
                var face1 = new GeometryData.Face(vX, vXm, vZm, fcIdx++, fcIdx++, fcIdx++, e_xxm, e_xmzm, e_xzm);
                var face2 = new GeometryData.Face(vXm, vY, vYm, fcIdx++, fcIdx++, fcIdx++, e_xmy, e_yym, e_xmym);
                var face3 = new GeometryData.Face(vYm, vZ, vZm, fcIdx++, fcIdx++, fcIdx++, e_ymz, e_zmz, e_ymzm);

                var edge_xy = new GeometryData.Edge(vXm, vYm, edges.Count + 0);
                var edge_yz = new GeometryData.Edge(vYm, vZm, edges.Count + 1);
                var edge_xz = new GeometryData.Edge(vXm, vZm, edges.Count + 2);
                
                //!! Setup face corners and uvs
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 0) {Vert = vXm});
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 0) {Vert = vYm});
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 0) {Vert = vZm});
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 1) {Vert = vX});
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 1) {Vert = vXm});
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 1) {Vert = vZm});
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 2) {Vert = vXm});
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 2) {Vert = vY});
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 2) {Vert = vYm});
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 3) {Vert = vYm});
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 3) {Vert = vZ});
                faceCorners.Add(new GeometryData.FaceCorner(faces.Count + 3) {Vert = vZm});
                uvs.Add(uXm);
                uvs.Add(uYm);
                uvs.Add(uZm);
                uvs.Add(uX);
                uvs.Add(uXm);
                uvs.Add(uZm);
                uvs.Add(uXm);
                uvs.Add(uY);
                uvs.Add(uYm);
                uvs.Add(uYm);
                uvs.Add(uZ);
                uvs.Add(uZm);
                
                foreach (var faceCornerAttribute in allFaceCornerAttributes) {
                    var attr_X = faceCornerAttribute.Values[face.FaceCornerA];
                    var attr_Y = faceCornerAttribute.Values[face.FaceCornerB];
                    var attr_Z = faceCornerAttribute.Values[face.FaceCornerC];
                    var attr_Xm = AttributeConvert.Average(faceCornerAttribute.Type, faceCornerAttribute.Values[face.FaceCornerA], faceCornerAttribute.Values[face.FaceCornerB]);
                    var attr_Ym = AttributeConvert.Average(faceCornerAttribute.Type, faceCornerAttribute.Values[face.FaceCornerB], faceCornerAttribute.Values[face.FaceCornerC]);
                    var attr_Zm = AttributeConvert.Average(faceCornerAttribute.Type, faceCornerAttribute.Values[face.FaceCornerA], faceCornerAttribute.Values[face.FaceCornerC]);
                    faceCornerAttribute.Item4.Add(attr_Xm);
                    faceCornerAttribute.Item4.Add(attr_Ym);
                    faceCornerAttribute.Item4.Add(attr_Zm);
                    faceCornerAttribute.Item4.Add(attr_X);
                    faceCornerAttribute.Item4.Add(attr_Xm);
                    faceCornerAttribute.Item4.Add(attr_Zm);
                    faceCornerAttribute.Item4.Add(attr_Xm);
                    faceCornerAttribute.Item4.Add(attr_Y);
                    faceCornerAttribute.Item4.Add(attr_Ym);
                    faceCornerAttribute.Item4.Add(attr_Ym);
                    faceCornerAttribute.Item4.Add(attr_Z);
                    faceCornerAttribute.Item4.Add(attr_Zm);
                }
                
                faces.Add(face0);
                faces.Add(face1);
                faces.Add(face2);
                faces.Add(face3);
                faceNormals.Add(faceNormalsOriginal[i]);
                faceNormals.Add(faceNormalsOriginal[i]);
                faceNormals.Add(faceNormalsOriginal[i]);
                faceNormals.Add(faceNormalsOriginal[i]);
                materialIndices.Add(materialIndicesOriginal[i]);
                materialIndices.Add(materialIndicesOriginal[i]);
                materialIndices.Add(materialIndicesOriginal[i]);
                materialIndices.Add(materialIndicesOriginal[i]);
                shadeSmooth.Add(shadeSmoothOriginal[i]);
                shadeSmooth.Add(shadeSmoothOriginal[i]);
                shadeSmooth.Add(shadeSmoothOriginal[i]);
                shadeSmooth.Add(shadeSmoothOriginal[i]);
                
                foreach (var faceAttribute in allFaceAttributes) {
                    faceAttribute.Item4.Add(faceAttribute.Values[i]);
                    faceAttribute.Item4.Add(faceAttribute.Values[i]);
                    faceAttribute.Item4.Add(faceAttribute.Values[i]);
                    faceAttribute.Item4.Add(faceAttribute.Values[i]);
                }
                
                edges.Add(edge_xy);
                edges.Add(edge_yz);
                edges.Add(edge_xz);
                crease.Add(0.0f);
                crease.Add(0.0f);
                crease.Add(0.0f);
                
                foreach (var edgeAttribute in allEdgeAttributes) {
                    edgeAttribute.Item4.Add(edgeAttribute.Values[e_xy]);
                    edgeAttribute.Item4.Add(edgeAttribute.Values[e_yz]);
                    edgeAttribute.Item4.Add(edgeAttribute.Values[e_xz]);
                }
            }
            
            var subdivided = new GeometryData(edges, faces, faceCorners, geometry.SubmeshCount, vertexPositions, faceNormals, materialIndices, shadeSmooth,  crease, uvs);
            
            foreach (var vertexAttribute in allVertexAttributes) {
                subdivided.StoreAttribute(vertexAttribute.Values.Into(vertexAttribute.Name, vertexAttribute.Type, AttributeDomain.Vertex), AttributeDomain.Vertex);
            }
            foreach (var edgeAttribute in allEdgeAttributes) {
                subdivided.StoreAttribute(edgeAttribute.Item4.Into(edgeAttribute.Name, edgeAttribute.Type, AttributeDomain.Edge), AttributeDomain.Edge);
            }
            foreach (var faceAttribute in allFaceAttributes) {
                subdivided.StoreAttribute(faceAttribute.Item4.Into(faceAttribute.Name, faceAttribute.Type, AttributeDomain.Face), AttributeDomain.Face);
            }
            foreach (var faceCornerAttribute in allFaceCornerAttributes) {
                subdivided.StoreAttribute(faceCornerAttribute.Item4.Into(faceCornerAttribute.Name, faceCornerAttribute.Type, AttributeDomain.FaceCorner), AttributeDomain.FaceCorner);
            }
            
            return subdivided;
        }

        private static (int eA, int eB, int eC) SortEdges(int x, int y, int z, GeometryData.Edge edge1, GeometryData.Edge edge2, GeometryData.Edge edge3) {
            int e1 = -1, e2 = -1, e3 = -1;

            // Find first edge (x->y)
            if (edge1.Contains(x) && edge1.Contains(y)) {
                e1 = edge1.SelfIndex;
            } else if (edge2.Contains(x) && edge2.Contains(y)) {
                e1 = edge2.SelfIndex;
            } else if (edge3.Contains(x) && edge3.Contains(y)){
                e1 = edge3.SelfIndex;
            }

            // Find second edge (y->z)
            if (edge1.Contains(y) && edge1.Contains(z)) {
                e2 = edge1.SelfIndex;
            } else if (edge2.Contains(y) && edge2.Contains(z)) {
                e2 = edge2.SelfIndex;
            } else if (edge3.Contains(y) && edge3.Contains(z)){
                e2 = edge3.SelfIndex;
            }

            // Find third edge (x->z)
            if (edge1.Contains(x) && edge1.Contains(z)) {
                e3 = edge1.SelfIndex;
            } else if (edge2.Contains(x) && edge2.Contains(z)) {
                e3 = edge2.SelfIndex;
            } else if (edge3.Contains(x) && edge3.Contains(z)){
                e3 = edge3.SelfIndex;
            }
            
            Debug.Assert(e1 != -1, $"e1 != -1 ;; with x:{x} y:{y} z:{z} ;; with e0A:{edge1.VertA} e0B:{edge1.VertB} e1A:{edge2.VertA} e1B:{edge2.VertB} e2A:{edge3.VertA} e2B:{edge3.VertB}");
            Debug.Assert(e2 != -1, $"e2 != -1 ;; with x:{x} y:{y} z:{z} ;; with e0A:{edge1.VertA} e0B:{edge1.VertB} e1A:{edge2.VertA} e1B:{edge2.VertB} e2A:{edge3.VertA} e2B:{edge3.VertB}");
            Debug.Assert(e3 != -1, $"e3 != -1 ;; with x:{x} y:{y} z:{z} ;; with e0A:{edge1.VertA} e0B:{edge1.VertB} e1A:{edge2.VertA} e1B:{edge2.VertB} e2A:{edge3.VertA} e2B:{edge3.VertB}");

            return (e1, e2, e3);
        }

        private static (int e0, int e1) GetSplitEdges(int v0, int v1, GeometryData.Edge edge, Dictionary<int, (int, int)> edgeDict) {
            Debug.Assert(edge.Contains(v0) && edge.Contains(v1), $"edge.Contains(v0) && edge.Contains(v1); [{edge.SelfIndex}]; {v0} & {v1}]");
            
            if (VerticesInOrder(edge, v0, v1)) {
                return edgeDict[edge.SelfIndex];
            }

            var (e1, e0) = edgeDict[edge.SelfIndex];
            return (e0, e1);
        }

        private static bool Contains(this GeometryData.Edge edge, int vertex) => edge.VertA == vertex || edge.VertB == vertex;
        private static bool VerticesInOrder(GeometryData.Edge edge, int v0, int v1) => edge.VertA == v0 && edge.VertB == v1;
    }
}