using System;
using System.Collections.Generic;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    public abstract class AbstractNode : Node {
        public SerializedNode Owner { get; set; }
        public Dictionary<GraphFrameworkPort, RuntimePort> RuntimePortDictionary;

        private string guid;
        public string Guid {
            get => guid;
            set {
                guid = value;
                OnNodeGuidChanged();
            }
        }

        public readonly List<GraphFrameworkPort> Ports = new();
        internal EdgeConnectorListener EdgeConnectorListener;

        public override bool expanded {
            get => base.expanded;
            set {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Expanded state changed");
                base.expanded = value;
                Owner.DrawState.Expanded = value;
            }
        }

        public void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            EdgeConnectorListener = edgeConnectorListener;
            Initialize();
            CreateNode();
            if (edgeConnectorListener != null) 
                BindPorts();
        }

        public void Refresh() {
            RefreshPorts();
            RefreshExpandedState();
        }

        public void SetExpandedWithoutNotify(bool value) {
            base.expanded = value;
        }

        // Abstract methods/properties
        protected abstract string Title { get; }
        protected abstract NodeCategory Category { get; }
        protected abstract void CreateNode();
        protected abstract void BindPorts();

        // Property specific
        public virtual void OnPropertyUpdated(AbstractProperty property) { }
        public virtual bool IsProperty { get; } = false;
        public virtual string PropertyGuid { get; set; } = string.Empty;
        public virtual AbstractProperty Property { get; set; } = null;

        // Edge callbacks
        protected virtual void OnEdgeConnected(Edge edge, GraphFrameworkPort port) { }
        protected virtual void OnEdgeDisconnected(Edge edge, GraphFrameworkPort port) { }

        // Serialization callbacks
        protected internal virtual void OnNodeSerialized() { }
        protected internal virtual void OnNodeDeserialized() { }

        // Serialization implementation
        protected internal virtual JObject Serialize() => new();
        protected internal virtual void Deserialize(JObject data) { }

        // Implemented and sealed by AbstractNode<TRuntimeNode>
        protected internal abstract void NotifyEdgeConnected(Edge edge, GraphFrameworkPort port);
        protected internal abstract void NotifyEdgeDisconnected(Edge edge, GraphFrameworkPort port);
        protected abstract void OnNodeGuidChanged();
        protected abstract void Initialize();
        protected internal abstract void NotifyRuntimeNodeRemoved();
        protected internal abstract RuntimeNode Runtime { get; }
    }
    
    public abstract class AbstractNode<TRuntimeNode> : AbstractNode where TRuntimeNode : RuntimeNode {
        protected internal sealed override RuntimeNode Runtime => RuntimeNode;
        protected TRuntimeNode RuntimeNode;

        protected sealed override void Initialize() {
            base.title = Title;
            base.SetPosition(EditorView.DefaultNodePosition);

            if (Guid == null) {
                string guid = System.Guid.NewGuid().ToString();
                Guid = guid;
                viewDataKey = guid;
            }

            if (Category != NodeCategory.None) {
                AddToClassList($"category-{Category}");
            }

            if (EdgeConnectorListener != null) {
                RuntimeNode alreadyExisting = Owner.EditorView.GraphObject.RuntimeGraph.RuntimeData.Nodes.Find(node => node.Guid == Guid);
                RuntimeNode = (TRuntimeNode) alreadyExisting ?? (TRuntimeNode)Activator.CreateInstance(typeof(TRuntimeNode), Guid);
            }
            RuntimePortDictionary = new Dictionary<GraphFrameworkPort, RuntimePort>();

            this.AddStyleSheet("Styles/Node/Node");
            InjectCustomStyle();
        }

        protected void BindPort(GraphFrameworkPort graphPort, RuntimePort runtimePort) {
            RuntimePortDictionary[graphPort] = runtimePort;
            runtimePort.Guid = graphPort.GUID;
        }

        protected void AddPort(GraphFrameworkPort port, bool alsoAddToHierarchy = true) {
            Ports.Add(port);

            if(!alsoAddToHierarchy) return;
            bool isInput = port.direction == Direction.Input;
            if (isInput) {
                inputContainer.Add(port);
            } else {
                outputContainer.Add(port);
            }
        }

        protected virtual void InjectCustomStyle() {
            VisualElement border = this.Q("node-border");
            StyleEnum<Overflow> overflowStyle = border.style.overflow;
            overflowStyle.value = Overflow.Visible;
            border.style.overflow = overflowStyle;

            VisualElement selectionBorder = this.Q("selection-border");
            selectionBorder.SendToBack();
        }

        protected internal sealed override void NotifyRuntimeNodeRemoved() {
            RuntimeNode.OnNodeRemoved();
        }

        protected sealed override void OnNodeGuidChanged() {
            if (RuntimeNode != null) RuntimeNode.Guid = Guid;
        }

        // Only input ports
        protected internal sealed override void NotifyEdgeConnected(Edge edge, GraphFrameworkPort port) {
            if (port.direction != Direction.Input) return;
            OnEdgeConnected(edge, port);
        }

        // Both input & output ports
        protected internal sealed override void NotifyEdgeDisconnected(Edge edge, GraphFrameworkPort port) {
            OnEdgeDisconnected(edge, port);
        }
    }
}