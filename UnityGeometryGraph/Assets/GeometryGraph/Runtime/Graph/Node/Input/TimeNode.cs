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
        private float GetSmoothDeltaTime() => UTime.smoothDeltaTime;
        
        [GetterMethod(nameof(TimeScale), Inline = true)] 
        private float GetTimeScale() => UTime.timeScale;
        
        [GetterMethod(nameof(RealtimeSinceStartup), Inline = true)] 
        private float GetRealtimeSinceStartup() => UTime.realtimeSinceStartup;
        
        [GetterMethod(nameof(TimeSinceLevelLoad), Inline = true)] 
        private float GetTimeSinceLevelLoad() => UTime.timeSinceLevelLoad;

        [GetterMethod(nameof(Time), Inline = true)]
        private float GetTime() {
            return IsFixed switch {
                true when IsUnscaled => UTime.fixedUnscaledTime,
                true => UTime.fixedTime,
                false when IsUnscaled => UTime.unscaledTime,
                false => UTime.time
            };
        }

        [GetterMethod(nameof(DeltaTime), Inline = true)]
        private float GetDeltaTime() {
            return IsFixed switch {
                true when IsUnscaled => UTime.fixedUnscaledDeltaTime,
                true => UTime.fixedDeltaTime,
                false when IsUnscaled => UTime.unscaledDeltaTime,
                false => UTime.deltaTime
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