using System;
using System.Collections.Generic;
using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Geometry;
using UnityCommons;

namespace GeometryGraph.Runtime.Graph {
    [AdditionalUsingStatements("System.Linq")]
    [GenerateRuntimeNode(OutputPath = "_Generated")]
    public partial class SampleCollectionNode {
        [In(
            UpdatedFromEditorNode = false, 
            DefaultValue = "Enumerable.Empty<GeometryData>()", 
            UpdateValueCode = "{self} = new List<GeometryData>({other})")
        ] public List<GeometryData> Collection { get; private set; } = new ();
        
        [In] public int Index { get; private set; }
        [In] public int Seed { get; private set; }
        [Setting] public SampleCollectionNode_SampleType SampleType { get; private set; }
        [Out] public GeometryData Result { get; private set; }
        
        [CalculatesProperty(nameof(Result))]
        private void CalculateResult() {
            if (Collection == null || Collection.Count == 0) {
                Result = GeometryData.Empty;
                return;
            }
            
            switch (SampleType) {
                case SampleCollectionNode_SampleType.AtIndex: {
                    Result = Collection[Index.mod(Collection.Count)];
                    break;
                }
                case SampleCollectionNode_SampleType.Random: {
                    Rand.PushState(Seed);
                    Result = Rand.ListItem(Collection);
                    Rand.PopState();
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(SampleType), SampleType, null);
            }
        }

        public enum SampleCollectionNode_SampleType {AtIndex = 0, Random = 1}
    }
}