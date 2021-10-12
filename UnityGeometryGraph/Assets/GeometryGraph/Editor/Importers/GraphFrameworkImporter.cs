using UnityEditor.AssetImporters;
using UnityEngine;

namespace GeometryGraph.Editor {
    [ScriptedImporter(0, Extension)]
    public class GraphFrameworkImporter : ScriptedImporter {
        public const string Extension = "geometrygraph";

        public override void OnImportAsset(AssetImportContext ctx) {
            var graphObject = GraphFrameworkUtility.LoadGraphAtPath(ctx.assetPath); 
            var icon = Resources.Load<Texture2D>(GraphFrameworkResources.IconBig);

            ctx.AddObjectToAsset("MainAsset", graphObject.RuntimeGraph, icon);
            ctx.SetMainObject(graphObject.RuntimeGraph);
            

            graphObject.hideFlags = HideFlags.HideInHierarchy;
            ctx.AddObjectToAsset("GraphAsset", graphObject); 
        }
    }
}