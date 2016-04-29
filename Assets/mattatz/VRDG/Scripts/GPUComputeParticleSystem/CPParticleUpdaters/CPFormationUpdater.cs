using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using mattatz.Utils;

namespace mattatz {

    public class CPFormationUpdater : CPParticleUpdater {

        [System.Serializable]
        enum FormationMode {
            Ring,
            Circle,
            Wave
        };

        [SerializeField] FormationMode mode = FormationMode.Ring;

        public Vector2 speedRange = new Vector2(0.2f, 2f);
        public float speed = 1f;

        public Vector2 sizeRange = new Vector2(1f, 4f);
        public float size = 1.5f;

        [Range(0f, 1f)] public float intensity = 0.5f;

        float ticker = 0f;

        protected override void Update() {
            base.Update();
            ticker += Time.deltaTime;
        }

        public override void Dispatch(GPUComputeParticleSystem system) {
            shader.SetFloat("_Size", size);
            shader.SetFloat("_Intensity", intensity);
            shader.SetFloat("_Speed", speed);

            shader.SetFloat("_R", 1f / system.ParticleBuffer.count);
            shader.SetFloat("_Time", ticker);
            shader.SetFloat("_DT", Time.deltaTime * speed);

            base.Dispatch((int)mode, system);
        }

        public override void Init (ComputeBuffer buffer) {
        }

        void Clear () {
        }

        void OnEnable () {
            Clear();
        }

        void OnDisable () {
            Clear();
        }

        public override void OnTrigger(OSCUnit unit) {
            if (unit.buttons[1]) {
                mode = FormationMode.Ring;
            } else if (unit.buttons[2]) {
                mode = FormationMode.Circle;
            } else if (unit.buttons[3]) {
                mode = FormationMode.Wave;
            }
        }

        public override void OnControl(OSCUnit unit) {
            speed = Mathf.Lerp(speedRange.x, speedRange.y, unit.sliders[0]);
            size = Mathf.Lerp(sizeRange.x, sizeRange.y, unit.sliders[1]);
        }

    }

}
