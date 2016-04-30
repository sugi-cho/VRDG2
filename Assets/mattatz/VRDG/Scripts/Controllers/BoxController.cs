using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using UnityOSC;

using mattatz.Utils;

namespace mattatz {

    public class BoxController : MonoBehaviour {

        [SerializeField] MetaballRenderer rnd;
        Material metaballMat;

        [SerializeField] float duration = 3f;
        [SerializeField] GPUComputeParticleSystem system;

        [SerializeField] Color lightColor = Color.white;
        [SerializeField, Range(0f, 1f)] float lightIntensity = 0.5f;

        bool started = false;

        void Start () {
            metaballMat = rnd.GetComponent<MeshRenderer>().material;
            metaballMat.SetFloat("_Alpha", 0f);
        }

        void Update() {
            // Shader.SetGlobalColor("_LightColor", lightColor);
            // Shader.SetGlobalFloat("_LightIntensity", lightIntensity);
        }

        public void Begin () {
            if (started) return;
            started = true;

            StartCoroutine(Easing.Ease(duration, Easing.Quadratic.Out, (float t) => {
                metaballMat.SetFloat("_Alpha", t);
            }, 0f, 1f));
        }

        public void Replay () {
            started = false;

            StartCoroutine(Easing.Ease(duration, Easing.Quadratic.Out, (float t) => {
                metaballMat.SetFloat("_Alpha", t);
            }, 1f, 0f));
        }

        public void OnTrigger (OSCUnit unit) {
            if (unit.index >= system.updaters.Count) return;

            var updater = system.updaters[unit.index];

            if(unit.buttons[0] != updater.gameObject.activeSelf) {
                updater.gameObject.SetActive(unit.buttons[0]);
            }

            if(unit.buttons[0]) {
                updater.OnTrigger(unit);
            }
        }

        public void OnControl(OSCUnit unit) {
            if (unit.index >= system.updaters.Count) return;

            var updater = system.updaters[unit.index];
            updater.OnControl(unit);
        }

    }

}


