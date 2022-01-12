using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace GeometryGraph.Editor {
    [Serializable]
    public class SerializedEdge {
        public string Input;
        public string Output;
        public string InputPort;
        public string OutputPort;
        public Port.Capacity InputCapacity;
        public Port.Capacity OutputCapacity;

        public Edge Edge;
        public EditorView EditorView;

        public void BuildEdge(EditorView editorView) {
            EditorView = editorView;
            AbstractNode inputNode = editorView.GraphView.nodes.First(node => node.viewDataKey == Input) as AbstractNode;
            AbstractNode outputNode = editorView.GraphView.nodes.First(node => node.viewDataKey == Output) as AbstractNode;
            try {
                Port inputPort = inputNode.Owner.GuidPortDictionary[InputPort];
                Port outputPort = outputNode.Owner.GuidPortDictionary[OutputPort];
                Edge = inputPort.ConnectTo(outputPort);
            } catch (Exception e) {
                Debug.LogException(e);
                return;
            }
            Edge.userData = this;
        }
    }
}