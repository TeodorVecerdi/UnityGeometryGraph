using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor.Access {
    [Title("Geometry", "Get Position")]
    public class GetPositionNode : AbstractNode<GeometryGraph.Runtime.Graph.GetPositionNode> {
        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort positionPort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Get Position", NodeCategory.Geometry);
            
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            positionPort = GraphFrameworkPort.Create("Position", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            
            AddPort(geometryPort);
            AddPort(positionPort);
        }

        public override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(positionPort, RuntimeNode.PositionPort);
        }
    }
}