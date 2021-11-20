using System;
using GeometryGraph.Runtime.Graph;

namespace GeometryGraph.Runtime.Attributes {
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateRuntimeNodeAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class GeneratorSettingsAttribute : Attribute {
        /// <summary>
        ///     Specifies where the generated file should be placed relative to the original file.
        /// </summary>
        public string OutputRelativePath { get; set; }

        /// <summary>
        ///     Whether to generate serialization and deserialization methods.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool GenerateSerialization { get; set; } = true;

        /// <summary>
        ///     Whether to run the calculate methods during deserialization.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool CalculateDuringDeserialization { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class GlobalSettingsAttribute : Attribute {
        /// <summary>
        ///     Specifies where the generated file should be placed relative to the original file.<br />
        ///     Default: <c>""</c>
        /// </summary>
        public string OutputRelativePath { get; set; } = "";

        /// <summary>
        ///     Whether to generate serialization and deserialization methods.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool GenerateSerialization { get; set; } = true;

        /// <summary>
        ///     Whether to run the calculate methods during deserialization.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool CalculateDuringDeserialization { get; set; } = true;

        /// <summary>
        ///     <para>
        ///         Specifies the default file name pattern for the generated files.<br />
        ///         Default: <c>"{fileName}.gen.{extension}"</c>
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <listheader>Supports the following placeholders:</listheader>
        ///             <item><c>{fileName}</c> - The name of the original file without extension</item> //todo:
        ///             <item><c>{extension}</c> - The extension of the original file</item> //todo:
        ///             <item><c>{namespace}</c> - The namespace of the original file</item> //todo:
        ///             <item><c>{className}</c> - The name of the original class</item> //todo:
        ///         </list>
        ///     </para>
        /// </summary>
        public string OutputFileNamePattern { get; set; } = "{fileName}.gen.{extension}";
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public class AdditionalUsingStatementsAttribute : Attribute {
        public AdditionalUsingStatementsAttribute(params string[] namespaces) {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CalculatesPropertyAttribute : Attribute {
        public CalculatesPropertyAttribute() {
        }

        public CalculatesPropertyAttribute(string property) {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CalculatesAllPropertiesAttribute : Attribute {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class UpdatesPropertiesAttribute : Attribute {
        public UpdatesPropertiesAttribute(params string[] properties) {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class InAttribute : Attribute {
        /// <summary>
        ///     Whether the property should be serialized.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool IsSerialized { get; set; } = true;

        /// <summary>
        ///     Whether equality checks should be generated for this property.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool GenerateEquality { get; set; } = true;

        /// <summary>
        ///     Whether the property is updated from the editor node.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool UpdatedFromEditorNode { get; set; } = true;

        /// <summary>
        ///     <para>
        ///         Overrides the default port name for the property.<br />
        ///         Default: <c>{Self}Port</c>
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <listheader>Supports the following placeholders:</listheader>
        ///             <item><c>{self}</c> - The name of the property</item> //todo:
        ///             <item><c>{Self}</c> - The capitalized name of the property</item> //todo:
        ///         </list>
        ///     </para>
        /// </summary>
        public string PortName { get; set; } = "{Self}Port";

        /// <summary>
        ///     <para>
        ///         Overrides the default value used when getting a new value.<br />
        ///         Default: <c>{self}</c>
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <listheader>Supports the following placeholders:</listheader>
        ///             <item><c>{self}</c> - The name of the property</item>
        ///             <item><c>{portName}</c> - The name of the port</item> //todo:
        ///             <item><c>{type}</c> - The name of the property</item>
        ///         </list>
        ///     </para>
        /// </summary>
        public string DefaultValue { get; set; } = "{self}";

        /// <summary>
        ///     <para>
        ///         Overrides the code used to get the new value when a port's value is changed.<br />
        ///         Default: <c>{other} = GetValue(connection, {default});</c>
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <listheader>Supports the following placeholders:</listheader>
        ///             <item><c>{self}</c> - The name of the property</item>
        ///             <item><c>{other}</c> - The name of the other variable (usually <c>newValue</c>)</item>
        ///             <item><c>{portName}</c> - The name of the port</item> //todo:
        ///             <item><c>{default}</c> - The default value of the property</item>
        ///             <item><c>{indent}</c> - The indentation level</item>
        ///         </list>
        ///     </para>
        /// </summary>
        public string GetValueCode { get; set; } = "var {other} = GetValue(connection, {default});";

        /// <summary>
        ///     <para>
        ///         Overrides the code used to update the value of the property when a port's value is changed.<br />
        ///         Default: <c>{self} = {other};</c>
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <listheader>Supports the following placeholders:</listheader>
        ///             <item><c>{self}</c> - The name of the property</item>
        ///             <item><c>{portName}</c> - The name of the port</item> //todo:
        ///             <item><c>{other}</c> - The name of the other variable (usually <c>newValue</c>)</item>
        ///             <item><c>{default}</c> - The default value of the property</item> //todo:
        ///             <item><c>{indent}</c> - The indentation level</item>
        ///         </list>
        ///     </para>
        /// </summary>
        public string UpdateValueCode { get; set; } = "{self} = {other};";

        /// <summary>
        ///     Whether to call NotifyPortValueChanged when the value of the property is changed.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool CallNotifyMethodsIfChanged { get; set; } = true;

        /// <summary>
        ///     Whether to call the calculate methods when the value of the property is changed.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool CallCalculateMethodsIfChanged { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SettingAttribute : Attribute {
        /// <summary>
        ///     Whether the property should be serialized.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool IsSerialized { get; set; } = true;

        /// <summary>
        ///     Whether the property is updated from the editor node.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool UpdatedFromEditorNode { get; set; } = true;

        /// <summary>
        ///     Whether equality checks should be generated for this property.<br />
        ///     Default: <c>true</c>
        /// </summary>
        public bool GenerateEquality { get; set; } = true;
        
        /// <summary>
        ///     <para>
        ///         Overrides the default value used when getting a new value.<br />
        ///         Default: <c>default({type})</c>
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <listheader>Supports the following placeholders:</listheader>
        ///             <item><c>{self}</c> - The name of the property</item>
        ///             <item><c>{type}</c> - The name of the property</item>
        ///         </list>
        ///     </para>
        /// </summary>
        public string DefaultValue { get; set; } = "default({type})";
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OutAttribute : Attribute {
        /// <summary>
        ///     <para>
        ///         Overrides the default port name for the property.<br />
        ///         Default: <c>{Self}Port</c>
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <listheader>Supports the following placeholders:</listheader>
        ///             <item><c>{self}</c> - The name of the property</item> //todo:
        ///             <item><c>{Self}</c> - The capitalized name of the property</item> //todo:
        ///         </list>
        ///     </para>
        /// </summary>
        public string PortName { get; set; } = "{Self}Port";
    }

    /// <summary>
    ///     <para>
    ///         Specifies custom serialization and deserialization code for the property.<br />
    ///         Use this to override the defaults, or when serialization is not supported for the property type.
    ///     </para>
    ///     <para>
    ///         <b>Parameters:</b><br />
    ///         <b><i>serializationCode</i></b> - The code used to serialize the property.<br />
    ///         Default: <c>{self}</c><br />
    ///         <list type="bullet">
    ///             <listheader>Supports the following placeholders:</listheader>
    ///             <item><c>{self}</c> - The name of the property</item> //todo:
    ///             <item><c>{type}</c> - The type of the property</item> //todo:
    ///             <item><c>{default}</c> - The default value of the property</item> //todo:
    ///         </list>
    ///         <b><i>deserializationCode</i></b> - The code used to deserialize the property.<br />
    ///         Default: <c>{self} = {storage}.Value&lt;{type}&gt;({index});</c><br />
    ///         <list type="bullet">
    ///             <listheader>Supports the following placeholders:</listheader>
    ///             <item><c>{self}</c> - The name of the property</item> //todo:
    ///             <item><c>{type}</c> - The type of the property</item> //todo:
    ///             <item><c>{default}</c> - The default value of the property</item> //todo:
    ///             <item><c>{storage}</c> - The name of the storage field</item> //todo:
    ///             <item><c>{index}</c> - The index of the property in the storage field</item> //todo:
    ///             <item><c>{indent}</c> - The indentation level</item> //todo:
    ///         </list>
    ///     </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CustomSerializationAttribute : Attribute {
        /// <summary>
        ///     Specifies custom serialization and deserialization code for the property.<br />
        ///     Use this to override the defaults, or when serialization is not supported for the property type.
        ///     See <see cref="CustomSerializationAttribute" /> for more information.
        /// </summary>
        /// <param name="serializationCode">
        ///     <para>
        ///         The code used to serialize the property.<br />
        ///         See <see cref="CustomSerializationAttribute" /> for more information.
        ///     </para>
        /// </param>
        /// <param name="deserializationCode">
        ///     <para>
        ///         The code used to deserialize the property.<br />
        ///         See <see cref="CustomSerializationAttribute" /> for more information.
        ///     </para>
        /// </param>
        public CustomSerializationAttribute(string serializationCode, string deserializationCode) {
        }
    }
    
    /// <summary>
    ///     <para>
    ///         Specifies custom equality check code for the property.<br />
    ///         Use this to override the default, or when equality is not supported for the property type.
    ///     </para>
    ///     <para>
    ///         <b>Parameters:</b><br />
    ///         <b><i>equalityCode</i></b> - The code used to check equality of the property.<br />
    ///         Default: <c>{self}</c><br />
    ///         <list type="bullet">
    ///             <listheader>Supports the following placeholders:</listheader>
    ///             <item><c>{self}</c> - The name of the property</item> //todo:
    ///             <item><c>{other}</c> - The name of the other variable (usually <c>newValue</c>)</item> //todo:
    ///             <item><c>{default}</c> - The name of the property</item> //todo:
    ///             <item><c>{type}</c> - The type of the property</item> //todo:
    ///             <item><c>{indent}</c> - The indentation level</item> //todo:
    ///         </list>
    ///     </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CustomEqualityAttribute : Attribute {
        /// <summary>
        ///     Specifies custom equality check code for the property.<br />
        ///     Use this to override the default, or when equality is not supported for the property type.
        ///     See <see cref="CustomEqualityAttribute" /> for more information.
        /// </summary>
        /// <param name="equalityCode">
        ///     <para>
        ///         The code used to check the equality of the property.<br />
        ///         See <see cref="CustomEqualityAttribute" /> for more information.
        ///     </para>
        /// </param>
        public CustomEqualityAttribute(string equalityCode) {
        }
    }

    /// <summary>
    ///     <para>
    ///         Overrides the code used to return the property value in <see cref=".RuntimeNode.GetValueForPort">GetValueForPort</see><br />
    ///         Default: <c>return {self};</c>
    ///     </para>
    ///     <para>
    /// <list type="bullet">
    ///     <listheader>Supports the following placeholders:</listheader>
    ///         <item><c>{self}</c> - The name of the property</item>
    ///         <item><c>{portName}</c> - The name of the port</item> //todo:
    ///         <item><c>{type}</c> - The type of the property</item> //todo:
    ///         <item><c>{default}</c> - The default value of the property</item> //todo:
    ///         <item><c>{indent}</c> - The indentation level</item> //todo:
    /// </list>
    ///     </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class GetterAttribute : Attribute {
        /// <summary>
        ///     <para>
        ///         Overrides the code used to return the property value in <see cref="RuntimeNode.GetValueForPort">GetValueForPort</see><br />
        ///         See <see cref="GetterAttribute" /> for more information.
        ///     </para>
        /// </summary>
        /// <param name="getterCode">
        ///     <para>
        ///         The code used to check the equality of the property.<br />
        ///         See <see cref="GetterAttribute" /> for more information.
        ///     </para>
        /// </param>
        public GetterAttribute(string getterCode) {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GetterMethodAttribute : Attribute {
        /// <summary>
        ///     Specifies a method that is used to return the property value in <see cref="RuntimeNode.GetValueForPort">GetValueForPort</see><br />
        /// </summary>
        /// <param name="property">The name of the property</param>
        public GetterMethodAttribute(string property) {
        }

        /// <summary>
        ///     Whether the methods body should be inlined instead of using a method call.<br />Default: <c>false</c>
        /// </summary>
        public bool Inline { get; set; } = false;
    }

    /// <summary>
    ///     <para>
    ///         Specifies additional code that is inserted at the specified location in the <see cref="RuntimeNode.OnPortValueChanged">OnPortValueChanged</see> method.
    ///     </para>
    ///     <para>
    ///         <list type="bullet">
    ///             <listheader>Supports the following placeholders:</listheader>
    ///             <item><c>{self}</c> - The name of the property</item> //todo:
    ///             <item><c>{portName}</c> - The name of the port</item> //todo:
    ///             <item><c>{type}</c> - The type of the property</item> //todo:
    ///             <item><c>{other}</c> - The name of the other variable (usually <c>newValue</c>)</item> //todo:
    ///             <item><c>{default}</c> - The default value of the property</item> //todo:
    ///             <item><c>{indent}</c> - The indentation level</item> //todo:
    ///         </list>
    ///     </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AdditionalValueChangedCodeAttribute : Attribute {
        /// <summary>
        ///     <para>
        ///         Specifies additional code that is inserted at the specified location in the <see cref="RuntimeNode.OnPortValueChanged">OnPortValueChanged</see> method.<br />
        ///         See <see cref="AdditionalValueChangedCodeAttribute" /> for more information.
        ///     </para>
        /// </summary>
        /// <param name="code">
        ///     <para>
        ///         The code used to check the equality of the property.<br />
        ///         See <see cref="AdditionalValueChangedCodeAttribute" /> for more information.
        ///     </para>
        /// </param>
        public AdditionalValueChangedCodeAttribute(string code) {
        }

        /// <summary>
        ///     Specifies where to add the code.<br />Default: <c>AfterUpdate</c>
        /// </summary>
        public Location Where { get; set; } = Location.AfterUpdate;

        public enum Location {
            BeforeGetValue,
            AfterGetValue,
            AfterEqualityCheck,
            AfterUpdate,
            AfterCalculate,
            AfterNotify
        }
    }

    public class SourceClassAttribute : Attribute {
        /// <summary>
        /// This attribute is used by the source generator to specify the class that is the source of the generated class.<br />
        /// Do not use this attribute yourself as it may cause unexpected behavior (notably, getting your files <b>deleted</b> by the source generator).
        /// </summary>
        /// <param name="name"></param>
        public SourceClassAttribute(string name) {
        }
    }
}