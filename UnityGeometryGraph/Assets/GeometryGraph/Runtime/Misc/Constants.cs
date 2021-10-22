﻿namespace GeometryGraph.Runtime {
    public static class Constants {
        public const float FLOAT_TOLERANCE = 0.00001f;
        
        // Primitive Geometry constraints
        public const int MAX_ICOSPHERE_SUBDIVISIONS = 5;
        public const int MIN_CIRCULAR_GEOMETRY_POINTS = 3;
        public const int MAX_CIRCULAR_GEOMETRY_POINTS = 1024;
        
        public const float MIN_CIRCULAR_GEOMETRY_RADIUS = 0.01f;
        public const float MIN_GEOMETRY_HEIGHT = 0.01f;
        
        // Primitive Curves constraints
        public const int MAX_CURVE_RESOLUTION = 1024;
        public const int MIN_LINE_CURVE_RESOLUTION = 1;
        public const int MIN_CIRCLE_CURVE_RESOLUTION = 3;
        public const int MIN_BEZIER_CURVE_RESOLUTION = 1;
        public const int MIN_HELIX_CURVE_RESOLUTION = 1;

        public const float MIN_CIRCULAR_CURVE_RADIUS = 0.01f;
    }
}