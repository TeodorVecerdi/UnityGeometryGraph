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
using TargetDomain = GeometryGraph.Runtime.Graph.AttributeRandomizeNode.AttributeRandomizeNode_Domain;
using TargetType = GeometryGraph.Runtime.Graph.AttributeRandomizeNode.AttributeRandomizeNode_Type;

namespace GeometryGraph.Editor {
    [Title("Attribute", "Attribute Randomize")]
    public class AttributeRandomizeNode : AbstractNode<GeometryGraph.Runtime.Graph.AttributeRandomizeNode> {
        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort attributePort;
        private GraphFrameworkPort seedPort;
        private GraphFrameworkPort minIntPort;
        private GraphFrameworkPort maxIntPort;
        private GraphFrameworkPort minFloatPort;
        private GraphFrameworkPort maxFloatPort;
        private GraphFrameworkPort minVectorPort;
        private GraphFrameworkPort maxVectorPort;
        private GraphFrameworkPort resultPort;

        private TextField attributeField;
        private IntegerField seedField;
        private IntegerField minIntField;
        private IntegerField maxIntField;
        private FloatField minFloatField;
        private FloatField maxFloatField;
        private Vector3Field minVectorField;
        private Vector3Field maxVectorField;
        private EnumSelectionDropdown<TargetDomain> domainDropdown;
        private EnumSelectionDropdown<TargetType> typeDropdown;

        private string attribute;
        private int seed;
        private int minInt = 0;
        private int maxInt = 100;
        private float minFloat = 0.0f;
        private float maxFloat = 1.0f;
        private float3 minVector = float3.zero;
        private float3 maxVector = float3_ext.one;
        private TargetDomain domain = TargetDomain.Auto;
        private TargetType type = TargetType.Float;
        
        private static readonly SelectionTree domainTree = new (new List<object>(Enum.GetValues(typeof(TargetDomain)).Convert(o => o))) {
            new SelectionCategory("Domain", false) {
                new ("Chooses the domain of the attribute if it already exists, otherwise it uses the Vertex domain", 0, true),
                new ("Store the attribute in the Vertex domain", 1, false),
                new ("Store the attribute in the Edge domain", 2, false),
                new ("Store the attribute in the Face domain", 3, false),
                new ("Store the attribute in the Face Corner domain", 4, false)
            }
        };
        
        private static readonly SelectionTree typeTree = new (new List<object>(Enum.GetValues(typeof(TargetType)).Convert(o => o))) {
            new SelectionCategory("Type", false) {
                new ("Randomizes the attribute with float values", 0, false),
                new ("Randomizes the attribute with integer values", 1, false),
                new ("Randomizes the attribute with vector values", 2, false),
                new ("Randomizes the attribute with boolean values", 3, false)
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Attribute Randomize", NodeCategory.Attribute);
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (attributePort, attributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Attribute", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateAttribute(attribute));
            (seedPort, seedField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Seed", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateSeed(seed));
            (minIntPort, minIntField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Min", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateMinInt(minInt));
            (maxIntPort, maxIntField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("Max", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateMaxInt(maxInt));
            (minFloatPort, minFloatField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Min", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateMinFloat(minFloat));
            (maxFloatPort, maxFloatField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("Max", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateMaxFloat(maxFloat));
            (minVectorPort, minVectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Min", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateMinVector(minVector));
            (maxVectorPort, maxVectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Max", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateMaxVector(maxVector));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);
            
            domainDropdown = new EnumSelectionDropdown<TargetDomain>(domain, domainTree, "Domain");
            typeDropdown = new EnumSelectionDropdown<TargetType>(type, typeTree);
            
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
            
            seedField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == seed) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change seed");
                seed = evt.newValue;
                RuntimeNode.UpdateSeed(seed);
            });
            
            minIntField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == minInt) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change integer min");
                minInt = evt.newValue;
                RuntimeNode.UpdateMinInt(minInt);
            });
            
            maxIntField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == maxInt) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change integer max");
                maxInt = evt.newValue;
                RuntimeNode.UpdateMaxInt(maxInt);
            });
            
            minFloatField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - minFloat) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change float min");
                minFloat = evt.newValue;
                RuntimeNode.UpdateMinFloat(minFloat);
            });
            
            maxFloatField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - maxFloat) < Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change float max");
                maxFloat = evt.newValue;
                RuntimeNode.UpdateMaxFloat(maxFloat);
            });
            
            minVectorField.RegisterValueChangedCallback(evt => {
                if (math.distancesq(evt.newValue, minVector) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector min");
                minVector = evt.newValue;
                RuntimeNode.UpdateMinVector(minVector);
            });
            
            maxVectorField.RegisterValueChangedCallback(evt => {
                if (math.distancesq(evt.newValue, maxVector) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector max");
                maxVector = evt.newValue;
                RuntimeNode.UpdateMaxVector(maxVector);
            });

            minIntField.SetValueWithoutNotify(minInt);
            maxIntField.SetValueWithoutNotify(maxInt);
            minFloatField.SetValueWithoutNotify(minFloat);
            maxFloatField.SetValueWithoutNotify(maxFloat);
            minVectorField.SetValueWithoutNotify(minVector);
            maxVectorField.SetValueWithoutNotify(maxVector);
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);
            
            attributePort.Add(attributeField);
            seedPort.Add(seedField);
            minIntPort.Add(minIntField);
            maxIntPort.Add(maxIntField);
            minFloatPort.Add(minFloatField);
            maxFloatPort.Add(maxFloatField);
            
            inputContainer.Add(domainDropdown);
            inputContainer.Add(typeDropdown);
            
            AddPort(geometryPort);
            AddPort(attributePort);
            AddPort(seedPort);
            AddPort(minIntPort);
            AddPort(maxIntPort);
            AddPort(minFloatPort);
            AddPort(maxFloatPort);
            AddPort(minVectorPort);
            inputContainer.Add(minVectorField);
            AddPort(maxVectorPort);
            inputContainer.Add(maxVectorField);
            AddPort(resultPort);

            OnTypeChanged();
        }

        private void OnTypeChanged() {
            switch (type) {
                case TargetType.Float:
                    minFloatPort.Show();
                    maxFloatPort.Show();
                    minIntPort.HideAndDisconnect();
                    maxIntPort.HideAndDisconnect();
                    minVectorPort.HideAndDisconnect();
                    maxVectorPort.HideAndDisconnect();
                    break;
                case TargetType.Integer:
                    minFloatPort.HideAndDisconnect();
                    maxFloatPort.HideAndDisconnect();
                    minIntPort.Show();
                    maxIntPort.Show();
                    minVectorPort.HideAndDisconnect();
                    maxVectorPort.HideAndDisconnect();
                    break;
                case TargetType.Vector:
                    minFloatPort.HideAndDisconnect();
                    maxFloatPort.HideAndDisconnect();
                    minIntPort.HideAndDisconnect();
                    maxIntPort.HideAndDisconnect();
                    minVectorPort.Show();
                    maxVectorPort.Show();
                    break;
                case TargetType.Boolean:
                    minFloatPort.HideAndDisconnect();
                    maxFloatPort.HideAndDisconnect();
                    minIntPort.HideAndDisconnect();
                    maxIntPort.HideAndDisconnect();
                    minVectorPort.HideAndDisconnect();
                    maxVectorPort.HideAndDisconnect();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(attributePort, RuntimeNode.AttributePort);
            BindPort(seedPort, RuntimeNode.SeedPort);
            BindPort(minIntPort, RuntimeNode.MinIntPort);
            BindPort(maxIntPort, RuntimeNode.MaxIntPort);
            BindPort(minFloatPort, RuntimeNode.MinFloatPort);
            BindPort(maxFloatPort, RuntimeNode.MaxFloatPort);
            BindPort(minVectorPort, RuntimeNode.MinVectorPort);
            BindPort(maxVectorPort, RuntimeNode.MaxVectorPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();
            JArray array = new() {
                attribute,
                seed,
                minFloat,
                maxFloat,
                minInt,
                maxInt,
                JsonConvert.SerializeObject(minVector, Formatting.None, float3Converter.Converter),
                JsonConvert.SerializeObject(maxVector, Formatting.None, float3Converter.Converter),
                (int) domain,
                (int) type
            };
            
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            JArray array = jsonData["d"] as JArray;
            attribute = array!.Value<string>(0);
            seed = array.Value<int>(1);
            minFloat = array.Value<float>(2);
            maxFloat = array.Value<float>(3);
            minInt = array.Value<int>(4);
            maxInt = array.Value<int>(5);
            minVector = JsonConvert.DeserializeObject<float3>(array.Value<string>(6), float3Converter.Converter);
            maxVector = JsonConvert.DeserializeObject<float3>(array.Value<string>(7), float3Converter.Converter);
            domain = (TargetDomain) array.Value<int>(8);
            type = (TargetType) array.Value<int>(9);
            
            attributeField.SetValueWithoutNotify(attribute);
            seedField.SetValueWithoutNotify(seed);
            minFloatField.SetValueWithoutNotify(minFloat);
            maxFloatField.SetValueWithoutNotify(maxFloat);
            minIntField.SetValueWithoutNotify(minInt);
            maxIntField.SetValueWithoutNotify(maxInt);
            minVectorField.SetValueWithoutNotify(minVector);
            maxVectorField.SetValueWithoutNotify(maxVector);
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);
            
            RuntimeNode.UpdateAttribute(attribute);
            RuntimeNode.UpdateSeed(seed);
            RuntimeNode.UpdateMinFloat(minFloat);
            RuntimeNode.UpdateMaxFloat(maxFloat);
            RuntimeNode.UpdateMinInt(minInt);
            RuntimeNode.UpdateMaxInt(maxInt);
            RuntimeNode.UpdateMinVector(minVector);
            RuntimeNode.UpdateMaxVector(maxVector);
            RuntimeNode.UpdateDomain(domain);
            RuntimeNode.UpdateType(type);
            
            OnTypeChanged();
            
            base.SetNodeData(jsonData);
        }
    }
}