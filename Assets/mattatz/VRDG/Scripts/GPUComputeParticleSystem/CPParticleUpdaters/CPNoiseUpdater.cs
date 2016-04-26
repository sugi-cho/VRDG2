using UnityEngine;
using System.Collections;

using mattatz.Utils;

namespace mattatz {

    public class CPNoiseUpdater : CPParticleUpdater {

        public Vector4 seed = new Vector4(0.3f, 0.15f, 0.31f, -1f);

        public Vector2 scaleRange = new Vector2(0.5f, 2f);
        public float scale = 1f;

        public Vector2 intensityRange = new Vector2(0.01f, 0.5f);
        public float intensity = 1f;

        public Vector2 speedRange = new Vector2(0.05f, 0.5f);
        public float speed = 1f;

        float ticker = 0f;

        protected override void Update() {
            base.Update();

            ticker += Time.deltaTime * speed;
        }

        public override void Dispatch(GPUComputeParticleSystem system) {
            shader.SetFloat("_Time", ticker);
            shader.SetFloat("_Scale", scale);
            shader.SetFloat("_Intensity", intensity);

            base.Dispatch(system);
        }

        public override void OnTrigger(OSCUnit unit) {
            if(unit.buttons[1]) {
                seed += new Vector4(Random.value, Random.value, Random.value, Random.value);
            }
        }

        public override void OnControl(OSCUnit unit) {
            scale = Mathf.Lerp(scaleRange.x, scaleRange.y, unit.sliders[0]);
            intensity = Mathf.Lerp(intensityRange.x, intensityRange.y, unit.sliders[1]);
            speed = Mathf.Lerp(speedRange.x, speedRange.y, unit.sliders[2]);
        }

    }

}


