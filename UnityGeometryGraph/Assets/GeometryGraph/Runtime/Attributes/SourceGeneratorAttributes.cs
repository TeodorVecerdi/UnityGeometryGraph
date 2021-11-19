using System;

namespace GeometryGraph.Runtime.Attributes {
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateRuntimeNodeAttribute: Attribute {
        /// <summary>
        /// Specifies where the generated file should be placed relative to the original file.
        /// </summary>
        public string OutputPath { get; set; }
    }

    public class SourceClassAttribute: Attribute {
        public SourceClassAttribute(string name) { }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class AdditionalUsingStatementsAttribute: Attribute {
        public AdditionalUsingStatementsAttribute(params string[] namespaces) {
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class CalculatesPropertyAttribute: Attribute {
        public CalculatesPropertyAttribute() {
        }
        public CalculatesPropertyAttribute(string property) {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CalculatesAllPropertiesAttribute: Attribute {
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UpdatesPropertiesAttribute: Attribute {
        public UpdatesPropertiesAttribute(params string[] properties) {
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InAttribute: Attribute {
        /// <summary>
        /// Whether the property should be serialized.<br/>Default: <c>true</c>
        /// </summary>
        public bool IsSerialized { get; set;  } = true;
        
        /// <summary>
        /// Whether equality checks should be generated for this property.<br/>Default: <c>true</c>
        /// </summary>
        public bool GenerateEquality { get; set; } = true;
        
        /// <summary>
        /// Whether the property is updated from the editor node.<br/>Default: <c>true</c>
        /// </summary>
        public bool UpdatedFromEditorNode { get; set; } = true;

        /// <summary>
        /// Overrides the default port name for the property.<br/>
        /// By default, the port name is the name of the property + "Port"
        /// </summary>
        public string PortName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OutAttribute : Attribute {
        /// <summary>
        /// Overrides the default port name for the property.<br/>
        /// By default, the port name is the name of the property + "Port"
        /// </summary>
        public string PortName { get; set; }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SettingAttribute : Attribute {
        /// <summary>
        /// Whether the property should be serialized.<br/>Default: <c>true</c>
        /// </summary>
        public bool IsSerialized { get; set; } = true;

        /// <summary>
        /// Whether equality checks should be generated for this property.<br/>Default: <c>true</c>
        /// </summary>
        public bool GenerateEquality { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CustomSerializationAttribute: Attribute {
        public CustomSerializationAttribute(string serializationCode, string deserializationCode) { }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CustomEqualityAttribute: Attribute {
        public CustomEqualityAttribute(string equalityCode) { }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class GetterAttribute: Attribute {
        public GetterAttribute(string getterCode) { }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class GetterMethodAttribute: Attribute {
        /// <summary>
        /// Whether the methods body should be inlined instead of using a method call.<br/>Default: <c>false</c>
        /// </summary>
        public bool Inline { get; set; } = false;
        
        public GetterMethodAttribute(string property) { }
    }
}