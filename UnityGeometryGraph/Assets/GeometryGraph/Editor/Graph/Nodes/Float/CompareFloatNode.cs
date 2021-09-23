using System.Linq;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Which = GeometryGraph.Runtime.Graph.CompareFloatNode.CompareFloatNode_Which;
using CompareOperation = GeometryGraph.Runtime.Graph.CompareFloatNode.CompareFloatNode_CompareOperation;

namespace GeometryGraph.Editor {
    [Title("Float", "Compare")]
    public class CompareFloatNode : AbstractNode<GeometryGraph.Runtime.Graph.CompareFloatNode> {
        
        private GraphFrameworkPort tolerancePort;
        private GraphFrameworkPort aPort;
        private GraphFrameworkPort bPort;
        private GraphFrameworkPort resultPort;

        private EnumField operationField;
        private FloatField toleranceField;
        private FloatField aField;
        private FloatField bField;
        private Button testButton;

        private CompareOperation operation;
        private float tolerance = 1e-6f;
        private float a;
        private float b;

        public enum TestEnum {
            ValueA,
            ValueB,
            ValueC,
            ValueD,
            ValueE,
            ValueF,
            ValueG,
            ValueH,
            ValueI,
            ValueJ,
            ValueK,
            ValueL,
            ValueM,
            ValueN,
            ValueO,
            ValueP,
            ValueQ,
            ValueR,
            ValueS,
            ValueT,
        }

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Compare", EditorView.DefaultNodePosition);

            operationField = new EnumField(operation);
            (tolerancePort, toleranceField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Tolerance", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this);
            (aPort, aField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("A", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this);
            (bPort, bField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("B", Orientation.Horizontal, PortType.Float, edgeConnectorListener, this);
            resultPort = GraphFrameworkPort.Create("Result", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, PortType.Boolean, edgeConnectorListener, this);

            testButton = new Button { text = "Test" };
            testButton.AddToClassList("enum-dropdown-button");
            var arrow = new VisualElement();
            arrow.AddToClassList("arrow-down");
            testButton.Add(arrow);
            testButton.clicked += () => {
                var pos = GUIUtility.GUIToScreenPoint(testButton.worldBound.position);
                pos.y += testButton.worldBound.height;
                EnumDropdownWindow.ShowWindow<TestEnum>(pos, v => {
                    testButton.text = RandomUtilities.WordBreakString(v.ToString());
                });
            };
            
            toleranceField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change tolerance");
                tolerance = evt.newValue;
                RuntimeNode.UpdateValue(tolerance, Which.Tolerance);
            });

            aField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                a = evt.newValue;
                RuntimeNode.UpdateValue(a, Which.A);
            });
            
            bField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change value");
                b = evt.newValue;
                RuntimeNode.UpdateValue(b, Which.B);
            });
            
            operationField.RegisterValueChangedCallback(evt => {
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change operation");
                operation = (CompareOperation) evt.newValue;
                RuntimeNode.UpdateCompareOperation(operation);
                OnOperationChanged();
            });

            tolerancePort.Add(toleranceField);
            aPort.Add(aField);
            bPort.Add(bField);
            
            inputContainer.Add(testButton);
            inputContainer.Add(operationField);
            AddPort(tolerancePort);
            AddPort(aPort);
            AddPort(bPort);
            AddPort(resultPort);
            
            OnOperationChanged();

            Refresh();
        }

        public override void BindPorts() {
            BindPort(tolerancePort, RuntimeNode.TolerancePort);
            BindPort(aPort, RuntimeNode.APort);
            BindPort(bPort, RuntimeNode.BPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            var root = base.GetNodeData();

            root["o"] = (int)operation;
            root["t"] = tolerance;
            root["a"] = a;
            root["b"] = b;
            
            return root;
        }

        private void OnOperationChanged() {
            var showTolerance = operation == CompareOperation.Equal || operation == CompareOperation.NotEqual;

            if (showTolerance) {
                tolerancePort.RemoveFromClassList("d-none");
            } else {
                tolerancePort.AddToClassList("d-none");
                tolerancePort.connections.Select(edge => (SerializedEdge)edge.userData).ToList().ForEach(edge => Owner.EditorView.GraphView.GraphData.RemoveEdge(edge));
                tolerancePort.DisconnectAll();
            }
        }

        public override void SetNodeData(JObject jsonData) {
            operation = (CompareOperation) jsonData.Value<int>("o");
            tolerance = jsonData.Value<float>("t");
            a = jsonData.Value<float>("a");
            b = jsonData.Value<float>("b");
            
            operationField.SetValueWithoutNotify(operation);
            toleranceField.SetValueWithoutNotify(tolerance);
            aField.SetValueWithoutNotify(a);
            bField.SetValueWithoutNotify(b);
            
            RuntimeNode.UpdateCompareOperation(operation);
            RuntimeNode.UpdateValue(tolerance, Which.Tolerance);
            RuntimeNode.UpdateValue(a, Which.A);
            RuntimeNode.UpdateValue(b, Which.B);

            OnOperationChanged();

            base.SetNodeData(jsonData);
        }
    }
}