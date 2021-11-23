using System;
using System.Collections.Generic;
using GeometryGraph.Runtime.AttributeSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Geometry {
    [Serializable] internal class AttributeDictionary : SerializedDictionary<string, BaseAttribute> {}
    [Serializable] internal class SerializedAttributeDictionary : SerializedDictionary<string, SerializedAttribute> {}

    [Serializable]
    public class AttributeManager : ISerializationCallbackReceiver {
        [ShowInInspector] private AttributeDictionary vertexAttributes;
        [ShowInInspector] private AttributeDictionary edgeAttributes;
        [ShowInInspector] private AttributeDictionary faceAttributes;
        [ShowInInspector] private AttributeDictionary faceCornerAttributes;

        [SerializeReference] private GeometryData owner;

        internal IReadOnlyDictionary<string, BaseAttribute> VertexAttributes => vertexAttributes;
        internal IReadOnlyDictionary<string, BaseAttribute> EdgeAttributes => edgeAttributes;
        internal IReadOnlyDictionary<string, BaseAttribute> FaceAttributes => faceAttributes;
        internal IReadOnlyDictionary<string, BaseAttribute> FaceCornerAttributes => faceCornerAttributes;

        private bool dirty;
        [SerializeField, HideInInspector] private SerializedAttributeDictionary serializedVertexAttributes;
        [SerializeField, HideInInspector] private SerializedAttributeDictionary serializedEdgeAttributes;
        [SerializeField, HideInInspector] private SerializedAttributeDictionary serializedFaceAttributes;
        [SerializeField, HideInInspector] private SerializedAttributeDictionary serializedFaceCornerAttributes;

        public AttributeManager(GeometryData owner) {
            vertexAttributes = new AttributeDictionary();
            edgeAttributes = new AttributeDictionary();
            faceAttributes = new AttributeDictionary();
            faceCornerAttributes = new AttributeDictionary();
            dirty = true;
            this.owner = owner;
        }

        public bool HasAttribute(string name) {
            if (name == null) return false;
            return vertexAttributes.ContainsKey(name) ||
                   edgeAttributes.ContainsKey(name) ||
                   faceAttributes.ContainsKey(name) ||
                   faceCornerAttributes.ContainsKey(name);
        }

        public bool HasAttribute(string name, AttributeType type) {
            if (name == null) return false;
            return vertexAttributes.ContainsKey(name) && vertexAttributes[name].Type == type ||
                   edgeAttributes.ContainsKey(name) && edgeAttributes[name].Type == type ||
                   faceAttributes.ContainsKey(name) && faceAttributes[name].Type == type ||
                   faceCornerAttributes.ContainsKey(name) && faceCornerAttributes[name].Type == type;
        }

        public bool HasAttribute(string name, AttributeDomain domain) {
            if (name == null) return false;
            AttributeDictionary searchDict = domain switch {
                AttributeDomain.Vertex => vertexAttributes,
                AttributeDomain.Edge => edgeAttributes,
                AttributeDomain.Face => faceAttributes,
                AttributeDomain.FaceCorner => faceCornerAttributes,
                _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
            };

            return searchDict.ContainsKey(name);
        }

        public bool HasAttribute(string name, AttributeType type, AttributeDomain domain) {
            if (name == null) return false;
            AttributeDictionary searchDict = domain switch {
                AttributeDomain.Vertex => vertexAttributes,
                AttributeDomain.Edge => edgeAttributes,
                AttributeDomain.Face => faceAttributes,
                AttributeDomain.FaceCorner => faceCornerAttributes,
                _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
            };

            return searchDict.ContainsKey(name) && searchDict[name].Type == type;
        }

        public bool Store(BaseAttribute attribute) {
            AttributeDictionary destDict = attribute.Domain switch {
                AttributeDomain.Vertex => vertexAttributes,
                AttributeDomain.Edge => edgeAttributes,
                AttributeDomain.Face => faceAttributes,
                AttributeDomain.FaceCorner => faceCornerAttributes,
                _ => throw new ArgumentOutOfRangeException(nameof(attribute.Domain), attribute.Domain, null)
            };


            dirty = true;
            bool overwritten = destDict.ContainsKey(attribute.Name);
            destDict[attribute.Name] = attribute;
            return overwritten;
        }

        public bool Store(BaseAttribute attribute, AttributeDomain targetDomain) {
            if (attribute.Domain != targetDomain) {
                AttributeConvert.ConvertDomain(owner, attribute, targetDomain).Into(attribute);
                attribute.Domain = targetDomain;
            }
            
            AttributeDictionary destDict = targetDomain switch {
                AttributeDomain.Vertex => vertexAttributes,
                AttributeDomain.Edge => edgeAttributes,
                AttributeDomain.Face => faceAttributes,
                AttributeDomain.FaceCorner => faceCornerAttributes,
                _ => throw new ArgumentOutOfRangeException(nameof(targetDomain), targetDomain, null)
            };
            
            dirty = true;
            bool overwritten = destDict.ContainsKey(attribute.Name);
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
            // NOTE: Maybe I should do `else find any attribute with name and convert to type`
            return null;
        }
        
        public BaseAttribute Request(string name, AttributeDomain domain) {
            AttributeDictionary searchDict = domain switch {
                AttributeDomain.Vertex => vertexAttributes,
                AttributeDomain.Edge => edgeAttributes,
                AttributeDomain.Face => faceAttributes,
                AttributeDomain.FaceCorner => faceCornerAttributes,
                _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
            };
            if (searchDict.ContainsKey(name)) return searchDict[name];
            // NOTE: Maybe I should do `else find any attribute with name and convert to domain`
            return null;
        }
        
        public BaseAttribute Request(string name, AttributeType type, AttributeDomain domain) {
            AttributeDictionary searchDict = domain switch {
                AttributeDomain.Vertex => vertexAttributes,
                AttributeDomain.Edge => edgeAttributes,
                AttributeDomain.Face => faceAttributes,
                AttributeDomain.FaceCorner => faceCornerAttributes,
                _ => throw new ArgumentOutOfRangeException(nameof(domain), domain, null)
            };
            if (searchDict.ContainsKey(name)) {
                if (searchDict[name].Type == type)
                    return searchDict[name];

                return searchDict[name].Yield(o => AttributeConvert.ConvertType(o, searchDict[name].Type, type)).Into(name, type, domain);
            }

            BaseAttribute attribute = vertexAttributes.ContainsKey(name) ? vertexAttributes[name] :
                edgeAttributes.ContainsKey(name) ? edgeAttributes[name] :
                faceAttributes.ContainsKey(name) ? faceAttributes[name] :
                faceCornerAttributes.ContainsKey(name) ? faceCornerAttributes[name] : null;
            if (attribute == null) return null;

            BaseAttribute clone = (BaseAttribute) attribute.Clone();
            return AttributeConvert.ConvertDomain(owner, clone, domain).Into(clone);
        }

        public void OnBeforeSerialize() {
            if (!dirty) return;
            
            dirty = false;
            SerializeDictionary(vertexAttributes, ref serializedVertexAttributes);
            SerializeDictionary(edgeAttributes, ref serializedEdgeAttributes);
            SerializeDictionary(faceAttributes, ref serializedFaceAttributes);
            SerializeDictionary(faceCornerAttributes, ref serializedFaceCornerAttributes);
        }

        public void OnAfterDeserialize() {
            DeserializeDictionary(serializedVertexAttributes, ref vertexAttributes);
            DeserializeDictionary(serializedEdgeAttributes, ref edgeAttributes);
            DeserializeDictionary(serializedFaceAttributes, ref faceAttributes);
            DeserializeDictionary(serializedFaceCornerAttributes, ref faceCornerAttributes);
        }

        private void SerializeDictionary(AttributeDictionary source, ref SerializedAttributeDictionary destination) {
            if(source == null) return;
            destination = new SerializedAttributeDictionary();
            
            foreach (KeyValuePair<string, BaseAttribute> keyValuePair in source) {
                destination[keyValuePair.Key] = SerializedAttribute.Serialize(keyValuePair.Value);
            }
        }

        private void DeserializeDictionary(SerializedAttributeDictionary source, ref AttributeDictionary destination) {
            if (source == null) {
                return;
            }
            if (source.Count == 0) source.OnAfterDeserialize();
            
            destination = new AttributeDictionary();
            foreach (KeyValuePair<string, SerializedAttribute> keyValuePair in source) {
                destination[keyValuePair.Key] = SerializedAttribute.Deserialize(keyValuePair.Value);
            }
        }
    }
}