using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class BasicCurvesSequence : VideoSequence {
        public DemoVideoCurves CurveMaker;
        public float HoldDuration = 1f;
        public int Resolution = 32;
        [Title("Line Settings")]
        public float LineTransitionDuration = 0.5f;
        public Vector3 LineStart = Vector3.zero;
        public Vector3 LineStartTo = Vector3.zero;
        public Vector3 LineEnd = Vector3.right + Vector3.forward;
        public Vector3 LineEndTo = Vector3.zero;
        [Title("Circle Settings")]
        public float CircleRadiusTransitionDuration = 0.5f;
        public float CircleResolutionTransitionDuration = 0.5f;
        public float CircleResolutionDelay = 0.5f;
        public float CircleRadius = 0.7f;
        public float CircleRadiusTo = 1.0f;
        public float CircleRadiusTo2 = 0.5f;
        public int CircleResolution = 32;
        public int CircleResolution2 = 8;
        public int CircleResolution3 = 256;
        [Title("QuadBezier Settings")]
        public float QuadBezierResolutionTransitionDuration = 2.0f;
        public Vector3 QuadBezierStart = Vector3.zero;
        public Vector3 QuadBezierControl = Vector3.right;
        public Vector3 QuadBezierEnd = Vector3.right + Vector3.forward;
        public int QuadBezierResolution = 32;
        public int QuadBezierResolution2 = 8;
        public int QuadBezierResolution3 = 256;
        [Title("CubicBezier Settings")]
        public float CubicBezierResolutionTransitionDuration = 2.0f;
        public Vector3 CubicBezierStart = Vector3.zero;
        public Vector3 CubicBezierControlA = Vector3.right;
        public Vector3 CubicBezierControlB = Vector3.right + Vector3.forward;
        public Vector3 CubicBezierEnd = Vector3.right + Vector3.forward * 2;
        public int CubicBezierResolution = 256;
        [Title("Helix Settings")]
        public float HelixTransitionDuration = 2.0f;
        public float HelixRadius = 0.4f;
        public float HelixRadiusTo = 0.3f;
        public float HelixPitch = 0.1f;
        public float HelixPitchTo = 0.06f;
        public float HelixRotations = 4.0f;
        public float HelixRotationsTo = 8.0f;
        public int HelixResolution = 512;

        private Tween tween1;
        private Tween tween2;

        public override void Enter() {
            tween1?.Kill();
            tween2?.Kill();

            CurveMaker.Clear();
            gameObject.SetActive(true);
        }

        public override void Play(VideoDriver driver) {
            Vector3 currentLineStart = LineStart;
            Vector3 currentLineEnd = LineEnd;
            float currentCircleRadius = CircleRadius;
            int currentCircleResolution = CircleResolution;
            int currentQuadBezierResolution = QuadBezierResolution;
            float currentHelixRadius = HelixRadius;
            float currentHelixPitch = HelixPitch;
            float currentHelixRotations = HelixRotations;

            CurveMaker.GenerateLine(LineStart, LineEnd, Resolution);
            MainTitle.text = "Curves";
            Subtitle.text = "Lines";
            tween1 = DOTween.Sequence()
                            // Lines
                            .AppendInterval(HoldDuration)
                            .AppendCallback(() => CurveMaker.GenerateLine(LineStart, LineEnd, Resolution))
                            // .Append(DOVirtual.Vector3(LineStart, LineStartTo, LineTransitionDuration, value => currentLineStart = value))
                            // .Join(DOVirtual.Vector3(LineEnd, LineEndTo, LineTransitionDuration, value => currentLineEnd = value))
                            // .Join(DOVirtual.Float(0.0f, 1.0f, LineTransitionDuration, _ => CurveMaker.GenerateLine(currentLineStart, currentLineEnd, Resolution)))
                            .AppendInterval(HoldDuration)

                            // Circles
                            .AppendCallback(() => {
                                Subtitle.text = "Circles";
                                CurveMaker.GenerateCircle(CircleRadius, CircleResolution);
                            })
                            .Append(DOVirtual.Float(CircleRadius, CircleRadiusTo, CircleRadiusTransitionDuration, value => currentCircleRadius = value))
                            .Join(DOVirtual.Float(0.0f, 1.0f, CircleRadiusTransitionDuration, _ => CurveMaker.GenerateCircle(currentCircleRadius, currentCircleResolution)))
                            .Join(DOVirtual.Int(CircleResolution, CircleResolution2, CircleResolutionTransitionDuration, value => currentCircleResolution = value).SetEase(Ease.Linear))
                            .Append(DOVirtual.Float(CircleRadiusTo, CircleRadiusTo2, CircleRadiusTransitionDuration, value => currentCircleRadius = value))
                            .Join(DOVirtual.Float(0.0f, 1.0f, CircleRadiusTransitionDuration, _ => CurveMaker.GenerateCircle(currentCircleRadius, currentCircleResolution)))
                            .Join(DOVirtual.Int(CircleResolution2, CircleResolution3, CircleResolutionTransitionDuration, value => currentCircleResolution = value).SetEase(Ease.Linear))
                            .AppendInterval(HoldDuration)

                            // QuadBezier
                            .AppendCallback(() => {
                                Subtitle.text = "Quadratic Bezier";
                                CurveMaker.GenerateQuadraticBezier(QuadBezierStart, QuadBezierControl, QuadBezierEnd, QuadBezierResolution);
                            })
                            .Append(DOVirtual.Int(QuadBezierResolution, QuadBezierResolution2, QuadBezierResolutionTransitionDuration, value => currentQuadBezierResolution = value).SetEase(Ease.Linear))
                            .Join(DOVirtual.Float(0.0f, 1.0f, QuadBezierResolutionTransitionDuration, _ => CurveMaker.GenerateQuadraticBezier(QuadBezierStart, QuadBezierControl, QuadBezierEnd, currentQuadBezierResolution)))
                            .Append(DOVirtual.Int(QuadBezierResolution2, QuadBezierResolution3, QuadBezierResolutionTransitionDuration, value => currentQuadBezierResolution = value).SetEase(Ease.Linear))
                            .Join(DOVirtual.Float(0.0f, 1.0f, QuadBezierResolutionTransitionDuration, _ => CurveMaker.GenerateQuadraticBezier(QuadBezierStart, QuadBezierControl, QuadBezierEnd, currentQuadBezierResolution)))
                            .AppendInterval(HoldDuration)

                            // CubicBezier
                            .AppendCallback(() => {
                                Subtitle.text = "Cubic Bezier";
                                CurveMaker.GenerateCubicBezier(CubicBezierStart, CubicBezierControlA, CubicBezierControlB, CubicBezierEnd, CubicBezierResolution);
                            })
                            .AppendInterval(CubicBezierResolutionTransitionDuration)
                            .AppendInterval(HoldDuration)

                            // Helix
                            .AppendCallback(() => {
                                Subtitle.text = "Spiral/Helix";
                                CurveMaker.GenerateHelix(HelixRadius, HelixPitch, HelixRotations, HelixResolution);
                            })
                            .Append(DOVirtual.Float(HelixRotations, HelixRotationsTo, HelixTransitionDuration, value => currentHelixRotations = value))
                            .Join(DOVirtual.Float(HelixRadius, HelixRadiusTo, HelixTransitionDuration, value => currentHelixRadius = value))
                            .Join(DOVirtual.Float(HelixPitch, HelixPitchTo, HelixTransitionDuration, value => currentHelixPitch = value))
                            .Join(DOVirtual.Float(0.0f, 1.0f, HelixTransitionDuration, _ => CurveMaker.GenerateHelix(currentHelixRadius, currentHelixPitch, currentHelixRotations, HelixResolution)))
                            .AppendInterval(HoldDuration)
                            .AppendInterval(HoldDuration)

                ;
            tween2 = DOVirtual.Float(0.0f, 720.0f,
                                     HoldDuration * 7.0f + CircleRadiusTransitionDuration * 2.0f + QuadBezierResolutionTransitionDuration * 2.0f + CubicBezierResolutionTransitionDuration + HelixTransitionDuration,
                                     value => CameraPivotTransform.localEulerAngles = new Vector3(0, value, 0)).SetEase(Ease.Linear);
            tween1.Play();
        }

        public override void Stop() {
            CurveMaker.Clear();
            gameObject.SetActive(false);
        }
    }
}