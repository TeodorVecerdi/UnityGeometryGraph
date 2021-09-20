using System;
using System.Collections.Generic;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class SerializedRuntimeNode {
        public string Guid;
        public string Type;
        public string CustomData;
        public List<RuntimePort> Ports;

        private SerializedRuntimeNode() {
        }

        public static SerializedRuntimeNode FromRuntimeNode(RuntimeNode node) {
            return new SerializedRuntimeNode {
                Guid = node.Guid,
                Type = node.GetType().FullName,
                CustomData = node.GetCustomData(),
                Ports = node.Ports
            };
        }

        public static RuntimeNode FromSerializedNode(SerializedRuntimeNode serializedNode) {
            var inst = (RuntimeNode)Activator.CreateInstance(System.Type.GetType(serializedNode.Type), serializedNode.Guid);
            inst.Ports = serializedNode.Ports;
            inst.SetCustomData(serializedNode.CustomData);
            return inst;
        }
    }
}