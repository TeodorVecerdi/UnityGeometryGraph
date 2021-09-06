using System;

namespace GraphFramework.Runtime {
    [Serializable] public class StringIntSerializableDictionary : SerializableDictionary<string, int> {}
    [Serializable] public class NodeDictionary : SerializableDictionary<string, Node> {}
    [Serializable] public class PropertyDictionary : SerializableDictionary<string, Property> {}
}