using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using mattatz.Utils;

namespace mattatz {

    public class CPReinstateUpdater : CPParticleUpdater {

        [System.Serializable]
        struct TRS {
            public Vector3 pos;
            public Quaternion rot;
            public Vector3 scale;
            public TRS(Vector3 p, Quaternion r, Vector3 s) {
                pos = p;
                rot = r;
                scale = s;
            }
        };

        public float duration = 3f;

        ComputeBuffer fromBuffer;
        ComputeBuffer toBuffer;

        [SerializeField, Range(0f, 1f)] float rt = 0f;

        protected override void Start() {
            base.Start();
        }

        protected override void Update() {
            base.Update();
        }

        void Setup(ComputeBuffer buffer) {
            GPUParticle[] particles = new GPUParticle[buffer.count];
            buffer.GetData(particles);

            var count = particles.Length;

            fromBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(TRS)));
            toBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(TRS)));

            var from = new TRS[count];
            var to = new TRS[count];
            var q = Quaternion.identity;
            var s = Vector3.one;

            for(int i = 0; i < count; i++) {
                var p = particles[i];
                from[i] = new TRS(p.pos, p.rot, p.scale);
                to[i] = new TRS(p.origin, q, s);
            }

            fromBuffer.SetData(from);
            toBuffer.SetData(to);

            shader.SetBuffer(0, "_From", fromBuffer);
            shader.SetBuffer(0, "_To", toBuffer);
        }

        void Animate () {
            rt = 0f;
            StartCoroutine(Easing.Ease(duration, Easing.Quadratic.Out, (float t) => {
                rt = t;
            }, 0f, 1f));
        }

        public override void Init(ComputeBuffer buffer) {
            rt = 0f;

            Clear();
            Setup(buffer);
            Animate();
        }

        public override void Dispatch(GPUComputeParticleSystem system) {
            if (fromBuffer == null) Init(system.ParticleBuffer);

            if(fromBuffer != null) {
                shader.SetFloat("_RT", rt);
                base.Dispatch(system);
            }
        }

        void Clear () {
            if(fromBuffer != null) {
                fromBuffer.Release();
                fromBuffer = null;
            }
            if(toBuffer != null) {
                toBuffer.Release();
                toBuffer = null;
            }
        }

        void OnEnable () {
            Clear();
        }

        void OnDisable () {
            Clear();
        }

    }

}


