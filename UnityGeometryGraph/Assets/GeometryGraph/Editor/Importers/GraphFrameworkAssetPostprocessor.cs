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
                var assetNameNoMeta = assetName[..^5];
                if (!DuplicatedAssets.Contains(assetNameNoMeta)) {
                    DuplicatedAssets.Add(assetNameNoMeta);
                    return;
                }
            }
            
            DuplicatedAssets.Add(assetName);
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath) {
            if (!destinationPath.EndsWith(GraphFrameworkImporter.Extension)) return AssetMoveResult.DidNotMove;
            var sourceFolder = sourcePath[..sourcePath.LastIndexOf('/')];
            var destinationFolder = destinationPath[..destinationPath.LastIndexOf('/')];

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

        private static void UpdateRenamedAssetsTitle(HashSet<string> renamedAssets) {
            var affectedWindows = Resources.FindObjectsOfTypeAll<GraphFrameworkEditorWindow>().Where(window => {
                var assetPath = AssetDatabase.GUIDToAssetPath(window.SelectedAssetGuid);
                return renamedAssets.Contains(assetPath);
            });
            foreach (var window in affectedWindows) {
                window.UpdateTitle();
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