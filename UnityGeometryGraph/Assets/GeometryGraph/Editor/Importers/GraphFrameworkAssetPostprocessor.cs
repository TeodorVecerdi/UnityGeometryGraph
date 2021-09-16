using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GraphFramework.Editor {
    public class GraphFrameworkAssetPostprocessor : AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            if (!deletedAssets.Any(path => path.EndsWith(GraphFrameworkImporter.Extension, StringComparison.InvariantCultureIgnoreCase)))
                return;
            
            DisplayDeletionDialog(deletedAssets);
        }

        private static void DisplayDeletionDialog(string[] deletedAssets) {
            var affectedWindows = Resources.FindObjectsOfTypeAll<GraphFrameworkEditorWindow>().Where(window => {
                var assetPath = AssetDatabase.GUIDToAssetPath(window.SelectedAssetGuid);
                return deletedAssets.Contains(assetPath);
            });
            
            foreach (var window in affectedWindows) {
                window.GraphDeleted();
            }
        }
    }
}