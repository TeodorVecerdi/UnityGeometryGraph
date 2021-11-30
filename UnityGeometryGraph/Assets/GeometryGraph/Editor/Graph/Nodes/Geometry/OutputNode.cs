using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Graph Output")]
    public class OutputNode : AbstractNode<GeometryGraph.Runtime.Graph.OutputNode> {
        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort curvePort;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Graph Output", NodeCategory.Geometry);

            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            curvePort = GraphFrameworkPort.Create("Display Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            AddPort(geometryPort);
            AddPort(curvePort);
            Refresh();
        }

        public override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(curvePort, RuntimeNode.CurvePort);
        }
    }
}