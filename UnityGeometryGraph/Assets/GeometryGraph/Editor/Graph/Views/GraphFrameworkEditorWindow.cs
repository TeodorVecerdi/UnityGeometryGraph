using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace GeometryGraph.Editor {
    public class GraphFrameworkEditorWindow : EditorWindow {
        private string selectedAssetGuid;
        private GraphFrameworkObject graphObject;
        private GraphFrameworkWindowEvents windowEvents;
        private EditorView editorView;

        private bool deleted;
        private bool skipOnDestroyCheck;

        public string SelectedAssetGuid {
            get => selectedAssetGuid;
            set => selectedAssetGuid = value;
        }
        public GraphFrameworkObject GraphObject => graphObject;
        public GraphFrameworkWindowEvents Events => windowEvents;

        public bool IsDirty {
            get {
                if (deleted) return false;
                if (graphObject == null) return false;
                string current = JsonUtility.ToJson(graphObject.GraphData);
                string saved = GraphFrameworkUtility.ReadCompressed(AssetDatabase.GUIDToAssetPath(selectedAssetGuid));
                return !string.Equals(current, saved, StringComparison.Ordinal);
            }
        }

        public void BuildWindow() {
            rootVisualElement.Clear();
            windowEvents = new GraphFrameworkWindowEvents {SaveRequested = SaveAsset, SaveAsRequested = SaveAs, ShowInProjectRequested = ShowInProject};

            editorView = new EditorView(this) {
                name = "Graph",
                IsBlackboardVisible = graphObject.IsBlackboardVisible,
                AreCategoriesEnabled = graphObject.AreCategoriesEnabled,
            };
            rootVisualElement.Add(editorView);
            if (VersionCheck()) {
                Refresh();
            }
        }

        private void Update() {
            if (deleted) {
                bool closed = DisplayDeletedFromDiskDialog();
                if (closed) return;
            }
            
            if (graphObject == null && selectedAssetGuid != null) {
                Debug.LogError("graphObject == null && selectedAssetGuid != null");
                string assetGuid = selectedAssetGuid;
                selectedAssetGuid = null;
                GraphFrameworkObject newObject = GraphFrameworkUtility.FindGraphAtGuid(assetGuid);
                if (newObject == null) {
                    Debug.LogError("did not find graph, reimporting");
                    AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(assetGuid));
                    newObject = GraphFrameworkUtility.FindGraphAtGuid(assetGuid);
                }
                Assert.IsNotNull(newObject);
                SetGraphObject(newObject);
                if(editorView == null) BuildWindow();
                Refresh();
            }

            if (graphObject == null) {
                Close();
                return;
            }

            if (editorView == null && graphObject != null) {
                BuildWindow();
            }

            if (editorView == null) {
                Close();
                return;
            }

            bool wasUndoRedoPerformed = graphObject.WasUndoRedoPerformed;
            if (wasUndoRedoPerformed) {
                // editorView.HandleChanges();
                graphObject.GraphData.ClearChanges();
                graphObject.HandleUndoRedo();
            }

            if (graphObject.IsDirty || wasUndoRedoPerformed) {
                UpdateTitle();
                graphObject.IsDirty = false;
            }

            editorView.HandleChanges();
            graphObject.GraphData.ClearChanges();
        }

        private bool DisplayDeletedFromDiskDialog() {
            SaveAsResult saveResult = new SaveAsResult(false, null);
            if (EditorUtility.DisplayDialog("Graph Missing", $"{AssetDatabase.GUIDToAssetPath(selectedAssetGuid)} has been deleted or moved outside of Unity.\n\nWould you like to save your Graph Asset?", "Save As", "Close Window")) {
                saveResult = SaveAs();
            }

            if (saveResult.Path == null) {
                Close();
                return true;
            }

            GraphFrameworkObject newGraphObject = GraphFrameworkUtility.FindOrLoadAtPath(saveResult.Path);
            SetGraphObject(newGraphObject);
            BuildWindow();
            Focus();
            deleted = false;
            return false;
        }

        internal void SetGraphObject(GraphFrameworkObject graphObject) {
            SelectedAssetGuid = graphObject.AssetGuid;
            this.graphObject = graphObject;
        }

        private void Refresh() {
            UpdateTitle();
            editorView.BuildGraph();
        }

        internal void GraphDeleted() {
            Debug.Log("Graph deleted");
            deleted = true;
        }

        internal void UpdateTitle() {
            titleContent.text = $"{graphObject.RuntimeGraph.name}{(IsDirty ? "*" : "")}";
        }

        private void OnEnable() {
            this.SetAntiAliasing(4);
            titleContent.image = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? GraphFrameworkResources.DARK_ICON_SMALL : GraphFrameworkResources.LIGHT_ICON_SMALL);
        }

        private bool VersionCheck() {
            SemVer fileVersion = (SemVer)graphObject.GraphData.GraphVersion;
            int comparison = fileVersion.CompareTo(GraphFrameworkVersion.Version.GetValue());
            if (comparison < 0) {
                if (EditorUtility.DisplayDialog("Version mismatch", $"The graph you are trying to load was saved with an older version of Geometry Graph.\nIf you proceed with loading it will be converted to the current version. (A backup will be created)\n\nDo you wish to continue?", "Yes", "No")) {
                    string assetPath = AssetDatabase.GUIDToAssetPath(graphObject.AssetGuid);
                    int assetNameSubEndIndex = assetPath.LastIndexOf('.');
                    string backupAssetPath = assetPath.Substring(0, assetNameSubEndIndex);
                    GraphFrameworkUtility.CreateFileNoUpdate($"{backupAssetPath}.backup_{fileVersion}.{GraphFrameworkImporter.Extension}", graphObject);
                    GraphFrameworkUtility.VersionConvert(fileVersion, graphObject);
                    Refresh();
                } else {
                    skipOnDestroyCheck = true;
                    Close();
                }
                return false;
            }

            if (comparison > 0) {
                if (EditorUtility.DisplayDialog("Version mismatch", $"The graph you are trying to load was saved with a newer version of Geometry Graph.\nLoading the file might cause unexpected behaviour or errors. (A backup will be created)\n\nDo you wish to continue?", "Yes", "No")) {
                    string assetPath = AssetDatabase.GUIDToAssetPath(graphObject.AssetGuid);
                    int assetNameSubEndIndex = assetPath.LastIndexOf('.');
                    string backupAssetPath = assetPath.Substring(0, assetNameSubEndIndex);
                    GraphFrameworkUtility.CreateFileNoUpdate($"{backupAssetPath}.backup_{fileVersion}.{GraphFrameworkImporter.Extension}", graphObject);
                    Refresh();
                } else {
                    skipOnDestroyCheck = true;
                    Close();
                }

                return false;
            }
            return true;
        }

        private void OnDestroy() {
            if (!skipOnDestroyCheck && IsDirty) {
                if (EditorUtility.DisplayDialog("Graph has unsaved changes", "Do you want to save the changes you made in the Geometry Graph?\nYour changes will be lost if you don't save them.", "Save", "Don't Save")) {
                    SaveAsset();
                } else {
                    // Reimport to reset changes
                    AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(SelectedAssetGuid));
                }
            }
        }

        public bool ShowReplaceGraphWindow() {
            int selection = EditorUtility.DisplayDialogComplex("Graph has unsaved changes", "Do you want to save the changes you made in the Geometry Graph?\nYour changes will be lost if you don't save them.", "Save As...", "Cancel", "Continue");
            // 0 = Save As, 1 = Cancel, 2 = Continue
            if (selection == 1) 
                return false;
            if (selection == 2) {
                // Reimport asset on disk, then replace
                string assetPath = AssetDatabase.GUIDToAssetPath(selectedAssetGuid);
                AssetDatabase.ImportAsset(assetPath);
                GraphFrameworkObject newGraphObject = GraphFrameworkUtility.FindOrLoadAtPath(assetPath);
                SetGraphObject(newGraphObject);
                BuildWindow();
                Refresh();
                Focus();
                return false;
            }

            if (selection == 0) {
                SaveAsResult result = SaveAs();
                if (result.ReplacedSameAsset) {
                    return false;
                }

                Debug.Log("Reimporting and replacing");
                
                string assetPath = AssetDatabase.GUIDToAssetPath(selectedAssetGuid);
                AssetDatabase.ImportAsset(assetPath);
                GraphFrameworkObject newGraphObject = GraphFrameworkUtility.FindOrLoadAtPath(assetPath);
                SetGraphObject(newGraphObject);
                BuildWindow();
                Refresh();
                Focus();
            }

            return false;
        }

        #region Window Events
        private void SaveAsset() {
            graphObject.GraphData.GraphVersion = GraphFrameworkVersion.Version.GetValue();
            GraphFrameworkUtility.SaveGraph(graphObject, false);
            graphObject.OnAssetSaved();
            // graphObject = GraphFrameworkUtility.FindGraphAtGuid(selectedAssetGuid);
            // Refresh();
            // graphObject.ResetVersion();
            UpdateTitle();
        }

        private SaveAsResult SaveAs() {
            if (!string.IsNullOrEmpty(selectedAssetGuid) && graphObject != null) {
                string assetPath = AssetDatabase.GUIDToAssetPath(selectedAssetGuid);
                if (string.IsNullOrEmpty(assetPath))
                    return new SaveAsResult(false, null);

                string directoryPath = Path.GetDirectoryName(assetPath);
                string savePath = EditorUtility.SaveFilePanelInProject("Save As...", Path.GetFileNameWithoutExtension(assetPath), GraphFrameworkImporter.Extension, "", directoryPath);
                savePath = savePath.Replace(Application.dataPath, "Assets");
                if (!string.Equals(savePath, assetPath, StringComparison.InvariantCulture)) {
                    GraphFrameworkObject objectToSave = graphObject.GetCloneForSerialization();
                    if (string.IsNullOrEmpty(savePath) || !GraphFrameworkUtility.CreateFile(savePath, objectToSave)) {
                        return new SaveAsResult(false, null);
                    }

                    graphObject.IsDirty = false;
                    return new SaveAsResult(false, savePath);
                }

                SaveAsset();
                graphObject.IsDirty = false;
                return new SaveAsResult(true, savePath);
            }

            return new SaveAsResult(false, null);
        }

        private void ShowInProject() {
            if (string.IsNullOrEmpty(selectedAssetGuid)) return;

            string path = AssetDatabase.GUIDToAssetPath(selectedAssetGuid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUIUtility.PingObject(asset);
        }

        internal readonly struct SaveAsResult {
            public readonly bool ReplacedSameAsset;
            public readonly string Path;

            public SaveAsResult(bool replacedSameAsset, string path) {
                ReplacedSameAsset = replacedSameAsset;
                Path = path;
            }
        }
        #endregion
    }
}