using System;
using GeometryGraph.Runtime.Serialization;

namespace GeometryGraph.Runtime.Graph {
    [Serializable] public class StringIntSerializableDictionary : SerializedDictionary<string, int> {}
    [Serializable] public class NodeDictionary : SerializedDictionary<string, RuntimeNode> {}
    [Serializable] public class PropertyDictionary : SerializedDictionary<string, Property> {}
}