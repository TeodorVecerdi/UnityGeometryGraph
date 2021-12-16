using System;
using System.Collections.Generic;
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
using TargetDomain = GeometryGraph.Runtime.Graph.AttributeFillNode.AttributeFillNode_Domain;
using FillType = GeometryGraph.Runtime.Graph.AttributeFillNode.AttributeFillNode_Type;

namespace GeometryGraph.Editor {
    [Title("Attribute", "Attribute Fill")]
    public class AttributeFillNode : AbstractNode<GeometryGraph.Runtime.Graph.AttributeFillNode> {
        protected override string Title => "Attribute Fill";
        protected override NodeCategory Category => NodeCategory.Attribute;

        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort attributePort;
        private GraphFrameworkPort floatPort;
        private GraphFrameworkPort integerPort;
        private GraphFrameworkPort vectorPort;
        private GraphFrameworkPort booleanPort;
        private GraphFrameworkPort resultPort;
        
        private EnumSelectionDropdown<TargetDomain> domainDropdown;
        private EnumSelectionDropdown<FillType> typeDropdown;
        private TextField attributeField;
        private FloatField floatField;
        private IntegerField integerField;
        private Vector3Field vectorField;
        private Toggle booleanField;
        
        private TargetDomain domain = TargetDomain.Auto;
        private FillType type = FillType.Float;
        private string attribute;
        private float floatValue;
        private int integerValue;
        private float3 vectorValue;
        private bool booleanValue;
        
        private static readonly SelectionTree domainTree = new (new List<object>(Enum.GetValues(typeof(TargetDomain)).Convert(o => o))) {
            new SelectionCategory("Domain", false) {
                new ("Chooses the domain of the attribute if it already exists, otherwise it uses the Vertex domain", 0, true),
                new ("Store the attribute in the Vertex domain", 1, false),
                new ("Store the attribute in the Edge domain", 2, false),
                new ("Store the attribute in the Face domain", 3, false),
                new ("Store the attribute in the Face Corner domain", 4, false)
            }
        };
        
        private static readonly SelectionTree typeTree = new (new List<object>(Enum.GetValues(typeof(FillType)).Convert(o => o))) {
            new SelectionCategory("Type", false) {
                new ("Fills the attribute with a float value", 0, false),
                new ("Fills the attribute with an integer value", 1, false),
                new ("Fills the attribute with a vector value", 2, false),
                new ("Fills the attribute with a boolean value", 3, false)
            }
        };

        protected override void CreateNode() {
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (attributePort, attributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Attribute", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateAttribute(attribute));
            (floatPort, floatField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Value", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateFloat(floatValue));
            (integerPort, integerField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Value", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateInteger(integerValue));
            (vectorPort, vectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Value", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateVector(vectorValue));
            (booleanPort, booleanField) = GraphFrameworkPort.CreateWithBackingField<Toggle, bool>("Value", PortType.Boolean, this, onDisconnect: (_, _) => RuntimeNode.UpdateBoolean(booleanValue));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            domainDropdown = new EnumSelectionDropdown<TargetDomain>(domain, domainTree, "Domain");
            typeDropdown = new EnumSelectionDropdown<FillType>(type, typeTree);
            
            domainDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == domain) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change domain");
                domain = evt.newValue;
                RuntimeNode.UpdateDomain(domain);
            });
            
            typeDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == type) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change type");
                type = evt.newValue;
                RuntimeNode.UpdateType(type);
                OnTypeChanged();
            });
            
            attributeField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == attribute) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change attribute");
                attribute = evt.newValue;
                RuntimeNode.UpdateAttribute(attribute);
            });
            
            floatField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - floatValue) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change float value");
                floatValue = evt.newValue;
                RuntimeNode.UpdateFloat(floatValue);
            });
            
            integerField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == integerValue) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change integer value");
                integerValue = evt.newValue;
                RuntimeNode.UpdateInteger(integerValue);
            });
            
            vectorField.RegisterValueChangedCallback(evt => {
                if (math.distancesq(vectorValue, evt.newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector value");
                vectorValue = evt.newValue;
                RuntimeNode.UpdateVector(vectorValue);
            });
            
            booleanField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == booleanValue) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change boolean value");
                booleanValue = evt.newValue;
                RuntimeNode.UpdateBoolean(booleanValue);
            });
            
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);
            
            attributePort.Add(attributeField);
            floatPort.Add(floatField);
            integerPort.Add(integerField);
            booleanPort.Add(booleanField);
            
            inputContainer.Add(domainDropdown);
            inputContainer.Add(typeDropdown);
            
            AddPort(geometryPort);
            AddPort(attributePort);
            AddPort(floatPort);
            AddPort(integerPort);
            AddPort(vectorPort);
            inputContainer.Add(vectorField);
            AddPort(booleanPort);
            AddPort(resultPort);

            OnTypeChanged();
        }

        private void OnTypeChanged() {
            switch (type) {
                case FillType.Float:
                    floatPort.Show();
                    integerPort.HideAndDisconnect();
                    vectorPort.HideAndDisconnect();
                    booleanPort.HideAndDisconnect();
                    break;
                case FillType.Integer:
                    integerPort.Show();
                    floatPort.HideAndDisconnect();
                    vectorPort.HideAndDisconnect();
                    booleanPort.HideAndDisconnect();
                    break;
                case FillType.Vector:
                    vectorPort.Show();
                    floatPort.HideAndDisconnect();
                    integerPort.HideAndDisconnect();
                    booleanPort.HideAndDisconnect();
                    break;
                case FillType.Boolean:
                    booleanPort.Show();
                    floatPort.HideAndDisconnect();
                    integerPort.HideAndDisconnect();
                    vectorPort.HideAndDisconnect();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        protected override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(attributePort, RuntimeNode.AttributePort);
            BindPort(floatPort, RuntimeNode.FloatPort);
            BindPort(integerPort, RuntimeNode.IntegerPort);
            BindPort(vectorPort, RuntimeNode.VectorPort);
            BindPort(booleanPort, RuntimeNode.BooleanPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject GetNodeData() {
            JObject root = base.GetNodeData();
            JArray array = new() {
                attribute,
                floatValue,
                integerValue,
                JsonConvert.SerializeObject(vectorValue, Formatting.None, float3Converter.Converter),
                booleanValue ? 1 : 0,
                (int) domain,
                (int) type
            };
            
            root["d"] = array;
            return root;
        }

        protected internal override void SetNodeData(JObject jsonData) {
            JArray array = jsonData["d"] as JArray;
            attribute = array!.Value<string>(0);
            floatValue = array.Value<float>(1);
            integerValue = array.Value<int>(2);
            vectorValue = JsonConvert.DeserializeObject<float3>(array.Value<string>(3), float3Converter.Converter);
            booleanValue = array.Value<int>(4) == 1;
            domain = (TargetDomain) array.Value<int>(5);
            type = (FillType) array.Value<int>(6);
            
            attributeField.SetValueWithoutNotify(attribute);
            floatField.SetValueWithoutNotify(floatValue);
            integerField.SetValueWithoutNotify(integerValue);
            vectorField.SetValueWithoutNotify(vectorValue);
            booleanField.SetValueWithoutNotify(booleanValue);
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);
            
            RuntimeNode.UpdateAttribute(attribute);
            RuntimeNode.UpdateFloat(floatValue);
            RuntimeNode.UpdateInteger(integerValue);
            RuntimeNode.UpdateVector(vectorValue);
            RuntimeNode.UpdateBoolean(booleanValue);
            RuntimeNode.UpdateDomain(domain);
            RuntimeNode.UpdateType(type);
            
            OnTypeChanged();
            
            base.SetNodeData(jsonData);
        }
    }
}