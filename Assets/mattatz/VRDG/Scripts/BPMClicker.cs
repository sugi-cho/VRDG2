using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace mattatz {

    public class BPMClicker : MonoBehaviour {

        [Range(1, 240)] public int bpm = 92;
        [Range(1, 16)] public int samples = 4;

        public List<GameObject> synchronizers;

        float nextClock;

        void Start() {
            StartCoroutine(Tick());
        }

        void Update() {
            samples = Mathf.Clamp(samples, 1, 16);
        }

        IEnumerator Tick() {
            yield return 0;

            while(true) {
                yield return new WaitForSeconds(60f / bpm / samples);
                OnClick();
            }
        }

        void OnClick () {
            synchronizers.ForEach(synchronizer => {
                if (!synchronizer.activeSelf) return;

                var ibpm = synchronizer.GetComponent<IBPMSynchronizable>();
                if(ibpm != null) {
                    ibpm.OnClick(bpm, samples);
                } else {
                    synchronizer.SendMessage("OnClick");
                }
            });
        }

    }

}


