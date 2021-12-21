using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Debug = UnityEngine.Debug;

namespace GeometryGraph.Runtime {
    public class Profiler {
        private static Stack<ProfileSession> sessionStack = new();
        private static List<ProfileSession> finishedSessions = new();
        
        public static IDisposable BeginSession(string name, bool printWhenDone) {
            ProfileSession session = new(name);
            sessionStack.Push(session);
            return new ProfileSessionDisposable(printWhenDone);
        }

        public static IDisposable ProfileMethod() {
            if (sessionStack.Count == 0) {
                // Debug.LogWarning("Attempting to profile a method without starting a profiling session! Call Profiler.BeginSession first.");
                return new DummyDisposable();
            }
            
            StackTrace stack = new(1);
            MethodBase stackMethod = stack.GetFrame(0).GetMethod();
            string className = stackMethod.DeclaringType?.Name;
            string methodName = stackMethod.Name;
            
            ProfileSession currentSession = sessionStack.Peek();
            Method currentMethod = null;
            if (currentSession.MethodStack.Count > 0) {
                currentMethod = currentSession.MethodStack.Peek();
            }
            Method method = new($"{className}:{methodName}", currentMethod);
            if(currentMethod != null) currentMethod.Children.Add(method);
            currentSession.MethodStack.Push(method);

            return new ProfileMethodDisposable(currentSession);
        }

        public static void Cleanup() {
            finishedSessions.Clear();
        }
        
        private class DummyDisposable : IDisposable {
            public void Dispose() { }
        }

        private class ProfileMethodDisposable : IDisposable {
            private ProfileSession currentSession;

            public ProfileMethodDisposable(ProfileSession currentSession) {
                this.currentSession = currentSession;
            }

            public void Dispose() {
                Method currentMethod = currentSession.MethodStack.Pop();
                currentMethod.End();
                if (currentMethod.Parent == null)
                    currentSession.FinishedMethods.Add(currentMethod);
            }
        }

        private class ProfileSessionDisposable : IDisposable {
            private bool printWhenDone;
            
            public ProfileSessionDisposable(bool printWhenDone) {
                this.printWhenDone = printWhenDone;
            }

            public void Dispose() {
                ProfileSession session = sessionStack.Pop();
                finishedSessions.Add(session);

                if (printWhenDone) session.Print();
            }
        }
    }

    

    public class ProfileSession {
        public string Name;
        public readonly Stack<Method> MethodStack = new();
        public List<Method> FinishedMethods = new();

        public ProfileSession(string name) {
            Name = name;
        }

        public void Print() {
            StringBuilder sb = new();
            sb.AppendLine($"PROFILING SESSION [{Name}]");
            foreach (Method finishedMethod in FinishedMethods) {
                finishedMethod.Print(sb, 0);
            }

            Debug.Log(sb.ToString());
        }
    }

    public class Method {
        private readonly Stopwatch methodStopwatch;
        private TimeSpan elapsed;
        
        public readonly string Name;
        public readonly Method Parent;
        public readonly List<Method> Children = new();
        
        public TimeSpan Elapsed => elapsed;

        public Method(string name, Method parent) {
            Name = name;
            Parent = parent;
            methodStopwatch = Stopwatch.StartNew();
            Children.ForEach(child => child.End());
        }

        public void End() {
            elapsed = methodStopwatch.Elapsed;
            methodStopwatch.Stop();
        }

        public void Print(StringBuilder stringBuilder, int indent) {
            string indentStr = $"{new string(' ', indent * 4)}";
            TimeSpan childrenElapsed = Children.Aggregate(TimeSpan.Zero, (total, m2) => total + m2.Elapsed);
            TimeSpan selfElapsed = elapsed - childrenElapsed;
            stringBuilder.AppendLine($"{indentStr}[Total: {elapsed.TotalMilliseconds}ms, Self: {selfElapsed.TotalMilliseconds}ms]\n{indentStr}{Name}:");
            foreach (Method method in Children) {
                method.Print(stringBuilder, indent + 1);
            }
        }
    }
}