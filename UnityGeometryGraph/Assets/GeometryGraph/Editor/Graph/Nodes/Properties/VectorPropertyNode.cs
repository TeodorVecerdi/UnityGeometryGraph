using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    public class VectorPropertyNode : AbstractNode<GeometryGraph.Runtime.Graph.VectorPropertyNode> {
        public override bool IsProperty => true;
        
        private string propertyGuid;
        private AbstractProperty property;
        private GraphFrameworkPort propertyPort;
        
        public override AbstractProperty Property {
            get => property;
            set {
                property = value;
                title = property.DisplayName;
                Refresh();
            }
        }

        public override string PropertyGuid {
            get => propertyGuid;
            set {
                propertyGuid = value;
                Owner.EditorView.GraphObject.RuntimeGraph.AssignProperty(RuntimeNode, propertyGuid);
                
                if (property == null) return;
                title = property.DisplayName;
                Refresh();
            }
        }

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize(property != null ? property.DisplayName : "ERROR");

            propertyPort = GraphFrameworkPort.Create("Value", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            AddPort(propertyPort);
            
            Refresh();
        }

        public override void BindPorts() {
            BindPort(propertyPort, RuntimeNode.Port);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();
            root["propertyGuid"] = propertyGuid;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            if(jsonData == null) return;
            base.SetNodeData(jsonData);
            PropertyGuid = jsonData.Value<string>("propertyGuid");
            Property = Owner.EditorView.GraphObject.GraphData.Properties.FirstOrGivenDefault(abstractProperty => abstractProperty.GUID == propertyGuid, null);
        }
        

        public override void OnPropertyUpdated(AbstractProperty property) {
            if(property != null && property.GUID != propertyGuid) return;
            var prop = Owner.EditorView.GraphObject.GraphData.Properties.FirstOrGivenDefault(abstractProperty => abstractProperty.GUID == propertyGuid, null);
            title = prop?.DisplayName ?? "null property";
            Refresh();
        }
    }
}