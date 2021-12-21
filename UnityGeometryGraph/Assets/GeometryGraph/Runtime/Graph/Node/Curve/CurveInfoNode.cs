using GeometryGraph.Runtime.Attributes;
using GeometryGraph.Runtime.Curve;
using JetBrains.Annotations;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    [GeneratorSettings(GenerateSerialization = false)]
    public partial class CurveInfoNode {
        [In(
            DefaultValue = "(CurveData)null",
            GetValueCode = "{self} = GetValue(connection, {default})",
            UpdateValueCode = ""
        )]
        public CurveData Curve { get; private set; }
        
        [Out] public int Points { get; private set; }
        [Out] public bool IsClosed { get; private set; }

        [GetterMethod(nameof(Points), Inline = true)] 
        private int GetPoints() => Curve?.Points ?? 0;
        
        [GetterMethod(nameof(IsClosed), Inline = true)] 
        private bool GetIsClosed() => Curve?.IsClosed ?? false;
    }
}