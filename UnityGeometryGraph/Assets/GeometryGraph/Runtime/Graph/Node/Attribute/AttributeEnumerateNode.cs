using System;
using System.Collections.Generic;
using Unity.Mathematics;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Geometry;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class AttributeEnumerateNode {
        [In] public GeometryData Geometry { get; private set; }
        [In] public string Attribute { get; private set; }
        [Setting] public AttributeEnumerateNode_Domain Domain { get; private set; } = AttributeEnumerateNode_Domain.Auto;
        [Setting] public AttributeEnumerateNode_Type Type { get; private set; } = AttributeEnumerateNode_Type.Float;
        [Out] public float Float { get; private set; }
        [Out] public int Integer { get; private set; }
        [Out] public float3 Vector { get; private set; }
        [Out] public bool Boolean { get; private set; }
        [Out] public int Index { get; private set; }
        [Out] public int Count { get; private set; }

        private readonly List<float> floatResults = new();
        private readonly List<int> intResults = new();
        private readonly List<float3> vectorResults = new();
        private readonly List<bool> boolResults = new();
        private readonly List<int> indexResults = new();
        private bool floatDirty = true;
        private bool intDirty = true;
        private bool vectorDirty = true;
        private bool boolDirty = true;
        private bool indexDirty = true;

        [CalculatesProperty(nameof(Float))] private void MarkFloatDirty() => floatDirty = true;
        [CalculatesProperty(nameof(Integer))] private void MarkIntDirty() => intDirty = true;
        [CalculatesProperty(nameof(Vector))] private void MarkVectorDirty() => vectorDirty = true;
        [CalculatesProperty(nameof(Boolean))] private void MarkBoolDirty() => boolDirty = true;
        [CalculatesProperty(nameof(Index))] private void MarkIndexDirty() => indexDirty = true;

        [GetterMethod(nameof(Count))]
        private int GetCount() {
            if (Geometry == null || string.IsNullOrWhiteSpace(Attribute)) return 0;
            if (!Geometry.HasAttribute(Attribute)) {
                return 0;
            }

            AttributeDomain targetDomain = Domain switch {
                AttributeEnumerateNode_Domain.Auto => Geometry.GetAttribute(Attribute)?.Domain ?? AttributeDomain.Vertex,
                AttributeEnumerateNode_Domain.Vertex => AttributeDomain.Vertex,
                AttributeEnumerateNode_Domain.Edge => AttributeDomain.Edge,
                AttributeEnumerateNode_Domain.Face => AttributeDomain.Face,
                AttributeEnumerateNode_Domain.FaceCorner => AttributeDomain.FaceCorner,
                _ => throw new ArgumentOutOfRangeException()
            };

            BaseAttribute attribute = Geometry.GetAttribute(Attribute, targetDomain);
            return attribute?.Count ?? 0;
        }

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (Geometry == null || string.IsNullOrWhiteSpace(Attribute) || count <= 0) yield break;
            AttributeDomain targetDomain = Domain switch {
                AttributeEnumerateNode_Domain.Auto => Geometry.GetAttribute(Attribute)?.Domain ?? AttributeDomain.Vertex,
                AttributeEnumerateNode_Domain.Vertex => AttributeDomain.Vertex,
                AttributeEnumerateNode_Domain.Edge => AttributeDomain.Edge,
                AttributeEnumerateNode_Domain.Face => AttributeDomain.Face,
                AttributeEnumerateNode_Domain.FaceCorner => AttributeDomain.FaceCorner,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (port == IndexPort) {
                if (port == IndexPort && !indexDirty && indexResults.Count == count) {
                    foreach (int indexResult in indexResults) {
                        yield return indexResult;
                    }

                    yield break;
                }
            } else {
                switch (Type) {
                    case AttributeEnumerateNode_Type.Float:
                        if (!floatDirty && floatResults.Count == count) {
                            foreach (float floatResult in floatResults) {
                                yield return floatResult;
                            }

                            yield break;
                        }

                        break;
                    case AttributeEnumerateNode_Type.Integer:
                        if (!intDirty && intResults.Count == count) {
                            foreach (int intResult in intResults) {
                                yield return intResult;
                            }

                            yield break;
                        }

                        break;
                    case AttributeEnumerateNode_Type.Vector:
                        if (!vectorDirty && vectorResults.Count == count) {
                            foreach (float3 vectorResult in vectorResults) {
                                yield return vectorResult;
                            }

                            yield break;
                        }

                        break;
                    case AttributeEnumerateNode_Type.Boolean:
                        if (!boolDirty && boolResults.Count == count) {
                            foreach (bool boolResult in boolResults) {
                                yield return boolResult;
                            }

                            yield break;
                        }

                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            if (!Geometry.HasAttribute(Attribute)) {
                if (port == IndexPort) {
                    indexResults.Clear();
                    indexDirty = false;
                    for (int i = 0; i < count; i++) {
                        indexResults.Add(default);
                        yield return default(int);
                    }
                } else if (port == CountPort) {
                    for (int i = 0; i < count; i++) {
                        yield return 0;
                    }
                } else {
                    switch (Type) {
                        case AttributeEnumerateNode_Type.Float:
                            floatResults.Clear();
                            floatDirty = false;
                            for (int i = 0; i < count; i++) {
                                floatResults.Add(default);
                                yield return default(float);
                            }

                            yield break;
                        case AttributeEnumerateNode_Type.Integer:
                            intResults.Clear();
                            intDirty = false;
                            for (int i = 0; i < count; i++) {
                                intResults.Add(default);
                                yield return default(int);
                            }

                            yield break;
                        case AttributeEnumerateNode_Type.Vector:
                            vectorResults.Clear();
                            vectorDirty = false;
                            for (int i = 0; i < count; i++) {
                                vectorResults.Add(default);
                                yield return default(float3);
                            }

                            yield break;
                        case AttributeEnumerateNode_Type.Boolean:
                            boolResults.Clear();
                            boolDirty = false;
                            for (int i = 0; i < count; i++) {
                                boolResults.Add(default);
                                yield return default(bool);
                            }

                            yield break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            }

            int actualCount = targetDomain switch {
                AttributeDomain.Vertex => Geometry.Vertices.Count,
                AttributeDomain.Edge => Geometry.Edges.Count,
                AttributeDomain.Face => Geometry.Faces.Count,
                AttributeDomain.FaceCorner => Geometry.FaceCorners.Count,
                _ => throw new ArgumentOutOfRangeException()
            };

            int yieldCount = Math.Min(count, actualCount);
            if (port == IndexPort) {
                indexResults.Clear();
                indexDirty = false;
                for (int i = 0; i < yieldCount; i++) {
                    indexResults.Add(i);
                    yield return i;
                }
            } else if (port == CountPort) {
                for (int i = 0; i < yieldCount; i++) {
                    yield return actualCount;
                }
            } else {
                switch (Type) {
                    case AttributeEnumerateNode_Type.Float: {
                        floatResults.Clear();
                        floatDirty = false;
                        FloatAttribute floatAttribute = Geometry.GetAttribute<FloatAttribute>(Attribute, targetDomain);
                        if (floatAttribute == null) {
                            for (int i = 0; i < count; i++) {
                                floatResults.Add(default);
                                yield return default(float);
                            }

                            yield break;
                        }

                        for (int i = 0; i < yieldCount; i++) {
                            float value = floatAttribute[i];
                            floatResults.Add(value);
                            yield return value;
                        }

                        int remainingCount = count - yieldCount;
                        if (remainingCount <= 0) yield break;

                        for (int i = 0; i < remainingCount; i++) {
                            floatResults.Add(default);
                            yield return default(float);
                        }

                        break;
                    }
                    case AttributeEnumerateNode_Type.Integer: {
                        intResults.Clear();
                        intDirty = false;
                        IntAttribute intAttribute = Geometry.GetAttribute<IntAttribute>(Attribute, targetDomain);
                        if (intAttribute == null) {
                            for (int i = 0; i < count; i++) {
                                intResults.Add(default);
                                yield return default(int);
                            }

                            yield break;
                        }

                        for (int i = 0; i < yieldCount; i++) {
                            int value = intAttribute[i];
                            intResults.Add(value);
                            yield return value;
                        }

                        int remainingCount = count - yieldCount;
                        if (remainingCount <= 0) yield break;

                        for (int i = 0; i < remainingCount; i++) {
                            intResults.Add(default);
                            yield return default(int);
                        }

                        break;
                    }
                    case AttributeEnumerateNode_Type.Vector: {
                        vectorResults.Clear();
                        vectorDirty = false;
                        Vector3Attribute vectorAttribute = Geometry.GetAttribute<Vector3Attribute>(Attribute, targetDomain);
                        if (vectorAttribute == null) {
                            for (int i = 0; i < count; i++) {
                                vectorResults.Add(default);
                                yield return default(float3);
                            }

                            yield break;
                        }

                        for (int i = 0; i < yieldCount; i++) {
                            float3 value = vectorAttribute[i];
                            vectorResults.Add(value);
                            yield return value;
                        }

                        int remainingCount = count - yieldCount;
                        if (remainingCount <= 0) yield break;

                        for (int i = 0; i < remainingCount; i++) {
                            vectorResults.Add(default);
                            yield return default(float3);
                        }

                        break;
                    }
                    case AttributeEnumerateNode_Type.Boolean: {
                        boolResults.Clear();
                        boolDirty = false;
                        BoolAttribute boolAttribute = Geometry.GetAttribute<BoolAttribute>(Attribute, targetDomain);
                        if (boolAttribute == null) {
                            for (int i = 0; i < count; i++) {
                                boolResults.Add(default);
                                yield return default(bool);
                            }

                            yield break;
                        }

                        for (int i = 0; i < yieldCount; i++) {
                            bool value = boolAttribute[i];
                            boolResults.Add(value);
                            yield return value;
                        }

                        int remainingCount = count - yieldCount;
                        if (remainingCount <= 0) yield break;

                        for (int i = 0; i < remainingCount; i++) {
                            boolResults.Add(default);
                            yield return default(bool);
                        }

                        break;
                    }
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        public enum AttributeEnumerateNode_Domain {
            Auto,
            Vertex,
            Edge,
            Face,
            FaceCorner,
        }

        public enum AttributeEnumerateNode_Type {
            Float,
            Integer,
            Vector,
            Boolean,
        }
    }
}