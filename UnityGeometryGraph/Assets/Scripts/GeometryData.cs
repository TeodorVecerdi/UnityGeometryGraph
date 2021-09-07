using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeometryData {
    [SerializeField] public List<Vector3> Vertices;
    [SerializeField] public List<int> Triangles;
    [SerializeField] public GeometryMetadata Metadata;

    public GeometryData(Mesh mesh) {
        Vertices = new List<Vector3>();
        Triangles = new List<int>();
        
        mesh.GetVertices(Vertices);
        mesh.GetTriangles(Triangles, 0);

        BuildMetadata(mesh.normals);
    }

    private void BuildMetadata(Vector3[] normals) {
        Metadata = new GeometryMetadata(this);

        for (var i = 0; i < Triangles.Count; i+=3) {
            var idxA = Triangles[i];
            var idxB = Triangles[i + 1];
            var idxC = Triangles[i + 2];

            var faceNormal = normals[idxA] + normals[idxB] + normals[idxC];
            
            var face = new Face(idxA, idxB, idxC, faceNormal / 3.0f);
            var edgeA = face.EdgeA = new Edge(idxA, idxB);
            var edgeB = face.EdgeB = new Edge(idxB, idxC);
            var edgeC = face.EdgeC = new Edge(idxC, idxA);
            
            Metadata.Faces.Add(face);
            Metadata.Edges.Add(edgeA);
            Metadata.Edges.Add(edgeB);
            Metadata.Edges.Add(edgeC);
        }
    }
    
    
    [Serializable]
    public class GeometryMetadata {
        [SerializeReference, HideInInspector] private GeometryData owner;

        public List<Edge> Edges;
        public List<Face> Faces;

        public GeometryMetadata(GeometryData owner) {
            this.owner = owner;
            Edges = new List<Edge>();
            Faces = new List<Face>();
        }
    }
    
    [Serializable]
    public class Edge {
        public int VertA;
        public int VertB;

        public Edge(int vertA, int vertB) {
            VertA = vertA;
            VertB = vertB;
        }
    }

    [Serializable]
    public class Face {
        public int VertA;
        public int VertB;
        public int VertC;
        public Vector3 FaceNormal;

        [SerializeReference] public Edge EdgeA;
        [SerializeReference] public Edge EdgeB;
        [SerializeReference] public Edge EdgeC;

        public Face(int vertA, int vertB, int vertC, Vector3 faceNormal) {
            VertA = vertA;
            VertB = vertB;
            VertC = vertC;
            FaceNormal = faceNormal;
        }
    }
}


