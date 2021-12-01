namespace GeometryGraph.Runtime.Graph {
    public enum PortType {
        Integer,
        Float,
        Vector,
        Boolean,
        Geometry,
        Collection,
        String,
        Curve,
        InstancedGeometry,
        
        Any = int.MaxValue
    }
}