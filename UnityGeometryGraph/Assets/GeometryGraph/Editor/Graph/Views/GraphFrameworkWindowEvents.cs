using System;

namespace GeometryGraph.Editor {
    public class GraphFrameworkWindowEvents {
        internal Action SaveRequested;
        internal Func<GraphFrameworkEditorWindow.SaveAsResult> SaveAsRequested;
        internal Action ShowInProjectRequested;
    }
}