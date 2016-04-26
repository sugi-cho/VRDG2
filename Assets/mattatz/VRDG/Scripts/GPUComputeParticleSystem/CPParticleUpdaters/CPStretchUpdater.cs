using UnityEngine;
using System.Collections;

namespace mattatz {

    public class CPStretchUpdater : CPParticleUpdater, IBPMSynchronizable {

        [Range(0f, 1f)] public float t = 1f;

        public Vector2 intensityRange = new Vector2(0.5f, 1f);
        public float intensity = 1f;

        public float scale = 0.1f;

        protected override void Update() {
            base.Update();

            shader.SetFloat("_T", t);
            shader.SetFloat("_Intensity", intensity);
            shader.SetFloat("_Scale", scale);
        }

        public void OnClick(int bpm, int samples) {
        }

    }

}


