using UnityEditor.AssetImporters;
using UnityEngine;

namespace GeometryGraph.Editor {
    [ScriptedImporter(0, Extension)]
    public class GraphFrameworkImporter : ScriptedImporter {
        public const string Extension = "geometrygraph";

        public override void OnImportAsset(AssetImportContext ctx) {
            GraphFrameworkObject graphObject = GraphFrameworkUtility.LoadGraphAtPath(ctx.assetPath);
            Texture2D icon = Resources.Load<Texture2D>(GraphFrameworkResources.DARK_ICON_BIG);

            ctx.AddObjectToAsset("MainAsset", graphObject.RuntimeGraph, icon);
            ctx.SetMainObject(graphObject.RuntimeGraph);


            graphObject.hideFlags = HideFlags.HideInHierarchy;
            ctx.AddObjectToAsset("GraphAsset", graphObject);
        }
    }
}