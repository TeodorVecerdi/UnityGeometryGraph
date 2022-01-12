using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class AttributeSplitNode {
        [In] public GeometryData Geometry { get; private set; }
        [In] public float3 Vector { get; private set; }
        [In] public string Attribute { get; private set; }
        [In] public string XResult { get; private set; }
        [In] public string YResult { get; private set; }
        [In] public string ZResult { get; private set; }
        [Out] public GeometryData Result { get; private set; }

        [Setting] public AttributeDomain TargetDomain { get; private set; } = AttributeDomain.Vertex;
        [Setting] public AttributeSplitNode_InputType InputType { get; private set; } = AttributeSplitNode_InputType.Attribute;

        [GetterMethod(nameof(Result), Inline = true)]
        private GeometryData GetResult() {
            if (Result == null) {
                CalculateResult();
            }
            return Result;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Geometry == null || string.IsNullOrWhiteSpace(Attribute)) {
                Result = GeometryData.Empty;
                return;
            }
            Result = Geometry.Clone();

            int count = TargetDomain switch {
                AttributeDomain.Vertex => Geometry.Vertices.Count,
                AttributeDomain.Edge => Geometry.Edges.Count,
                AttributeDomain.Face => Geometry.Faces.Count,
                AttributeDomain.FaceCorner => Geometry.FaceCorners.Count,
                _ => throw new ArgumentOutOfRangeException()
            };

            // check if at least 2 of the 3 results are not null or whitespace
            int numResults = 0;
            if (!string.IsNullOrWhiteSpace(XResult) is var xHasValue && xHasValue) numResults++;
            if (!string.IsNullOrWhiteSpace(YResult) is var yHasValue && yHasValue) numResults++;
            if (!string.IsNullOrWhiteSpace(ZResult) is var zHasValue && zHasValue) numResults++;

            if (numResults == 0) return;

            if (InputType is AttributeSplitNode_InputType.Vector) {
                IEnumerable<float3> values = GetValues(VectorPort, count, Vector);
                switch (numResults) {
                    case 1: {
                        if (xHasValue) {
                            Result.StoreAttribute(new FloatAttribute(XResult, values.Select(v => v.x)), TargetDomain);
                        } else if (yHasValue) {
                            Result.StoreAttribute(new FloatAttribute(YResult, values.Select(v => v.y)), TargetDomain);
                        } else if (zHasValue) {
                            Result.StoreAttribute(new FloatAttribute(ZResult, values.Select(v => v.z)), TargetDomain);
                        }

                        break;
                    }
                    case > 1: {
                        List<float3> valuesList = values.ToList();
                        if (xHasValue) {
                            Result.StoreAttribute(new FloatAttribute(XResult, valuesList.Select(v => v.x)), TargetDomain);
                        }
                        if (yHasValue) {
                            Result.StoreAttribute(new FloatAttribute(YResult, valuesList.Select(v => v.y)), TargetDomain);
                        }
                        if (zHasValue) {
                            Result.StoreAttribute(new FloatAttribute(ZResult, valuesList.Select(v => v.z)), TargetDomain);
                        }

                        break;
                    }
                }
            } else {
                Vector3Attribute attribute = Geometry.GetAttribute<Vector3Attribute>(Attribute, TargetDomain);
                if (attribute == null) {
                    if (xHasValue) {
                        Result.StoreAttribute(new FloatAttribute(XResult, Enumerable.Repeat(0.0f, count)), TargetDomain);
                    }
                    if (yHasValue) {
                        Result.StoreAttribute(new FloatAttribute(YResult, Enumerable.Repeat(0.0f, count)), TargetDomain);
                    }
                    if (zHasValue) {
                        Result.StoreAttribute(new FloatAttribute(ZResult, Enumerable.Repeat(0.0f, count)), TargetDomain);
                    }
                } else {
                    if (xHasValue) {
                        Result.StoreAttribute(new FloatAttribute(XResult, attribute.Select(v => v.x)), TargetDomain);
                    }
                    if (yHasValue) {
                        Result.StoreAttribute(new FloatAttribute(YResult, attribute.Select(v => v.y)), TargetDomain);
                    }
                    if (zHasValue) {
                        Result.StoreAttribute(new FloatAttribute(ZResult, attribute.Select(v => v.z)), TargetDomain);
                    }
                }
            }
        }

        public enum AttributeSplitNode_InputType {
            Vector,
            Attribute
        }
    }
}