using GeometryGraph.Runtime.Attributes;
using UTime = UnityEngine.Time;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class TimeNode {
        [Setting, UpdatesProperties(nameof(Time), nameof(DeltaTime))]
        public bool IsFixed { get; private set; }
        [Setting, UpdatesProperties(nameof(Time), nameof(DeltaTime))]
        public bool IsUnscaled { get; private set; }
        [Out] public float SmoothDeltaTime { get; private set; }
        [Out] public float TimeScale { get; private set; }
        [Out] public float RealtimeSinceStartup { get; private set; }
        [Out] public float TimeSinceLevelLoad { get; private set; }
        [Out] public float Time { get; private set; }
        [Out] public float DeltaTime { get; private set; }

        [GetterMethod(nameof(SmoothDeltaTime), Inline = true)]
        private float GetSmoothDeltaTime() => Utils.IfNotSerializing(() => UTime.smoothDeltaTime, "UnityEngine.Time.smoothDeltaTime", 0.016f);

        [GetterMethod(nameof(TimeScale), Inline = true)]
        private float GetTimeScale() => Utils.IfNotSerializing(() => UTime.timeScale, "UnityEngine.timeScale", 1.0f);

        [GetterMethod(nameof(RealtimeSinceStartup), Inline = true)]
        private float GetRealtimeSinceStartup() => Utils.IfNotSerializing(() => UTime.realtimeSinceStartup, "UnityEngine.realtimeSinceStartup");

        [GetterMethod(nameof(TimeSinceLevelLoad), Inline = true)]
        private float GetTimeSinceLevelLoad() => Utils.IfNotSerializing(() => UTime.timeSinceLevelLoad, "UnityEngine.timeSinceLevelLoad");

        [GetterMethod(nameof(Time), Inline = true)]
        private float GetTime() {
            return Utils.IfNotSerializing(
                () => IsFixed switch {
                    true when IsUnscaled => UTime.fixedUnscaledTime,
                    true => UTime.fixedTime,
                    false when IsUnscaled => UTime.unscaledTime,
                    false => UTime.time
                }, "UnityEngine.Time");
        }

        [GetterMethod(nameof(DeltaTime), Inline = true)]
        private float GetDeltaTime() {
            return Utils.IfNotSerializing(
                () => IsFixed switch {
                true when IsUnscaled => UTime.fixedUnscaledDeltaTime,
                true => UTime.fixedDeltaTime,
                false when IsUnscaled => UTime.unscaledDeltaTime,
                false => UTime.deltaTime
            }, "UnityEngine.Time", 0.016f);
        }

        internal void NotifyAllPorts() {
            NotifyPortValueChanged(SmoothDeltaTimePort);
            NotifyPortValueChanged(TimeScalePort);
            NotifyPortValueChanged(RealtimeSinceStartupPort);
            NotifyPortValueChanged(TimeSinceLevelLoadPort);
            NotifyPortValueChanged(TimePort);
            NotifyPortValueChanged(DeltaTimePort);
        }
    }
}