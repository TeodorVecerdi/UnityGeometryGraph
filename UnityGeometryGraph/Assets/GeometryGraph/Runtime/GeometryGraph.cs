using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using GeometryGraph.Runtime.Curve;
using GeometryGraph.Runtime.Geometry;
using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Runtime {
    public partial class GeometryGraph : MonoBehaviour {
        [SerializeField] private RuntimeGraphObject graph;
        [SerializeField] private GeometryExporter exporter;
        
        [SerializeField] private string graphGuid;
        [SerializeField] private GeometryGraphSceneData sceneData = new();
        [SerializeField] private CurveVisualizerSettings curveVisualizerSettings = new();

        [SerializeField] private CurveData curveData;
        [SerializeField] private GeometryData geometryData;
        
        internal RuntimeGraphObject Graph => graph;
        internal GeometryGraphSceneData SceneData => sceneData;
        
        internal string GraphGuid {
            get => graphGuid;
            set => graphGuid = value;
        }

        internal void OnPropertiesChanged(int newPropertyHashCode) {
            sceneData.PropertyHashCode = newPropertyHashCode;
            List<string> toRemove = SceneData.PropertyData.Keys.Where(key => Graph.RuntimeData.Properties.All(property => !string.Equals(property.Guid, key, StringComparison.InvariantCulture))).ToList();
            foreach (string key in toRemove) {
                SceneData.RemoveProperty(key);
            }

            foreach (Property property in Graph.RuntimeData.Properties) {
                if (!SceneData.PropertyData.ContainsKey(property.Guid)) {
                    SceneData.AddProperty(property.Guid, property.ReferenceName, new PropertyValue(property));
                }
                SceneData.EnsureCorrectReferenceName(property.Guid, property.ReferenceName);
            }

            SceneData.EnsureCorrectGuidsAndReferences(Graph.RuntimeData.Properties);
        }

        internal void OnDefaultPropertyValueChanged(Property property) {
            sceneData.UpdatePropertyDefaultValue(property.Guid, property.DefaultValue);
        }
        
        private void OnDrawGizmos() {
            if (!curveVisualizerSettings.Enabled || curveData == null || curveData.Type == CurveType.None) return;

            Handles.matrix = Gizmos.matrix = transform.localToWorldMatrix;

            if (curveVisualizerSettings.ShowPoints || curveVisualizerSettings.ShowSpline) {
                Vector3[] points = curveData.Position.Select(float3 => (Vector3)float3).ToArray();
                Handles.color = curveVisualizerSettings.SplineColor;
                if (curveVisualizerSettings.ShowSpline) {
                    Handles.DrawAAPolyLine(curveVisualizerSettings.SplineWidth, points);
                    if (curveData.IsClosed) Handles.DrawAAPolyLine(curveVisualizerSettings.SplineWidth, points[0], points[^1]);
                }

                if (curveVisualizerSettings.ShowPoints) {
                    Gizmos.color = curveVisualizerSettings.PointColor;
                    foreach (Vector3 p in points) {
                        Gizmos.DrawSphere(p, curveVisualizerSettings.PointSize);
                    }
                }
            }

            if (curveVisualizerSettings.ShowDirectionVectors) {
                for (int i = 0; i < curveData.Position.Count; i++) {
                    float3 p = curveData.Position[i];
                    float3 t = curveData.Tangent[i];
                    float3 n = curveData.Normal[i];
                    float3 b = curveData.Binormal[i];

                    Handles.color = curveVisualizerSettings.DirectionTangentColor;
                    Handles.DrawAAPolyLine(curveVisualizerSettings.DirectionVectorWidth, p, p + t * curveVisualizerSettings.DirectionVectorLength);
                    Handles.color = curveVisualizerSettings.DirectionNormalColor;
                    Handles.DrawAAPolyLine(curveVisualizerSettings.DirectionVectorWidth, p, p + n * curveVisualizerSettings.DirectionVectorLength);
                    Handles.color = curveVisualizerSettings.DirectionBinormalColor;
                    Handles.DrawAAPolyLine(curveVisualizerSettings.DirectionVectorWidth, p, p + b * curveVisualizerSettings.DirectionVectorLength);
                }
            }
        }

    }
}