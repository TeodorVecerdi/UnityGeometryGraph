using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Graph;
using GeometryGraph.Runtime.Serialization;
using Unity.Mathematics;
using UnityEngine;

namespace GeometryGraph.Runtime {
    [Serializable] 
    internal class GeometryGraphSceneData {
        [SerializeField] private PropertyDataDictionary propertyData = new();
        [SerializeField] private StringDictionary referenceToGuidDictionary = new();
        [SerializeField] private StringDictionary guidToReferenceDictionary = new();
        [SerializeField] private int propertyHashCode;

        internal ReadOnlyDictionary<string, PropertyValue> PropertyData => new(propertyData);
        internal ReadOnlyDictionary<string, string> ReferenceToGuidDictionary => new(referenceToGuidDictionary);

        internal int PropertyHashCode {
            get => propertyHashCode;
            set => propertyHashCode = value;
        }

        internal void AddProperty(string guid, string reference, PropertyValue propertyValue) {
            guidToReferenceDictionary[guid] = reference;
            referenceToGuidDictionary[reference] = guid;
            propertyData[guid] = propertyValue;
        }

        internal void RemoveProperty(string guid) {
            string reference = guidToReferenceDictionary[guid];
            propertyData.Remove(guid);
            referenceToGuidDictionary.Remove(reference);
            guidToReferenceDictionary.Remove(guid);
        }

        internal void UpdatePropertyValue(string guid, object value) {
            PropertyValue propertyValue = propertyData[guid];
            switch (propertyValue.PropertyType) {
                case PropertyType.GeometryObject:
                    propertyValue.ObjectValue = (GeometryObject) value;
                    break;
                case PropertyType.GeometryCollection:
                    propertyValue.CollectionValue = (GeometryCollection) value;
                    break;
                case PropertyType.Integer:
                    propertyValue.IntValue = (int) value;
                    break;
                case PropertyType.Float:
                    propertyValue.FloatValue = (float) value;
                    break;
                case PropertyType.Vector:
                    propertyValue.VectorValue = (Vector3) value;
                    break;
                case PropertyType.String:
                    propertyValue.StringValue = (string) value;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(propertyValue.PropertyType), propertyValue.PropertyType, null);
            }
        }

        internal void UpdatePropertyDefaultValue(string guid, DefaultPropertyValue defaultPropertyValue) {
            propertyData[guid].UpdateDefaultValue(defaultPropertyValue);
        }

        internal void Reset() {
            Debug.Log("Reset Properties");
            
            propertyData.Clear();
            referenceToGuidDictionary.Clear();
            guidToReferenceDictionary.Clear();
            propertyHashCode = 0;
        }

        //NOTE: I have no idea if this is correct. I have to check it later.
        internal void EnsureCorrectReferenceName(string guid, string referenceName) {
            if(guidToReferenceDictionary.ContainsKey(guid)) {
                string oldReference = guidToReferenceDictionary[guid];
                if (oldReference == referenceName) return;
                
                Debug.Log($"Reference name changed from {oldReference} to {referenceName}");
                
                guidToReferenceDictionary[guid] = referenceName;
                referenceToGuidDictionary[referenceName] = guid;
                referenceToGuidDictionary.Remove(oldReference);
            } else {
                guidToReferenceDictionary[guid] = referenceName;
            }
        }

        //NOTE: I have no idea if this is correct. I have to check it later.
        internal void EnsureCorrectGuidsAndReferences(List<Property> runtimeDataProperties) {
            HashSet<string> allGuids = runtimeDataProperties.Select(property => property.Guid).ToHashSet();
            HashSet<string> allReferences = runtimeDataProperties.Select(property => property.ReferenceName).ToHashSet();
            
            // Remove all that don't exist anymore
            foreach (string guid in guidToReferenceDictionary.Keys.ToList()) {
                if (allGuids.Contains(guid)) continue;
                
                string reference = guidToReferenceDictionary[guid];
                guidToReferenceDictionary.Remove(guid);
                referenceToGuidDictionary.Remove(reference);
                Debug.Log($"Removed invalid guid: `{guid}` and `{reference}`");
            }
            
            foreach (string reference in referenceToGuidDictionary.Keys.ToList()) {
                if (allReferences.Contains(reference)) continue;
                
                string guid = referenceToGuidDictionary[reference];
                referenceToGuidDictionary.Remove(reference);
                guidToReferenceDictionary.Remove(guid);
                Debug.Log($"Removed invalid reference: `{guid}` and `{reference}`");
            }
        }
    }

    [Serializable] internal class PropertyDataDictionary : SerializedDictionary<string, PropertyValue> {}
    [Serializable] internal class StringDictionary : SerializedDictionary<string, string> {}

    [Serializable] internal class PropertyValue {
        [SerializeField] public GeometryObject ObjectValue;
        [SerializeField] public GeometryCollection CollectionValue;
        [SerializeField] public int IntValue;
        [SerializeField] public float FloatValue;
        [SerializeField] public Vector3 VectorValue; 
        [SerializeField] public string StringValue;
        
        [SerializeField] public GeometryObject DefaultObjectValue;
        [SerializeField] public GeometryCollection DefaultCollectionValue;
        [SerializeField] public int DefaultIntValue;
        [SerializeField] public float DefaultFloatValue;
        [SerializeField] public Vector3 DefaultVectorValue;
        [SerializeField] public string DefaultStringValue;
        //!! Add more here as needed

        [SerializeField] public PropertyType PropertyType;

        public PropertyValue(Property property) {
            PropertyType = property.Type;
            UpdateDefaultValue(property.DefaultValue, true);
        }

        public bool HasDefaultValue() {
            return PropertyType switch {
                PropertyType.GeometryObject => ObjectValue != DefaultObjectValue,
                PropertyType.GeometryCollection => CollectionValue != DefaultCollectionValue,
                PropertyType.Integer => IntValue != DefaultIntValue,
                PropertyType.Float => Math.Abs(FloatValue - DefaultFloatValue) > Constants.FLOAT_TOLERANCE,
                PropertyType.Vector => (VectorValue - DefaultVectorValue).sqrMagnitude > Constants.FLOAT_TOLERANCE * Constants.FLOAT_TOLERANCE,
                PropertyType.String => StringValue != DefaultStringValue,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void UpdateDefaultValue(DefaultPropertyValue defaultPropertyValue, bool forceUpdate = false) {
            switch(PropertyType) {
                case PropertyType.GeometryObject:
                    if (forceUpdate || !HasDefaultValue()) ObjectValue = null;
                    DefaultObjectValue = null;
                    break;
                case PropertyType.GeometryCollection:
                    if (forceUpdate || !HasDefaultValue()) CollectionValue = null;
                    DefaultCollectionValue = null;
                    break;
                case PropertyType.Integer:
                    if (forceUpdate || !HasDefaultValue()) IntValue = defaultPropertyValue.IntValue;
                    DefaultIntValue = defaultPropertyValue.IntValue;
                    break;
                case PropertyType.Float:
                    if (forceUpdate || !HasDefaultValue()) FloatValue = defaultPropertyValue.FloatValue;
                    DefaultFloatValue = defaultPropertyValue.FloatValue;
                    break;
                case PropertyType.Vector:
                    if (forceUpdate || !HasDefaultValue()) VectorValue = defaultPropertyValue.VectorValue;
                    DefaultVectorValue = defaultPropertyValue.VectorValue;
                    break;
                case PropertyType.String:
                    if (forceUpdate || !HasDefaultValue()) StringValue = defaultPropertyValue.StringValue;
                    DefaultStringValue = defaultPropertyValue.StringValue;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(PropertyType), PropertyType, null);
            }
        }

        public object GetValue() {
            switch (PropertyType) {
                case PropertyType.GeometryObject: return ObjectValue;
                case PropertyType.GeometryCollection: return CollectionValue;
                case PropertyType.Integer: return IntValue;
                case PropertyType.Float: return FloatValue;
                case PropertyType.Vector: return (float3)VectorValue;
                case PropertyType.String: return StringValue;

                default: throw new ArgumentOutOfRangeException(nameof(PropertyType), PropertyType, null);
            }
        }

        public override string ToString() {
            string value = PropertyType switch {
                PropertyType.GeometryObject => ObjectValue != null ? ObjectValue.name : "null",
                PropertyType.GeometryCollection => CollectionValue != null ? CollectionValue.name : "null",
                PropertyType.Integer => $"{IntValue}",
                PropertyType.Float => $"{FloatValue}",
                PropertyType.Vector => $"{VectorValue}",
                PropertyType.String => $"{StringValue}",
                _ => throw new ArgumentOutOfRangeException()
            };
            
            string defaultValue = PropertyType switch {
                PropertyType.GeometryObject => DefaultObjectValue != null ? DefaultObjectValue.name : "null",
                PropertyType.GeometryCollection => DefaultCollectionValue != null ? DefaultCollectionValue.name : "null",
                PropertyType.Integer => $"{DefaultIntValue}",
                PropertyType.Float => $"{DefaultFloatValue}",
                PropertyType.Vector => $"{DefaultVectorValue}",
                PropertyType.String => $"{DefaultStringValue}",
                _ => throw new ArgumentOutOfRangeException()
            };
            
            return $"{value} (default: {defaultValue})";
        }
    }

    [Serializable]
    internal class CurveVisualizerSettings {
        // Enable/disable different visualizations
        public bool Enabled = true;
        public bool ShowSpline = true;
        public bool ShowPoints = false;
        public bool ShowDirectionVectors = false;
        
        // Spline settings
        public float SplineWidth = 2.0f;
        public Color SplineColor = Color.white;
        
        // Point settings
        public float PointSize = 0.01f;
        public Color PointColor = Color.white;
        
        // Direction vector settings
        public float DirectionVectorLength = 0.1f;
        public float DirectionVectorWidth = 2.0f;
        public Color DirectionTangentColor = Color.blue;
        public Color DirectionNormalColor = Color.red;
        public Color DirectionBinormalColor = Color.green;
    }

    [Serializable]
    internal class InstancedGeometrySettings {
        // TODO(#17): Provide a way to add per-instance per-submesh materials
        // Maybe using a List<List<Material>>?
        public List<Material> Materials = new();
    }
}