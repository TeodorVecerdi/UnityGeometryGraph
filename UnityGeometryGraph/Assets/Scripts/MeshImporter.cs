using NaughtyAttributes;
using Sirenix.OdinInspector;
using UnityCommons;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class MeshImporter : MonoBehaviour {
    [SerializeField] private MeshFilter source;
    [SerializeField, Range(0.0f, 0.1f)] private float duplicateDistanceThreshold = 0.01f;
    [SerializeField, Range(0.0f, 180.0f)] private float duplicateNormalAngleThreshold = 5.0f;
    [SerializeField, OnValueChanged("__ResetIndex")] private ElementGizmoType gizmoType;
    [SerializeField] private float handleSize = 0.1f;
    [SerializeField] private bool showFaceNormals;
    [SerializeField] private bool showByElement;
    [SerializeField, IntIncrement, ShowIf(nameof(showByElement)), Min(0), OnValueChanged("__ClampIndex")] private int index;

    private void __ResetIndex() => index = 0;
    private void __ClampIndex() =>
        index = index.Clamped(0, gizmoType switch {
            ElementGizmoType.Vertices => data.Vertices.Count - 1,
            ElementGizmoType.Edges => data.Metadata.Edges.Count - 1,
            ElementGizmoType.Faces => data.Metadata.Faces.Count - 1,
            _ => 0
        });

    [SerializeField] private GeometryData data;


    [Button]
    private void Load() {
        if (source == null) {
            Debug.LogError("Source MeshFilter is null");
            return;
        }
        
        data = new GeometryData(source.sharedMesh, duplicateDistanceThreshold, duplicateNormalAngleThreshold);
    }

    private void OnDrawGizmosSelected() {
        if (gizmoType == ElementGizmoType.None) return;
        
        Gizmos.matrix = transform.localToWorldMatrix;
        Handles.matrix = transform.localToWorldMatrix;
        var zTest = Handles.zTest;
        Handles.zTest = CompareFunction.LessEqual;
        Random.InitState(0);
        
        if(showByElement) __ClampIndex();
        
        
        if (gizmoType == ElementGizmoType.Vertices) {
            if (showByElement) {
                var size = handleSize * HandleUtility.GetHandleSize(data.Vertices[index]);
                var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                Gizmos.color = Handles.color = color;
                Gizmos.DrawSphere(data.Vertices[index], size);
            } else {
                foreach (var vertex in data.Vertices) {
                    var size = handleSize * HandleUtility.GetHandleSize(vertex);
                    var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                    Gizmos.color = Handles.color = color;
                    Gizmos.DrawSphere(vertex, size);
                }
            }
        } else if (gizmoType == ElementGizmoType.Edges) {
            if (showByElement) {
                var edge = data.Metadata.Edges[index];
                var v0 = data.Vertices[edge.VertA];
                var v1 = data.Vertices[edge.VertB];
                var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                Gizmos.color = Handles.color = color;
                Handles.DrawAAPolyLine(4.0f, v0, v1);
            } else {
                foreach (var edge in data.Metadata.Edges) {
                    var v0 = data.Vertices[edge.VertA];
                    var v1 = data.Vertices[edge.VertB];
                    var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                    Gizmos.color = Handles.color = color;
                    Handles.DrawAAPolyLine(4.0f, v0, v1);
                }
            }
        } else if (gizmoType == ElementGizmoType.Faces) {
            var dist = 0.015f * HandleUtility.GetHandleSize(transform.position);

            if (showByElement) {
                var face = data.Metadata.Faces[index];
                
                var normal = face.FaceNormal;
                var v0 = data.Vertices[face.VertA] + normal * dist;
                var v1 = data.Vertices[face.VertB] + normal * dist;
                var v2 = data.Vertices[face.VertC] + normal * dist;
                var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                Gizmos.color = Handles.color = color;
                Handles.DrawAAConvexPolygon(v0, v1, v2);

                if (showFaceNormals) {
                    var mid = (v0 + v1 + v2) / 3.0f;
                    Gizmos.color = Handles.color = Color.white;
                    Handles.DrawAAPolyLine(4.0f, mid, mid + normal * 0.15f);
                }
            } else {
                foreach (var face in data.Metadata.Faces) {
                    var normal = face.FaceNormal;
                    var v0 = data.Vertices[face.VertA] + normal * dist;
                    var v1 = data.Vertices[face.VertB] + normal * dist;
                    var v2 = data.Vertices[face.VertC] + normal * dist;
                    var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                    Gizmos.color = Handles.color = color;
                    Handles.DrawAAConvexPolygon(v0, v1, v2);
                }

                if (showFaceNormals) {
                    // Normals
                    foreach (var face in data.Metadata.Faces) {
                        var normal = face.FaceNormal;
                        var v0 = data.Vertices[face.VertA];
                        var v1 = data.Vertices[face.VertB];
                        var v2 = data.Vertices[face.VertC];
                        var mid = (v0 + v1 + v2) / 3.0f;
                        Gizmos.color = Handles.color = Color.white;
                        Handles.DrawAAPolyLine(4.0f, mid, mid + normal * 0.15f);
                    }
                }
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