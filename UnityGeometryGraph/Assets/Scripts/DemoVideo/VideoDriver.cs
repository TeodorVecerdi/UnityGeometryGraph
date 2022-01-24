using System.Collections.Generic;
using UnityCommons;
using UnityEngine;

namespace GeometryGraph.Runtime.Testing {
    public class VideoDriver : MonoBehaviour {
        public List<VideoSequence> Sequences;
        public VideoSequence CurrentSequence;
        public int SequenceIndex;
        public bool AutoPlayNext;

        public void Start() {
            // PlayAtIndex(SequenceIndex);
        }

        public void Finish(VideoSequence sequence) {
            if (CurrentSequence != sequence) {
                Debug.LogError("Finish called on wrong sequence");
                return;
            }
            CurrentSequence.Stop();
            Debug.Log($"Finished sequence {CurrentSequence.name}");
            CurrentSequence = null;

            if (AutoPlayNext) {
                PlayNext();
            }
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (CurrentSequence != null) {
                    CurrentSequence.Stop();
                }

                SequenceIndex = 0;
                PlayAtIndex(0);
            }

            if (Input.GetKeyDown(KeyCode.R)) {
                if (CurrentSequence != null) {
                    CurrentSequence.Stop();
                }

                PlayAtIndex(SequenceIndex);
            }
        }

        private void PlayNext() {
            SequenceIndex++;
            if (SequenceIndex >= Sequences.Count) {
                CurrentSequence = null;
                Debug.Log("Finished all sequences");
                return;
            }
            PlayAtIndex(SequenceIndex);
        }

        private void PlayAtIndex(int index) {
            index = index.Clamped(0, Sequences.Count - 1);
            CurrentSequence = Sequences[index];
            CurrentSequence.Enter();
            CurrentSequence.Play(this);
        }
    }
}
