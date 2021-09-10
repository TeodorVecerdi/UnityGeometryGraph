using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

namespace Misc {
    [Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
        [SerializeField, OdinSerialize] private List<TKey> keys = new List<TKey>();
        [SerializeField, OdinSerialize] private List<TValue> values = new List<TValue>();

        /// <summary>
        ///   <para>Implement this method to receive a callback before Unity serializes your object.</para>
        /// </summary>
        /// <footer><a href="file:///C:/Program%20Files/Unity/Hub/Editor/2021.1.19f1/Editor/Data/Documentation/en/ScriptReference/ISerializationCallbackReceiver.OnBeforeSerialize.html">External documentation for `ISerializationCallbackReceiver.OnBeforeSerialize`</a></footer>
        public void OnBeforeSerialize() {
            keys.Clear();
            values.Clear();

            foreach (var item in this)
            {
                keys.Add(item.Key);
                values.Add(item.Value);
            }
        }

        /// <summary>
        ///   <para>Implement this method to receive a callback after Unity deserializes your object.</para>
        /// </summary>
        /// <footer><a href="file:///C:/Program%20Files/Unity/Hub/Editor/2021.1.19f1/Editor/Data/Documentation/en/ScriptReference/ISerializationCallbackReceiver.OnAfterDeserialize.html">External documentation for `ISerializationCallbackReceiver.OnAfterDeserialize`</a></footer>
        public void OnAfterDeserialize() {
            Clear();
            for (int i = 0; i < keys.Count && i < values.Count; i++)
            {
                this[keys[i]] = values[i];
            }
        }
    }
}