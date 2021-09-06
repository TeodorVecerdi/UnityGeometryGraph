using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GraphFramework.Editor {
    public static class GraphFrameworkVersion {
        private static readonly SemVer fallbackVersion = (SemVer) "1.0.0";
        
        private static Ref<SemVer> version;
        private static GraphVersion versionFile;

        public static Ref<SemVer> Version {
            get {
                if (version == null) {
                    LoadVersionFile();
                    version = Ref<SemVer>.MakeRef(versionFile.Version, () => versionFile.Version, () => versionFile.Version = version!.GetValueUnbound());
                }

                return version;
            }
        }

        public static void SaveVersion(SemVer newVersion) {
            Version.Set(newVersion);
            versionFile.Apply();
            
            /*// Load package.json file and update version
            var packagePath = $"{GraphFrameworkUtility.DialogueGraphPath}\\package.json";
            var packageText = File.ReadAllText(packagePath);
            var package = JObject.Parse(packageText);
            package["version"] = versionFile.Version.ToString();
            File.WriteAllText(packagePath, package.ToString(Formatting.Indented));*/
        }

        private static void LoadVersionFile() {
            if (versionFile == null) {
                versionFile = Resources.Load<GraphVersion>("GraphVersion");
            }

            if (versionFile == null) {
                Debug.LogWarning($"Unable to load GraphVersion. Creating a new file with fallback version = {fallbackVersion}");
                versionFile = ScriptableObject.CreateInstance<GraphVersion>();
                versionFile.Version = fallbackVersion;
            }
        }
    }
}