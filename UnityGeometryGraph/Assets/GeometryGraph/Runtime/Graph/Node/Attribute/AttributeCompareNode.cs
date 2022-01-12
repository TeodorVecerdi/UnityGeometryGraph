using System;
using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;
using Unity.Burst;
using Unity.Mathematics;
using CompareOperation = GeometryGraph.Runtime.Graph.AttributeCompareNode.AttributeCompareNode_Operation;
using CompareType = GeometryGraph.Runtime.Graph.AttributeCompareNode.AttributeCompareNode_Type;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class AttributeCompareNode {
        [In] public GeometryData Geometry { get; private set; }
        [In] public string AttributeX { get; private set; }
        [In] public string AttributeY { get; private set; }
        [In] public string ResultAttribute { get; private set; }
        [In] public float FloatX { get; private set; }
        [In] public float FloatY { get; private set; }
        [In] public float Tolerance { get; private set; } = 0.01f;
        [Setting] public CompareOperation Operation { get; private set; } = CompareOperation.LessThan;
        [Setting] public CompareType TypeX { get; private set; } = CompareType.Attribute;
        [Setting] public CompareType TypeY { get; private set; } = CompareType.Attribute;
        [Out] public GeometryData Result { get; private set; }

        [GetterMethod(nameof(Result), Inline = true)]
        private GeometryData GetResult() {
            if (Result == null) {
                CalculateResult();
            }
            return Result;
        }

        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Geometry == null) {
                Result = GeometryData.Empty;
                return;
            }

            Result = Geometry.Clone();

            if (string.IsNullOrWhiteSpace(ResultAttribute)) {
                return;
            }

            int count = -1;
            AttributeType attributeType = AttributeType.Invalid;
            AttributeDomain attributeDomain = AttributeDomain.Vertex;
            if (TypeX == CompareType.Attribute && Geometry.GetAttribute(AttributeX) is {} baseAttributeX) {
                attributeType = baseAttributeX.Type;
                attributeDomain = baseAttributeX.Domain;
                count = baseAttributeX.Count;
            } else if (TypeY == CompareType.Attribute && Geometry.GetAttribute(AttributeY) is {} baseAttributeY) {
                attributeType = baseAttributeY.Type;
                attributeDomain = baseAttributeY.Domain;
                count = baseAttributeY.Count;
            }

            if (count == -1) {
                return;
            }

            if (attributeType == AttributeType.Vector3) {
                Vector3Attribute attributeX = TypeX switch {
                    AttributeCompareNode_Type.Attribute => Geometry.GetAttribute<Vector3Attribute>(AttributeX, attributeDomain),
                    AttributeCompareNode_Type.Float => GetValues(FloatXPort, count, FloatX).Into<Vector3Attribute>("tmpX", attributeDomain),
                    _ => throw new ArgumentOutOfRangeException()
                };
                Vector3Attribute attributeY = TypeY switch {
                    AttributeCompareNode_Type.Attribute => Geometry.GetAttribute<Vector3Attribute>(AttributeY, attributeDomain),
                    AttributeCompareNode_Type.Float => GetValues(FloatYPort, count, FloatY).Into<Vector3Attribute>("tmpY", attributeDomain),
                    _ => throw new ArgumentOutOfRangeException()
                };

                List<bool> result = new();
                float sqrTolerance = Tolerance * Tolerance;
                for (int i = 0; i < count; i++) {
                    float3 x = attributeX![i];
                    float3 y = attributeY![i];
                    result.Add(AttributeCompareNode_Burst.CompareVector(ref x, ref y, sqrTolerance, Operation));
                }

                Result.StoreAttribute(result.Into<BoolAttribute>(ResultAttribute, attributeDomain));
            } else {
                FloatAttribute attributeX = TypeX switch {
                    AttributeCompareNode_Type.Attribute => Geometry.GetAttribute<FloatAttribute>(AttributeX, attributeDomain),
                    AttributeCompareNode_Type.Float => GetValues(FloatXPort, count, FloatX).Into<FloatAttribute>("tmpX", attributeDomain),
                    _ => throw new ArgumentOutOfRangeException()
                };
                FloatAttribute attributeY = TypeY switch {
                    AttributeCompareNode_Type.Attribute => Geometry.GetAttribute<FloatAttribute>(AttributeY, attributeDomain),
                    AttributeCompareNode_Type.Float => GetValues(FloatYPort, count, FloatY).Into<FloatAttribute>("tmpY", attributeDomain),
                    _ => throw new ArgumentOutOfRangeException()
                };

                List<bool> result = new();
                for (int i = 0; i < count; i++) {
                    result.Add(AttributeCompareNode_Burst.CompareFloat(attributeX![i], attributeY![i], Tolerance, Operation));
                }

                Result.StoreAttribute(result.Into<BoolAttribute>(ResultAttribute, attributeDomain));
            }
        }

        public enum AttributeCompareNode_Operation {
            [DisplayName("a < b")] LessThan,
            [DisplayName("a ≤ b")] LessThanOrEqual,
            [DisplayName("a > b")] GreaterThan,
            [DisplayName("a ≥ b")] GreaterThanOrEqual,
            [DisplayName("a = b")] Equal,
            [DisplayName("a ≠ b")] NotEqual
        }

        public enum AttributeCompareNode_Type {
            Attribute,
            Float
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    internal static class AttributeCompareNode_Burst {
        [BurstCompile(CompileSynchronously = true)]
        internal static bool CompareFloat(float a, float b, float tolerance, CompareOperation operation) {
            return operation switch {
                AttributeCompareNode.AttributeCompareNode_Operation.LessThan => a < b,
                AttributeCompareNode.AttributeCompareNode_Operation.LessThanOrEqual => a <= b,
                AttributeCompareNode.AttributeCompareNode_Operation.GreaterThan => a > b,
                AttributeCompareNode.AttributeCompareNode_Operation.GreaterThanOrEqual => a >= b,
                AttributeCompareNode.AttributeCompareNode_Operation.Equal => math.abs(a - b) < tolerance,
                AttributeCompareNode.AttributeCompareNode_Operation.NotEqual => math.abs(a - b) >= tolerance,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
            };
        }

        [BurstCompile(CompileSynchronously = true)]
        internal static bool CompareVector(ref float3 a, ref float3 b, float tolerance, CompareOperation operation) {
            return operation switch {
                AttributeCompareNode.AttributeCompareNode_Operation.LessThan => math.lengthsq(a) < math.lengthsq(b),
                AttributeCompareNode.AttributeCompareNode_Operation.LessThanOrEqual => math.lengthsq(a) <= math.lengthsq(b),
                AttributeCompareNode.AttributeCompareNode_Operation.GreaterThan => math.lengthsq(a) > math.lengthsq(b),
                AttributeCompareNode.AttributeCompareNode_Operation.GreaterThanOrEqual => math.lengthsq(a) >= math.lengthsq(b),
                AttributeCompareNode.AttributeCompareNode_Operation.Equal => math.distancesq(a, b) < tolerance,
                AttributeCompareNode.AttributeCompareNode_Operation.NotEqual => math.distancesq(a, b) >= tolerance,
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
            };
        }
    }
}