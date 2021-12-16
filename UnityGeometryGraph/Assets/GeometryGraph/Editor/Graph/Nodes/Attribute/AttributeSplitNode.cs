using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.AttributeSystem;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using InputType = GeometryGraph.Runtime.Graph.AttributeSplitNode.AttributeSplitNode_InputType;

namespace GeometryGraph.Editor {
    [Title("Attribute", "Attribute Split")]
    public class AttributeSplitNode : AbstractNode<GeometryGraph.Runtime.Graph.AttributeSplitNode> {
        protected override string Title => "Attribute Split";
        protected override NodeCategory Category => NodeCategory.Attribute;

        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort vectorPort;
        private GraphFrameworkPort attributePort;
        private GraphFrameworkPort xResultPort;
        private GraphFrameworkPort yResultPort;
        private GraphFrameworkPort zResultPort;
        private GraphFrameworkPort resultPort;
        
        private EnumSelectionDropdown<AttributeDomain> domainDropdown;
        private EnumSelectionDropdown<InputType> typeDropdown;
        private Vector3Field vectorField;
        private TextField attributeField;
        private TextField xResultField;
        private TextField yResultField;
        private TextField zResultField;

        private float3 vector;
        private string attribute;
        private string xResult;
        private string yResult;
        private string zResult;
        private AttributeDomain domain = AttributeDomain.Vertex;
        private InputType type = InputType.Attribute;
        
        private static readonly SelectionTree typeTree = new (new List<object>(Enum.GetValues(typeof(InputType)).Convert(o => o))) {
            new SelectionCategory("Type", false) {
                new ("Vector", 0, false),
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
            (vectorPort, vectorField) = GraphFrameworkPort.CreateWithBackingField<Vector3Field, Vector3>("Vector", PortType.Vector, this, showLabelOnField: false, onDisconnect: (_, _) => RuntimeNode.UpdateVector(vector));
            (attributePort, attributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Vector", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateAttribute(attribute));
            (xResultPort, xResultField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("X Result", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateXResult(xResult));
            (yResultPort, yResultField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Y Result", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateYResult(yResult));
            (zResultPort, zResultField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Z Result", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateZResult(zResult));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            domainDropdown = new EnumSelectionDropdown<AttributeDomain>(domain, domainTree, "Domain");
            typeDropdown = new EnumSelectionDropdown<InputType>(type, typeTree, "Type");
            
            domainDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == domain) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change domain");
                domain = evt.newValue;
                RuntimeNode.UpdateTargetDomain(domain);
            });
            
            typeDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == type) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change type");
                type = evt.newValue;
                RuntimeNode.UpdateInputType(type);
                OnTypeChanged();
            });
            
            vectorField.RegisterValueChangedCallback(evt => {
                if (math.distancesq(vector, evt.newValue) < Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change vector");
                vector = evt.newValue;
                RuntimeNode.UpdateVector(vector);
            });
            
            attributeField.RegisterValueChangedCallback(evt => {
                if (attribute == evt.newValue) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change source attribute name");
                attribute = evt.newValue;
                RuntimeNode.UpdateAttribute(attribute);
            });
            
            xResultField.RegisterValueChangedCallback(evt => {
                if (xResult == evt.newValue) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change X result attribute name");
                xResult = evt.newValue;
                RuntimeNode.UpdateXResult(xResult);
            });
            
            yResultField.RegisterValueChangedCallback(evt => {
                if (yResult == evt.newValue) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Y result attribute name");
                yResult = evt.newValue;
                RuntimeNode.UpdateYResult(yResult);
            });
            
            zResultField.RegisterValueChangedCallback(evt => {
                if (zResult == evt.newValue) return;
                
                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change Z result attribute name");
                zResult = evt.newValue;
                RuntimeNode.UpdateZResult(zResult);
            });
            
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);
                
            attributePort.Add(attributeField);
            xResultPort.Add(xResultField);
            yResultPort.Add(yResultField);
            zResultPort.Add(zResultField);
            
            inputContainer.Add(domainDropdown);
            inputContainer.Add(typeDropdown);
            
            AddPort(geometryPort);
            AddPort(vectorPort);
            inputContainer.Add(vectorField);
            AddPort(attributePort);
            AddPort(xResultPort);
            AddPort(yResultPort);
            AddPort(zResultPort);
            AddPort(resultPort);

            OnTypeChanged();
        }

        private void OnTypeChanged() {
            if (type is InputType.Vector) {
                vectorPort.Show();
                attributePort.HideAndDisconnect();
            } else {
                vectorPort.HideAndDisconnect();
                attributePort.Show();
            }
        }
        
        protected override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(vectorPort, RuntimeNode.VectorPort);
            BindPort(attributePort, RuntimeNode.AttributePort);
            BindPort(xResultPort, RuntimeNode.XResultPort);
            BindPort(yResultPort, RuntimeNode.YResultPort);
            BindPort(zResultPort, RuntimeNode.ZResultPort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray array = new() {
                JsonConvert.SerializeObject(vector, Formatting.None, float3Converter.Converter),
                attribute,
                xResult,
                yResult,
                zResult,
                (int)domain,
                (int)type
            };
            root["d"] = array;
            return root;
        }
        
        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;
            vector = JsonConvert.DeserializeObject<float3>(array!.Value<string>(0), float3Converter.Converter);
            attribute = array.Value<string>(1);
            xResult = array.Value<string>(2);
            yResult = array.Value<string>(3);
            zResult = array.Value<string>(4);
            domain = (AttributeDomain)array.Value<int>(5);
            type = (InputType)array.Value<int>(6);
            
            vectorField.SetValueWithoutNotify(vector);
            attributeField.SetValueWithoutNotify(attribute);
            xResultField.SetValueWithoutNotify(xResult);
            yResultField.SetValueWithoutNotify(yResult);
            zResultField.SetValueWithoutNotify(zResult);
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);
            
            RuntimeNode.UpdateVector(vector);
            RuntimeNode.UpdateAttribute(attribute);
            RuntimeNode.UpdateXResult(xResult);
            RuntimeNode.UpdateYResult(yResult);
            RuntimeNode.UpdateZResult(zResult);
            RuntimeNode.UpdateTargetDomain(domain);
            RuntimeNode.UpdateInputType(type);
            
            OnTypeChanged();
            
            base.Deserialize(data);
        }
    }
}