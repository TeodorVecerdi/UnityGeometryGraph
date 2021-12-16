using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using TargetDomain = GeometryGraph.Runtime.Graph.AttributeEnumerateNode.AttributeEnumerateNode_Domain;
using TargetType = GeometryGraph.Runtime.Graph.AttributeEnumerateNode.AttributeEnumerateNode_Type;

namespace GeometryGraph.Editor {
    [Title("Attribute", "Attribute Enumerate")]
    public class AttributeEnumerateNode : AbstractNode<GeometryGraph.Runtime.Graph.AttributeEnumerateNode> {
        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort attributePort;
        private GraphFrameworkPort floatPort;
        private GraphFrameworkPort integerPort;
        private GraphFrameworkPort vectorPort;
        private GraphFrameworkPort booleanPort;

        private TextField attributeField;
        private EnumSelectionDropdown<TargetDomain> domainDropdown;
        private EnumSelectionDropdown<TargetType> typeDropdown;

        private string attribute;
        private TargetDomain domain = TargetDomain.Auto;
        private TargetType type = TargetType.Float;

        private static readonly SelectionTree domainTree = new (new List<object>(Enum.GetValues(typeof(TargetDomain)).Convert(o => o))) {
            new SelectionCategory("Domain", false) {
                new ("Chooses the domain of the attribute if it already exists, otherwise it uses the Vertex domain", 0, true),
                new ("Enumerates the attribute in the Vertex domain (performing domain conversion if necessary)", 1, false),
                new ("Enumerates the attribute in the Edge domain (performing domain conversion if necessary)", 2, false),
                new ("Enumerates the attribute in the Face domain (performing domain conversion if necessary)", 3, false),
                new ("Enumerates the attribute in the Face Corner domain (performing domain conversion if necessary)", 4, false)
            }
        };
        private static readonly SelectionTree typeTree = new (new List<object>(Enum.GetValues(typeof(TargetType)).Convert(o => o))) {
            new SelectionCategory("Type", false) {
                new ("Enumerates the attribute as float values (performing type conversion if necessary)", 0, false),
                new ("Enumerates the attribute as integer values (performing type conversion if necessary)", 1, false),
                new ("Enumerates the attribute as vector values (performing type conversion if necessary)", 2, false),
                new ("Enumerates the attribute as boolean values (performing type conversion if necessary)", 3, false)
            }
        };

        public override void CreateNode() {
            Initialize("Attribute Enumerate", NodeCategory.Attribute);
            
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (attributePort, attributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Attribute", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateAttribute(attribute));
            floatPort = GraphFrameworkPort.Create("Value", Direction.Output, Port.Capacity.Multi, PortType.Float, this);
            integerPort = GraphFrameworkPort.Create("Value", Direction.Output, Port.Capacity.Multi, PortType.Integer, this);
            vectorPort = GraphFrameworkPort.Create("Value", Direction.Output, Port.Capacity.Multi, PortType.Vector, this);
            booleanPort = GraphFrameworkPort.Create("Value", Direction.Output, Port.Capacity.Multi, PortType.Boolean, this);

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
            
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);
            
            attributePort.Add(attributeField);
            
            inputContainer.Add(domainDropdown);
            inputContainer.Add(typeDropdown);
            
            AddPort(geometryPort);
            AddPort(attributePort);
            AddPort(floatPort); 
            AddPort(integerPort);
            AddPort(vectorPort);
            AddPort(booleanPort);
            
            OnTypeChanged();
        }

        private void OnTypeChanged() {
            switch (type) {
                case TargetType.Float:
                    floatPort.Show();
                    integerPort.HideAndDisconnect();
                    vectorPort.HideAndDisconnect();
                    booleanPort.HideAndDisconnect();
                    break;
                case TargetType.Integer:
                    integerPort.Show();
                    floatPort.HideAndDisconnect();
                    vectorPort.HideAndDisconnect();
                    booleanPort.HideAndDisconnect();
                    break;
                case TargetType.Vector:
                    vectorPort.Show();
                    floatPort.HideAndDisconnect();
                    integerPort.HideAndDisconnect();
                    booleanPort.HideAndDisconnect();
                    break;
                case TargetType.Boolean:
                    booleanPort.Show();
                    floatPort.HideAndDisconnect();
                    integerPort.HideAndDisconnect();
                    vectorPort.HideAndDisconnect();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        public override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(attributePort, RuntimeNode.AttributePort);
            BindPort(floatPort, RuntimeNode.FloatPort);
            BindPort(integerPort, RuntimeNode.IntegerPort);
            BindPort(vectorPort, RuntimeNode.VectorPort);
            BindPort(booleanPort, RuntimeNode.BooleanPort);
        }

        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();
            JArray array = new() {
                attribute,
                (int)domain,
                (int)type
            };
            root["d"] = array;
            return root;
        }
        
        public override void SetNodeData(JObject data) {
            JArray array = data["d"] as JArray;
            attribute = array!.Value<string>(0);
            domain = (TargetDomain)array.Value<int>(1);
            type = (TargetType)array.Value<int>(2);
            
            attributeField.SetValueWithoutNotify(attribute);
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);
            
            RuntimeNode.UpdateAttribute(attribute);
            RuntimeNode.UpdateType(type);
            RuntimeNode.UpdateDomain(domain);
            
            OnTypeChanged();
            
            base.SetNodeData(data);
        }
    }
}