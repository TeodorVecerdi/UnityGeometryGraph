using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Debug")]
    public class DebugNode : AbstractNode<GeometryGraph.Runtime.Graph.DebugNode> {
        protected override string Title => "Debug";
        protected override NodeCategory Category => NodeCategory.None;

        private GraphFrameworkPort valuePort;
        private Label label;

        protected override void CreateNode() {
            valuePort = GraphFrameworkPort.Create("Value", Direction.Input, Port.Capacity.Single, PortType.Any, this);

            label = new Label("Value: [none]");
            RuntimeNode?.SetOnValueChanged(OnValueChanged);
            
            AddPort(valuePort);
            inputContainer.Add(label);
        }

        private void OnValueChanged(object obj) {
            label.text = $"Value: {obj ?? "[none]"}";
        }

        protected override void BindPorts() {
            BindPort(valuePort, RuntimeNode.Port);
        }
    }
}