using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attribute;
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
        public static GeometryData Empty => new GeometryData();

        public static void Merge(GeometryData geometry, Mesh mesh) {
            var rhs = new GeometryData(mesh, 0.0f, 179.99f);
            Merge(geometry, rhs);
        }
        
        public static void Merge(GeometryData lhs, GeometryData rhs) {
            //!! 1. Update metadata on `rhs`
            var rhsVertexOffset = lhs.vertices.Count;
            var rhsEdgeOffset = lhs.edges.Count;
            var rhsFaceOffset = lhs.faces.Count;
            var rhsFaceCornerOffset = lhs.faceCorners.Count;

            var rhsVertices = new List<Vertex>();
            var rhsEdges = new List<Edge>();
            var rhsFaces = new List<Face>();
            var rhsFaceCorners = new List<FaceCorner>();
            
            rhs.vertices.ForEach(v => {
                var vertex = (Vertex) v.Clone();
                for (var i = 0; i < vertex.Edges.Count; i++) {
                    vertex.Edges[i] += rhsEdgeOffset;
                }

                for (var i = 0; i < vertex.Faces.Count; i++) {
                    vertex.Faces[i] += rhsFaceOffset;
                }
                
                for (var i = 0; i < vertex.FaceCorners.Count; i++) {
                    vertex.FaceCorners[i] += rhsFaceCornerOffset;
                }
                rhsVertices.Add(vertex);
            });
            
            rhs.edges.ForEach(e => {
                var edge = (Edge) e.Clone();
                edge.VertA += rhsVertexOffset;
                edge.VertB += rhsVertexOffset;
                // Note: SelfIndex is no longer used after edge duplicate detection, so there really isn't any point in updating it here
                edge.SelfIndex += rhsEdgeOffset;
                edge.FaceA += rhsFaceOffset;
                if(edge.FaceB != -1) edge.FaceB += rhsFaceOffset;
                
                rhsEdges.Add(edge);
            });
            
            rhs.faces.ForEach(f => {
                var face = (Face)f.Clone();
                face.VertA += rhsVertexOffset;
                face.VertB += rhsVertexOffset;
                face.VertC += rhsVertexOffset;
                face.EdgeA += rhsEdgeOffset;
                face.EdgeB += rhsEdgeOffset;
                face.EdgeC += rhsEdgeOffset;
                face.FaceCornerA += rhsFaceCornerOffset;
                face.FaceCornerB += rhsFaceCornerOffset;
                face.FaceCornerC += rhsFaceCornerOffset;
                for (var i = 0; i < face.AdjacentFaces.Count; i++) {
                    face.AdjacentFaces[i] += rhsFaceOffset;
                }
                rhsFaces.Add(face);
            });
            
            rhs.faceCorners.ForEach(fc => {
                var faceCorner = (FaceCorner)fc.Clone();
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
            var rhsMaterialIndexOffset = lhs.submeshCount;
            if (rhs.attributeManager.HasAttribute("material_index", AttributeDomain.Face)) {
                var rhsMaterialIndexAttr = rhs.GetAttribute<IntAttribute>("material_index", AttributeDomain.Face).Select(i => i + rhsMaterialIndexOffset);
                
                //!! 4. Merge attributes on `lhs`
                // Material index is treated separately
                if (lhs.attributeManager.HasAttribute("material_index", AttributeDomain.Face)) {
                    var lhsMaterialIndexAttr = lhs.GetAttribute<IntAttribute>("material_index", AttributeDomain.Face);
                    lhsMaterialIndexAttr.AppendMany(rhsMaterialIndexAttr).Into(lhsMaterialIndexAttr);
                } else {
                    var attr = rhsMaterialIndexAttr.Into<IntAttribute>("material_index", AttributeDomain.Face);
                    lhs.attributeManager.Store(attr);
                }
            }

            

            // Rest of attributes just get merged normally
            // First attributes in lhs & rhs
            var allLhsAttributeDictionaries = 
                lhs.attributeManager.VertexAttributes
                   .Union(lhs.attributeManager.EdgeAttributes)
                   .Union(lhs.attributeManager.FaceAttributes)
                   .Union(lhs.attributeManager.FaceCornerAttributes);
           
            foreach (var pair in allLhsAttributeDictionaries) {
                if (string.Equals(pair.Key, "material_index", StringComparison.InvariantCulture) && pair.Value.Domain == AttributeDomain.Face) continue;
                if(!rhs.attributeManager.HasAttribute(pair.Key, pair.Value.Domain)) continue;

                pair.Value
                    .Convert(o => o)
                    .AppendMany(rhs.attributeManager.Request(pair.Key, pair.Value.Domain).Convert(o => o))
                    .Into(pair.Value);
            }
            
            // Then attributes in rhs but not in lhs
            var allRhsAttributeDictionaries = 
                rhs.attributeManager.VertexAttributes
                   .Union(rhs.attributeManager.EdgeAttributes)
                   .Union(rhs.attributeManager.FaceAttributes)
                   .Union(rhs.attributeManager.FaceCornerAttributes);
            foreach (var pair in allRhsAttributeDictionaries) {
                // Skipping already existing attributes because they were merged in previous foreach-loop
                if(lhs.attributeManager.HasAttribute(pair.Key, pair.Value.Domain)) continue;
                lhs.attributeManager.Store((BaseAttribute) pair.Value.Clone());
            }
            
            //!! 5. Update metadata on `lhs`
            lhs.submeshCount += rhs.submeshCount;
        }
    }
}