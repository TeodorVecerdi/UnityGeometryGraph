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
using TargetDomain = GeometryGraph.Runtime.Graph.AttributeMapRangeNode.AttributeMapRangeNode_Domain;
using TargetType = GeometryGraph.Runtime.Graph.AttributeMapRangeNode.AttributeMapRangeNode_Type;

namespace GeometryGraph.Editor {
    [Title("Attribute", "Attribute Map Range")]
    public class AttributeMapRangeNode : AbstractNode<GeometryGraph.Runtime.Graph.AttributeMapRangeNode> {
        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort attributePort;
        private GraphFrameworkPort resultAttributePort;
        private GraphFrameworkPort fromMinIntPort;
        private GraphFrameworkPort fromMaxIntPort;
        private GraphFrameworkPort toMinIntPort;
        private GraphFrameworkPort toMaxIntPort;
        private GraphFrameworkPort fromMinFloatPort;
        private GraphFrameworkPort fromMaxFloatPort;
        private GraphFrameworkPort toMinFloatPort;
        private GraphFrameworkPort toMaxFloatPort;
        private GraphFrameworkPort fromMinVectorPort;
        private GraphFrameworkPort fromMaxVectorPort;
        private GraphFrameworkPort toMinVectorPort;
        private GraphFrameworkPort toMaxVectorPort;
        private GraphFrameworkPort resultPort;

        private TextField attributeField;
        private TextField resultAttributeField;
        private IntegerField fromMinIntField;
        private IntegerField fromMaxIntField;
        private IntegerField toMinIntField;
        private IntegerField toMaxIntField;
        private FloatField fromMinFloatField;
        private FloatField fromMaxFloatField;
        private FloatField toMinFloatField;
        private FloatField toMaxFloatField;
        private Vector3Field fromMinVectorField;
        private Vector3Field fromMaxVectorField;
        private Vector3Field toMinVectorField;
        private Vector3Field toMaxVectorField;
        private EnumSelectionDropdown<TargetDomain> domainDropdown;
        private EnumSelectionDropdown<TargetType> typeDropdown;

        private string attribute;
        private string resultAttribute;
        private int fromMinInt = 0;
        private int fromMaxInt = 100;
        private int toMinInt = 0;
        private int toMaxInt = 100;
        private float fromMinFloat = 0.0f;
        private float fromMaxFloat = 1.0f;
        private float toMinFloat = 0.0f;
        private float toMaxFloat = 1.0f;
        private float3 fromMinVector = float3.zero;
        private float3 fromMaxVector = float3_ext.one;
        private float3 toMinVector = float3.zero;
        private float3 toMaxVector = float3_ext.one;
        private TargetDomain domain = TargetDomain.Auto;
        private TargetType type = TargetType.Auto;

        private static readonly SelectionTree domainTree = new(new List<object>(Enum.GetValues(typeof(TargetDomain)).Convert(o => o))) {
            new SelectionCategory("Domain", false) {
                new("Uses the domain of the original attribute", 0, true),
                new("Uses the Vertex domain (performing domain conversion if necessary)", 1, false),
                new("Uses the Edge domain (performing domain conversion if necessary)", 2, false),
                new("Uses the Face domain (performing domain conversion if necessary)", 3, false),
                new("Uses the Face Corner domain (performing domain conversion if necessary)", 4, false)
            }
        };

        private static readonly SelectionTree typeTree = new(new List<object>(Enum.GetValues(typeof(TargetType)).Convert(o => o))) {
            new SelectionCategory("Type", false) {
                new("Uses the type of the original attribute", 0, true),
                new("Uses a float attribute (performing type conversion if necessary)", 1, false),
                new("Uses an integer attribute (performing type conversion if necessary)", 2, false),
                new("Uses a vector attribute (performing type conversion if necessary)", 3, false),
            }
        };

        public override void InitializeNode(EdgeConnectorListener edgeConnectorListener) {
            base.InitializeNode(edgeConnectorListener);
            Initialize("Attribute Map Range", NodeCategory.Attribute);

            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (attributePort, attributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Attribute", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateAttribute(attribute));
            (resultAttributePort, resultAttributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Result", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateResultAttribute(resultAttribute));
            (fromMinIntPort, fromMinIntField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("From Min", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateFromMinInt(fromMinInt));
            (fromMaxIntPort, fromMaxIntField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("From Max", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateFromMaxInt(fromMaxInt));
            (toMinIntPort, toMinIntField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("To Min", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateToMinInt(toMinInt));
            (toMaxIntPort, toMaxIntField) = GraphFrameworkPort.CreateWithBackingField<IntegerField, int>("To Max", PortType.Integer, this, onDisconnect: (_, _) => RuntimeNode.UpdateToMaxInt(toMaxInt));
            (fromMinFloatPort, fromMinFloatField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("From Min", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateFromMinFloat(fromMinFloat));
            (fromMaxFloatPort, fromMaxFloatField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("From Max", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateFromMaxFloat(fromMaxFloat));
            (toMinFloatPort, toMinFloatField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("To Min", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateToMinFloat(toMinFloat));
            (toMaxFloatPort, toMaxFloatField) = GraphFrameworkPort.CreateWithBackingField<FloatField, float>("To Max", PortType.Float, this, onDisconnect: (_, _) => RuntimeNode.UpdateToMaxFloat(toMaxFloat));
            (fromMinVectorPort, fromMinVectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("From Min", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateFromMinVector(fromMinVector));
            (fromMaxVectorPort, fromMaxVectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("From Max", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateFromMaxVector(fromMaxVector));
            (toMinVectorPort, toMinVectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("To Min", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateToMinVector(toMinVector));
            (toMaxVectorPort, toMaxVectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("To Max", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateToMaxVector(toMaxVector));
            resultPort = GraphFrameworkPort.Create("Geometry", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);
            
            fromMinVectorField.AddClasses("inline", "inline-small");
            fromMinVectorPort.AddToClassList("inline-backing-field");
            fromMaxVectorField.AddClasses("inline", "inline-small");
            fromMaxVectorPort.AddToClassList("inline-backing-field");
            toMinVectorField.AddClasses("inline", "inline-small");
            toMinVectorPort.AddToClassList("inline-backing-field");
            toMaxVectorField.AddClasses("inline", "inline-small");
            toMaxVectorPort.AddToClassList("inline-backing-field");

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

            resultAttributeField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == resultAttribute) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change result attribute");
                resultAttribute = evt.newValue;
                RuntimeNode.UpdateResultAttribute(resultAttribute);
            });

            fromMinIntField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == fromMinInt) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change from min value");
                fromMinInt = evt.newValue;
                RuntimeNode.UpdateFromMinInt(fromMinInt);
            });

            fromMaxIntField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == fromMaxInt) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change from max value");
                fromMaxInt = evt.newValue;
                RuntimeNode.UpdateFromMaxInt(fromMaxInt);
            });

            toMinIntField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == toMinInt) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change to min value");
                toMinInt = evt.newValue;
                RuntimeNode.UpdateToMinInt(toMinInt);
            });

            toMaxIntField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == toMaxInt) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change to max value");
                toMaxInt = evt.newValue;
                RuntimeNode.UpdateToMaxInt(toMaxInt);
            });

            fromMinFloatField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - fromMinFloat) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change from min value");
                fromMinFloat = evt.newValue;
                RuntimeNode.UpdateFromMinFloat(fromMinFloat);
            });

            fromMaxFloatField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - fromMaxFloat) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change from max value");
                fromMaxFloat = evt.newValue;
                RuntimeNode.UpdateFromMaxFloat(fromMaxFloat);
            });

            toMinFloatField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - toMinFloat) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change to min value");
                toMinFloat = evt.newValue;
                RuntimeNode.UpdateToMinFloat(toMinFloat);
            });

            toMaxFloatField.RegisterValueChangedCallback(evt => {
                if (Math.Abs(evt.newValue - toMaxFloat) < Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change to max value");
                toMaxFloat = evt.newValue;
                RuntimeNode.UpdateToMaxFloat(toMaxFloat);
            });

            fromMinVectorField.RegisterValueChangedCallback(evt => {
                if (math.distancesq(evt.newValue, fromMinVector) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change from min value");
                fromMinVector = evt.newValue;
                RuntimeNode.UpdateFromMinVector(fromMinVector);
            });

            fromMaxVectorField.RegisterValueChangedCallback(evt => {
                if (math.distancesq(evt.newValue, fromMaxVector) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change from max value");
                fromMaxVector = evt.newValue;
                RuntimeNode.UpdateFromMaxVector(fromMaxVector);
            });

            toMinVectorField.RegisterValueChangedCallback(evt => {
                if (math.distancesq(evt.newValue, toMinVector) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change to min value");
                toMinVector = evt.newValue;
                RuntimeNode.UpdateToMinVector(toMinVector);
            });

            toMaxVectorField.RegisterValueChangedCallback(evt => {
                if (math.distancesq(evt.newValue, toMaxVector) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change to max value");
                toMaxVector = evt.newValue;
                RuntimeNode.UpdateToMaxVector(toMaxVector);
            });

            fromMinIntField.SetValueWithoutNotify(fromMinInt);
            fromMaxIntField.SetValueWithoutNotify(fromMaxInt);
            toMinIntField.SetValueWithoutNotify(toMinInt);
            toMaxIntField.SetValueWithoutNotify(toMaxInt);
            fromMinFloatField.SetValueWithoutNotify(fromMinFloat);
            fromMaxFloatField.SetValueWithoutNotify(fromMaxFloat);
            toMinFloatField.SetValueWithoutNotify(toMinFloat);
            toMaxFloatField.SetValueWithoutNotify(toMaxFloat);
            fromMinVectorField.SetValueWithoutNotify(fromMinVector);
            fromMaxVectorField.SetValueWithoutNotify(fromMaxVector);
            toMinVectorField.SetValueWithoutNotify(toMinVector);
            toMaxVectorField.SetValueWithoutNotify(toMaxVector);
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);

            attributePort.Add(attributeField);
            resultAttributePort.Add(resultAttributeField);
            fromMinIntPort.Add(fromMinIntField);
            fromMaxIntPort.Add(fromMaxIntField);
            toMinIntPort.Add(toMinIntField);
            toMaxIntPort.Add(toMaxIntField);
            fromMinFloatPort.Add(fromMinFloatField);
            fromMaxFloatPort.Add(fromMaxFloatField);
            toMinFloatPort.Add(toMinFloatField);
            toMaxFloatPort.Add(toMaxFloatField);

            inputContainer.Add(domainDropdown);
            inputContainer.Add(typeDropdown);
            AddPort(geometryPort);
            AddPort(attributePort);
            AddPort(resultAttributePort);
            AddPort(fromMinIntPort);
            AddPort(fromMaxIntPort);
            AddPort(toMinIntPort);
            AddPort(toMaxIntPort);
            AddPort(fromMinFloatPort);
            AddPort(fromMaxFloatPort);
            AddPort(toMinFloatPort);
            AddPort(toMaxFloatPort);
            AddPort(fromMinVectorPort);
            inputContainer.Add(fromMinVectorField);
            AddPort(fromMaxVectorPort);
            inputContainer.Add(fromMaxVectorField);
            AddPort(toMinVectorPort);
            inputContainer.Add(toMinVectorField);
            AddPort(toMaxVectorPort);
            inputContainer.Add(toMaxVectorField);
            AddPort(resultPort);

            OnTypeChanged();
        }

        private void OnTypeChanged() {
            switch (type) {
                case TargetType.Auto:
                case TargetType.Float:
                    fromMinFloatPort.Show();
                    toMinFloatPort.Show();
                    fromMaxFloatPort.Show();
                    toMaxFloatPort.Show();
                    fromMinIntPort.HideAndDisconnect();
                    toMinIntPort.HideAndDisconnect();
                    fromMaxIntPort.HideAndDisconnect();
                    toMaxIntPort.HideAndDisconnect();
                    fromMinVectorPort.HideAndDisconnect();
                    toMinVectorPort.HideAndDisconnect();
                    fromMaxVectorPort.HideAndDisconnect();
                    toMaxVectorPort.HideAndDisconnect();
                    break;
                case TargetType.Integer:
                    fromMinFloatPort.HideAndDisconnect();
                    toMinFloatPort.HideAndDisconnect();
                    fromMaxFloatPort.HideAndDisconnect();
                    toMaxFloatPort.HideAndDisconnect();
                    fromMinIntPort.Show();
                    toMinIntPort.Show();
                    fromMaxIntPort.Show();
                    toMaxIntPort.Show();
                    fromMinVectorPort.HideAndDisconnect();
                    toMinVectorPort.HideAndDisconnect();
                    fromMaxVectorPort.HideAndDisconnect();
                    toMaxVectorPort.HideAndDisconnect();
                    break;
                case TargetType.Vector:
                    fromMinFloatPort.HideAndDisconnect();
                    toMinFloatPort.HideAndDisconnect();
                    fromMaxFloatPort.HideAndDisconnect();
                    toMaxFloatPort.HideAndDisconnect();
                    fromMinIntPort.HideAndDisconnect();
                    toMinIntPort.HideAndDisconnect();
                    fromMaxIntPort.HideAndDisconnect();
                    toMaxIntPort.HideAndDisconnect();
                    fromMinVectorPort.Show();
                    toMinVectorPort.Show();
                    fromMaxVectorPort.Show();
                    toMaxVectorPort.Show();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(attributePort, RuntimeNode.AttributePort);
            BindPort(resultAttributePort, RuntimeNode.ResultAttributePort);
            BindPort(fromMinIntPort, RuntimeNode.FromMinIntPort);
            BindPort(fromMaxIntPort, RuntimeNode.FromMaxIntPort);
            BindPort(toMinIntPort, RuntimeNode.ToMinIntPort);
            BindPort(toMaxIntPort, RuntimeNode.ToMaxIntPort);
            BindPort(fromMinFloatPort, RuntimeNode.FromMinFloatPort);
            BindPort(fromMaxFloatPort, RuntimeNode.FromMaxFloatPort);
            BindPort(toMinFloatPort, RuntimeNode.ToMinFloatPort);
            BindPort(toMaxFloatPort, RuntimeNode.ToMaxFloatPort);
            BindPort(fromMinVectorPort, RuntimeNode.FromMinVectorPort);
            BindPort(fromMaxVectorPort, RuntimeNode.FromMaxVectorPort);
            BindPort(toMinVectorPort, RuntimeNode.ToMinVectorPort);
            BindPort(toMaxVectorPort, RuntimeNode.ToMaxVectorPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();
            JArray array = new() {
                attribute,
                resultAttribute,
                fromMinInt,
                fromMaxInt,
                toMinInt,
                toMaxInt,
                fromMinFloat,
                fromMaxFloat,
                toMinFloat,
                toMaxFloat,
                JsonConvert.SerializeObject(fromMinVector, Formatting.None, float3Converter.Converter),
                JsonConvert.SerializeObject(fromMaxVector, Formatting.None, float3Converter.Converter),
                JsonConvert.SerializeObject(toMinVector, Formatting.None, float3Converter.Converter),
                JsonConvert.SerializeObject(toMaxVector, Formatting.None, float3Converter.Converter),
                (int)domain,
                (int)type
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            JArray array = jsonData["d"] as JArray;
            attribute = array!.Value<string>(0);
            resultAttribute = array.Value<string>(1);
            fromMinInt = array.Value<int>(2);
            fromMaxInt = array.Value<int>(3);
            toMinInt = array.Value<int>(4);
            toMaxInt = array.Value<int>(5);
            fromMinFloat = array.Value<float>(6);
            fromMaxFloat = array.Value<float>(7);
            toMinFloat = array.Value<float>(8);
            toMaxFloat = array.Value<float>(9);
            fromMinVector = JsonConvert.DeserializeObject<float3>(array.Value<string>(10), float3Converter.Converter);
            fromMaxVector = JsonConvert.DeserializeObject<float3>(array.Value<string>(11), float3Converter.Converter);
            toMinVector = JsonConvert.DeserializeObject<float3>(array.Value<string>(12), float3Converter.Converter);
            toMaxVector = JsonConvert.DeserializeObject<float3>(array.Value<string>(13), float3Converter.Converter);
            domain = (TargetDomain)array.Value<int>(14);
            type = (TargetType)array.Value<int>(15);

            attributeField.SetValueWithoutNotify(attribute);
            resultAttributeField.SetValueWithoutNotify(resultAttribute);
            fromMinIntField.SetValueWithoutNotify(fromMinInt);
            fromMaxIntField.SetValueWithoutNotify(fromMaxInt);
            toMinIntField.SetValueWithoutNotify(toMinInt);
            toMaxIntField.SetValueWithoutNotify(toMaxInt);
            fromMinFloatField.SetValueWithoutNotify(fromMinFloat);
            fromMaxFloatField.SetValueWithoutNotify(fromMaxFloat);
            toMinFloatField.SetValueWithoutNotify(toMinFloat);
            toMaxFloatField.SetValueWithoutNotify(toMaxFloat);
            fromMinVectorField.SetValueWithoutNotify(fromMinVector);
            fromMaxVectorField.SetValueWithoutNotify(fromMaxVector);
            toMinVectorField.SetValueWithoutNotify(toMinVector);
            toMaxVectorField.SetValueWithoutNotify(toMaxVector);
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);

            RuntimeNode.UpdateAttribute(attribute);
            RuntimeNode.UpdateResultAttribute(resultAttribute);
            RuntimeNode.UpdateFromMinInt(fromMinInt);
            RuntimeNode.UpdateFromMaxInt(fromMaxInt);
            RuntimeNode.UpdateToMinInt(toMinInt);
            RuntimeNode.UpdateToMaxInt(toMaxInt);
            RuntimeNode.UpdateFromMinFloat(fromMinFloat);
            RuntimeNode.UpdateFromMaxFloat(fromMaxFloat);
            RuntimeNode.UpdateToMinFloat(toMinFloat);
            RuntimeNode.UpdateToMaxFloat(toMaxFloat);
            RuntimeNode.UpdateFromMinVector(fromMinVector);
            RuntimeNode.UpdateFromMaxVector(fromMaxVector);
            RuntimeNode.UpdateToMinVector(toMinVector);
            RuntimeNode.UpdateToMaxVector(toMaxVector);
            RuntimeNode.UpdateDomain(domain);
            RuntimeNode.UpdateType(type);

            OnTypeChanged();

            base.SetNodeData(jsonData);
        }
    }
}