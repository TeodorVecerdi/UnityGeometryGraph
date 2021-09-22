using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

// Note: Aliases for easily differentiating between runtime and editor classes 
using RuntimeConnection = GeometryGraph.Runtime.Graph.Connection;
using RuntimeProperty = GeometryGraph.Runtime.Graph.Property;

namespace GeometryGraph.Editor {
    [ScriptedImporter(0, Extension, 3)]
    public class GraphFrameworkImporter : ScriptedImporter {
        public const string Extension = "geometrygraph";

        public override void OnImportAsset(AssetImportContext ctx) {
            var graphObject = GraphFrameworkUtility.LoadGraphAtPath(ctx.assetPath);
            var icon = Resources.Load<Texture2D>(GraphFrameworkResources.IconBig);

            if (string.IsNullOrEmpty(graphObject.AssetGuid) || graphObject.AssetGuid != AssetDatabase.AssetPathToGUID(ctx.assetPath)) {
                graphObject.RecalculateAssetGuid(ctx.assetPath);
                GraphFrameworkUtility.SaveGraph(graphObject, false);
            }

            ctx.AddObjectToAsset("MainAsset", graphObject.RuntimeGraph, icon);
            ctx.SetMainObject(graphObject.RuntimeGraph);
            

            graphObject.hideFlags = HideFlags.HideInHierarchy;
            ctx.AddObjectToAsset("GraphAsset", graphObject);
        }
    }
}