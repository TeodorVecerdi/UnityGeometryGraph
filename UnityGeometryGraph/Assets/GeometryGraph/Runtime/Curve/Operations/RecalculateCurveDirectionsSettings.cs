namespace GeometryGraph.Runtime.Curve {
    public readonly struct RecalculateCurveDirectionsSettings {
        public bool FlipTangents { get; }
        public bool FlipNormals { get; }
        public bool FlipBinormals { get; }

        public RecalculateCurveDirectionsSettings(bool flipTangents, bool flipNormals, bool flipBinormals) {
            FlipTangents = flipTangents;
            FlipNormals = flipNormals;
            FlipBinormals = flipBinormals;
        }
    }
}