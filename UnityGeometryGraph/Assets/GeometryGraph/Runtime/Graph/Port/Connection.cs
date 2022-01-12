using System;
using UnityEngine;

namespace GeometryGraph.Runtime.Graph {
    [Serializable]
    public class Connection : ISerializationCallbackReceiver {
        [NonSerialized] public RuntimePort Output;
        [NonSerialized] public RuntimePort Input;
        [SerializeField] public string OutputGuid;
        [SerializeField] public string InputGuid;

        /// <summary>
        ///   <para>Implement this method to receive a callback before Unity serializes your object.</para>
        /// </summary>
        /// <footer><a href="file:///C:/Program%20Files/Unity/Hub/Editor/2021.1.19f1/Editor/Data/Documentation/en/ScriptReference/ISerializationCallbackReceiver.OnBeforeSerialize.html">External documentation for `ISerializationCallbackReceiver.OnBeforeSerialize`</a></footer>
        public void OnBeforeSerialize() {
            if(Output == null || Input == null) return;

            OutputGuid = Output.Guid;
            InputGuid = Input.Guid;
        }

        /// <summary>
        ///   <para>Implement this method to receive a callback after Unity deserializes your object.</para>
        /// </summary>
        /// <footer><a href="file:///C:/Program%20Files/Unity/Hub/Editor/2021.1.19f1/Editor/Data/Documentation/en/ScriptReference/ISerializationCallbackReceiver.OnAfterDeserialize.html">External documentation for `ISerializationCallbackReceiver.OnAfterDeserialize`</a></footer>
        public void OnAfterDeserialize() {
        }
    }
}