using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    [Title("Curve", "Enumerate Curve")]
    public class EnumerateCurveNode : AbstractNode<GeometryGraph.Runtime.Graph.EnumerateCurveNode> {
        protected override string Title => "Enumerate Curve";
        protected override NodeCategory Category => NodeCategory.Curve;

        private GraphFrameworkPort curvePort;
        private GraphFrameworkPort countPort;
        private GraphFrameworkPort indexPort;
        private GraphFrameworkPort positionPort;
        private GraphFrameworkPort tangentPort;
        private GraphFrameworkPort normalPort;
        private GraphFrameworkPort binormalPort;

        protected override void CreateNode() {
            curvePort = GraphFrameworkPort.Create("Curve", Direction.Input, Port.Capacity.Single, PortType.Curve, this);
            countPort = GraphFrameworkPort.Create("Count", Direction.Output, Port.Capacity.Single, PortType.Integer, this);
            indexPort = GraphFrameworkPort.Create("Index", Direction.Output, Port.Capacity.Multi, PortType.Integer, this);
            positionPort = GraphFrameworkPort.Create("Position", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            tangentPort = GraphFrameworkPort.Create("Tangent", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            normalPort = GraphFrameworkPort.Create("Normal", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            binormalPort = GraphFrameworkPort.Create("Binormal", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);

            AddPort(curvePort);
            AddPort(countPort);
            AddPort(indexPort);
            AddPort(positionPort);
            AddPort(tangentPort);
            AddPort(normalPort);
            AddPort(binormalPort);
        }

        protected override void BindPorts() {
            BindPort(curvePort, RuntimeNode.CurvePort);
            BindPort(countPort, RuntimeNode.CountPort);
            BindPort(indexPort, RuntimeNode.IndexPort);
            BindPort(positionPort, RuntimeNode.PositionPort);
            BindPort(tangentPort, RuntimeNode.TangentPort);
            BindPort(normalPort, RuntimeNode.NormalPort);
            BindPort(binormalPort, RuntimeNode.BinormalPort);
        }
    }
}