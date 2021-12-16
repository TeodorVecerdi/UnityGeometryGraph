using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Integer", "Map Range")]
    public class MapRangeIntegerNode : AbstractNode<GeometryGraph.Runtime.Graph.MapRangeIntegerNode> {
        protected override string Title => "Map Range (Integer)";
        protected override NodeCategory Category => NodeCategory.Integer;

        private GraphFrameworkPort inputPort;
        private GraphFrameworkPort fromMinPort;
        private GraphFrameworkPort fromMaxPort;
        private GraphFrameworkPort toMinPort;
        private GraphFrameworkPort toMaxPort;
        private GraphFrameworkPort resultPort;

        private Toggle clampField;
        private IntegerField inputField;
        private IntegerField fromMinField;
        private IntegerField toMinField;
        private IntegerField fromMaxField;
        private IntegerField toMaxField;

        private bool clamp;
        private int inputValue;
        private int fromMin = 0;
        private int fromMax = 1;
        private int toMin = 0;
        private int toMax = 1;

        protected override void CreateNode() {
            (inputPort, inputField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Input", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateValue(inputValue));
            (fromMinPort, fromMinField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("From Min", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateFromMin(fromMin));
            (fromMaxPort, fromMaxField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("From Max", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateFromMax(fromMax));
            (toMinPort, toMinField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("To Min", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateToMin(toMin));
            (toMaxPort, toMaxField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("To Max", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateToMax(toMax));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Integer, this);

            clampField = new Toggle("Clamp");
            clampField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change clamp");
                clamp = evt.newValue;
                RuntimeNode.UpdateClamp(clamp);
            });
            
            inputField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                inputValue = evt.newValue;
                RuntimeNode.UpdateValue(inputValue);
            });
            
            fromMinField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                fromMin = evt.newValue;
                RuntimeNode.UpdateFromMin(fromMin);
            });
            fromMaxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                fromMax = evt.newValue;
                RuntimeNode.UpdateFromMax(fromMax);
            });
            toMinField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                toMin = evt.newValue;
                RuntimeNode.UpdateToMin(toMin);
            });
            toMaxField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                toMax = evt.newValue;
                RuntimeNode.UpdateToMax(toMax);
            });
            
            fromMinField.SetValueWithoutNotify(0);
            fromMaxField.SetValueWithoutNotify(1);
            toMinField.SetValueWithoutNotify(0);
            toMaxField.SetValueWithoutNotify(1);

            inputPort.Add(inputField);
            fromMinPort.Add(fromMinField);
            fromMaxPort.Add(fromMaxField);
            toMinPort.Add(toMinField);
            toMaxPort.Add(toMaxField);

            inputContainer.Add(clampField);
            AddPort(inputPort);
            AddPort(fromMinPort);
            AddPort(fromMaxPort);
            AddPort(toMinPort);
            AddPort(toMaxPort);
            AddPort(resultPort);
            
            Refresh();
        }
        
        protected override void BindPorts() {
            BindPort(inputPort, RuntimeNode.ValuePort);
            BindPort(fromMinPort, RuntimeNode.FromMinPort);
            BindPort(fromMaxPort, RuntimeNode.FromMaxPort);
            BindPort(toMinPort, RuntimeNode.ToMinPort);
            BindPort(toMaxPort, RuntimeNode.ToMaxPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();

            root["c"] = clamp ? 1 : 0;
            root["i"] = inputValue;
            root["f"] = fromMin;
            root["F"] = fromMax;
            root["t"] = toMin;
            root["T"] = toMax;
            
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            clamp = data.Value<int>("c") == 1;
            inputValue = data.Value<int>("i");
            fromMin = data.Value<int>("f");
            fromMax = data.Value<int>("F");
            toMin = data.Value<int>("t");
            toMax = data.Value<int>("T");
            
            inputField.SetValueWithoutNotify(inputValue);
            fromMinField.SetValueWithoutNotify(fromMin);
            fromMaxField.SetValueWithoutNotify(fromMax);
            toMinField.SetValueWithoutNotify(toMin);
            toMaxField.SetValueWithoutNotify(toMax);
            
            RuntimeNode.UpdateClamp(clamp);
            RuntimeNode.UpdateValue(inputValue);
            RuntimeNode.UpdateFromMin(fromMin);
            RuntimeNode.UpdateFromMax(fromMax);
            RuntimeNode.UpdateToMin(toMin);
            RuntimeNode.UpdateToMax(toMax);

            base.Deserialize(data);
        }
    }
}