using System;
using System.Collections.Generic;

namespace GeometryGraph.Runtime.Geometry {
    public partial class GeometryData {
        [Serializable]
        public class Vertex : ICloneable {
            public List<int> Edges;
            public List<int> Faces;
            public List<int> FaceCorners;

            public Vertex() {
                Edges = new List<int>();
                Faces = new List<int>();
                FaceCorners = new List<int>();
            }

            public object Clone() {
                return new Vertex { Edges = new List<int>(Edges), Faces = new List<int>(Faces), FaceCorners = new List<int>(FaceCorners) };
            }
        }

        [Serializable]
        public class Edge : ICloneable {
            public int VertA;
            public int VertB;
            public int FaceA = -1;
            public int FaceB = -1;

            // Note: Only used for edge duplicate detection/removal and is not needed from there on.
            // It's also not valid from that point forwards 
            public int SelfIndex;

            public Edge(int vertA, int vertB, int selfIndex) {
                VertA = vertA;
                VertB = vertB;
                SelfIndex = selfIndex;
            }

            public object Clone() {
                return new Edge(VertA, VertB, SelfIndex) { FaceA = FaceA, FaceB = FaceB };
            }
        }

        [Serializable]
        public class Face : ICloneable {
            public int VertA;
            public int VertB;
            public int VertC;

            public int FaceCornerA;
            public int FaceCornerB;
            public int FaceCornerC;

            public int EdgeA;
            public int EdgeB;
            public int EdgeC;

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

            public object Clone() {
                return new Face(VertA, VertB, VertC, FaceCornerA, FaceCornerB, FaceCornerC, EdgeA, EdgeB, EdgeC) { AdjacentFaces = new List<int>(AdjacentFaces) };
            }
        }

        [Serializable]
        public class FaceCorner : ICloneable {
            public int Vert = -1;
            public int Face = -1;

            public FaceCorner(int face) {
                Face = face;
            }

            public object Clone() {
                return new FaceCorner(Face) { Vert = Vert };
            }
        }
    }
}