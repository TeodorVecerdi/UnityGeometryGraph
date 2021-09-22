using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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
                var current = JsonUtility.ToJson(graphObject.GraphData);
                var saved = File.ReadAllText(AssetDatabase.GUIDToAssetPath(selectedAssetGuid));
                return !string.Equals(current, saved, StringComparison.Ordinal);
            }
        }

        public void BuildWindow() {
            rootVisualElement.Clear();
            windowEvents = new GraphFrameworkWindowEvents {SaveRequested = SaveAsset, SaveAsRequested = SaveAs, ShowInProjectRequested = ShowInProject};

            editorView = new EditorView(this, graphObject) {
                name = "Graph",
                IsBlackboardVisible = graphObject.IsBlackboardVisible
            };
            rootVisualElement.Add(editorView);
            if (VersionCheck()) {
                Refresh();
            }
        }

        private void Update() {
            if (focusedWindow == this && deleted) {
                DisplayDeletedFromDiskDialog();
            }

            if (graphObject == null && selectedAssetGuid != null) {
                var assetGuid = selectedAssetGuid;
                selectedAssetGuid = null;
                var newObject = GraphFrameworkUtility.LoadGraphAtGuid(assetGuid);
                SetGraphObject(newObject);
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
            }

            var wasUndoRedoPerformed = graphObject.WasUndoRedoPerformed;
            if (wasUndoRedoPerformed) {
                editorView.HandleChanges();
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

        private void DisplayDeletedFromDiskDialog() {
            var shouldClose = true; // Close unless if the same file was replaced

            if (EditorUtility.DisplayDialog("Graph Missing", $"{AssetDatabase.GUIDToAssetPath(selectedAssetGuid)} has been deleted or moved outside of Unity.\n\nWould you like to save your Graph Asset?", "Save As", "Close Window")) {
                shouldClose = !SaveAs();
            }

            if (shouldClose)
                Close();
            else
                deleted = false; // Was restored
        }

        public void SetGraphObject(GraphFrameworkObject graphObject) {
            SelectedAssetGuid = graphObject.AssetGuid;
            this.graphObject = graphObject;
        }

        public void Refresh() {
            UpdateTitle();
            editorView.BuildGraph();
        }

        public void GraphDeleted() {
            deleted = true;
        }

        private void UpdateTitle() {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(selectedAssetGuid));
            titleContent.text = asset.name.Split('/').Last() + (IsDirty ? "*" : "");
        }

        private void OnEnable() {
            this.SetAntiAliasing(4);
        }

        private bool VersionCheck() {
            var fileVersion = (SemVer)graphObject.GraphData.GraphVersion;
            var comparison = fileVersion.CompareTo(GraphFrameworkVersion.Version.GetValue());
            if (comparison < 0) {
                if (EditorUtility.DisplayDialog("Version mismatch", $"The graph you are trying to load was saved with an older version of Dialogue Graph.\nIf you proceed with loading it will be converted to the current version. (A backup will be created)\n\nDo you wish to continue?", "Yes", "No")) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(graphObject.AssetGuid);
                    var assetNameSubEndIndex = assetPath.LastIndexOf('.');
                    var backupAssetPath = assetPath.Substring(0, assetNameSubEndIndex);
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
                if (EditorUtility.DisplayDialog("Version mismatch", $"The graph you are trying to load was saved with a newer version of Dialogue Graph.\nLoading the file might cause unexpected behaviour or errors. (A backup will be created)\n\nDo you wish to continue?", "Yes", "No")) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(graphObject.AssetGuid);
                    var assetNameSubEndIndex = assetPath.LastIndexOf('.');
                    var backupAssetPath = assetPath.Substring(0, assetNameSubEndIndex);
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
            if (!skipOnDestroyCheck && IsDirty && EditorUtility.DisplayDialog("Graph has unsaved changes", "Do you want to save the changes you made in the Dialogue Graph?\nYour changes will be lost if you don't save them.", "Save", "Don't Save")) {
                SaveAsset();
            }
        }

        #region Window Events
        private void SaveAsset() {
            graphObject.GraphData.GraphVersion = GraphFrameworkVersion.Version.GetValue();
            GraphFrameworkUtility.SaveGraph(graphObject, false);
            // graphObject.ResetVersion();
            UpdateTitle();
        }

        private bool SaveAs() {
            if (!string.IsNullOrEmpty(selectedAssetGuid) && graphObject != null) {
                var assetPath = AssetDatabase.GUIDToAssetPath(selectedAssetGuid);
                if (string.IsNullOrEmpty(assetPath) || graphObject == null)
                    return false;

                var directoryPath = Path.GetDirectoryName(assetPath);
                var savePath = EditorUtility.SaveFilePanelInProject("Save As...", Path.GetFileNameWithoutExtension(assetPath), GraphFrameworkImporter.Extension, "", directoryPath);
                savePath = savePath.Replace(Application.dataPath, "Assets");
                if (savePath != directoryPath) {
                    if (!string.IsNullOrEmpty(savePath)) {
                        if (GraphFrameworkUtility.CreateFile(savePath, graphObject)) {
                            graphObject.RecalculateAssetGuid(savePath);
                            GraphFrameworkImporterEditor.OpenEditorWindow(savePath);
                        }
                    }

                    graphObject.IsDirty = false;
                    return false;
                }

                SaveAsset();
                graphObject.IsDirty = false;
                return true;
            }

            return false;
        }

        private void ShowInProject() {
            if (string.IsNullOrEmpty(selectedAssetGuid)) return;

            var path = AssetDatabase.GUIDToAssetPath(selectedAssetGuid);
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUIUtility.PingObject(asset);
        }
        #endregion
    }
}