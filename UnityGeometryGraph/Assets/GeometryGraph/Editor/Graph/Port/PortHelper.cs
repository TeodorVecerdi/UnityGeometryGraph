using System.Collections.Generic;
using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    public static class PortHelper {
        public static bool IsCompatibleWith(this GraphFrameworkPort port, GraphFrameworkPort other) {
            if (port.node == other.node) return false;
            if (port.direction == Direction.Input && port.Type == PortType.Any ||
                other.direction == Direction.Input && other.Type == PortType.Any) 
                return true;
            return port.Type == other.Type || compatiblePortTypes.Contains((port.Type, other.Type));
        }
        
        // !! Update Runtime.PortValueConverter.Convert when adding stuff to this!
        private static readonly HashSet<(PortType, PortType)> compatiblePortTypes = new(new PortTypeEqualityComparer()) {
            (PortType.Float, PortType.Integer), 
            (PortType.Float, PortType.Boolean), 
            (PortType.Integer, PortType.Float),
            (PortType.Integer, PortType.Boolean)
        };
    }
}