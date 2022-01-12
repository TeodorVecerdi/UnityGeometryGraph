using System;
using System.Collections;
using GeometryGraph.Runtime.Data;
using GeometryGraph.Runtime.Graph;
using UnityEngine;

namespace GeometryGraph.Runtime {
    public partial class GeometryGraph {
        public void Evaluate() {
            if (graph == null) return;
            EnsureCorrectState();
            GeometryGraphEvaluationResult evaluationResult = graph.Evaluate(sceneData);
            HandleEvaluationResult(evaluationResult);
        }

        public IEnumerator EvaluateAsync(bool export, Action<GeometryGraphEvaluationResult> onComplete) {
            if (graph == null) {
                isAsyncEvaluationComplete = true;
                yield break;
            }
            EnsureCorrectState();
            isAsyncEvaluationComplete = false;

            GeometryGraphEvaluationResult evaluationResult = graph.Evaluate(sceneData);
            HandleEvaluationResult(evaluationResult, export);
            onComplete?.Invoke(evaluationResult);
            yield return null;
            isAsyncEvaluationComplete = true;
        }

        #region Set Property Value Using Reference Name

        public void SetPropertyIntegerValue(string referenceName, int value) {
            if (!sceneData.ReferenceToGuidDictionary.ContainsKey(referenceName)) {
                Debug.LogError($"Property with reference name `{referenceName}` not found!");
                return;
            }
            SetPropertyValueByGuidImpl(sceneData.ReferenceToGuidDictionary[referenceName], value, PropertyType.Integer, $"with reference name `{referenceName}`");
        }

        public void SetPropertyFloatValue(string referenceName, float value) {
            if (!sceneData.ReferenceToGuidDictionary.ContainsKey(referenceName)) {
                Debug.LogError($"Property with reference name `{referenceName}` not found!");
                return;
            }
            SetPropertyValueByGuidImpl(sceneData.ReferenceToGuidDictionary[referenceName], value, PropertyType.Float, $"with reference name `{referenceName}`");
        }

        public void SetPropertyVector3Value(string referenceName, Vector3 value) {
            if (!sceneData.ReferenceToGuidDictionary.ContainsKey(referenceName)) {
                Debug.LogError($"Property with reference name `{referenceName}` not found!");
                return;
            }
            SetPropertyValueByGuidImpl(sceneData.ReferenceToGuidDictionary[referenceName], value, PropertyType.Vector, $"with reference name `{referenceName}`");
        }

        public void SetPropertyStringValue(string referenceName, string value) {
            if (!sceneData.ReferenceToGuidDictionary.ContainsKey(referenceName)) {
                Debug.LogError($"Property with reference name `{referenceName}` not found!");
                return;
            }
            SetPropertyValueByGuidImpl(sceneData.ReferenceToGuidDictionary[referenceName], value, PropertyType.String, $"with reference name `{referenceName}`");
        }

        public void SetPropertyGeometryObjectValue(string referenceName, GeometryObject value) {
            if (!sceneData.ReferenceToGuidDictionary.ContainsKey(referenceName)) {
                Debug.LogError($"Property with reference name `{referenceName}` not found!");
                return;
            }
            SetPropertyValueByGuidImpl(sceneData.ReferenceToGuidDictionary[referenceName], value, PropertyType.GeometryObject, $"with reference name `{referenceName}`");
        }

        public void SetPropertyGeometryCollectionValue(string referenceName, GeometryCollection value) {
            if (!sceneData.ReferenceToGuidDictionary.ContainsKey(referenceName)) {
                Debug.LogError($"Property with reference name `{referenceName}` not found!");
                return;
            }
            SetPropertyValueByGuidImpl(sceneData.ReferenceToGuidDictionary[referenceName], value, PropertyType.GeometryCollection, $"with reference name `{referenceName}`");
        }

        #endregion

        internal void SetPropertyValueByGuidImpl(string guid, object value, PropertyType valueType, string friendlyPropertyName = null) {
            string propertyName = friendlyPropertyName ?? $"with guid `{guid}`";
            if (!sceneData.PropertyData.ContainsKey(guid)) {
                Debug.LogError($"Property {propertyName} not found!");
                return;
            }

            if(sceneData.PropertyData[guid].PropertyType != valueType) {
                Debug.LogError($"Type mismatch while attempting to set value for property {propertyName}! Expected `{sceneData.PropertyData[guid].PropertyType}`, tried to set `{valueType}`.");
                return;
            }


            sceneData.UpdatePropertyValue(guid, value);
        }
    }
}