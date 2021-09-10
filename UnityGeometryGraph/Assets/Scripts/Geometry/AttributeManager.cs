using System;
using Attribute;
using Misc;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Geometry {
    [Serializable] internal class AttributeDictionary : SerializedDictionary<string, BaseAttribute> {}
    [Serializable] internal class SerializedAttributeDictionary : SerializedDictionary<string, SerializedAttribute> {}

    [Serializable]
    public class AttributeManager : ISerializationCallbackReceiver {
        [ShowInInspector, ReadOnly] private AttributeDictionary vertexAttributes;
        [ShowInInspector, ReadOnly] private AttributeDictionary edgeAttributes;
        [ShowInInspector, ReadOnly] private AttributeDictionary faceAttributes;
        [ShowInInspector, ReadOnly] private AttributeDictionary faceCornerAttributes;

        [SerializeField, HideInInspector] private bool dirty;
        [SerializeField, HideInInspector] private SerializedAttributeDictionary serializedVertexAttributes;
        [SerializeField, HideInInspector] private SerializedAttributeDictionary serializedEdgeAttributes;
        [SerializeField, HideInInspector] private SerializedAttributeDictionary serializedFaceAttributes;
        [SerializeField, HideInInspector] private SerializedAttributeDictionary serializedFaceCornerAttributes;

        public AttributeManager() {
            vertexAttributes = new AttributeDictionary();
            edgeAttributes = new AttributeDictionary();
            faceAttributes = new AttributeDictionary();
            faceCornerAttributes = new AttributeDictionary();
            dirty = true;
        }

        public bool Store(BaseAttribute attribute) {
            var destDict = attribute.Domain switch {
                AttributeDomain.Vertex => vertexAttributes,
                AttributeDomain.Edge => edgeAttributes,
                AttributeDomain.Face => faceAttributes,
                AttributeDomain.FaceCorner => faceCornerAttributes,
                _ => throw new ArgumentOutOfRangeException(nameof(attribute.Domain), attribute.Domain, null)
            };


            dirty = true;
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
            
            dirty = true;
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

        /// <summary>
        ///   <para>Implement this method to receive a callback before Unity serializes your object.</para>
        /// </summary>
        /// <footer><a href="file:///C:/Program%20Files/Unity/Hub/Editor/2021.1.19f1/Editor/Data/Documentation/en/ScriptReference/ISerializationCallbackReceiver.OnBeforeSerialize.html">External documentation for `ISerializationCallbackReceiver.OnBeforeSerialize`</a></footer>
        public void OnBeforeSerialize() {
            if (!dirty) return;
            
            dirty = false;
            SerializeDictionary(vertexAttributes, ref serializedVertexAttributes);
            SerializeDictionary(edgeAttributes, ref serializedEdgeAttributes);
            SerializeDictionary(faceAttributes, ref serializedFaceAttributes);
            SerializeDictionary(faceCornerAttributes, ref serializedFaceCornerAttributes);
        }

        /// <summary>
        ///   <para>Implement this method to receive a callback after Unity deserializes your object.</para>
        /// </summary>
        /// <footer><a href="file:///C:/Program%20Files/Unity/Hub/Editor/2021.1.19f1/Editor/Data/Documentation/en/ScriptReference/ISerializationCallbackReceiver.OnAfterDeserialize.html">External documentation for `ISerializationCallbackReceiver.OnAfterDeserialize`</a></footer>
        public void OnAfterDeserialize() {
            DeserializeDictionary(serializedVertexAttributes, ref vertexAttributes);
            DeserializeDictionary(serializedEdgeAttributes, ref edgeAttributes);
            DeserializeDictionary(serializedFaceAttributes, ref faceAttributes);
            DeserializeDictionary(serializedFaceCornerAttributes, ref faceCornerAttributes);
        }

        private void SerializeDictionary(AttributeDictionary source, ref SerializedAttributeDictionary destination) {
            if(source == null || source.Count == 0) return;
            destination = new SerializedAttributeDictionary();
            
            foreach (var keyValuePair in source) {
                destination[keyValuePair.Key] = SerializedAttribute.Serialize(keyValuePair.Value);
            }
        }

        private void DeserializeDictionary(SerializedAttributeDictionary source, ref AttributeDictionary destination) {
            if (source == null || destination == null) return;

            destination = new AttributeDictionary();
            foreach (var keyValuePair in source) {
                destination[keyValuePair.Key] = SerializedAttribute.Deserialize(keyValuePair.Value);
            }
        }
    }
}