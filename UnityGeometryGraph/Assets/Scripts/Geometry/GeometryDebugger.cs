using Attribute;
using Sirenix.OdinInspector;
using UnityCommons;
using UnityEngine;
using UnityEngine.Rendering;

namespace Geometry {
    public class GeometryDebugger : MonoBehaviour {
        [SerializeField] private ElementGizmoType gizmoType;
        [SerializeField] private float handleSize = 0.1f;
        [SerializeField] private bool showFaceNormals;
        [SerializeField] private bool showByElement;
    
        [SerializeField, ShowIf(nameof(showByElement)), MinValue(0), MaxValue(nameof(__GetMaxIndex))]
        [InlineButton("@((PropertyValueEntry<int>)$property.ValueEntry).SmartValue += 1", "+")]
        [InlineButton("@((PropertyValueEntry<int>)$property.ValueEntry).SmartValue -= 1", "-")]
        private int index;

        private int __GetMaxIndex() {
            if (source == null || data == null) return -1;
            return gizmoType switch {
                ElementGizmoType.Vertices => source.GeometryData.Vertices.Count - 1,
                ElementGizmoType.Edges => source.GeometryData.Edges.Count - 1,
                ElementGizmoType.Faces => source.GeometryData.Faces.Count - 1,
                ElementGizmoType.FaceEdges => source.GeometryData.Faces.Count - 1,
                _ => 0
            };
        }

        [SerializeField] private GeometryImporter source;
        private GeometryData data => source?.GeometryData;

        private void OnDrawGizmosSelected() {
#if UNITY_EDITOR
            if (gizmoType == ElementGizmoType.None || source == null || data == null) return;
        
            UnityEditor.Handles.matrix = Gizmos.matrix = source.transform.localToWorldMatrix;
            var zTest = UnityEditor.Handles.zTest;
            UnityEditor.Handles.zTest = CompareFunction.LessEqual;
            Random.InitState(0);
        
            if(showByElement) index = index.Clamped(0, __GetMaxIndex());

            var vertices = data.GetAttribute<Vector3Attribute>("position");
        
            if (gizmoType == ElementGizmoType.Vertices) {
                if (showByElement) {
                    var size = handleSize * UnityEditor.HandleUtility.GetHandleSize(vertices[index]);
                    var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                    Gizmos.color = UnityEditor.Handles.color = color;
                    Gizmos.DrawSphere(vertices[index], size);
                } else {
                    foreach (var vertex in vertices) {
                        var size = handleSize * UnityEditor.HandleUtility.GetHandleSize(vertex);
                        var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                        Gizmos.color = UnityEditor.Handles.color = color;
                        Gizmos.DrawSphere(vertex, size);
                    }
                }
            } else if (gizmoType == ElementGizmoType.Edges) {
                if (showByElement) {
                    var edge = data.Edges[index];
                    var v0 = vertices[edge.VertA];
                    var v1 = vertices[edge.VertB];
                    var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                    Gizmos.color = UnityEditor.Handles.color = color;
                    UnityEditor.Handles.DrawAAPolyLine(4.0f, v0, v1);
                } else {
                    foreach (var edge in data.Edges) {
                        var v0 = vertices[edge.VertA];
                        var v1 = vertices[edge.VertB];
                        var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                        Gizmos.color = UnityEditor.Handles.color = color;
                        UnityEditor.Handles.DrawAAPolyLine(4.0f, v0, v1);
                    }
                }
            } else if (gizmoType == ElementGizmoType.Faces) {
                var dist = 0.015f * UnityEditor.HandleUtility.GetHandleSize(transform.position);
                var faceNormals = data.GetAttribute<Vector3Attribute>("normal");

                if (showByElement) {
                    var face = data.Faces[index];
                    var normal = faceNormals[index];
                    var v0 = vertices[face.VertA] + normal * dist;
                    var v1 = vertices[face.VertB] + normal * dist;
                    var v2 = vertices[face.VertC] + normal * dist;
                    var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                    Gizmos.color = UnityEditor.Handles.color = color;
                    UnityEditor.Handles.DrawAAConvexPolygon(v0, v1, v2);

                    if (showFaceNormals) {
                        var mid = (v0 + v1 + v2) / 3.0f;
                        Gizmos.color = UnityEditor.Handles.color = Color.white;
                        UnityEditor.Handles.DrawAAPolyLine(4.0f, mid, mid + normal * 0.15f);
                    }
                } else {
                    for (var i = 0; i < data.Faces.Count; i++) {
                        var face = data.Faces[i];
                        var normal = faceNormals[i];
                        var v0 = vertices[face.VertA] + normal * dist;
                        var v1 = vertices[face.VertB] + normal * dist;
                        var v2 = vertices[face.VertC] + normal * dist;
                        var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                        Gizmos.color = UnityEditor.Handles.color = color;
                        UnityEditor.Handles.DrawAAConvexPolygon(v0, v1, v2);
                    }

                    if (showFaceNormals) {
                        for (var i = 0; i < data.Faces.Count; i++) {
                            var face = data.Faces[i];
                            var normal = faceNormals[i];
                            var v0 = vertices[face.VertA];
                            var v1 = vertices[face.VertB];
                            var v2 = vertices[face.VertC];
                            var mid = (v0 + v1 + v2) / 3.0f;
                            Gizmos.color = UnityEditor.Handles.color = Color.white;
                            UnityEditor.Handles.DrawAAPolyLine(4.0f, mid, mid + normal * 0.15f);
                        }
                    }
                }
            } else if (gizmoType == ElementGizmoType.FaceEdges) {
                if (showByElement) {
                    var face = data.Faces[index];
                
                    var e0v0 = vertices[data.Edges[face.EdgeA].VertA];
                    var e0v1 = vertices[data.Edges[face.EdgeA].VertB];
                    var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                    Gizmos.color = UnityEditor.Handles.color = color;
                    UnityEditor.Handles.DrawAAPolyLine(4.0f, e0v0, e0v1);
                
                    var e1v0 = vertices[data.Edges[face.EdgeB].VertA];
                    var e1v1 = vertices[data.Edges[face.EdgeB].VertB];
                    color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                    Gizmos.color = UnityEditor.Handles.color = color;
                    UnityEditor.Handles.DrawAAPolyLine(4.0f, e1v0, e1v1);
                
                    var e2v0 = vertices[data.Edges[face.EdgeC].VertA];
                    var e2v1 = vertices[data.Edges[face.EdgeC].VertB];
                    color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                    Gizmos.color = UnityEditor.Handles.color = color;
                    UnityEditor.Handles.DrawAAPolyLine(4.0f, e2v0, e2v1);
                } else {
                    foreach (var face in data.Faces) {
                        var e0v0 = vertices[data.Edges[face.EdgeA].VertA];
                        var e0v1 = vertices[data.Edges[face.EdgeA].VertB];
                        var color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                        Gizmos.color = UnityEditor.Handles.color = color;
                        UnityEditor.Handles.DrawAAPolyLine(4.0f, e0v0, e0v1);
                
                        var e1v0 = vertices[data.Edges[face.EdgeB].VertA];
                        var e1v1 = vertices[data.Edges[face.EdgeB].VertB];
                        color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                        Gizmos.color = UnityEditor.Handles.color = color;
                        UnityEditor.Handles.DrawAAPolyLine(4.0f, e1v0, e1v1);
                
                        var e2v0 = vertices[data.Edges[face.EdgeC].VertA];
                        var e2v1 = vertices[data.Edges[face.EdgeC].VertB];
                        color = Random.ColorHSV(0f, 1f, 0.5f, 1f, .75f, 1f);
                        Gizmos.color = UnityEditor.Handles.color = color;
                        UnityEditor.Handles.DrawAAPolyLine(4.0f, e2v0, e2v1);
                    }
                }
            }
            UnityEditor.Handles.zTest = zTest;
#endif
        }
    }

    public enum ElementGizmoType {
        None,
        Vertices,
        Edges,
        Faces,
        FaceEdges,
    }
}