using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;

namespace GeometryGraph.Editor {
    public class EdgeConnectorListener : IEdgeConnectorListener {
        private readonly EditorView editorView;
        private readonly SearchWindowProvider searchWindowProvider;
        
        public EdgeConnectorListener(EditorView editorView, SearchWindowProvider searchWindowProvider) {
            this.editorView = editorView;
            this.searchWindowProvider = searchWindowProvider;
        }
        
        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            Port port = edge.output?.edgeConnector.edgeDragHelper.draggedPort ?? edge.input?.edgeConnector.edgeDragHelper.draggedPort;
            searchWindowProvider.ConnectedPort = port as GraphFrameworkPort;
            searchWindowProvider.RegenerateEntries = true;
            SearcherWindow.Show(editorView.EditorWindow, searchWindowProvider.LoadSearchWindow(), item => searchWindowProvider.OnSelectEntry(item, position), position, null);
            searchWindowProvider.RegenerateEntries = true;
        }

        public void OnDrop(GraphView graphView, Edge edge) {
            if(editorView.GraphObject.GraphData.HasEdge(edge)) return;
            editorView.GraphObject.RegisterCompleteObjectUndo("Connect edge");
            editorView.GraphObject.GraphData.AddEdge(edge);
        }
    }
}