using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GeometryGraph.Runtime;
using GeometryGraph.Runtime.Graph;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace GeometryGraph.Editor {
    public static class GraphFrameworkUtility {
        #region IO Utilities

        private static readonly LZ4EncoderSettings encoderSettings = new() { CompressionLevel = LZ4Level.L09_HC };

        public static bool CreateFile(string path, GraphFrameworkObject graphObject, bool refreshAsset = true) {
            if (graphObject == null || string.IsNullOrEmpty(path)) return false;

            string assetGuid = AssetDatabase.AssetPathToGUID(path);
            graphObject.GraphData.AssetGuid = assetGuid;
            graphObject.RuntimeGraph.RuntimeData.Guid = Guid.NewGuid().ToString();

            CreateFileNoUpdate(path, graphObject, refreshAsset);
            return true;
        }

        public static void CreateFileNoUpdate(string path, GraphFrameworkObject graphObject, bool refreshAsset = true) {
            string json = JsonUtility.ToJson(graphObject.GraphData);
            WriteCompressed(json, path);
            if (refreshAsset) AssetDatabase.ImportAsset(path);
        }

        public static bool SaveGraph(GraphFrameworkObject graphObject, bool refreshAsset = true) {
            if (graphObject == null) return false;
            if (string.IsNullOrEmpty(graphObject.GraphData.AssetGuid)) return false;

            string assetPath = AssetDatabase.GUIDToAssetPath(graphObject.GraphData.AssetGuid);
            if (string.IsNullOrEmpty(assetPath)) return false;

            string jsonString = JsonUtility.ToJson(graphObject.GraphData);
            WriteCompressed(jsonString, assetPath);
            // if (refreshAsset) AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            if (refreshAsset) AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            return true;
        }

        public static GraphFrameworkObject LoadGraphAtPath(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) return null;
            // Debug.LogWarning("GraphFrameworkUtility::LoadGraphAtPath");
            string jsonString = ReadCompressed(assetPath);
            try {
                RuntimeGraphObjectData.DeserializingFromJson = true;
                GraphFrameworkData graphData = JsonUtility.FromJson<GraphFrameworkData>(jsonString);
                RuntimeGraphObjectData.DeserializingFromJson = false;
                GraphFrameworkObject graphObject = ScriptableObject.CreateInstance<GraphFrameworkObject>();
                graphObject.Initialize(graphData);
                graphObject.AssetGuid = graphData.AssetGuid;

                if (string.IsNullOrEmpty(graphObject.AssetGuid)) {
                    graphObject.RecalculateAssetGuid(assetPath);
                    SaveGraph(graphObject, false);
                }

                return graphObject;
            } catch (ArgumentNullException exception) {
                Debug.LogException(exception);
                return null;
            }
        }

        public static GraphFrameworkObject LoadGraphAtGuid(string assetGuid) {
            if (string.IsNullOrEmpty(assetGuid)) return null;

            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            if (string.IsNullOrEmpty(assetPath)) return null;

            return LoadGraphAtPath(assetPath);
        }

        public static GraphFrameworkObject FindGraphAtGuid(string assetGuid) {
            if (string.IsNullOrEmpty(assetGuid)) return null;

            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            if (string.IsNullOrEmpty(assetPath)) return null;
            return FindGraphAtPath(assetPath);
        }

        public static GraphFrameworkObject FindGraphAtPath(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) return null;
            Object[] allAssetsAtPath = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            GraphFrameworkObject graphFrameworkObject = null;
            RuntimeGraphObject runtimeGraphObject = null;
            foreach (Object asset in allAssetsAtPath) {
                if (asset is GraphFrameworkObject gfo) {
                    graphFrameworkObject = gfo;
                } else if (asset is RuntimeGraphObject rgo) {
                    runtimeGraphObject = rgo;
                }
            }

            if (graphFrameworkObject != null) {
                if (runtimeGraphObject != null) {
                    graphFrameworkObject.RuntimeGraph = runtimeGraphObject;
                    graphFrameworkObject.GraphData.RuntimeGraphData = runtimeGraphObject.RuntimeData;
                } else {
                    Debug.LogWarning($"RuntimeGraphObject not found while loading Graph at path: `{assetPath}`");
                }

                graphFrameworkObject.RecalculateAssetGuid(assetPath);
                return graphFrameworkObject;
            }
            return null;
        }

        public static GraphFrameworkObject FindOrLoadAtPath(string assetPath) {
            if (string.IsNullOrEmpty(assetPath)) return null;
            return FindGraphAtPath(assetPath) ?? LoadGraphAtPath(assetPath);
        }

        internal static void WriteCompressed(string value, string path) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            using MemoryStream byteStream = new(bytes);
            using LZ4EncoderStream lz4Stream = LZ4Stream.Encode(File.Create(path), encoderSettings);
            byteStream.CopyTo(lz4Stream);
        }

        internal static string ReadCompressed(string path) {
            using LZ4DecoderStream lz4Stream = LZ4Stream.Decode(File.OpenRead(path));
            using MemoryStream memoryStream = new();
            lz4Stream.CopyTo(memoryStream);

            return System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
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
            List<string> existingNamesList = existingNames.ToList();
            if (!existingNamesList.ToList().Contains(name))
                return name;

            string escapedDuplicateFormat = Regex.Escape(duplicateFormat);

            // Escaped format will escape string interpolation, so the escape characters must be removed for these.
            escapedDuplicateFormat = escapedDuplicateFormat.Replace(@"\{0}", @"{0}");
            escapedDuplicateFormat = escapedDuplicateFormat.Replace(@"\{1}", @"{1}");

            Regex baseRegex = new(string.Format(escapedDuplicateFormat, @"^(.*)", @"(\d+)"));

            Match baseMatch = baseRegex.Match(name);
            if (baseMatch.Success)
                name = baseMatch.Groups[1].Value;

            string baseNameExpression = $@"^{Regex.Escape(name)}";
            Regex regex = new(string.Format(escapedDuplicateFormat, baseNameExpression, @"(\d+)") + "$");

            List<int> existingDuplicateNumbers = existingNamesList.Select(existingName => regex.Match(existingName)).Where(m => m.Success).Select(m => int.Parse(m.Groups[1].Value)).Where(n => n > 0).Distinct().ToList();

            int duplicateNumber = 1;
            existingDuplicateNumbers.Sort();
            if (existingDuplicateNumbers.Any() && existingDuplicateNumbers.First() == 1) {
                duplicateNumber = existingDuplicateNumbers.Last() + 1;
                for (int i = 1; i < existingDuplicateNumbers.Count; i++) {
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