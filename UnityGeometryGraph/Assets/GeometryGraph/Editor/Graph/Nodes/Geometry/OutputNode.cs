using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Graph Output")]
    public class OutputNode : AbstractNode<GeometryGraph.Runtime.Graph.OutputNode> {
        protected override string Title => "Graph Output";
        protected override NodeCategory Category => NodeCategory.Geometry;

        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort instancedGeometryPort;
        private GraphFrameworkPort curvePort;

        public override void CreateNode() {
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            instancedGeometryPort = GraphFrameworkPort.Create("Instances", Direction.Input, Port.Capacity.Single, PortType.Instances, this);
            curvePort = GraphFrameworkPort.Create("Display Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            AddPort(geometryPort);
            AddPort(instancedGeometryPort);
            AddPort(curvePort);
            Refresh();
        }

        public override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(curvePort, RuntimeNode.CurvePort);
            BindPort(instancedGeometryPort, RuntimeNode.InstancedGeometryPort);
        }
    }
}