using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GeometryGraph.Runtime.Graph;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GeometryGraph.Editor {
    public static class GraphFrameworkUtility {
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
            { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, PreserveReferencesHandling = PreserveReferencesHandling.Objects };
        
        
        #region IO Utilities
        public static bool CreateFile(string path, GraphFrameworkObject graphObject, bool refreshAsset = true) {
            if (graphObject == null || string.IsNullOrEmpty(path)) return false;

            var assetGuid = AssetDatabase.AssetPathToGUID(path);
            graphObject.GraphData.AssetGuid = assetGuid;

            CreateFileNoUpdate(path, graphObject, refreshAsset);
            return true;
        }

        public static void CreateFileNoUpdate(string path, GraphFrameworkObject graphObject, bool refreshAsset = true) {
            var jsonString = JsonUtility.ToJson(graphObject.GraphData);
            File.WriteAllText(path, jsonString);
            if (refreshAsset) AssetDatabase.ImportAsset(path);
        }

        public static bool SaveGraph(GraphFrameworkObject graphObject, bool refreshAsset = true) {
            if (graphObject == null) return false;
            if (string.IsNullOrEmpty(graphObject.GraphData.AssetGuid)) return false;

            var assetPath = AssetDatabase.GUIDToAssetPath(graphObject.GraphData.AssetGuid);
            if (string.IsNullOrEmpty(assetPath)) return false;

            var jsonString = JsonUtility.ToJson(graphObject.GraphData);
            File.WriteAllText(assetPath, jsonString);
            if (refreshAsset) AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return true;
        }

        public static GraphFrameworkObject LoadGraphAtPath(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) return null;

            var jsonString = File.ReadAllText(assetPath);
            try {
                var graphData = JsonUtility.FromJson<GraphFrameworkData>(jsonString);
                var graphObject = ScriptableObject.CreateInstance<GraphFrameworkObject>();
                graphObject.Initialize(graphData);
                graphObject.AssetGuid = graphData.AssetGuid;
                return graphObject;
            } catch (ArgumentNullException exception) {
                Debug.LogException(exception);
                return null;
            }
        }

        public static GraphFrameworkObject LoadGraphAtGuid(string assetGuid) {
            if (string.IsNullOrEmpty(assetGuid)) return null;

            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            if (string.IsNullOrEmpty(assetPath)) return null;

            return LoadGraphAtPath(assetPath);
        }
        #endregion

        /// <summary>
        /// Converts (back-ports or forward-ports) graphObject from <paramref name="fromVersion"/> to the current version.
        /// </summary>
        /// <param name="fromVersion">Graph object version</param>
        /// <param name="graphObject">Graph object to be converted</param>
        public static void VersionConvert(SemVer fromVersion, GraphFrameworkObject graphObject) {
            VersionConverter.ConvertVersion(fromVersion, GraphFrameworkVersion.Version.GetValue(), graphObject);
        }
        
        /**
    !!   * Found this nifty method inside the codebase of ShaderGraph while reverse engineering some functionality.
    !!   * I needed something like this so it didn't make sense to reinvent the wheel, so I took this and slightly modified it.
    !!   * The original can be found in your unity project at: {PROJECT_ROOT}/Library/PackageCache/com.unity.shadergraph@{YOUR_SHADERGRAPH_VERSION}/Editor/Data/Util/GraphUtil.cs 
         */
        /// <summary>
        /// Sanitizes a supplied string such that it does not collide
        /// with any other name in a collection.
        /// </summary>
        /// <param name="existingNames">
        /// A collection of names that the new name should not collide with.
        /// </param>
        /// <param name="duplicateFormat">
        /// The format applied to the name if a duplicate exists.
        /// This must be a format string that contains `{0}` and `{1}`
        /// once each. An example could be `{0} ({1})`, which will append ` (n)`
        /// to the name for the n`th duplicate.
        /// </param>
        /// <param name="name">
        /// The name to be sanitized.
        /// </param>
        /// <returns>
        /// A name that is distinct form any name in `existingNames`.
        /// </returns>
        public static string SanitizeName(IEnumerable<string> existingNames, string duplicateFormat, string name) {
            var existingNamesList = existingNames.ToList();
            if (!existingNamesList.ToList().Contains(name))
                return name;

            var escapedDuplicateFormat = Regex.Escape(duplicateFormat);

            // Escaped format will escape string interpolation, so the escape characters must be removed for these.
            escapedDuplicateFormat = escapedDuplicateFormat.Replace(@"\{0}", @"{0}");
            escapedDuplicateFormat = escapedDuplicateFormat.Replace(@"\{1}", @"{1}");

            var baseRegex = new Regex(string.Format(escapedDuplicateFormat, @"^(.*)", @"(\d+)"));

            var baseMatch = baseRegex.Match(name);
            if (baseMatch.Success)
                name = baseMatch.Groups[1].Value;

            var baseNameExpression = $@"^{Regex.Escape(name)}";
            var regex = new Regex(string.Format(escapedDuplicateFormat, baseNameExpression, @"(\d+)") + "$");

            var existingDuplicateNumbers = existingNamesList.Select(existingName => regex.Match(existingName)).Where(m => m.Success).Select(m => int.Parse(m.Groups[1].Value)).Where(n => n > 0).Distinct().ToList();

            var duplicateNumber = 1;
            existingDuplicateNumbers.Sort();
            if (existingDuplicateNumbers.Any() && existingDuplicateNumbers.First() == 1) {
                duplicateNumber = existingDuplicateNumbers.Last() + 1;
                for (var i = 1; i < existingDuplicateNumbers.Count; i++) {
                    if (existingDuplicateNumbers[i - 1] != existingDuplicateNumbers[i] - 1) {
                        duplicateNumber = existingDuplicateNumbers[i - 1] + 1;
                        break;
                    }
                }
            }

            return string.Format(duplicateFormat, name, duplicateNumber);
        }
    }
}