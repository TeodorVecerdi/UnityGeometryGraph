using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace GraphFramework.Editor {
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
            var inputNode = editorView.GraphFrameworkGraphView.nodes.First(node => node.viewDataKey == Input) as AbstractNode;
            var outputNode = editorView.GraphFrameworkGraphView.nodes.First(node => node.viewDataKey == Output) as AbstractNode;
            var inputPort = inputNode.Owner.GuidPortDictionary[InputPort];
            var outputPort = outputNode.Owner.GuidPortDictionary[OutputPort];
            Edge = inputPort.ConnectTo(outputPort);
            Edge.userData = this;
        }
    }
}