using Newtonsoft.Json.Linq;

namespace GeometryGraph.Editor {
    public class PropertyNode : AbstractNode {
        private string propertyGuid;
        private string currentType;
        private AbstractProperty property;
        private EdgeConnectorListener edgeConnectorListener;

        public AbstractProperty Property {
            set => property = value;
        }

        public string PropertyGuid {
            get => propertyGuid;
            set {
                if (propertyGuid == value) return;
                propertyGuid = value;
                
                if (property == null) return;
                if(!string.IsNullOrEmpty(currentType)) 
                    RemoveFromClassList(currentType);
                currentType = property.Type.ToString();
                AddToClassList(currentType);
                CreatePorts(property);
            }
        }

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            Initialize("", EditorView.DefaultNodePosition);
            this.edgeConnectorListener = edgeConnectorListener;
            Refresh();
        }

        public override object GetValueForPort(GraphFrameworkPort port) {
            return property.GetValue();
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
        }

        private void CreatePorts(AbstractProperty property) {
            // Note: Original CreatePorts implementation left as reference
            /*Port createdPort;
            switch (property.Type) {
                case PropertyType.Trigger:
                    createdPort = DlogPort.Create("", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, PortType.Trigger, false,edgeConnectorListener); 
                    break;
                case PropertyType.Check:
                    createdPort = DlogPort.Create("", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Check, false, edgeConnectorListener);
                    break;
                case PropertyType.Actor:
                    createdPort = DlogPort.Create("", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Actor, false, edgeConnectorListener);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            AddPort(createdPort, false);
            if(createdPort.direction == Direction.Output)
                titleContainer.Add(createdPort);
            else {
                titleContainer.Insert(0, createdPort);
                titleContainer.AddToClassList("property-port-input");
            }
            */
            Update(property);
            Refresh();
        }

        public void Update(AbstractProperty property) {
            title = property.DisplayName;
            Refresh();
        }
    }
}