using System;
using System.Collections.Generic;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Domain = GeometryGraph.Runtime.Graph.AttributeRemoveNode.AttributeRemoveNode_Domain;

namespace GeometryGraph.Editor {
    [Title("Attribute", "Attribute Remove")]
    public class AttributeRemoveNode : AbstractNode<GeometryGraph.Runtime.Graph.AttributeRemoveNode> {
        protected override string Title => "Attribute Remove";
        protected override NodeCategory Category => NodeCategory.Attribute;

        private GraphFrameworkPort geometryPort;
        private GraphFrameworkPort attributePort;
        private GraphFrameworkPort resultPort;

        private TextField attributeField;
        private EnumSelectionDropdown<Domain> domainDropdown;

        private string attribute;
        private Domain domain = Domain.All;

        private static readonly SelectionTree domainTree = new (new List<object>(Enum.GetValues(typeof(Domain)).Convert(o => o))) {
            new SelectionCategory("Domain", false) {
                new ("Removes the attribute from all domains it exists in", 0, true),
                new ("Removes the attribute from the Vertex domain (if it exists)", 1, false),
                new ("Removes the attribute from the Edge domain (if it exists)", 2, false),
                new ("Removes the attribute from the Face domain (if it exists)", 3, false),
                new ("Removes the attribute from the Face Corner domain (if it exists)", 4, false)
            }
        };

        public override void CreateNode() {
            geometryPort = GraphFrameworkPort.Create("Geometry", Direction.Input, Port.Capacity.Single, PortType.Geometry, this);
            (attributePort, attributeField) = GraphFrameworkPort.CreateWithBackingField<TextField, string>("Attribute", PortType.String, this, onDisconnect: (_, _) => RuntimeNode.UpdateAttribute(attribute));
            resultPort = GraphFrameworkPort.Create("Result", Direction.Output, Port.Capacity.Multi, PortType.Geometry, this);
            
            domainDropdown = new EnumSelectionDropdown<Domain>(domain, domainTree, "Domain");

            domainDropdown.RegisterValueChangedCallback(evt => {
                if (evt.newValue == domain) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change domain");
                domain = evt.newValue;
                RuntimeNode.UpdateDomain(domain);
            });
            
            attributeField.RegisterValueChangedCallback(evt => {
                if (evt.newValue == attribute) return;

                Owner.EditorView.GraphObject.RegisterCompleteObjectUndo("Change attribute");
                attribute = evt.newValue;
                RuntimeNode.UpdateAttribute(attribute);
            });
            
            domainDropdown.SetValueWithoutNotify(domain);

            attributePort.Add(attributeField);
            
            inputContainer.Add(domainDropdown);
            AddPort(geometryPort);
            AddPort(attributePort);
            AddPort(resultPort);
        }

        public override void BindPorts() {
            BindPort(geometryPort, RuntimeNode.GeometryPort);
            BindPort(attributePort, RuntimeNode.AttributePort);
            BindPort(resultPort, RuntimeNode.ResultPort);
        }

        public override JObject GetNodeData() {
            JObject root = base.GetNodeData();
            JArray array = new() {
                attribute,
                (int)domain,
            };
            root["d"] = array;
            return root;
        }

        public override void SetNodeData(JObject jsonData) {
            JArray array = jsonData["d"] as JArray;
            attribute = array!.Value<string>(0);
            domain = (Domain)array.Value<int>(1);
            
            attributeField.SetValueWithoutNotify(attribute);
            domainDropdown.SetValueWithoutNotify(domain);
            
            RuntimeNode.UpdateAttribute(attribute);
            RuntimeNode.UpdateDomain(domain);
            
            base.SetNodeData(jsonData);
        }
    }
}