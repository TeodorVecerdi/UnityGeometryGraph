using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Integer", "Branch")]
    public class IntegerBranchNode : AbstractNode<GeometryGraph.Runtime.Graph.IntegerBranchNode> {
        protected override string Title => "Branch (Integer)";
        protected override NodeCategory Category => NodeCategory.Integer;

        private GraphFrameworkPort conditionPort;
        private GraphFrameworkPort ifTruePort;
        private GraphFrameworkPort ifFalsePort;
        private GraphFrameworkPort resultPort;

        private Toggle conditionToggle;
        private IntegerField ifTrueField;
        private IntegerField ifFalseField;

        private bool condition;
        private int ifTrue;
        private int ifFalse;

        protected override void CreateNode() {
            (conditionPort, conditionToggle) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("Condition", PortType.Boolean, this, onDisconnect: (_, _) => RuntimeNode.UpdateCondition(condition));
            (ifTruePort, ifTrueField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("If True", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateIfTrue(ifTrue));
            (ifFalsePort, ifFalseField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("If False", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateIfFalse(ifFalse));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Integer, this);

            conditionToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == condition) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change branch node condition");
                condition = evt.newValue;
                RuntimeNode.UpdateCondition(condition);
            });
            
            ifTrueField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == ifTrue) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change branch node if true value");
                ifTrue = evt.newValue;
                RuntimeNode.UpdateIfTrue(ifTrue);
            });
            
            ifFalseField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == ifFalse) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change branch node if false value");
                ifFalse = evt.newValue;
                RuntimeNode.UpdateIfFalse(ifFalse);
            });
            

            conditionPort.Add(conditionToggle);
            ifTruePort.Add(ifTrueField);
            ifFalsePort.Add(ifFalseField);
            
            AddPort(conditionPort);
            AddPort(ifTruePort);
            AddPort(ifFalsePort);
            AddPort(resultPort);
            
            Refresh();
        }

        protected override void BindPorts() {
            BindPort(conditionPort, RuntimeNode.ConditionPort);
            BindPort(ifTruePort, RuntimeNode.IfTruePort);
            BindPort(ifFalsePort, RuntimeNode.IfFalsePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray array = new() {
                condition ? 1 : 0,
                ifTrue,
                ifFalse,
            };
            root["d"] = array;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;
            
            condition = array!.Value<int>(0) == 1;
            ifTrue = array.Value<int>(1);
            ifFalse = array.Value<int>(2);
            
            conditionToggle.SetValueWithoutNotify(condition);
            ifTrueField.SetValueWithoutNotify(ifTrue);
            ifFalseField.SetValueWithoutNotify(ifFalse);
            
            RuntimeNode.UpdateCondition(condition);
            RuntimeNode.UpdateIfTrue(ifTrue);
            RuntimeNode.UpdateIfFalse(ifFalse);
            
            base.Deserialize(data);
        }
    }
}