using GeometryGraph.Runtime.Attributes;
using UTime = UnityEngine.Time;

namespace GeometryGraph.Runtime.Graph {
    [GenerateRuntimeNode]
    public partial class TimeNode {
        [Setting, UpdatesProperties(nameof(Time), nameof(DeltaTime))] public bool IsFixed { get; private set; }
        [Setting, UpdatesProperties(nameof(Time), nameof(DeltaTime))] public bool IsUnscaled { get; private set; }
        [Out] public float SmoothDeltaTime { get; private set; }
        [Out] public float TimeScale { get; private set; }
        [Out] public float RealtimeSinceStartup { get; private set; }
        [Out] public float TimeSinceLevelLoad { get; private set; }
        [Out] public float Time { get; private set; }
        [Out] public float DeltaTime { get; private set; }

        [GetterMethod(nameof(SmoothDeltaTime), Inline = true)]
        private float GetSmoothDeltaTime() => Utils.IfNotSerializing(() => UTime.smoothDeltaTime, "UTime.smoothDeltaTime", 0.016f);

        [GetterMethod(nameof(TimeScale), Inline = true)]
        private float GetTimeScale() => Utils.IfNotSerializing(() => UTime.timeScale, "UTime.timeScale", 1.0f);

        [GetterMethod(nameof(RealtimeSinceStartup), Inline = true)]
        private float GetRealtimeSinceStartup() => Utils.IfNotSerializing(() => UTime.realtimeSinceStartup, "UTime.realtimeSinceStartup");

        [GetterMethod(nameof(TimeSinceLevelLoad), Inline = true)]
        private float GetTimeSinceLevelLoad() => Utils.IfNotSerializing(() => UTime.timeSinceLevelLoad, "UTime.timeSinceLevelLoad");

        [GetterMethod(nameof(Time), Inline = true)]
        private float GetTime() {
            return IsFixed switch {
                true when IsUnscaled => Utils.IfNotSerializing(() => UTime.fixedUnscaledTime, "UTime.fixedUnscaledTime"),
                true => Utils.IfNotSerializing(() => UTime.fixedTime, "UTime.fixedTime"),
                false when IsUnscaled => Utils.IfNotSerializing(() => UTime.unscaledTime, "UTime.unscaledTime"),
                false => Utils.IfNotSerializing(() => UTime.time, "UTime.time")
            };
        }

        [GetterMethod(nameof(DeltaTime), Inline = true)]
        private float GetDeltaTime() {
            return IsFixed switch {
                true when IsUnscaled => Utils.IfNotSerializing(() => UTime.fixedUnscaledDeltaTime, "UTime.fixedUnscaledDeltaTime", 0.016f),
                true => Utils.IfNotSerializing(() => UTime.fixedDeltaTime, "UTime.fixedDeltaTime", 0.016f),
                false when IsUnscaled => Utils.IfNotSerializing(() => UTime.unscaledDeltaTime, "UTime.unscaledDeltaTime", 0.016f),
                false => Utils.IfNotSerializing(() => UTime.deltaTime, "UTime.deltaTime", 0.016f)
            };
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