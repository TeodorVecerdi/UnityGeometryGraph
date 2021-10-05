using System;
using GeometryGraph.Runtime.Graph;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GeometryGraph.Editor {
    public class GraphFrameworkObject : ScriptableObject, ISerializationCallbackReceiver {
        [NonSerialized] private GraphFrameworkData graphData;
        [NonSerialized] private int objectVersion;

        [SerializeField] public string AssetGuid;
        [SerializeField] public bool IsBlackboardVisible;
        [SerializeField] private string serializedGraph;
        [SerializeField] private int fileVersion;
        [SerializeField] private bool isDirty;
        [SerializeField] public RuntimeGraphObject RuntimeGraph;

        public GraphFrameworkData GraphData {
            get => graphData;
            set {
                graphData = value;
                if (graphData != null)
                    graphData.Owner = this;
            }
        }

        public void Initialize(GraphFrameworkData graphData) {
            GraphData = graphData;
            RuntimeGraph = CreateInstance<RuntimeGraphObject>();
            GraphData.Load(RuntimeGraph);

            IsBlackboardVisible = GraphData.IsBlackboardVisible;
        }

        public bool IsDirty {
            get => isDirty;
            set => isDirty = value;
        }

        public bool WasUndoRedoPerformed => objectVersion != fileVersion;

        public void RegisterCompleteObjectUndo(string operation) {
            Undo.RegisterCompleteObjectUndo(this, operation);
            fileVersion++;
            objectVersion++;
            isDirty = true;
        }

        public void OnBeforeSerialize() {
            if (graphData == null) return;

            serializedGraph = JsonUtility.ToJson(graphData);
            AssetGuid = graphData.AssetGuid;
        }

        public void OnAfterDeserialize() {
            if (GraphData != null) return;
            GraphData = Deserialize();

            if (RuntimeGraph != null && GraphData != null) GraphData.Load(RuntimeGraph);
        }

        public void HandleUndoRedo() {
            if (!WasUndoRedoPerformed) {
                Debug.LogError("Trying to handle undo/redo when undo/redo was not performed", this);
                return;
            }

            var deserialized = Deserialize();
            graphData.ReplaceWith(deserialized);
        }

        private GraphFrameworkData Deserialize() {
            var deserialized = JsonUtility.FromJson<GraphFrameworkData>(serializedGraph);
            if (deserialized == null) return null;
            deserialized.AssetGuid = AssetGuid;
            objectVersion = fileVersion;
            serializedGraph = "";
            return deserialized;
        }

        public void RecalculateAssetGuid(string assetPath) {
            AssetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            graphData.AssetGuid = AssetGuid;
        }
    }
}