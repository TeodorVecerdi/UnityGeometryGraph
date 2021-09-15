using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Edge = Geometry.GeometryData.Edge;

namespace Geometry {
    public class EdgeEqualityComparer : EqualityComparer<Edge> {
        private readonly List<float3> vertices;
        private readonly Dictionary<int, List<int>> duplicates;

        public Dictionary<int, List<int>> Duplicates => duplicates;

        public EdgeEqualityComparer(List<float3> vertices) {
            this.vertices = vertices;
            duplicates = new Dictionary<int, List<int>>();
        }
        
        public override bool Equals(Edge first, Edge second) {
            if (first == null && second == null) return true;
            if (first == null || second == null) return false;
            
            var edgeAVertA = vertices[first.VertA];
            var edgeAVertB = vertices[first.VertB];
            var edgeBVertA = vertices[second.VertA];
            var edgeBVertB = vertices[second.VertB];

            var checkA = edgeAVertA.Equals(edgeBVertA) && edgeAVertB.Equals(edgeBVertB);
            var checkB = edgeAVertA.Equals(edgeBVertB) && edgeAVertB.Equals(edgeBVertA);

            if (!checkA && !checkB) return false;
            
            if (!duplicates.ContainsKey(first.SelfIndex)) 
                duplicates[first.SelfIndex] = new List<int>();
            duplicates[first.SelfIndex].Add(second.SelfIndex);

            return true;
        }

        public override int GetHashCode(GeometryData.Edge edge) {
            // return vertices[edge.VertA].GetHashCode() * 37 + vertices[edge.VertB].GetHashCode();
            // return 0;
            return vertices[edge.VertA].GetHashCode() + vertices[edge.VertB].GetHashCode();
        }
    }
}