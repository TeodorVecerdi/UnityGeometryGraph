using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GeometryGraph.Editor {
    public static class VersionConverter {
        private static readonly SemVer v100 = (SemVer) "1.0.0";

        private static readonly SemVer[] sortedVersions = {v100};

        private static bool builtMethodCache;
        private static Dictionary<SemVer, Action<GraphFrameworkObject>> upgradeMethodCache;

        private static SemVer GetNextVersion(SemVer from) {
            for (int i = 1; i < sortedVersions.Length; i++) {
                int comparePrev = from.CompareTo(sortedVersions[i - 1]);
                int compareNext = from.CompareTo(sortedVersions[i]);
                if (comparePrev >= 0 && compareNext < 0) return sortedVersions[i];
            }

            return SemVer.Invalid;
        }

        public static void ConvertVersion(SemVer from, SemVer to, GraphFrameworkObject graphObject) {
            if (from == to) return;
            SemVer next = GetNextVersion(from);
            if (next == SemVer.Invalid) {
                Debug.Log($"Could not find upgrading method [{from} -> {to}]");
                return;
            }
            
            UpgradeTo(next, graphObject);
            ConvertVersion(next, to, graphObject);
        }


        /*
         Note: Use this to upgrade from one version of a graph to another
         Bug: Currently incompatible with structure changes (adding/removing fields) as Unity's JsonUtility fails to deserialize
        [ConvertMethod("1.1.2")] 
        private static void U_112(GraphFrameworkObject graphObject) {
            graphObject.GraphData.GraphVersion = v112;
        }
        */

        private static void UpgradeTo(SemVer version, GraphFrameworkObject graphObject) {
            if (!builtMethodCache) {
                BuildMethodCache();
            }
            if(upgradeMethodCache.ContainsKey(version))
                upgradeMethodCache[version](graphObject);
            else Debug.LogWarning($"Upgrade conversion with [target={version}] is not supported.");
        }


        private static void BuildMethodCache() {
            Debug.Log("Building cache");
            builtMethodCache = true;
            upgradeMethodCache = new Dictionary<SemVer, Action<GraphFrameworkObject>>();

            MethodInfo[] methods = typeof(VersionConverter).GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo method in methods) {
                List<ConvertMethodAttribute> attributes = method.GetCustomAttributes<ConvertMethodAttribute>(false).ToList();
                if (attributes.Count <= 0) continue;
                ConvertMethodAttribute attribute = attributes[0];

                Action<GraphFrameworkObject> methodCall = method.CreateDelegate(typeof(Action<GraphFrameworkObject>)) as Action<GraphFrameworkObject>;
                upgradeMethodCache.Add(attribute.TargetVersion, methodCall);
            }
        }
    }
}