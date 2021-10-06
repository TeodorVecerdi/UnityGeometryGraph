using System;
using System.Collections.Generic;
using System.Linq;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class SerializedRuntimeNode {
        public string Guid;
        public string Type;
        public string CustomData;
        public List<string> PortGuids;

        private SerializedRuntimeNode() {
        }

        public static SerializedRuntimeNode FromRuntimeNode(RuntimeNode node) {
            var serializedNode = new SerializedRuntimeNode {
                Guid = node.Guid,
                Type = node.GetType().FullName,
                CustomData = node.GetCustomData(),
                PortGuids = node.Ports.Select(port => port.Guid).ToList()
            };

            return serializedNode;
        }

        public static RuntimeNode FromSerializedNode(SerializedRuntimeNode serializedNode) {
            var inst = (RuntimeNode)Activator.CreateInstance(System.Type.GetType(serializedNode.Type), serializedNode.Guid);
            for (var i = 0; i < inst.Ports.Count; i++) {
                inst.Ports[i].Guid = serializedNode.PortGuids[i];
            }

            return inst;
        }
    }
}