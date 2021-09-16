using System;
using System.Linq;
using Attribute;
using UnityCommons;
using UnityEngine;

namespace Geometry {
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
            var rhs = new GeometryData(mesh, 0.0f, 179.99f);
            Merge(geometry, rhs);
        }
        
        public static void Merge(GeometryData lhs, GeometryData rhs) {
            //!! 1. Update metadata on `rhs`
            var rhsVertexOffset = lhs.vertices.Count;
            var rhsEdgeOffset = lhs.edges.Count;
            var rhsFaceOffset = lhs.faces.Count;
            var rhsFaceCornerOffset = lhs.faceCorners.Count;
            
            rhs.vertices.ForEach(vertex => {
                for (var i = 0; i < vertex.Edges.Count; i++) {
                    vertex.Edges[i] += rhsEdgeOffset;
                }

                for (var i = 0; i < vertex.Faces.Count; i++) {
                    vertex.Faces[i] += rhsFaceOffset;
                }
                
                for (var i = 0; i < vertex.FaceCorners.Count; i++) {
                    vertex.FaceCorners[i] += rhsFaceCornerOffset;
                }
            });
            
            rhs.edges.ForEach(edge => {
                edge.VertA += rhsVertexOffset;
                edge.VertB += rhsVertexOffset;
                // Note: SelfIndex is no longer used after edge duplicate detection, so there really isn't any point in updating it here
                edge.SelfIndex += rhsEdgeOffset;
                edge.FaceA += rhsFaceOffset;
                if(edge.FaceB != -1) edge.FaceB += rhsFaceOffset;
            });
            
            rhs.faces.ForEach(face => {
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
            });
            
            rhs.faceCorners.ForEach(faceCorner => {
                faceCorner.Face += rhsFaceOffset;
                faceCorner.Vert += rhsVertexOffset;
            });
            
            //!! 2. Merge metadata into `lhs`
            lhs.vertices.AddRange(rhs.vertices);
            lhs.edges.AddRange(rhs.edges);
            lhs.faces.AddRange(rhs.faces);
            lhs.faceCorners.AddRange(rhs.faceCorners);

            //!! 3. Update attributes on `rhs`
            var rhsMaterialIndexOffset = lhs.submeshCount;
            var rhsMaterialIndexAttr = rhs.GetAttribute<IntAttribute>("material_index", AttributeDomain.Face).Select(i => i + rhsMaterialIndexOffset);

            //!! 4. Merge attributes on `lhs`
            // Material index is treated separately
            var lhsMaterialIndexAttr = lhs.GetAttribute<IntAttribute>("material_index", AttributeDomain.Face);
            lhsMaterialIndexAttr.AppendMany(rhsMaterialIndexAttr).Into(lhsMaterialIndexAttr);
            
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
                lhs.attributeManager.Store(pair.Value);
            }
            
            //!! 5. Update metadata on `lhs`
            lhs.submeshCount += rhs.submeshCount;
        }
    }
}