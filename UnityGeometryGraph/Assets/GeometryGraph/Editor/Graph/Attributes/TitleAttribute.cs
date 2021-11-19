using System;
using JetBrains.Annotations;

namespace GeometryGraph.Editor {
    [AttributeUsage(AttributeTargets.Class), MeansImplicitUse]
    public class TitleAttribute : System.Attribute {
        public readonly string[] Title;
        public TitleAttribute(params string[] title) {
            Title = title;
        }
    }
}