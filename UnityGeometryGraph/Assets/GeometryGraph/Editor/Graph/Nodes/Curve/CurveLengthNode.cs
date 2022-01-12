using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Curve", "Curve Length")]
    public class CurveLengthNode : AbstractNode<GeometryGraph.Runtime.Graph.CurveLengthNode> {
        protected override string Title => "Curve Length";
        protected override NodeCategory Category => NodeCategory.Curve;

        private GraphFrameworkPort curvePort;
        private GraphFrameworkPort lengthPort;

        protected override void CreateNode() {
            curvePort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            lengthPort = GraphFrameworkPort.Create("Length", Direction.Output, Port.Capacity.Multi, PortType.Float, this);

            AddPort(curvePort);
            AddPort(lengthPort);
        }

        protected override void BindPorts() {
            BindPort(curvePort, RuntimeNode.CurvePort);
            BindPort(lengthPort, RuntimeNode.LengthPort);
        }
    }
}