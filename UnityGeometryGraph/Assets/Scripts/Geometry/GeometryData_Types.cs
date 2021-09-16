using System;
using System.Collections.Generic;

namespace Geometry {
    public partial class GeometryData {
        [Serializable]
        public class Vertex {
            public List<int> Edges;
            public List<int> Faces;
            public List<int> FaceCorners;

            public Vertex() {
                Edges = new List<int>();
                Faces = new List<int>();
                FaceCorners = new List<int>();
            }
        }

        [Serializable]
        public class Edge {
            public int VertA;
            public int VertB;
            public int FaceA = -1;
            public int FaceB = -1;

            public int SelfIndex;

            public Edge(int vertA, int vertB, int selfIndex) {
                VertA = vertA;
                VertB = vertB;
                SelfIndex = selfIndex;
            }
        }

        [Serializable]
        public class Face {
            public int VertA;
            public int VertB;
            public int VertC;

            public int FaceCornerA;
            public int FaceCornerB;
            public int FaceCornerC;

            public int EdgeA = -1;
            public int EdgeB = -1;
            public int EdgeC = -1;

            public List<int> AdjacentFaces;

            public Face(int vertA, int vertB, int vertC, int faceCornerA, int faceCornerB, int faceCornerC, int edgeA, int edgeB, int edgeC) {
                VertA = vertA;
                VertB = vertB;
                VertC = vertC;

                FaceCornerA = faceCornerA;
                FaceCornerB = faceCornerB;
                FaceCornerC = faceCornerC;

                EdgeA = edgeA;
                EdgeB = edgeB;
                EdgeC = edgeC;

                AdjacentFaces = new List<int>();
            }
        }

        [Serializable]
        public class FaceCorner {
            public int Vert = -1;
            public int Face = -1;

            public FaceCorner(int face) {
                Face = face;
            }
        }
    }
}