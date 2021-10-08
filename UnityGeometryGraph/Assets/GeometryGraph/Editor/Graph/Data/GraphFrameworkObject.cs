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
                Debug.LogWarning("GraphFrameworkObject::set_GraphData");
                graphData = value;
                if (graphData != null)
                    graphData.Owner = this;
            }
        }

        public void Initialize(GraphFrameworkData graphData) {
            Debug.LogWarning("GraphFrameworkObject::Initialize");
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
            Debug.LogWarning($"GraphFrameworkObject::OnBeforeSerialize {GetInstanceID()}");
            if (graphData == null) return;

            serializedGraph = JsonUtility.ToJson(graphData);
            AssetGuid = graphData.AssetGuid;
        }

        public void OnAfterDeserialize() {
            Debug.LogWarning($"GraphFrameworkObject::OnAfterDeserialize {GetInstanceID()}");
            if (GraphData != null) {
                Debug.LogWarning($"Graph data not null {GetInstanceID()}");
                return;
            }
            GraphData = Deserialize();

            if (RuntimeGraph != null && GraphData != null) GraphData.Load(RuntimeGraph);
        }

        public void HandleUndoRedo() {
            Debug.LogWarning("GraphFrameworkObject::HandleUndoRedo");
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

        public GraphFrameworkObject GetCloneForSerialization() {
            var gfo = CreateInstance<GraphFrameworkObject>();

            var cloneData = JsonUtility.FromJson<GraphFrameworkData>(JsonUtility.ToJson(graphData));
            cloneData.AssetGuid = AssetGuid;
            gfo.Initialize(cloneData);

            return gfo;
        }

        public void RecalculateAssetGuid(string assetPath) {
            AssetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            graphData.AssetGuid = AssetGuid;
        }

        public void OnAssetSaved() {
            objectVersion = fileVersion;
        }

        public void ResetGuids() {
            graphData.RuntimeGraphData.Guid = Guid.NewGuid().ToString();
        }
    }
}