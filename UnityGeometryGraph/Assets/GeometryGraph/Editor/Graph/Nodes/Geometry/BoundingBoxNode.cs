using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Geometry", "Bounding Box")]
    public class BoundingBoxNode : AbstractNode<GeometryGraph.Runtime.Graph.BoundingBoxNode> {
        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort minPort;
        private GraphFrameworkPort maxPort;
        private GraphFrameworkPort boundingBoxPort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Bounding Box");

            inputPort = GraphFrameworkPort.Create("Geometry", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Geometry, edgeConnectorListener, this);
            
            minPort = GraphFrameworkPort.Create("Min", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Vector, edgeConnectorListener, this);
            maxPort = GraphFrameworkPort.Create("Max", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Vector, edgeConnectorListener, this);
            boundingBoxPort = GraphFrameworkPort.Create("Box", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Geometry, edgeConnectorListener, this);
            
            AddPort(inputPort);
            AddPort(minPort);
            AddPort(maxPort);
            AddPort(boundingBoxPort);
            
            Refresh();
        }
        
        public override void BindPorts() {
            BindPort(inputPort, RuntimeNode.InputPort);
            BindPort(minPort, RuntimeNode.MinPort);
            BindPort(maxPort, RuntimeNode.MaxPort);
            BindPort(boundingBoxPort, RuntimeNode.ResultPort);
        }
    }
}