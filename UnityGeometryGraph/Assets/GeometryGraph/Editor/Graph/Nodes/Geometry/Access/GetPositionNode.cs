using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor.Access {
    [Title("Geometry", "Get Position")]
    public class GetPositionNode : AbstractNode<GeometryGraph.Runtime.Graph.GetPositionNode> {
        protected override string Title => "Get Position";
        protected override NodeCategory Category => NodeCategory.Geometry;

        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort positionPort;

        protected override void CreateNode() {
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            positionPort = GraphFrameworkPort.Create("Position", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            
            AddPort(geometryPort);
            AddPort(positionPort);
        }

        protected override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(positionPort, RuntimeNode.PositionPort);
        }
    }
}