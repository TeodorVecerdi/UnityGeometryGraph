namespace GeometryGraph.Runtime {
    public static class Constants {
        // Primitive Geometry constraints
        public const int MAX_ICOSPHERE_SUBDIVISIONS = 5;
        public const int MIN_CIRCULAR_GEOMETRY_POINTS = 3;
        public const int MAX_CIRCULAR_GEOMETRY_POINTS = 128;
        
        // Primitive Curves constraints
        public const int MAX_CURVE_RESOLUTION = 1024;
        public const int MIN_LINE_CURVE_RESOLUTION = 1;
        public const int MIN_CIRCLE_CURVE_RESOLUTION = 3;
        public const int MIN_BEZIER_CURVE_RESOLUTION = 1;
        public const int MIN_HELIX_CURVE_RESOLUTION = 1;

        public const float MIN_CIRCULAR_CURVE_RADIUS = 0.01f;
    }
}