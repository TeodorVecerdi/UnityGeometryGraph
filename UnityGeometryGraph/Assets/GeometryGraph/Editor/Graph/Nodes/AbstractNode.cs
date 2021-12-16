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
        
        public readonly List<GraphFrameworkPort> Ports = new List<GraphFrameworkPort>();
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
            CreateNode();
        }

        public void Refresh() {
            RefreshPorts();
            RefreshExpandedState();
        }

        public void SetExpandedWithoutNotify(bool value) {
            base.expanded = value;
        }

        public abstract void CreateNode();
        public abstract void BindPorts();

        // Property specific
        public virtual void OnPropertyUpdated(AbstractProperty property) {}
        public virtual bool IsProperty { get; } = false;
        public virtual string PropertyGuid { get; set; } = string.Empty;
        public virtual AbstractProperty Property { get; set; } = null;

        // Virtual
        protected internal virtual void OnEdgeConnected(Edge edge, GraphFrameworkPort port) {}
        protected internal virtual void OnEdgeDisconnected(Edge edge, GraphFrameworkPort port) {}
        protected virtual void OnNodeGuidChanged() { }
        public virtual void NotifyEdgeConnected(Edge edge, GraphFrameworkPort port) { }
        public virtual void NotifyEdgeDisconnected(Edge edge, GraphFrameworkPort port) { }
        public virtual void OnNodeSerialized() { }
        public virtual void OnNodeDeserialized() { }
        public virtual void SetNodeData(JObject jsonData) { }
        public virtual JObject GetNodeData() => new JObject();
        
        // Abstract
        public abstract void NotifyRuntimeNodeRemoved();
        public abstract RuntimeNode Runtime { get; }
    }
    
    public abstract class AbstractNode<TRuntimeNode> : AbstractNode where TRuntimeNode : RuntimeNode {
        private static readonly Type runtimeNodeType = typeof(TRuntimeNode);

        public sealed override RuntimeNode Runtime => RuntimeNode;
        protected TRuntimeNode RuntimeNode;

        protected void Initialize(string nodeTitle, NodeCategory category) {
            base.title = nodeTitle;
            base.SetPosition(EditorView.DefaultNodePosition);

            if (Guid == null) {
                string guid = System.Guid.NewGuid().ToString();
                Guid = guid;
                viewDataKey = guid;
            }

            if (category != NodeCategory.None) {
                AddToClassList($"category-{category}");
            }

            if (EdgeConnectorListener != null) {
                RuntimeNode alreadyExisting = Owner.EditorView.GraphObject.RuntimeGraph.RuntimeData.Nodes.Find(node => node.Guid == Guid);
                RuntimeNode = (TRuntimeNode) alreadyExisting ?? (TRuntimeNode)Activator.CreateInstance(runtimeNodeType, Guid);
            }
            RuntimePortDictionary = new Dictionary<GraphFrameworkPort, RuntimePort>();
            
            this.AddStyleSheet("Styles/Node/Node");
            InjectCustomStyle();
        }
        
        public abstract override void BindPorts();

        public override void NotifyRuntimeNodeRemoved() {
            RuntimeNode.OnNodeRemoved();
        }

        protected void BindPort(GraphFrameworkPort graphPort, RuntimePort runtimePort) {
            RuntimePortDictionary[graphPort] = runtimePort;
            runtimePort.Guid = graphPort.GUID;
        }
        
        protected virtual void InjectCustomStyle() {
            VisualElement border = this.Q("node-border");
            StyleEnum<Overflow> overflowStyle = border.style.overflow;
            overflowStyle.value = Overflow.Visible;
            border.style.overflow = overflowStyle;

            VisualElement selectionBorder = this.Q("selection-border");
            selectionBorder.SendToBack();
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

        protected sealed override void OnNodeGuidChanged() {
            if (RuntimeNode != null) RuntimeNode.Guid = Guid;
        }

        // Only input ports
        public sealed override void NotifyEdgeConnected(Edge edge, GraphFrameworkPort port) {
            if (port.direction != Direction.Input) return;
            OnEdgeConnected(edge, port);
        }

        // Both input & output ports
        public sealed override void NotifyEdgeDisconnected(Edge edge, GraphFrameworkPort port) {
            OnEdgeDisconnected(edge, port);
        }
    }
}