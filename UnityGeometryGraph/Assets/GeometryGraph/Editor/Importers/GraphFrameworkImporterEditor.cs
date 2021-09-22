using System.IO;
using GeometryGraph.Runtime.Graph;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.Callbacks;

using UnityEngine;

namespace GeometryGraph.Editor {
    [CustomEditor(typeof(GraphFrameworkImporter))]
    public class GraphFrameworkImporterEditor : ScriptedImporterEditor {
        protected override bool needsApplyRevert => false;
        

        public override void OnInspectorGUI() {
            var importer = target as GraphFrameworkImporter;
            if (GUILayout.Button("Open Graph Editor")) {
                OpenEditorWindow(importer!.assetPath);
            }
            ApplyRevertGUI();
        }

        public static bool OpenEditorWindow(string assetPath) {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var extension = Path.GetExtension(assetPath);
            if (string.IsNullOrEmpty(extension))
                return false;

            extension = extension.Substring(1).ToLowerInvariant();
            if (extension != GraphFrameworkImporter.Extension)
                return false;

            var allAssetsAtPath = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            GraphFrameworkObject graphObject = null;
            foreach (var asset in allAssetsAtPath) {
                if (!(asset is GraphFrameworkObject frameworkObject)) continue;
                graphObject = frameworkObject;
                break;
            }

            if (graphObject == null) {
                Debug.LogWarning("GRAPH OBJECT NULL AFTER LOAD");
                graphObject = GraphFrameworkUtility.LoadGraphAtPath(assetPath);
            }
            
            if (string.IsNullOrEmpty(graphObject.AssetGuid)) {
                graphObject.RecalculateAssetGuid(assetPath);
                GraphFrameworkUtility.SaveGraph(graphObject, false);
            }

            foreach (var activeWindow in Resources.FindObjectsOfTypeAll<GraphFrameworkEditorWindow>()) {
                if (activeWindow.SelectedAssetGuid != guid)
                    continue;

                // TODO: Ask user if they want to replace the current window (maybe ask to save before opening)
                activeWindow.SetGraphObject(graphObject);
                activeWindow.BuildWindow();
                activeWindow.Focus();
                return true;
            }

            var window = EditorWindow.CreateWindow<GraphFrameworkEditorWindow>(typeof(GraphFrameworkEditorWindow), typeof(SceneView));
            window.titleContent = EditorGUIUtility.TrTextContentWithIcon(guid, Resources.Load<Texture2D>(GraphFrameworkResources.IconSmall));
            window.SetGraphObject(graphObject);
            window.BuildWindow();
            window.Focus();
            return true;
        }

        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line) {
            var path = AssetDatabase.GetAssetPath(instanceID);
            return OpenEditorWindow(path);
        }
    }
}