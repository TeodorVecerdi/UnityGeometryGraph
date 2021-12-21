using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GeometryGraph.Editor {
    [Serializable]
    public class SerializedNode : ISerializationCallbackReceiver {
        public string GUID;
        public string Type;
        public NodeDrawState DrawState;
        public string NodeData;
        public List<string> PortData;
        [NonSerialized] public Dictionary<string, Port> GuidPortDictionary;

        public EditorView EditorView;
        public AbstractNode Node;

        public SerializedNode(Type type, Rect position) {
            Type = type.FullName;
            DrawState.Position = position;
            DrawState.Expanded = true;
            GUID = Guid.NewGuid().ToString();
        }

        public void BuildNode(EditorView editorView, EdgeConnectorListener edgeConnectorListener, bool buildPortData = true) {
            EditorView = editorView;
            Node = (AbstractNode) Activator.CreateInstance(System.Type.GetType(Type));
            Node.Owner = this;
            Node.Guid = GUID;
            Node.viewDataKey = GUID;
            Node.InitializeNode(edgeConnectorListener);
            Node.SetExpandedWithoutNotify(DrawState.Expanded);
            Node.SetPosition(DrawState.Position);
            if (!string.IsNullOrEmpty(NodeData))
                Node.Deserialize(JObject.Parse(NodeData));
            Node.Refresh();

            if (buildPortData)
                BuildPortData();
        }

        public void BuildPortData() {
            if ((Node.Ports == null || Node.Ports.Count == 0) && (PortData == null || PortData.Count == 0)) {
                return;
            }

            if ((PortData == null || PortData.Count == 0) && Node.Ports.Count != 0 || (PortData != null && PortData.Count != Node.Ports.Count && Node.Ports.Count != 0)) {
                // GET
                PortData = new List<string>();
                foreach (GraphFrameworkPort port in Node.Ports) {
                    PortData.Add(port.GUID);
                }
            } else {
                // SET
                if (PortData == null)
                    throw new Exception("Serialized port data somehow ended up as null when it was not supposed to.");
                for (int i = 0; i < PortData.Count; i++) {
                    Node.Ports[i].GUID = PortData[i];
                }
            }

            // Build dictionary
            GuidPortDictionary = new Dictionary<string, Port>();
            foreach (GraphFrameworkPort port in Node.Ports) {
                GuidPortDictionary.Add(port.GUID, port);
            }
        }

        public void OnBeforeSerialize() {
            if (Node == null)
                return;
            Node.OnNodeSerialized();
            NodeData = Node.Serialize().ToString(Formatting.None);
        }

        public void OnAfterDeserialize() {
            if (Node == null)
                return;
            Node.OnNodeDeserialized();
            if (!string.IsNullOrEmpty(NodeData)) {
                try {
                    Node.Deserialize(JObject.Parse(NodeData));
                } catch (Exception e) {
                    // TODO: Change temporary fix for deserialization after a change in the serialization format with a proper way to handle it
                    Debug.LogError($"Error deserializing node data: {e.Message}.\nReverting to default values.");
                    Node.Deserialize(Node.Serialize());
                }
            }
        }
    }
}