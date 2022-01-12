using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;

namespace GeometryGraph.Editor {
    public class GeometryObjectPropertyNode : AbstractNode<GeometryGraph.Runtime.Graph.GeometryObjectPropertyNode> {
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

        protected override void CreateNode() {
            propertyPort = GraphFrameworkPort.Create("Geometry", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);
            AddPort(propertyPort);
            
            Refresh();
        }

        protected override void BindPorts() {
            BindPort(propertyPort, RuntimeNode.Port);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            root["propertyGuid"] = propertyGuid;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            if(data == null) return;
            base.Deserialize(data);
            PropertyGuid = data.Value<string>("propertyGuid");
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