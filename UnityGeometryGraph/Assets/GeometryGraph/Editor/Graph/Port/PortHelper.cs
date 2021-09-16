using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphFramework.Editor {
    public static class PortHelper {
        public static bool IsCompatibleWith(this GraphFrameworkPort port, GraphFrameworkPort other) {
            if (port.node == other.node) return false;
            if (port.direction == Direction.Input && port.Type == PortType.Any ||
                other.direction == Direction.Input && other.Type == PortType.Any) 
                return true;
            return port.Type == other.Type;
        }

        public static Color PortColor(GraphFrameworkPort port) {
            return Color.red;
            // Note: Original PortColor implementation left as reference 
            /*switch (port.Type) {
                case PortType.Check:
                    if (port.direction == Direction.Input)
                        return new Color(0.2f, 0.73f, 1f);
                    return new Color(0.5f, 0.98f, 1f);
                case PortType.Trigger:
                    if (port.direction == Direction.Input)
                        return new Color(1f, 0.15f, 0.26f);
                    return new Color(0.84f, 0.26f, 0.16f);
                case PortType.Actor:
                    if (port.direction == Direction.Input)
                        return new Color(0.55f, 1f, 0.3f);
                    return new Color(0.75f, 1f, 0.36f);
                case PortType.Branch:
                    if (port.direction == Direction.Input)
                        return new Color(0.9f, 1f, 0.99f);
                    return new Color(0.91f, 0.93f, 1f);
                case PortType.Combiner:
                    if (port.direction == Direction.Input)
                        return new Color(0.45f, 0.25f, 1f);
                    return new Color(0.45f, 0.25f, 1f);
                case PortType.Fake:
                    return Color.clear;
                default:
                    throw new ArgumentOutOfRangeException(nameof(port), port, "Undefined color for port type.");
            }*/
        }
    }
}