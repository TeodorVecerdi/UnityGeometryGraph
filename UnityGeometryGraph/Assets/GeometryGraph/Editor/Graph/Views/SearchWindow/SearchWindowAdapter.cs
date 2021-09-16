using UnityEditor.Searcher;

namespace GeometryGraph.Editor {
    public class SearchWindowAdapter : SearcherAdapter {
        public override bool HasDetailsPanel => false;

        public SearchWindowAdapter(string title) : base(title) {
        }
    }

    internal class SearchNodeItem : SearcherItem {
        public SearchWindowProvider.NodeEntry NodeEntry;
        public SearchNodeItem(string name, SearchWindowProvider.NodeEntry nodeEntry) : base(name) {
            NodeEntry = nodeEntry;
        }
    }
}