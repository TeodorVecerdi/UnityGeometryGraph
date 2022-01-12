using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Mode = GeometryGraph.Runtime.Graph.GeometryInstanceNode.GeometryInstanceNode_Mode;

namespace GeometryGraph.Editor {
    [Title("Instances", "Geometry Instance")]
    public class GeometryInstanceNode : AbstractNode<GeometryGraph.Runtime.Graph.GeometryInstanceNode> {
        protected override string Title => "Geometry Instance";
        protected override NodeCategory Category => NodeCategory.Instances;

        private GraphFrameworkPort pointsPort;
        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort collectionPort;
        private GraphFrameworkPort collectionSamplingSeedPort;
        private GraphFrameworkPort resultPort;

        private IntegerField collectionSamplingSeedField;
        private EnumSelectionToggle<Mode> modeToggle;

        private int collectionSamplingSeed;
        private Mode mode = Mode.Geometry;

        protected override void CreateNode() {
            pointsPort = GraphFrameworkPort.Create("Points", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            collectionPort = GraphFrameworkPort.Create("Collection", Direction.Input, Port.Capacity.Single, PortType.Collection, this);
            (collectionSamplingSeedPort, collectionSamplingSeedField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Sampling Seed", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateCollectionSamplingSeed(collectionSamplingSeed));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Instances, this);

            modeToggle = new EnumSelectionToggle<Mode>(mode);
            modeToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == mode) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Geometry Instance mode");
                mode = evt.newValue;
                RuntimeNode.UpdateMode(mode);
                OnModeChanged();
            });
            
            collectionSamplingSeedField.RegisterCallback<ChangeEvent<int>>(evt => {
                if (evt.newValue == collectionSamplingSeed) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Geometry Instance collection sampling seed");
                collectionSamplingSeed = evt.newValue;
                RuntimeNode.UpdateCollectionSamplingSeed(collectionSamplingSeed);
            });
            
            collectionSamplingSeedPort.Add(collectionSamplingSeedField);
            
            inputContainer.Add(modeToggle);
            AddPort(pointsPort);
            AddPort(geometryPort);
            AddPort(collectionPort);
            AddPort(collectionSamplingSeedPort);
            AddPort(resultPort);

            OnModeChanged();
        }

        private void OnModeChanged() {
            if (mode == Mode.Geometry) {
                geometryPort.Show();
                collectionPort.HideAndDisconnect();
                collectionSamplingSeedPort.HideAndDisconnect();
            } else {
                geometryPort.HideAndDisconnect();
                collectionPort.Show();
                collectionSamplingSeedPort.Show();
            }
        }

        protected override void BindPorts() {
            BindPort(pointsPort, RuntimeNode.PointsPort);
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(collectionPort, RuntimeNode.CollectionPort);
            BindPort(collectionSamplingSeedPort, RuntimeNode.CollectionSamplingSeedPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root =  base.Serialize();
            JArray data = new() {
                collectionSamplingSeed,
                (int) mode
            };
            root["d"] = data;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray d = data["d"].Value<JArray>();
            collectionSamplingSeed = d.Value<int>(0);
            mode = (Mode) d.Value<int>(1);
            
            collectionSamplingSeedField.SetValueWithoutNotify(collectionSamplingSeed);
            modeToggle.SetValueWithoutNotify(mode);
            
            RuntimeNode.UpdateCollectionSamplingSeed(collectionSamplingSeed);
            RuntimeNode.UpdateMode(mode);
            
            OnModeChanged();
            
            base.Deserialize(data);
        }
    }
}