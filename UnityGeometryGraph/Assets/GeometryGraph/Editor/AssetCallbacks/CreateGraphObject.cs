using GeometryGraph.Runtime.Graph;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace GeometryGraph.Editor {
    internal class CreateGraphObject : EndNameEditAction {
        [MenuItem("Assets/Create/Geometry Graph/Empty Graph", false, 1)]
        public static void CreateObject()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateGraphObject>(),
                $"New Geometry Graph.{GraphFrameworkImporter.Extension}", Resources.Load<Texture2D>(GraphFrameworkResources.IconBig), null);
        }

        public override void Action(int instanceId, string pathName, string resourceFile) {
            RuntimeGraphObject runtimeGraphObject = CreateInstance<RuntimeGraphObject>();

            GraphFrameworkData graphData = new GraphFrameworkData(runtimeGraphObject);
            GraphFrameworkObject graphObject = CreateInstance<GraphFrameworkObject>();
            
            graphObject.Initialize(graphData);
            graphObject.GraphData.AssetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(instanceId));
            graphObject.GraphData.GraphVersion = GraphFrameworkVersion.Version.GetValue();
            graphObject.AssetGuid = graphObject.GraphData.AssetGuid;

            GraphFrameworkUtility.CreateFile(pathName, graphObject, false);
            
            AssetDatabase.ImportAsset(pathName);
        }
    }
}