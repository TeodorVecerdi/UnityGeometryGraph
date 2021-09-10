namespace Attribute {
    public enum AttributeDomain {
        Vertex,
        Edge,
        Face,
        FaceCorner,
        Spline, // Unused yet
    }
    
    public enum AttributeType {
        Boolean,
        Integer,
        Float,
        ClampedFloat, // Float, but in range 0-1
        Vector2,
        Vector3,
        
        Invalid = int.MaxValue,
    }
}