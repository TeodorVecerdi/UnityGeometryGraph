using System;
using System.Collections.Generic;
using System.Linq;
using UnityCommons;
using UnityEditor;
using UnityEngine;

namespace GeometryGraph.Editor {
    public class GraphFrameworkAssetModificationProcessor : AssetModificationProcessor {
        public static readonly HashSet<string> DuplicatedAssets = new HashSet<string>();
        private static void OnWillCreateAsset(string assetName) {
            if (!assetName.EndsWith(GraphFrameworkImporter.Extension) && !assetName.EndsWith($"{GraphFrameworkImporter.Extension}.meta")) return;

            if (assetName.EndsWith($"{GraphFrameworkImporter.Extension}.meta")) {
                var assetNameNoMeta = assetName[..^5];
                if (!DuplicatedAssets.Contains(assetNameNoMeta)) {
                    DuplicatedAssets.Add(assetNameNoMeta);
                    return;
                }
            }
            
            DuplicatedAssets.Add(assetName);
        }
    }
    
    public class GraphFrameworkAssetPostprocessor : AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            if (importedAssets.Length > 0 && GraphFrameworkAssetModificationProcessor.DuplicatedAssets.Count > 0) {
                foreach (var importedAsset in importedAssets) {
                    if (!GraphFrameworkAssetModificationProcessor.DuplicatedAssets.Contains(importedAsset)) continue;
                    var gfo = GraphFrameworkUtility.FindOrLoadAtPath(importedAsset);
                    gfo.ResetGuids();
                    GraphFrameworkUtility.SaveGraph(gfo);
                }

                GraphFrameworkAssetModificationProcessor.DuplicatedAssets.Clear();
            }
            if (deletedAssets.Any(path => path.EndsWith(GraphFrameworkImporter.Extension, StringComparison.InvariantCultureIgnoreCase))) {
                DisplayDeletionDialog(deletedAssets);
            }
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