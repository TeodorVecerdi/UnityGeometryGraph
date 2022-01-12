using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using ComponentType = GeometryGraph.Runtime.Graph.AttributeCombineNode.AttributeCombineNode_ComponentType;

namespace GeometryGraph.Editor {
    [Title("Attribute", "Attribute Combine")]
    public class AttributeCombineNode : AbstractNode<GeometryGraph.Runtime.Graph.AttributeCombineNode> {
        protected override string Title => "Attribute Combine";
        protected override NodeCategory Category => NodeCategory.Attribute;

        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort xFloatPort;
        private GraphFrameworkPort yFloatPort;
        private GraphFrameworkPort zFloatPort;
        private GraphFrameworkPort xAttributePort;
        private GraphFrameworkPort yAttributePort;
        private GraphFrameworkPort zAttributePort;
        private GraphFrameworkPort resultAttributePort;
        private GraphFrameworkPort resultPort;

        private EnumSelectionDropdown<AttributeDomain> domainDropdown;
        private EnumSelectionDropdown<ComponentType> xTypeDropdown;
        private EnumSelectionDropdown<ComponentType> yTypeDropdown;
        private EnumSelectionDropdown<ComponentType> zTypeDropdown;
        private FloatField xFloatField;
        private FloatField yFloatField;
        private FloatField zFloatField;
        private TextField xAttributeField;
        private TextField yAttributeField;
        private TextField zAttributeField;
        private TextField resultAttributeField;

        private float xFloat = 0.0f;
        private float yFloat = 0.0f;
        private float zFloat = 0.0f;
        private string xAttribute;
        private string yAttribute;
        private string zAttribute;
        private string resultAttribute;

        private AttributeDomain domain = AttributeDomain.Vertex;
        private ComponentType xType = ComponentType.Attribute;
        private ComponentType yType = ComponentType.Attribute;
        private ComponentType zType = ComponentType.Attribute;

        private static readonly SelectionTree componentTypeTree = new (new List<object>(Enum.GetValues(typeof(ComponentType)).Convert(o => o))) {
            new SelectionCategory("Type", false) {
                new ("Float", 0, false),
                new ("Attribute", 1, false)
            }
        };

        private static readonly SelectionTree domainTree = new (new List<object>(Enum.GetValues(typeof(AttributeDomain)).Convert(o => o))) {
            new SelectionCategory("Domain", false) {
                new ("Vertex", 0, false),
                new ("Edge", 1, false),
                new ("Face", 2, false),
                new ("FaceCorner", 3, false)
            }
        };

        protected override void CreateNode() {
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (xFloatPort, xFloatField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("X", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateXFloat(xFloat));
            (yFloatPort, yFloatField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Y", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateYFloat(yFloat));
            (zFloatPort, zFloatField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Z", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateZFloat(zFloat));
            (xAttributePort, xAttributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("X", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateXAttribute(xAttribute));
            (yAttributePort, yAttributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Y", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateYAttribute(yAttribute));
            (zAttributePort, zAttributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Z", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateZAttribute(zAttribute));
            (resultAttributePort, resultAttributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Result", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateResultAttribute(resultAttribute));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            domainDropdown = new EnumSelectionDropdown<AttributeDomain>(domain, domainTree, "Domain");
            xTypeDropdown = new EnumSelectionDropdown<ComponentType>(xType, componentTypeTree, "X");
            yTypeDropdown = new EnumSelectionDropdown<ComponentType>(yType, componentTypeTree, "Y");
            zTypeDropdown = new EnumSelectionDropdown<ComponentType>(zType, componentTypeTree, "Z");

            domainDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == domain) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change domain");
                domain = evt.newValue;
                RuntimeNode.UpdateTargetDomain(domain);
            });
            
            xTypeDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == xType) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change X Type");
                xType = evt.newValue;
                RuntimeNode.UpdateXType(xType);
                OnTypeChanged();
            });
            
            yTypeDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == yType) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Y Type");
                yType = evt.newValue;
                RuntimeNode.UpdateYType(yType);
                OnTypeChanged();
            });
            
            zTypeDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == zType) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Z Type");
                zType = evt.newValue;
                RuntimeNode.UpdateZType(zType);
                OnTypeChanged();
            });
            
            xFloatField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - xFloat) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change X float value");
                xFloat = evt.newValue;
                RuntimeNode.UpdateXFloat(xFloat);
            });
            
            yFloatField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - yFloat) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Y float value");
                yFloat = evt.newValue;
                RuntimeNode.UpdateYFloat(yFloat);
            });
            
            zFloatField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - zFloat) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Z float value");
                zFloat = evt.newValue;
                RuntimeNode.UpdateZFloat(zFloat);
            });
            
            xAttributeField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == xAttribute) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change X attribute name");
                xAttribute = evt.newValue;
                RuntimeNode.UpdateXAttribute(xAttribute);
            });
            
            yAttributeField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == yAttribute) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Y attribute name");
                yAttribute = evt.newValue;
                RuntimeNode.UpdateYAttribute(yAttribute);
            });
            
            zAttributeField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == zAttribute) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Z attribute name");
                zAttribute = evt.newValue;
                RuntimeNode.UpdateZAttribute(zAttribute);
            });
            
            resultAttributeField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == resultAttribute) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change result attribute name");
                resultAttribute = evt.newValue;
                RuntimeNode.UpdateResultAttribute(resultAttribute);
            });
            
            domainDropdown.SetValueWithoutNotify(domain);
            xTypeDropdown.SetValueWithoutNotify(xType);
            yTypeDropdown.SetValueWithoutNotify(yType);
            zTypeDropdown.SetValueWithoutNotify(zType);
            
            xFloatPort.Add(xFloatField);
            yFloatPort.Add(yFloatField);
            zFloatPort.Add(zFloatField);
            xAttributePort.Add(xAttributeField);
            yAttributePort.Add(yAttributeField);
            zAttributePort.Add(zAttributeField);
            resultAttributePort.Add(resultAttributeField);
            
            inputContainer.Add(domainDropdown);
            inputContainer.Add(xTypeDropdown);
            inputContainer.Add(yTypeDropdown);
            inputContainer.Add(zTypeDropdown);
            
            AddPort(geometryPort);
            AddPort(xFloatPort);
            AddPort(xAttributePort);
            AddPort(yFloatPort);
            AddPort(yAttributePort);
            AddPort(zFloatPort);
            AddPort(zAttributePort);
            AddPort(resultAttributePort);
            AddPort(resultPort);

            OnTypeChanged();
        }

        private void OnTypeChanged() {
            if (xType is ComponentType.Float) {
                xFloatPort.Show();
                xAttributePort.HideAndDisconnect();
            } else {
                xFloatPort.HideAndDisconnect();
                xAttributePort.Show();
            }
            
            if (yType is ComponentType.Float) {
                yFloatPort.Show();
                yAttributePort.HideAndDisconnect();
            } else {
                yFloatPort.HideAndDisconnect();
                yAttributePort.Show();
            }
            
            if (zType is ComponentType.Float) {
                zFloatPort.Show();
                zAttributePort.HideAndDisconnect();
            } else {
                zFloatPort.HideAndDisconnect();
                zAttributePort.Show();
            }
        }

        protected override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(xFloatPort, RuntimeNode.XFloatPort);
            BindPort(yFloatPort, RuntimeNode.YFloatPort);
            BindPort(zFloatPort, RuntimeNode.ZFloatPort);
            BindPort(xAttributePort, RuntimeNode.XAttributePort);
            BindPort(yAttributePort, RuntimeNode.YAttributePort);
            BindPort(zAttributePort, RuntimeNode.ZAttributePort);
            BindPort(resultAttributePort, RuntimeNode.ResultAttributePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray array = new() {
                xFloat,
                yFloat,
                zFloat,
                xAttribute,
                yAttribute,
                zAttribute,
                resultAttribute,
                (int)xType,
                (int)yType,
                (int)zType
            };
            
            root["d"] = array;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            xFloat = array!.Value<float>(0);
            yFloat = array.Value<float>(1);
            zFloat = array.Value<float>(2);
            xAttribute = array.Value<string>(3);
            yAttribute = array.Value<string>(4);
            zAttribute = array.Value<string>(5);
            resultAttribute = array.Value<string>(6);
            xType = (ComponentType)array.Value<int>(7);
            yType = (ComponentType)array.Value<int>(8);
            zType = (ComponentType)array.Value<int>(9);
            
            xFloatField.SetValueWithoutNotify(xFloat);
            yFloatField.SetValueWithoutNotify(yFloat);
            zFloatField.SetValueWithoutNotify(zFloat);
            xAttributeField.SetValueWithoutNotify(xAttribute);
            yAttributeField.SetValueWithoutNotify(yAttribute);
            zAttributeField.SetValueWithoutNotify(zAttribute);
            resultAttributeField.SetValueWithoutNotify(resultAttribute);
            xTypeDropdown.SetValueWithoutNotify(xType);
            yTypeDropdown.SetValueWithoutNotify(yType);
            zTypeDropdown.SetValueWithoutNotify(zType);
            
            RuntimeNode.UpdateXFloat(xFloat);
            RuntimeNode.UpdateYFloat(yFloat);
            RuntimeNode.UpdateZFloat(zFloat);
            RuntimeNode.UpdateXAttribute(xAttribute);
            RuntimeNode.UpdateYAttribute(yAttribute);
            RuntimeNode.UpdateZAttribute(zAttribute);
            RuntimeNode.UpdateResultAttribute(resultAttribute);
            RuntimeNode.UpdateXType(xType);
            RuntimeNode.UpdateYType(yType);
            RuntimeNode.UpdateZType(zType);
            
            OnTypeChanged();
            
            base.Deserialize(data);
        }
    }
}