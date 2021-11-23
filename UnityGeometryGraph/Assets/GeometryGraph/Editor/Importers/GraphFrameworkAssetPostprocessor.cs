using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GeometryGraph.Editor {
    public class GraphFrameworkAssetModificationProcessor : AssetModificationProcessor {
        public static readonly HashSet<string> DuplicatedAssets = new HashSet<string>();
        public static readonly HashSet<string> RenamedAssets = new HashSet<string>();
        private static void OnWillCreateAsset(string assetName) {
            if (!assetName.EndsWith(GraphFrameworkImporter.Extension) && !assetName.EndsWith($"{GraphFrameworkImporter.Extension}.meta")) return;

            if (assetName.EndsWith($"{GraphFrameworkImporter.Extension}.meta")) {
                string assetNameNoMeta = assetName[..^5];
                if (!DuplicatedAssets.Contains(assetNameNoMeta)) {
                    DuplicatedAssets.Add(assetNameNoMeta);
                    return;
                }
            }
            
            DuplicatedAssets.Add(assetName);
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath) {
            if (!destinationPath.EndsWith(GraphFrameworkImporter.Extension)) return AssetMoveResult.DidNotMove;
            string sourceFolder = sourcePath[..sourcePath.LastIndexOf('/')];
            string destinationFolder = destinationPath[..destinationPath.LastIndexOf('/')];

            if (!string.Equals(sourceFolder, destinationFolder, StringComparison.InvariantCulture)) {
                return AssetMoveResult.DidNotMove;
            }

            RenamedAssets.Add(destinationPath);
            return AssetMoveResult.DidNotMove;
        }
    }
    
    public class GraphFrameworkAssetPostprocessor : AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            if (GraphFrameworkAssetModificationProcessor.RenamedAssets.Count > 0) {
                UpdateRenamedAssetsTitle(GraphFrameworkAssetModificationProcessor.RenamedAssets);
                GraphFrameworkAssetModificationProcessor.RenamedAssets.Clear();    
            }
            
            
            if (importedAssets.Length > 0 && GraphFrameworkAssetModificationProcessor.DuplicatedAssets.Count > 0) {
                foreach (string importedAsset in importedAssets) {
                    if (!GraphFrameworkAssetModificationProcessor.DuplicatedAssets.Contains(importedAsset)) continue;
                    GraphFrameworkObject gfo = GraphFrameworkUtility.FindOrLoadAtPath(importedAsset);
                    gfo.ResetGuids();
                    GraphFrameworkUtility.SaveGraph(gfo);
                }

                GraphFrameworkAssetModificationProcessor.DuplicatedAssets.Clear();
            }
            if (deletedAssets.Any(path => path.EndsWith(GraphFrameworkImporter.Extension, StringComparison.InvariantCultureIgnoreCase))) {
                DisplayDeletionDialog(deletedAssets);
            }
        }

        private static void UpdateRenamedAssetsTitle(HashSet<string> renamedAssets) {
            IEnumerable<GraphFrameworkEditorWindow> affectedWindows = Resources.FindObjectsOfTypeAll<GraphFrameworkEditorWindow>().Where(window => {
                string assetPath = AssetDatabase.GUIDToAssetPath(window.SelectedAssetGuid);
                return renamedAssets.Contains(assetPath);
            });
            foreach (GraphFrameworkEditorWindow window in affectedWindows) {
                window.UpdateTitle();
            }
        }

        private static void DisplayDeletionDialog(string[] deletedAssets) {
            IEnumerable<GraphFrameworkEditorWindow> affectedWindows = Resources.FindObjectsOfTypeAll<GraphFrameworkEditorWindow>().Where(window => {
                string assetPath = AssetDatabase.GUIDToAssetPath(window.SelectedAssetGuid);
                return deletedAssets.Contains(assetPath);
            });
            
            foreach (GraphFrameworkEditorWindow window in affectedWindows) {
                window.GraphDeleted();
            }
        }
    }
}