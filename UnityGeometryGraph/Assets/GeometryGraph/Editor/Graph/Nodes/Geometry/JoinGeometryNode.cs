using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Join Geometry")]
    public class JoinGeometryNode : AbstractNode<GeometryGraph.Runtime.Graph.JoinGeometryNode> {
        protected override string Title => "Join Geometry";
        protected override NodeCategory Category => NodeCategory.Geometry;

        private GraphFrameworkPort aPort;
        private GraphFrameworkPort resultPort;

        public override void CreateNode() {
            aPort = GraphFrameworkPort.Create("Values", Direction.Input, Port.Capacity.Multi, PortType.Geometry, this);
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);
            
            AddPort(aPort);
            AddPort(resultPort);
            
            Refresh();
        }
        
        public override void BindPorts() {
            BindPort(aPort, RuntimeNode.InputPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }
    }
}