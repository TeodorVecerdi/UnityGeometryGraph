namespace GeometryGraph.Runtime.Geometry {
    public readonly struct CylinderUVSettings {
        public readonly int HorizontalRepetitions;
        public readonly int VerticalRepetitions;

        public CylinderUVSettings(int horizontalRepetitions, int verticalRepetitions) {
            HorizontalRepetitions = horizontalRepetitions;
            VerticalRepetitions = verticalRepetitions;
        }
    }
}