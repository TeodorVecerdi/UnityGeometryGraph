using DG.Tweening;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class CurveThicknessSequence : VideoSequence {
        public GeometryGraph Graph;
        public float Delay = 0.5f;
        public float Hold = 2.0f;
        public float Duration = 1.0f;
        public float StartThickness = 0.03f;
        public float EndThickness = 0.05f;
        public float EndThickness2 = 0.02f;
        private Tween tween1;
        private Tween tween2;

        public override void Enter() {
            tween1?.Kill();
            tween2?.Kill();
            gameObject.SetActive(true);
        }

        public override void Play(VideoDriver driver) {
            Graph.SetPropertyFloatValue("_Thickness", StartThickness);
            Graph.Evaluate();

            MainTitle.text = "Curves";
            Subtitle.text = "Solidify Curve";

            tween1 = DOTween.Sequence()
                            .AppendInterval(Delay)
                            .Append(DOVirtual.Float(StartThickness, EndThickness, Duration, value => {
                                Graph.SetPropertyFloatValue("_Thickness", value);
                                StartCoroutine(Graph.EvaluateAsync(true, null));
                            }))
                            .AppendInterval(Hold)
                            .Append(DOVirtual.Float(EndThickness, EndThickness2, Duration, value => {
                                Graph.SetPropertyFloatValue("_Thickness", value);
                                StartCoroutine(Graph.EvaluateAsync(true, null));
                            }))
                            .AppendInterval(Hold);
            tween2 = DOVirtual.Float(0.0f, 360.0f, Delay + Hold * 2.0f + Duration * 2.0f,
                                     value => CameraPivotTransform.localEulerAngles = new Vector3(0, value, 0));
            tween1.Play();
        }

        public override void Stop() {
            gameObject.SetActive(false);
        }
    }
}