using System;
using System.Collections.Generic;
using System.Linq;
using GeometryGraph.Runtime.AttributeSystem;
using JetBrains.Annotations;

namespace GeometryGraph.Runtime.Geometry {
    public partial class GeometryData {
        #region Attribute API

        /// <summary>
        /// Returns true if the geometry has any attribute with name <paramref name="name"/>
        /// </summary>
        [Pure] public bool HasAttribute(string name) => attributeManager.HasAttribute(name);
        
        /// <summary>
        /// Returns true if the geometry has an attribute of type <paramref name="type"/> with name <paramref name="name"/>
        /// </summary>
        [Pure] public bool HasAttribute(string name, AttributeType type) => attributeManager.HasAttribute(name, type);
        
        /// <summary>
        /// Returns true if the geometry has an attribute with name <paramref name="name"/> in the <paramref name="domain"/> domain
        /// </summary>
        [Pure] public bool HasAttribute(string name, AttributeDomain domain) => attributeManager.HasAttribute(name, domain);
        
        /// <summary>
        /// Returns true if the geometry has an attribute of type <paramref name="type"/> with name <paramref name="name"/> in the <paramref name="domain"/> domain
        /// </summary>
        [Pure] public bool HasAttribute(string name, AttributeType type, AttributeDomain domain) => attributeManager.HasAttribute(name, type, domain);
        
        /// <summary>
        /// Returns an attribute with name <paramref name="name"/>, or null if one can't be found
        /// </summary>
        [CanBeNull, MustUseReturnValue]
        public BaseAttribute GetAttribute(string name) {
            return attributeManager.Request(name);
        }

        /// <summary>
        /// Returns an attribute with name <paramref name="name"/> in domain <paramref name="domain"/>, or null if one can't be found
        /// </summary>
        [CanBeNull, MustUseReturnValue]
        public BaseAttribute GetAttribute(string name, AttributeDomain domain) {
            return attributeManager.Request(name, domain);
        }
        
        /// <summary>
        /// Searches for an attribute with name <paramref name="name"/>, type <paramref name="type"/>, and domain <paramref name="domain"/>.
        /// If an attribute with a matching name, type and domain cannot be found, but an attribute of a different type or domain is found,
        /// then it is converted to the target type and domain.
        ///
        /// Returns null if an attribute with <paramref name="name"/> name cannot be found.
        /// </summary>
        [CanBeNull, MustUseReturnValue]
        public BaseAttribute GetAttribute(string name, AttributeType type, AttributeDomain domain) {
            return attributeManager.Request(name, type, domain);
        }

        /// <summary>
        /// Returns an attribute with name <paramref name="name"/>, or null if one can't be found.
        /// It uses the type <typeparamref name="TAttribute"/> to figure out the attribute type.
        ///
        /// This method will convert an attribute to the target type if needed.
        /// </summary>
        [CanBeNull, MustUseReturnValue]
        public TAttribute GetAttribute<TAttribute>(string name) where TAttribute : BaseAttribute {
            return (TAttribute)attributeManager.Request(name, AttributeUtility.SystemTypeToAttributeType(typeof(TAttribute)));
        }

        /// <summary>
        /// Returns an attribute with name <paramref name="name"/> in domain <paramref name="domain"/>, or null if one can't be found.
        /// It uses the type <typeparamref name="TAttribute"/> to figure out the attribute type.
        ///
        /// This method will convert an attribute to the target type or domain if needed.
        /// </summary>
        [CanBeNull, MustUseReturnValue]
        public TAttribute GetAttribute<TAttribute>(string name, AttributeDomain domain) where TAttribute : BaseAttribute {
            return (TAttribute)attributeManager.Request(name, AttributeUtility.SystemTypeToAttributeType(typeof(TAttribute)), domain);
        }
        
        /// <summary>
        /// Returns an attribute with name <paramref name="name"/>, type (inferred from <typeparamref name="TAttribute"/>) and domain <paramref name="domain"/>.
        /// If an attribute cannot be found, then it creates an attribute filled with default value <typeparamref name="T"/> and returns it. 
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <param name="domain">Attribute domain</param>
        /// <param name="defaultValue">Default value in case an attribute is not found</param>
        /// <typeparam name="TAttribute">Attribute class type</typeparam>
        /// <typeparam name="T">Attribute backing value type</typeparam>
        [NotNull, MustUseReturnValue]
        public TAttribute GetAttributeOrDefault<TAttribute, T>(string name, AttributeDomain domain, T defaultValue) where TAttribute : BaseAttribute<T> {
            if (HasAttribute(name, domain)) return GetAttribute<TAttribute>(name, domain)!;
            var attribute = Enumerable.Repeat(defaultValue, domain switch {
                AttributeDomain.Vertex => vertices.Count,
                AttributeDomain.Edge => edges.Count,
                AttributeDomain.Face => faces.Count,
                AttributeDomain.FaceCorner => faceCorners.Count,
                _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
            }).Into<TAttribute>(name, domain);
            return attribute;
        }

        /// <summary>
        /// Returns all attributes in domain <paramref name="domain"/>
        /// </summary>
        /// <param name="domain">Attribute domain</param>
        public IEnumerable<BaseAttribute> GetAttributes(AttributeDomain domain) {
            return domain switch {
                AttributeDomain.Vertex => attributeManager.VertexAttributes.Values,
                AttributeDomain.Edge => attributeManager.EdgeAttributes.Values,
                AttributeDomain.Face => attributeManager.FaceAttributes.Values,
                AttributeDomain.FaceCorner => attributeManager.FaceCornerAttributes.Values,
                _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
            };
        }

        /// <summary>
        /// Saves attribute <paramref name="attribute"/> in the geometry.
        /// </summary>
        /// <param name="attribute">Attribute to save</param>
        /// <returns>True if an attribute with the same name was overwritten, false otherwise</returns>
        public bool StoreAttribute(BaseAttribute attribute) {
            return attributeManager.Store(attribute);
        }
        
        /// <summary>
        /// Saves attribute <paramref name="attribute"/> in domain <paramref name="targetDomain"/> in the geometry.
        /// If attribute domain and the target domain don't match, then the attribute will be converted to the target domain.
        /// </summary>
        /// <param name="attribute">Attribute to save</param>
        /// <param name="targetDomain">Domain in which to save attribute</param>
        /// <returns>True if an attribute with the same name was overwritten, false otherwise</returns>
        public bool StoreAttribute(BaseAttribute attribute, AttributeDomain targetDomain) {
            return attributeManager.Store(attribute, targetDomain);
        }
        
        #endregion
    }
}