using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using Unity.Mathematics;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class EnumerateCurveNode {
        [In(
            DefaultValue = "CurveData.Empty",
            GetValueCode = "{self} = GetValue(connection, {default})",
            UpdateValueCode = ""
        )]
        public CurveData Curve { get; private set; }

        [Out] public int Count { get; private set; }
        [Out, Getter("return 0;")] public int Index { get; private set; }
        [Out, Getter("return float3.zero;")] public float3 Position { get; private set; }
        [Out, Getter("return float3.zero;")] public float3 Tangent { get; private set; }
        [Out, Getter("return float3.zero;")] public float3 Normal { get; private set; }
        [Out, Getter("return float3.zero;")] public float3 Binormal { get; private set; }

        private readonly List<int> indexResults = new();
        private readonly List<float3> positionResults = new();
        private readonly List<float3> tangentResults = new();
        private readonly List<float3> normalResults = new();
        private readonly List<float3> binormalResults = new();
        private bool indexDirty = true;
        private bool positionDirty = true;
        private bool tangentDirty = true;
        private bool normalDirty = true;
        private bool binormalDirty = true;

        [CalculatesProperty(nameof(Index))] private void MarkIndexDirty() => indexDirty = true;
        [CalculatesProperty(nameof(Position))] private void MarkPositionDirty() => positionDirty = true;
        [CalculatesProperty(nameof(Tangent))] private void MarkTangentDirty() => tangentDirty = true;
        [CalculatesProperty(nameof(Normal))] private void MarkNormalDirty() => normalDirty = true;
        [CalculatesProperty(nameof(Binormal))] private void MarkBinormalDirty() => binormalDirty = true;

        [GetterMethod(nameof(Count), Inline = true)]
        private int GetCount() => Curve?.Points ?? 0;

        public override IEnumerable<object> GetValuesForPort(RuntimePort port, int count) {
            if (Curve == null || count <= 0) {
                yield break;
            }

            if (port == IndexPort) {
                if (!indexDirty && indexResults.Count == count) {
                    foreach (int indexResult in indexResults) {
                        yield return indexResult;
                    }
                    yield break;
                }

                indexDirty = false;
                indexResults.Clear();
                indexResults.AddRange(Enumerable.Range(0, Curve.Points));

                int yieldCount = Math.Min(count, indexResults.Count);
                for (int i = 0; i < yieldCount; i++) {
                    yield return indexResults[i];
                }

                if (yieldCount < count) {
                    for(int i = 0; i < count - yieldCount; i++) {
                        yield return 0;
                    }
                }
            } else if (port == PositionPort) {
                if (!positionDirty && positionResults.Count == count) {
                    foreach (float3 positionResult in positionResults) {
                        yield return positionResult;
                    }
                    yield break;
                }

                positionDirty = false;
                positionResults.Clear();
                positionResults.AddRange(Curve.Position);

                int yieldCount = Math.Min(count, positionResults.Count);
                for (int i = 0; i < yieldCount; i++) {
                    yield return positionResults[i];
                }

                if (yieldCount < count) {
                    for (int i = 0; i < count - yieldCount; i++) {
                        yield return float3.zero;
                    }
                }
            } else if (port == TangentPort) {
                if (!tangentDirty && tangentResults.Count == count) {
                    foreach (float3 tangentResult in tangentResults) {
                        yield return tangentResult;
                    }
                    yield break;
                }

                tangentDirty = false;
                tangentResults.Clear();
                tangentResults.AddRange(Curve.Tangent);

                int yieldCount = Math.Min(count, tangentResults.Count);
                for (int i = 0; i < yieldCount; i++) {
                    yield return tangentResults[i];
                }

                if (yieldCount < count) {
                    for (int i = 0; i < count - yieldCount; i++) {
                        yield return float3.zero;
                    }
                }
            } else if (port == NormalPort) {
                if (!normalDirty && normalResults.Count == count) {
                    foreach (float3 normalResult in normalResults) {
                        yield return normalResult;
                    }
                    yield break;
                }

                normalDirty = false;
                normalResults.Clear();
                normalResults.AddRange(Curve.Normal);

                int yieldCount = Math.Min(count, normalResults.Count);
                for (int i = 0; i < yieldCount; i++) {
                    yield return normalResults[i];
                }

                if (yieldCount < count) {
                    for (int i = 0; i < count - yieldCount; i++) {
                        yield return float3.zero;
                    }
                }
            } else if (port == BinormalPort) {
                if (!binormalDirty && binormalResults.Count == count) {
                    foreach (float3 binormalResult in binormalResults) {
                        yield return binormalResult;
                    }
                    yield break;
                }

                binormalDirty = false;
                binormalResults.Clear();
                binormalResults.AddRange(Curve.Binormal);

                int yieldCount = Math.Min(count, binormalResults.Count);
                for (int i = 0; i < yieldCount; i++) {
                    yield return binormalResults[i];
                }

                if (yieldCount < count) {
                    for (int i = 0; i < count - yieldCount; i++) {
                        yield return float3.zero;
                    }
                }
            }
        }
    }
}