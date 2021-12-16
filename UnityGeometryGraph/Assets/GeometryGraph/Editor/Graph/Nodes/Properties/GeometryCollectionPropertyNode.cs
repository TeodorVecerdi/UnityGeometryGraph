using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    public class GeometryCollectionPropertyNode : AbstractNode<GeometryGraph.Runtime.Graph.GeometryCollectionPropertyNode> {
        protected override string Title => property != null ? property.DisplayName : "ERROR";
        protected override NodeCategory Category => NodeCategory.Properties;

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

        public override void CreateNode() {
            propertyPort = GraphFrameworkPort.Create("Collection", Direction.Output, Port.Capacity.Multi, PortType.Collection, this);
            AddPort(propertyPort);
            
            Refresh();
        }

        public override void BindPorts() {
            BindPort(propertyPort, RuntimeNode.Port);
        }

        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();
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
            AbstractProperty prop = Owner.EditorView.GraphObject.GraphData.Properties.FirstOrGivenDefault(abstractProperty => abstractProperty.GUID == propertyGuid, null);
            title = prop?.DisplayName ?? "null property";
            Refresh();
        }
    }
}