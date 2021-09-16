using System;

namespace GraphFramework.Runtime {
    [Serializable] public class StringIntSerializableDictionary : SerializedDictionary<string, int> {}
    [Serializable] public class NodeDictionary : SerializedDictionary<string, Node> {}
    [Serializable] public class PropertyDictionary : SerializedDictionary<string, Property> {}
}