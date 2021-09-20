using System;

namespace GeometryGraph {
    public class GraphFrameworkWindowEvents {
        public Action SaveRequested;
        public Func<bool> SaveAsRequested;
        public Action ShowInProjectRequested;
    }
}