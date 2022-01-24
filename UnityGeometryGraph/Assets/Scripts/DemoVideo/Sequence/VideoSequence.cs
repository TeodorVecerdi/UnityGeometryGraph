using TMPro;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public abstract class VideoSequence : MonoBehaviour {
        public Transform CameraTransform;
        public Transform CameraPivotTransform;
        public TextMeshProUGUI MainTitle;
        public TextMeshProUGUI Subtitle;

        public abstract void Enter();
        public abstract void Play(VideoDriver driver);
        public abstract void Stop();
    }
}