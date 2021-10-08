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

        /// <summary>
        ///   <para>This function is called when the object is loaded.</para>
        /// </summary>
        /// <footer><a href="https://docs.unity3d.com/2021.2/Documentation/ScriptReference/30_search.html?q=AssetImporters.AssetImporterEditor.OnEnable">`AssetImporterEditor.OnEnable` on docs.unity3d.com</a></footer>
        public override void OnEnable() {
            base.OnEnable();
        }

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

            var graphObject = GraphFrameworkUtility.FindOrLoadAtPath(assetPath);
            
            if (string.IsNullOrEmpty(graphObject.AssetGuid)) {
                graphObject.RecalculateAssetGuid(assetPath);
                GraphFrameworkUtility.SaveGraph(graphObject, false);
            }

            foreach (var activeWindow in Resources.FindObjectsOfTypeAll<GraphFrameworkEditorWindow>()) {
                if (activeWindow.SelectedAssetGuid != guid)
                    continue;

                if (activeWindow.IsDirty && !activeWindow.ShowReplaceGraphWindow()) {
                    return true;
                }
                
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