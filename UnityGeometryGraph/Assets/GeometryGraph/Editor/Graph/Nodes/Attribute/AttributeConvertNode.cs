using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using TargetDomain = GeometryGraph.Runtime.Graph.AttributeConvertNode.AttributeConvertNode_Domain;
using TargetType = GeometryGraph.Runtime.Graph.AttributeConvertNode.AttributeConvertNode_Type;

namespace GeometryGraph.Editor {
    [Title("Attribute", "Attribute Convert")]
    public class AttributeConvertNode : AbstractNode<GeometryGraph.Runtime.Graph.AttributeConvertNode> {
        protected override string Title => "Attribute Convert";
        protected override NodeCategory Category => NodeCategory.Attribute;

        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort attributePort;
        private GraphFrameworkPort resultAttributePort;
        private GraphFrameworkPort resultPort;

        private TextField attributeField;
        private TextField resultAttributeField;
        private EnumSelectionDropdown<TargetDomain> domainDropdown;
        private EnumSelectionDropdown<TargetType> typeDropdown;

        private string attribute;
        private string resultAttribute;
        private TargetDomain domain = TargetDomain.Auto;
        private TargetType type = TargetType.Auto;

        private static readonly SelectionTree domainTree = new (new List<object>(Enum.GetValues(typeof(TargetDomain)).Convert(o => o))) {
            new SelectionCategory("Domain", false) {
                new ("Chooses the domain of the result attribute if it already exists, otherwise uses the domain of the source attribute if it exists, otherwise it uses the Vertex domain", 0, true),
                new ("Store the attribute in the Vertex domain", 1, false),
                new ("Store the attribute in the Edge domain", 2, false),
                new ("Store the attribute in the Face domain", 3, false),
                new ("Store the attribute in the Face Corner domain", 4, false)
            }
        };

        private static readonly SelectionTree typeTree = new (new List<object>(Enum.GetValues(typeof(TargetType)).Convert(o => o))) {
            new SelectionCategory("Type", false) {
                new ("Chooses the type of the result attribute if it already exists, otherwise uses the type of the source attribute if it exists, otherwise it uses the Float type", 0, true),
                new ("Fills the attribute with a float value", 1, false),
                new ("Fills the attribute with an integer value", 2, false),
                new ("Fills the attribute with a vector value", 3, false),
                new ("Fills the attribute with a boolean value", 4, false)
            }
        };

        protected override void CreateNode() {
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (attributePort, attributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Attribute", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateAttribute(attribute));
            (resultAttributePort, resultAttributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Result", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateResultAttribute(resultAttribute));
            resultPort = GraphFrameworkPort.Create("Geometry", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);

            domainDropdown = new EnumSelectionDropdown<TargetDomain>(domain, domainTree, "Domain");
            typeDropdown = new EnumSelectionDropdown<TargetType>(type, typeTree, "Type");

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
            
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);
            
            inputContainer.Add(domainDropdown);
            inputContainer.Add(typeDropdown);
            
            attributePort.Add(attributeField);
            resultAttributePort.Add(resultAttributeField);
            
            AddPort(geometryPort);
            AddPort(attributePort);
            AddPort(resultAttributePort);
            AddPort(resultPort);
        }

        protected override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(attributePort, RuntimeNode.AttributePort);
            BindPort(resultAttributePort, RuntimeNode.ResultAttributePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        protected internal override JObject Serialize() {
            JObject root = base.Serialize();
            JArray array = new() {
                attribute,
                resultAttribute,
                (int) domain,
                (int) type
            };
            root["d"] = array;
            return root;
        }

        protected internal override void Deserialize(JObject data) {
            JArray array = data["d"] as JArray;

            attribute = array!.Value<string>(0);
            resultAttribute = array.Value<string>(1);
            domain = (TargetDomain) array.Value<int>(2);
            type = (TargetType) array.Value<int>(3);
            
            attributeField.SetValueWithoutNotify(attribute);
            resultAttributeField.SetValueWithoutNotify(resultAttribute);
            domainDropdown.SetValueWithoutNotify(domain);
            typeDropdown.SetValueWithoutNotify(type);
            
            RuntimeNode.UpdateAttribute(attribute);
            RuntimeNode.UpdateResultAttribute(resultAttribute);
            RuntimeNode.UpdateDomain(domain);
            RuntimeNode.UpdateType(type);
            
            base.Deserialize(data);
        }
    }
}