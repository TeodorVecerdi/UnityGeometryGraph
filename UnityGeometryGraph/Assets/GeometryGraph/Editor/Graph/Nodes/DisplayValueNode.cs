using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace GeometryGraph.Editor {
    [Title("Display Value")]
    public class DisplayValueNode : AbstractNode<GeometryGraph.Runtime.Graph.DisplayValueNode> {
        private GraphFrameworkPort valuePort;
        private Label valueLabel;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            
            Initialize("Display Value", EditorView.DefaultNodePosition);
            valueLabel = new Label();
            extensionContainer.Add(valueLabel);
            
            valuePort = GraphFrameworkPort.Create("Value", Orientation.Horizontal, Direction.Input, Port.Capacity.Single, PortType.Any, edgeConnectorListener);
            
            AddPort(valuePort);
        }

        protected internal override void OnPortValueChanged(Edge edge, GraphFrameworkPort port) {
            if (port != valuePort) return;
            
            valueLabel.text = GetValueFromEdge(edge, (object)null).ToString();
        }

        /*protected override void OnEdgeConnected(Edge edge, GraphFrameworkPort port) {
            if (valueLabel == null || valuePort == null || edge.output == null) return;
            var value = GetValueFromEdge(edge, (object)null);
            if (value == null) {
                Debug.Log("value is null");
                value = "";
            }
            valueLabel.text = value.ToString();
        }*/

        public override object GetValueForPort(GraphFrameworkPort port) {
            return null;
        }
    }
}