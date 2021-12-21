using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.AttributeSystem;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    public static class GeometryDataExtensions {
        public static void MergeWith(this GeometryData geometry, Mesh mesh) {
            GeometryData.Merge(geometry, mesh);
        }
        
        public static void MergeWith(this GeometryData lhs, GeometryData rhs) {
            GeometryData.Merge(lhs, rhs);   
        }
    }
    
    public partial class GeometryData {
        public static void Merge(GeometryData geometry, Mesh mesh) {
            GeometryData rhs = new(mesh, 179.99f);
            Merge(geometry, rhs);
        }
        
        public static void Merge(GeometryData lhs, GeometryData rhs) {
            //!! 1. Update metadata on `rhs`
            int rhsVertexOffset = lhs.vertices.Count;
            int rhsEdgeOffset = lhs.edges.Count;
            int rhsFaceOffset = lhs.faces.Count;
            int rhsFaceCornerOffset = lhs.faceCorners.Count;

            List<Vertex> rhsVertices = new();
            List<Edge> rhsEdges = new();
            List<Face> rhsFaces = new();
            List<FaceCorner> rhsFaceCorners = new();
            
            rhs.vertices.ForEach(v => {
                Vertex vertex = (Vertex) v.Clone();
                for (int i = 0; i < vertex.Edges.Count; i++) {
                    vertex.Edges[i] += rhsEdgeOffset;
                }

                for (int i = 0; i < vertex.Faces.Count; i++) {
                    vertex.Faces[i] += rhsFaceOffset;
                }
                
                for (int i = 0; i < vertex.FaceCorners.Count; i++) {
                    vertex.FaceCorners[i] += rhsFaceCornerOffset;
                }
                rhsVertices.Add(vertex);
            });
            
            rhs.edges.ForEach(e => {
                Edge edge = (Edge) e.Clone();
                edge.VertA += rhsVertexOffset;
                edge.VertB += rhsVertexOffset;
                // NOTE: SelfIndex is no longer used after edge duplicate detection, so there really isn't any point in updating it here
                edge.SelfIndex += rhsEdgeOffset;
                edge.FaceA += rhsFaceOffset;
                if(edge.FaceB != -1) edge.FaceB += rhsFaceOffset;
                
                rhsEdges.Add(edge);
            });
            
            rhs.faces.ForEach(f => {
                Face face = (Face)f.Clone();
                face.VertA += rhsVertexOffset;
                face.VertB += rhsVertexOffset;
                face.VertC += rhsVertexOffset;
                face.EdgeA += rhsEdgeOffset;
                face.EdgeB += rhsEdgeOffset;
                face.EdgeC += rhsEdgeOffset;
                face.FaceCornerA += rhsFaceCornerOffset;
                face.FaceCornerB += rhsFaceCornerOffset;
                face.FaceCornerC += rhsFaceCornerOffset;
                for (int i = 0; i < face.AdjacentFaces.Count; i++) {
                    face.AdjacentFaces[i] += rhsFaceOffset;
                }
                rhsFaces.Add(face);
            });
            
            rhs.faceCorners.ForEach(fc => {
                FaceCorner faceCorner = (FaceCorner)fc.Clone();
                faceCorner.Face += rhsFaceOffset;
                faceCorner.Vert += rhsVertexOffset;
                rhsFaceCorners.Add(faceCorner);
            });
            
            //!! 2. Merge metadata into `lhs`
            lhs.vertices.AddRange(rhsVertices);
            lhs.edges.AddRange(rhsEdges);
            lhs.faces.AddRange(rhsFaces);
            lhs.faceCorners.AddRange(rhsFaceCorners);

            //!! 3. Update attributes on `rhs`
            int rhsMaterialIndexOffset = lhs.submeshCount;
            if (rhs.attributeManager.HasAttribute(AttributeId.Material, AttributeDomain.Face)) {
                IEnumerable<int> rhsMaterialIndexAttr = rhs.GetAttribute<IntAttribute>(AttributeId.Material, AttributeDomain.Face).Select(i => i + rhsMaterialIndexOffset);
                
                //!! 4. Merge attributes on `lhs`
                // Material index is treated separately
                if (lhs.attributeManager.HasAttribute(AttributeId.Material, AttributeDomain.Face)) {
                    IntAttribute lhsMaterialIndexAttr = lhs.GetAttribute<IntAttribute>(AttributeId.Material, AttributeDomain.Face);
                    lhsMaterialIndexAttr.AppendMany(rhsMaterialIndexAttr).Into(lhsMaterialIndexAttr);
                } else {
                    IntAttribute attr = rhsMaterialIndexAttr.Into<IntAttribute>(AttributeId.Material, AttributeDomain.Face);
                    lhs.attributeManager.Store(attr);
                }
            }

            

            // Rest of attributes just get merged normally
            // First attributes in lhs & rhs
            IEnumerable<KeyValuePair<string, BaseAttribute>> allLhsAttributeDictionaries = 
                lhs.attributeManager.VertexAttributes
                   .Union(lhs.attributeManager.EdgeAttributes)
                   .Union(lhs.attributeManager.FaceAttributes)
                   .Union(lhs.attributeManager.FaceCornerAttributes);
           
            foreach (KeyValuePair<string, BaseAttribute> pair in allLhsAttributeDictionaries) {
                if (string.Equals(pair.Key, AttributeId.Material, StringComparison.InvariantCulture) && pair.Value.Domain == AttributeDomain.Face) continue;
                if(!rhs.attributeManager.HasAttribute(pair.Key, pair.Value.Domain)) continue;

                pair.Value
                    .Convert(o => o)
                    .AppendMany(rhs.attributeManager.Request(pair.Key, pair.Value.Domain).Convert(o => o))
                    .Into(pair.Value);
            }
            
            // Then attributes in rhs but not in lhs
            IEnumerable<KeyValuePair<string, BaseAttribute>> allRhsAttributeDictionaries = 
                rhs.attributeManager.VertexAttributes
                   .Union(rhs.attributeManager.EdgeAttributes)
                   .Union(rhs.attributeManager.FaceAttributes)
                   .Union(rhs.attributeManager.FaceCornerAttributes);
            foreach (KeyValuePair<string, BaseAttribute> pair in allRhsAttributeDictionaries) {
                // Skipping already existing attributes because they were merged in previous foreach-loop
                if(lhs.attributeManager.HasAttribute(pair.Key, pair.Value.Domain)) continue;
                lhs.attributeManager.Store((BaseAttribute) pair.Value.Clone());
            }
            
            //!! 5. Update metadata on `lhs`
            lhs.submeshCount += rhs.submeshCount;
        }
    }
}