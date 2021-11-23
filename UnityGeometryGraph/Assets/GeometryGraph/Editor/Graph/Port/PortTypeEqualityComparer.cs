using System.Collections.Generic;
using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Editor {
    public class PortTypeEqualityComparer : EqualityComparer<(PortType, PortType)> {
        public override bool Equals((PortType, PortType) x, (PortType, PortType) y) {
            (PortType pAA, PortType pAB) = x;
            (PortType pBA, PortType pBB) = y;
            return pAA == pBA && pAB == pBB;
        }

        public override int GetHashCode((PortType, PortType) pair) {
            (PortType portTypeA, PortType portTypeB) = pair;
            return portTypeA.GetHashCode() * 17 +  portTypeB.GetHashCode();
        }
    }
}