using System;
using Attributes;
using UnityEngine;

namespace Geometry {
    [Serializable] internal class AttributeDictionary : SerializableDictionary<string, BaseAttribute> {}

    [Serializable]
    public class AttributeManager {
        [SerializeField, HideInInspector] private AttributeDictionary vertexAttributes;
        [SerializeField, HideInInspector] private AttributeDictionary edgeAttributes;
        [SerializeField, HideInInspector] private AttributeDictionary faceAttributes;
        [SerializeField, HideInInspector] private AttributeDictionary faceCornerAttributes;

        public AttributeManager() {
            vertexAttributes = new AttributeDictionary();
            edgeAttributes = new AttributeDictionary();
            faceAttributes = new AttributeDictionary();
            faceCornerAttributes = new AttributeDictionary();
        }

        public bool Store(BaseAttribute attribute) {
            var destDict = attribute.Domain switch {
                AttributeDomain.Vertex => vertexAttributes,
                AttributeDomain.Edge => edgeAttributes,
                AttributeDomain.Face => faceAttributes,
                AttributeDomain.FaceCorner => faceCornerAttributes,
                _ => throw new ArgumentOutOfRangeException(nameof(attribute.Domain), attribute.Domain, null)
            };

            var overwritten = destDict.ContainsKey(attribute.Name);
            destDict[attribute.Name] = attribute;
            return overwritten;
        }

        public bool Store(BaseAttribute attribute, AttributeDomain targetDomain) {
            // Todo: perform conversion from `attribute.Domain` to `targetDomain`
            var destDict = targetDomain switch {
                AttributeDomain.Vertex => vertexAttributes,
                AttributeDomain.Edge => edgeAttributes,
                AttributeDomain.Face => faceAttributes,
                AttributeDomain.FaceCorner => faceCornerAttributes,
                _ => throw new ArgumentOutOfRangeException(nameof(targetDomain), targetDomain, null)
            };
            
            var overwritten = destDict.ContainsKey(attribute.Name);
            destDict[attribute.Name] = attribute;
            return overwritten;
        }

        public BaseAttribute Request(string name) {
            if (vertexAttributes.ContainsKey(name)) return vertexAttributes[name];
            if (edgeAttributes.ContainsKey(name)) return edgeAttributes[name];
            if (faceAttributes.ContainsKey(name)) return faceAttributes[name];
            if (faceCornerAttributes.ContainsKey(name)) return faceCornerAttributes[name];
            return null;
        }
        
        public BaseAttribute Request(string name, AttributeType type) {
            if (vertexAttributes.ContainsKey(name) && vertexAttributes[name].Type == type) return vertexAttributes[name];
            if (edgeAttributes.ContainsKey(name) && edgeAttributes[name].Type == type) return edgeAttributes[name];
            if (faceAttributes.ContainsKey(name) && faceAttributes[name].Type == type) return faceAttributes[name];
            if (faceCornerAttributes.ContainsKey(name) && faceCornerAttributes[name].Type == type) return faceCornerAttributes[name];
            // note: Maybe I should do `else find any attribute with name and convert to type`
            return null;
        }
        
        public BaseAttribute Request(string name, AttributeDomain domain) {
            var searchDict = domain switch {
                AttributeDomain.Vertex => vertexAttributes,
                AttributeDomain.Edge => edgeAttributes,
                AttributeDomain.Face => faceAttributes,
                AttributeDomain.FaceCorner => faceCornerAttributes,
                _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
            };
            if (searchDict.ContainsKey(name)) return searchDict[name];
            // note: Maybe I should do `else find any attribute with name and convert to domain`
            return null;
        }
    }
}