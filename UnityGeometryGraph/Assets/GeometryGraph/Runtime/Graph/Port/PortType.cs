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
        Instances,

        Any = int.MaxValue
    }
}