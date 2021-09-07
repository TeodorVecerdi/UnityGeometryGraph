using System;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public class MeshImporter : MonoBehaviour {
    [SerializeField] private MeshFilter source;
    [SerializeField] private ElementGizmoType gizmoType;
    [SerializeField] private float handleSize = 0.1f;
    
    [SerializeField] private GeometryData data;


    [Button]
    private void Load() {
        if (source == null) {
            Debug.LogError("Source MeshFilter is null");
            return;
        }
        
        data = new GeometryData(source.sharedMesh);
    }

    private void OnDrawGizmosSelected() {
        if (gizmoType == ElementGizmoType.None) return;
        
        Gizmos.matrix = transform.localToWorldMatrix;
        Handles.matrix = transform.localToWorldMatrix;
        var zTest = Handles.zTest;
        Handles.zTest = CompareFunction.Less;
        Random.InitState(0);
        
        
        if (gizmoType == ElementGizmoType.Vertices) {
            foreach (var vertex in data.Vertices) {
                var size = handleSize * HandleUtility.GetHandleSize(vertex);
                var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                Gizmos.color = Handles.color = color;
                Gizmos.DrawSphere(vertex, size);
            }
        } else if (gizmoType == ElementGizmoType.Edges) {
            foreach (var edge in data.Metadata.Edges) {
                var v0 = data.Vertices[edge.VertA];
                var v1 = data.Vertices[edge.VertB];
                var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                Gizmos.color = Handles.color = color;
                Handles.DrawAAPolyLine(4.0f, v0, v1);
            }
        } else if (gizmoType == ElementGizmoType.Faces) {
            foreach (var face in data.Metadata.Faces) {
                var normal = face.FaceNormal;
                var v0 = data.Vertices[face.VertA] + normal * 0.007f;
                var v1 = data.Vertices[face.VertB] + normal * 0.007f;
                var v2 = data.Vertices[face.VertC] + normal * 0.007f;
                var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                Gizmos.color = Handles.color = color;
                Handles.DrawAAConvexPolygon(v0, v1, v2);
            }
            
            // Normals
            foreach (var face in data.Metadata.Faces) {
                var normal = face.FaceNormal;
                var v0 = data.Vertices[face.VertA] + normal * 0.007f;
                var v1 = data.Vertices[face.VertB] + normal * 0.007f;
                var v2 = data.Vertices[face.VertC] + normal * 0.007f;
                var mid = (v0 + v1 + v2) / 3.0f;
                Gizmos.color = Handles.color = Color.white;
                Handles.DrawAAPolyLine(4.0f, mid, mid + normal * 0.15f);
            }
        }
        Handles.zTest = zTest;
    }
}

public enum ElementGizmoType {
    None,
    Vertices,
    Edges,
    Faces,
}