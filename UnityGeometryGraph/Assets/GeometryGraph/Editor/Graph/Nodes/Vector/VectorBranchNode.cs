using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeometryGraph.Editor {
    [Title("Vector", "Branch")]
    public class VectorBranchNode : AbstractNode<GeometryGraph.Runtime.Graph.VectorBranchNode> {
        
        private GraphFrameworkPort conditionPort;
        private GraphFrameworkPort ifTruePort;
        private GraphFrameworkPort ifFalsePort;
        private GraphFrameworkPort resultPort;

        private Toggle conditionToggle;
        private Vector3Field ifTrueField;
        private Vector3Field ifFalseField;

        private bool condition;
        private float3 ifTrue;
        private float3 ifFalse;

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Branch (Vector)");

            (conditionPort, conditionToggle) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("Condition", PortType.Vector, this, onDisconnect: (_, _) => RuntimeNode.UpdateCondition(condition));
            (ifTruePort, ifTrueField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("If True", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateIfTrue(ifTrue));
            (ifFalsePort, ifFalseField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("If False", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateIfFalse(ifFalse));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);

            conditionToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue == condition) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change branch node condition");
                condition = evt.newValue;
                RuntimeNode.UpdateCondition(condition);
            });
            
            ifTrueField.RegisterValueChangedCallback(evt => {
                var newValue = (float3)evt.newValue;
                if (math.lengthsq(newValue - ifTrue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change branch node if true value");
                ifTrue = newValue;
                RuntimeNode.UpdateIfTrue(ifTrue);
            });
            
            ifFalseField.RegisterValueChangedCallback(evt => {
                var newValue = (float3)evt.newValue;
                if (math.lengthsq(newValue - ifFalse) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change branch node if false value");
                ifFalse = newValue;
                RuntimeNode.UpdateIfFalse(ifFalse);
            });

            conditionPort.Add(conditionToggle);
            
            AddPort(conditionPort);
            AddPort(ifTruePort);
            inputContainer.Add(ifTrueField);
            AddPort(ifFalsePort);
            inputContainer.Add(ifFalseField);
            AddPort(resultPort);
            
            Refresh();
        }
        
        public override void BindPorts() {
            BindPort(conditionPort, RuntimeNode.ConditionPort);
            BindPort(ifTruePort, RuntimeNode.IfTruePort);
            BindPort(ifFalsePort, RuntimeNode.IfFalsePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();
            var array = new JArray {
                condition ? 1 : 0,
                JsonConvert.SerializeObject(ifTrue, Formatting.None, float3Converter.Converter),
                JsonConvert.SerializeObject(ifFalse, Formatting.None, float3Converter.Converter)
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            var array = jsonData["d"] as JArray;
            
            condition = array!.Value<int>(0) == 1;
            ifTrue = JsonConvert.DeserializeObject<float3>(array.Value<string>(1), float3Converter.Converter);
            ifFalse = JsonConvert.DeserializeObject<float3>(array.Value<string>(2), float3Converter.Converter);
            
            conditionToggle.SetValueWithoutNotify(condition);
            ifTrueField.SetValueWithoutNotify(ifTrue);
            ifFalseField.SetValueWithoutNotify(ifFalse);
            
            RuntimeNode.UpdateCondition(condition);
            RuntimeNode.UpdateIfTrue(ifTrue);
            RuntimeNode.UpdateIfFalse(ifFalse);
            
            base.SetNodeData(jsonData);
        }
    }
}