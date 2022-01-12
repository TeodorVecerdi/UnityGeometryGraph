using System;
using System.Linq;
using GeometryGraph.Runtime.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace GeometryGraph.Editor {
    public class GraphFrameworkPort : Port {
        public string GUID {
            get => viewDataKey;
            set {
                viewDataKey = value;
                if (node == null || !node.RuntimePortDictionary.ContainsKey(this)) return;

                node.RuntimePortDictionary[this].Guid = value;
            }
        }
        public PortType Type { get; private set; }
        public new AbstractNode node { get; }
        public event Action<Edge, GraphFrameworkPort> OnConnect;
        public event Action<Edge, GraphFrameworkPort> OnDisconnect;

        private string label;
        private readonly string originalLabel;
        private bool portVisible;
        private bool fieldVisible;
        private Label fieldLabel;
        private VisualElement field;

        public string Label {
            get => label;
            set {
                label = value;
                if (!fieldVisible) m_ConnectorText.text = label;
                if (fieldLabel != null) fieldLabel.text = label;
            }
        }

        internal string OriginalLabel => originalLabel;
        internal bool FieldVisible => fieldVisible;
        internal bool PortVisible => portVisible;

        private GraphFrameworkPort(string label, AbstractNode ownerNode, Orientation portOrientation, Direction portDirection, Capacity portCapacity) : base(
            portOrientation, portDirection, portCapacity, typeof(object)) {
            node = ownerNode;
            this.label = label;
            originalLabel = label;
            portVisible = true;
        }

        internal void Show() {
            portVisible = true;
            RemoveFromClassList("d-none");
            if (fieldVisible) {
                field?.RemoveFromClassList("d-none");
            }
        }

        internal void HideAndDisconnect() {
            portVisible = false;
            AddToClassList("d-none");
            field?.AddToClassList("d-none");
            connections.Select(edge => (SerializedEdge)edge.userData).ToList().ForEach(edge => node.Owner.EditorView.GraphView.GraphData.RemoveEdge(edge));
            DisconnectAll();
        }

        /// <summary>
        ///   <para>Connect and edge to the port.</para>
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <footer><a href="file:///C:/Program%20Files/Unity/Hub/Editor/2021.1.15f1/Editor/Data/Documentation/en/ScriptReference/Experimental.GraphView.Port.Connect.html">External documentation for `Port.Connect`</a></footer>
        public override void Connect(Edge edge) {
            node.NotifyEdgeConnected(edge, this);
            base.Connect(edge);
            OnConnect?.Invoke(edge, this);
        }

        /// <summary>
        ///   <para>Disconnect edge from port.</para>
        /// </summary>
        /// <param name="edge">The edge to disconnect.</param>
        /// <footer><a href="file:///C:/Program%20Files/Unity/Hub/Editor/2021.1.15f1/Editor/Data/Documentation/en/ScriptReference/Experimental.GraphView.Port.Disconnect.html">External documentation for `Port.Disconnect`</a></footer>
        public override void Disconnect(Edge edge) {
            node.NotifyEdgeDisconnected(edge, this);
            base.Disconnect(edge);
            OnDisconnect?.Invoke(edge, this);
        }

        public static GraphFrameworkPort Create(string name, Direction portDirection, Capacity portCapacity, PortType type, AbstractNode ownerNode, bool hideLabel = false, Action<Edge, GraphFrameworkPort> onConnect = null, Action<Edge, GraphFrameworkPort> onDisconnect = null) {
            GraphFrameworkPort port = new(name, ownerNode, Orientation.Horizontal, portDirection, portCapacity);
            if (ownerNode.EdgeConnectorListener != null) {
                port.m_EdgeConnector = new EdgeConnector<Edge>(ownerNode.EdgeConnectorListener);
                port.AddManipulator(port.m_EdgeConnector);
            }
            
            port.AddStyleSheet("Styles/Node/Port");
            port.AddToClassList($"portType-{type}");

            port.Type = type;
            port.GUID = Guid.NewGuid().ToString();
            port.portName = name;
            
            if(onConnect != null) port.OnConnect += onConnect;
            if(onDisconnect != null) port.OnDisconnect += onDisconnect;

            if (hideLabel) {
                Label label = port.Q<Label>();
                StyleLength fs = label.style.fontSize;
                fs.value = -1;
                label.style.fontSize = fs;
                StyleColor color = label.style.color;
                color.value = Color.clear;
                label.style.color = color;
            }
            
            return port;
        }

        public static (GraphFrameworkPort port, T field) CreateWithBackingField<T, TVal>(string name, PortType type, AbstractNode ownerNode, bool hideLabel = false, bool showLabelOnField = true, Action<Edge, GraphFrameworkPort> onConnect = null, Action<Edge, GraphFrameworkPort> onDisconnect = null) where T : BaseField<TVal>, new() {
            GraphFrameworkPort port = Create(name, Direction.Input, Capacity.Single, type, ownerNode, hideLabel);
            T field = new();
            if (showLabelOnField) field.label = name;
            field.AddToClassList("port-backing-field");
            port.fieldLabel = field.labelElement;
            port.field = field;

            if (showLabelOnField) {
                port.m_ConnectorText.text = string.Empty;
                port.fieldVisible = true;
                port.OnConnect += (_, _) => SetCompFieldVisible(port, false);
                port.OnDisconnect += (_, _) => SetCompFieldVisible(port, true);
            } else {
                port.fieldVisible = true;
                port.OnConnect += (_, _) => SetFieldVisible(port, false);
                port.OnDisconnect += (_, _) => SetFieldVisible(port, true);
            }

            if(onConnect != null) port.OnConnect += onConnect;
            if(onDisconnect != null) port.OnDisconnect += onDisconnect;
            
            field.visible = true;
            field.SetEnabled(true);
            field.RemoveFromClassList("d-none");
            
            return (port, field);
        }

        private static void SetFieldVisible(GraphFrameworkPort port, bool visible) {
            port.field.SetEnabled(visible);
            port.fieldVisible = visible;
            
            if(!port.portVisible) return;

            if (visible) {
                port.field.RemoveFromClassList("d-none");
            } else {
                port.field.AddToClassList("d-none");
            }
        }


        private static void SetCompFieldVisible(GraphFrameworkPort port, bool visible) {
            port.field.SetEnabled(visible);
            port.fieldVisible = visible;

            if(!port.portVisible) return;
            
            if (visible) {
                port.field.RemoveFromClassList("d-none");
                port.m_ConnectorText.text = string.Empty;
            } else {
                port.field.AddToClassList("d-none");
                port.m_ConnectorText.text = port.label;
            }
        }
    }
}
