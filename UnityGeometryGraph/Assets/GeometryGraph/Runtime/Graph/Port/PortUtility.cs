using System;

namespace GeometryGraph.Runtime.Graph {
    public static class PortTypeUtility {
        public static bool IsUnmanagedType(PortType type) {
            switch (type) {
                case PortType.Integer:
                case PortType.Float:
                case PortType.Vector:
                case PortType.Boolean:
                    return true;
                
                case PortType.Geometry:
                case PortType.Collection:
                case PortType.String:
                case PortType.Curve:
                case PortType.Any:
                    return false;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
    }
}