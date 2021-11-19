﻿using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode(OutputPath = "_Generated")]
    public partial class RotatePointNode {
        [In] public GeometryData Input { get; private set; }
        [In] public float3 Rotation { get; private set; }
        [In] public string RotationAttribute { get; private set; }
        [In] public float3 Axis { get; private set; }
        [In] public string AxisAttribute { get; private set; }
        [In] public float Angle { get; private set; }
        [In] public string AngleAttribute { get; private set; }
        [Setting] public RotatePointNode_RotationMode RotationMode { get; private set; }
        [Setting] public RotatePointNode_AxisMode AxisMode { get; private set; }
        [Setting] public RotatePointNode_AngleMode AngleMode { get; private set; }
        [Setting] public RotatePointNode_RotationType RotationType { get; private set; } = RotatePointNode_RotationType.Euler;
        [Out] public GeometryData Result { get; private set; }
        
        protected override void OnConnectionRemoved(Connection connection, RuntimePort port) {
            if (port != InputPort) return;
            Input = GeometryData.Empty;
            Result = GeometryData.Empty;
        }

        [CalculatesProperty(nameof(Result))]
        private void Calculate() {
            if (Input == null) return;
            Result = Input.Clone();
            Vector3Attribute rotAttribute = Result.GetAttributeOrDefault<Vector3Attribute, float3>("rotation", AttributeDomain.Vertex, float3.zero);
            if (RotationType == RotatePointNode_RotationType.Euler) {
                Vector3Attribute tmpAttribute = RotationMode == RotatePointNode_RotationMode.Vector 
                    ? Enumerable.Repeat(Rotation, rotAttribute.Count).Into<Vector3Attribute>("tmp", AttributeDomain.Vertex) 
                    : Result.GetAttributeOrDefault<Vector3Attribute, float3>(RotationAttribute, AttributeDomain.Vertex, float3.zero);
                
                rotAttribute.YieldWithAttribute(tmpAttribute, (rot, euler) => math.rotate(quaternion.Euler(euler), rot)).Into(rotAttribute);
            } else {
                Vector3Attribute axisAttribute = AxisMode == RotatePointNode_AxisMode.Vector 
                    ? Enumerable.Repeat(Axis, rotAttribute.Count).Into<Vector3Attribute>("axisAttribute", AttributeDomain.Vertex) 
                    : Result.GetAttributeOrDefault<Vector3Attribute, float3>(AxisAttribute, AttributeDomain.Vertex, float3_ext.up);
                FloatAttribute angleAttribute = AngleMode == RotatePointNode_AngleMode.Float
                    ? Enumerable.Repeat(Angle, rotAttribute.Count).Into<FloatAttribute>("angleAttribute", AttributeDomain.Vertex)
                    : Result.GetAttributeOrDefault<FloatAttribute, float>(AngleAttribute, AttributeDomain.Vertex, 0.0f);
                rotAttribute.YieldWithAttribute(axisAttribute, angleAttribute,
                                                     (rot, axis, angle) => math.rotate(quaternion.AxisAngle(axis, angle), rot))
                                 .Into(rotAttribute);
            }
            Result.StoreAttribute(rotAttribute);
        }

        public enum RotatePointNode_RotationType {AxisAngle = 0, Euler = 1}
        public enum RotatePointNode_RotationMode {Vector = 0, Attribute = 1}
        public enum RotatePointNode_AxisMode {Vector = 0, Attribute = 1}
        public enum RotatePointNode_AngleMode {Float = 0, Attribute = 1}
    }
}