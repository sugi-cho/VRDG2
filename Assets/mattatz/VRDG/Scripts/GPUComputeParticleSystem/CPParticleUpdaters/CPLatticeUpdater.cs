using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using mattatz.Utils;

namespace mattatz {

    public class CPLatticeUpdater : CPParticleUpdater {

        [System.Serializable]
        struct State {
            public Vector3 from;
            public Vector3 to;
            public float time;
            public bool flag;
            public State(Vector3 from, Vector3 to, float time, bool flag) {
                this.from = from;
                this.to = to;
                this.time = time;
                this.flag = flag;
            }
        };

        const float _RatioMax = 0.2f;
        [Range(0f, _RatioMax)] public float ratio = 0.1f;

        public Vector2 speedRange = new Vector2(0.2f, 2f);
        public float speed = 1f;

        public Vector2 scaleRange = new Vector2(2f, 10f);
        public float scale = 1f;

        ComputeBuffer stateBuffer;

        public override void Dispatch(GPUComputeParticleSystem system) {
            if (stateBuffer == null) {
                Init(system.ParticleBuffer);
            }

            shader.SetFloat("_DT", Time.deltaTime * speed);
            shader.SetFloat("_Scale", scale);
            base.Dispatch(system);
        }

        public override void Init (ComputeBuffer buffer) {
            if (stateBuffer != null) Clear();

            GPUParticle[] particles = new GPUParticle[buffer.count];
            buffer.GetData(particles);

            var count = particles.Length;
            var step = 1f / Mathf.Pow(count, 1f / 3f);

            stateBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(State)));

            var states = new State[count];
            for (int i = 0; i < count; i++) {
                var p = particles[i];
                states[i] = new State(p.pos, p.pos, Random.value, Random.value < ratio ? true : false);
            }
            stateBuffer.SetData(states);

            shader.SetFloat("_Step", step);
            shader.SetBuffer(0, "_States", stateBuffer);
        }

        void Clear () {
            if(stateBuffer != null) {
                stateBuffer.Release();
                stateBuffer= null;
            }
        }

        void OnEnable () {
            Clear();
        }

        void OnDisable () {
            Clear();
        }

        public override void OnTrigger(OSCUnit unit) {
            if(unit.buttons[1]) {
                Clear();
            }
        }

        public override void OnControl(OSCUnit unit) {
            speed = Mathf.Lerp(speedRange.x, speedRange.y, unit.sliders[0]);
            scale = Mathf.Lerp(scaleRange.x, scaleRange.y, unit.sliders[1]);
            ratio = Mathf.Lerp(0f, _RatioMax, unit.sliders[2]);
        }

    }

}
