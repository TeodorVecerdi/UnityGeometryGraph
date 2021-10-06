using GeometryGraph.Runtime.Graph;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace GeometryGraph.Editor {
    public static class MenuItems {
        [MenuItem("Geometry Graph/Toggle Debug")]
        public static void ToggleDebug() {
            Debug.Log(RuntimeGraphObject.DebugEnabled ? "Disabled debug." : "Enabled debug.");
            RuntimeGraphObject.DebugEnabled = !RuntimeGraphObject.DebugEnabled;
        }
        
        [MenuItem("Geometry Graph/Print InstanceID")]
        public static void PrintInstanceId() {
            var selection = Selection.activeObject;
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(selection));
            foreach (var asset in allAssets) {
                Debug.LogWarning($"{asset.name}:{asset.GetType().Name}:{asset.GetInstanceID()}");
                
            }
        }
        
        [MenuItem("Geometry Graph/Find all instances")]
        public static void FindGFOInstances() {
            var instances = Resources.FindObjectsOfTypeAll<GraphFrameworkObject>();
            Debug.Log(instances.Length);
            foreach (var graphFrameworkObject in instances) {
                Debug.Log($"{graphFrameworkObject.name}:{graphFrameworkObject.GetInstanceID()}");
            }
        }
        
        [MenuItem("Geometry Graph/Delete all instances")]
        public static void DeleteGFOInstances() {
            var instances = Resources.FindObjectsOfTypeAll<GraphFrameworkObject>();
            Debug.Log(instances.Length);
            foreach (var graphFrameworkObject in instances) {
                Debug.Log($"{graphFrameworkObject.name}:{graphFrameworkObject.GetInstanceID()}");
                Object.DestroyImmediate(graphFrameworkObject, true);
            }
        }
        
        [MenuItem("Geometry Graph/Force recompile")]
        public static void ForceRecompile() {
            CompilationPipeline.RequestScriptCompilation();
        }
    }
}