using System;

namespace GeometryGraph.Editor {
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ConvertMethodAttribute : Attribute {
        public readonly SemVer TargetVersion;

        /// <summary>
        /// Specifies that tha attached method is a converting method (from one version of Geometry Graph to another)
        /// </summary>
        /// <param name="targetVersion">The target version the method converts to.</param>
        public ConvertMethodAttribute(string targetVersion) {
            TargetVersion = (SemVer)targetVersion;
        }
    }
}