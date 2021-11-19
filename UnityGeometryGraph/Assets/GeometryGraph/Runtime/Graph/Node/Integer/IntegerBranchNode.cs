using System;
using GeometryGraph.Runtime.Attributes;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class IntegerBranchNode : RuntimeNode {
        [In]  public bool Condition { get; private set; }
        [In]  public int IfTrue { get; private set; }
        [In]  public int IfFalse { get; private set; }
        [Out] public int Result { get; private set; }
        
        public override void RebindPorts() {
            throw new InvalidOperationException();
        }
    }
}