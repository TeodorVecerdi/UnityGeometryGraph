using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.AttributeSystem;
using Unity.Mathematics;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    public static class SimpleSubdivision {
        public static GeometryData Subdivide(GeometryData geometryData, int levels = 1) {
            if (levels <= 0) return geometryData.Clone();
            levels = levels.Clamped(0, Constants.MAX_SUBDIVISIONS);

            GeometryData subdivided = geometryData.Clone();
            for (int i = 0; i < levels; i++) {
                subdivided = Subdivide_Impl(subdivided);
            }

            return subdivided;
        }

        private static GeometryData Subdivide_Impl(GeometryData geometry) {
            if (!geometry.HasAttribute("position", AttributeDomain.Vertex)) return GeometryData.Empty;
            
            Dictionary<int, (int, int)> edgeDict = new Dictionary<int, (int, int)>();
            Dictionary<int, int> midPointDict = new Dictionary<int, int>();
            List<float3> vertexPositions = geometry.GetAttribute<Vector3Attribute>("position", AttributeDomain.Vertex)!.ToList();
            
            List<float2> uvsOriginal;
            if (geometry.HasAttribute("uv", AttributeDomain.FaceCorner))
                uvsOriginal = geometry.GetAttribute<Vector2Attribute>("uv", AttributeDomain.FaceCorner)!.ToList();
            else 
                uvsOriginal = new float2[geometry.FaceCorners.Count].ToList();
            List<float2> uvs = new List<float2>();
            
            ClampedFloatAttribute creaseOriginal = geometry.GetAttribute<ClampedFloatAttribute>("crease", AttributeDomain.Edge)!;
            List<float> crease = new List<float>();
            Vector3Attribute faceNormalsOriginal = geometry.GetAttribute<Vector3Attribute>("normal", AttributeDomain.Face)!;
            List<float3> faceNormals = new List<float3>();
            IntAttribute materialIndicesOriginal = geometry.GetAttribute<IntAttribute>("material_index", AttributeDomain.Face)!;
            List<int> materialIndices = new List<int>();
            BoolAttribute shadeSmoothOriginal = geometry.GetAttribute<BoolAttribute>("shade_smooth", AttributeDomain.Face)!;
            List<bool> shadeSmooth = new List<bool>();

            List<GeometryData.Edge> edges = new List<GeometryData.Edge>();
            List<GeometryData.Face> faces = new List<GeometryData.Face>();
            List<GeometryData.FaceCorner> faceCorners = new List<GeometryData.FaceCorner>();

            HashSet<string> builtinFaceAttributeNames = new HashSet<string> { "normal", "material_index", "shade_smooth" };
            List<(string Name, AttributeType Type, List<object> Values)> allVertexAttributes = geometry.GetAttributes(AttributeDomain.Vertex)
                                                                                                       .Where(attribute => attribute.Name != "position")
                                                                                                       .Select(attribute => (attribute.Name, attribute.Type, attribute.Values)).ToList();
            List<(string Name, AttributeType Type, List<object> Values, List<object>)> allEdgeAttributes = geometry.GetAttributes(AttributeDomain.Edge)
                                                                                                                   .Where(attribute => attribute.Name != "crease")
                                                                                                                   .Select(attribute => (attribute.Name, attribute.Type, attribute.Values, new List<object>())).ToList();
            List<(string Name, AttributeType Type, List<object> Values, List<object>)> allFaceAttributes = geometry.GetAttributes(AttributeDomain.Face)
                                                                                                                   .Where(attribute => !builtinFaceAttributeNames.Contains(attribute.Name))
                                                                                                                   .Select(attribute => (attribute.Name, attribute.Type, attribute.Values, new List<object>())).ToList();
            List<(string Name, AttributeType Type, List<object> Values, List<object>)> allFaceCornerAttributes = geometry.GetAttributes(AttributeDomain.FaceCorner)
                                                                                                                         .Where(attribute => attribute.Name != "uv")
                                                                                                                         .Select(attribute => (attribute.Name, attribute.Type, attribute.Values, new List<object>())).ToList();

            for (int i = 0; i < geometry.Edges.Count; i++) {
                GeometryData.Edge edge = geometry.Edges[i];
                
                float3 midPoint = (vertexPositions[edge.VertA] + vertexPositions[edge.VertB]) * 0.5f;
                GeometryData.Edge edgeA = new GeometryData.Edge(edge.VertA, vertexPositions.Count, edges.Count);
                GeometryData.Edge edgeB = new GeometryData.Edge(vertexPositions.Count, edge.VertB, edges.Count + 1);
                foreach ((string Name, AttributeType Type, List<object> Values, List<object>) edgeAttribute in allEdgeAttributes) {
                    edgeAttribute.Item4.Add(edgeAttribute.Values[edge.SelfIndex]);
                    edgeAttribute.Item4.Add(edgeAttribute.Values[edge.SelfIndex]);
                }
                
                foreach ((string Name, AttributeType Type, List<object> Values) vertexAttribute in allVertexAttributes) {
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

            int fcIdx = 0;
            
            for (int i = 0; i < geometry.Faces.Count; i++) {
                GeometryData.Face face = geometry.Faces[i];
                (int vX, int vY, int vZ) = (face.VertA, face.VertB, face.VertC);
                //!! uvs
                float2 uX = uvsOriginal[face.FaceCornerA];
                float2 uY = uvsOriginal[face.FaceCornerB];
                float2 uZ = uvsOriginal[face.FaceCornerC];
                float2 uXm = math.lerp(uvsOriginal[face.FaceCornerA], uvsOriginal[face.FaceCornerB], 0.5f);
                float2 uYm = math.lerp(uvsOriginal[face.FaceCornerB], uvsOriginal[face.FaceCornerC], 0.5f);
                float2 uZm = math.lerp(uvsOriginal[face.FaceCornerA], uvsOriginal[face.FaceCornerC], 0.5f);
                
                //!! edges in order x->y, y->z, x->z
                (int e_xy, int e_yz, int e_xz) = SortEdges(vX, vY, vZ, geometry.Edges[face.EdgeA], geometry.Edges[face.EdgeB], geometry.Edges[face.EdgeC]);
                //!! midpoint vertex idx
                (int vXm, int vYm, int vZm) = (midPointDict[e_xy], midPointDict[e_yz], midPointDict[e_xz]);
                //!! split edges
                (int e_xxm, int e_xmy) = GetSplitEdges(vX, vY, geometry.Edges[e_xy], edgeDict);
                (int e_yym, int e_ymz) = GetSplitEdges(vY, vZ, geometry.Edges[e_yz], edgeDict);
                (int e_xzm, int e_zmz) = GetSplitEdges(vX, vZ, geometry.Edges[e_xz], edgeDict);
                //!! middle edges
                (int e_xmym, int e_ymzm, int e_xmzm) = (edges.Count, edges.Count + 1, edges.Count + 2); // will add them later

                GeometryData.Face face0 = new GeometryData.Face(vXm, vYm, vZm, fcIdx++, fcIdx++, fcIdx++, e_xmym, e_ymzm, e_xmzm);
                GeometryData.Face face1 = new GeometryData.Face(vX, vXm, vZm, fcIdx++, fcIdx++, fcIdx++, e_xxm, e_xmzm, e_xzm);
                GeometryData.Face face2 = new GeometryData.Face(vXm, vY, vYm, fcIdx++, fcIdx++, fcIdx++, e_xmy, e_yym, e_xmym);
                GeometryData.Face face3 = new GeometryData.Face(vYm, vZ, vZm, fcIdx++, fcIdx++, fcIdx++, e_ymz, e_zmz, e_ymzm);

                GeometryData.Edge edge_xy = new GeometryData.Edge(vXm, vYm, edges.Count + 0);
                GeometryData.Edge edge_yz = new GeometryData.Edge(vYm, vZm, edges.Count + 1);
                GeometryData.Edge edge_xz = new GeometryData.Edge(vXm, vZm, edges.Count + 2);
                
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
                
                foreach ((string Name, AttributeType Type, List<object> Values, List<object>) faceCornerAttribute in allFaceCornerAttributes) {
                    object attr_X = faceCornerAttribute.Values[face.FaceCornerA];
                    object attr_Y = faceCornerAttribute.Values[face.FaceCornerB];
                    object attr_Z = faceCornerAttribute.Values[face.FaceCornerC];
                    object attr_Xm = AttributeConvert.Average(faceCornerAttribute.Type, faceCornerAttribute.Values[face.FaceCornerA], faceCornerAttribute.Values[face.FaceCornerB]);
                    object attr_Ym = AttributeConvert.Average(faceCornerAttribute.Type, faceCornerAttribute.Values[face.FaceCornerB], faceCornerAttribute.Values[face.FaceCornerC]);
                    object attr_Zm = AttributeConvert.Average(faceCornerAttribute.Type, faceCornerAttribute.Values[face.FaceCornerA], faceCornerAttribute.Values[face.FaceCornerC]);
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
                
                foreach ((string Name, AttributeType Type, List<object> Values, List<object>) faceAttribute in allFaceAttributes) {
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
                
                foreach ((string Name, AttributeType Type, List<object> Values, List<object>) edgeAttribute in allEdgeAttributes) {
                    edgeAttribute.Item4.Add(edgeAttribute.Values[e_xy]);
                    edgeAttribute.Item4.Add(edgeAttribute.Values[e_yz]);
                    edgeAttribute.Item4.Add(edgeAttribute.Values[e_xz]);
                }
            }
            
            GeometryData subdivided = new GeometryData(edges, faces, faceCorners, geometry.SubmeshCount, vertexPositions, faceNormals, materialIndices, shadeSmooth,  crease, uvs);
            
            foreach ((string Name, AttributeType Type, List<object> Values) vertexAttribute in allVertexAttributes) {
                subdivided.StoreAttribute(vertexAttribute.Values.Into(vertexAttribute.Name, vertexAttribute.Type, AttributeDomain.Vertex), AttributeDomain.Vertex);
            }
            foreach ((string Name, AttributeType Type, List<object> Values, List<object>) edgeAttribute in allEdgeAttributes) {
                subdivided.StoreAttribute(edgeAttribute.Item4.Into(edgeAttribute.Name, edgeAttribute.Type, AttributeDomain.Edge), AttributeDomain.Edge);
            }
            foreach ((string Name, AttributeType Type, List<object> Values, List<object>) faceAttribute in allFaceAttributes) {
                subdivided.StoreAttribute(faceAttribute.Item4.Into(faceAttribute.Name, faceAttribute.Type, AttributeDomain.Face), AttributeDomain.Face);
            }
            foreach ((string Name, AttributeType Type, List<object> Values, List<object>) faceCornerAttribute in allFaceCornerAttributes) {
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

            (int e1, int e0) = edgeDict[edge.SelfIndex];
            return (e0, e1);
        }

        private static bool Contains(this GeometryData.Edge edge, int vertex) => edge.VertA == vertex || edge.VertB == vertex;
        private static bool VerticesInOrder(GeometryData.Edge edge, int v0, int v1) => edge.VertA == v0 && edge.VertB == v1;
    }
}