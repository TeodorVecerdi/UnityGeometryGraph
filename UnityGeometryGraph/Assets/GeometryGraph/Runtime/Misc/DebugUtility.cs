using System.Diagnostics;
using System.Runtime.CompilerServices;
using GeometryGraph.Runtime.Graph;
using UnityEngine;

namespace GeometryGraph.Runtime {
    public static class DebugUtility {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string CurrentMethodName(int skipFrames = 2) => new StackTrace(new StackFrame(skipFrames)).GetFrame(0).GetMethod().Name;
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static StackFrame CurrentFrame(int skipFrames = 2) => new StackTrace(new StackFrame(skipFrames)).GetFrame(0);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Log(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0) {
            if (!RuntimeGraphObject.DebugEnabled) return;
            string fileName = filePath[(filePath.LastIndexOfAny(new[] { '/', '\\' }) + 1)..];
            string className = fileName[..^3];
            UnityEngine.Debug.unityLogger.Log(LogType.Log, "<color=#FFA500>DEBUG</color>", $"{className}::{memberName}:\n<color=#FF2400><b>{message}</b></color> (at {fileName}:{lineNumber})");
        }
    }
}