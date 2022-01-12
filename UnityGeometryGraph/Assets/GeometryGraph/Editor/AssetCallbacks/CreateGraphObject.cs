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
                $"New Geometry Graph.{GraphFrameworkImporter.Extension}", Resources.Load<Texture2D>(GraphFrameworkResources.DARK_ICON_BIG), null);
        }

        public static void CreateObject(string path) {
            RuntimeGraphObject runtimeGraphObject = CreateInstance<RuntimeGraphObject>();

            GraphFrameworkData graphData = new(runtimeGraphObject);
            GraphFrameworkObject graphObject = CreateInstance<GraphFrameworkObject>();

            graphObject.Initialize(graphData);
            graphObject.GraphData.GraphVersion = GraphFrameworkVersion.Version.GetValue();

            GraphFrameworkUtility.CreateFile(path, graphObject, false);

            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            DeleteObjectsWithNegativeInstanceID();
        }

        public override void Action(int instanceId, string pathName, string resourceFile) {
            RuntimeGraphObject runtimeGraphObject = CreateInstance<RuntimeGraphObject>();

            GraphFrameworkData graphData = new(runtimeGraphObject);
            GraphFrameworkObject graphObject = CreateInstance<GraphFrameworkObject>();

            graphObject.Initialize(graphData);
            graphObject.GraphData.AssetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(instanceId));
            graphObject.GraphData.GraphVersion = GraphFrameworkVersion.Version.GetValue();
            graphObject.AssetGuid = graphObject.GraphData.AssetGuid;

            GraphFrameworkUtility.CreateFile(pathName, graphObject, false);
            AssetDatabase.ImportAsset(pathName);

            DeleteObjectsWithNegativeInstanceID();
        }

        private static void DeleteObjectsWithNegativeInstanceID() {
            GraphFrameworkObject[] instances = Resources.FindObjectsOfTypeAll<GraphFrameworkObject>();
            foreach (GraphFrameworkObject graphFrameworkObject in instances) {
                if (graphFrameworkObject.GetInstanceID() < 0) {
                    Object.DestroyImmediate(graphFrameworkObject, true);
                }
            }

            RuntimeGraphObject[] runtimeGraphObjects = Resources.FindObjectsOfTypeAll<RuntimeGraphObject>();
            foreach (RuntimeGraphObject runtimeGraphObject in runtimeGraphObjects) {
                if (runtimeGraphObject.GetInstanceID() < 0) {
                    Object.DestroyImmediate(runtimeGraphObject, true);
                }
            }
        }
    }
}