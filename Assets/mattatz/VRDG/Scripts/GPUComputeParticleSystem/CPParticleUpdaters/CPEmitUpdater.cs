using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using mattatz.Utils;

namespace mattatz {

    public class CPEmitUpdater : CPParticleUpdater {

        Vector4 seed = new Vector4(7.7f, 1.3f, 13.1f, -1f);
        public float lifetimeSpeed = 1f;
        public Bounds emissionArea;
        public Vector3 force = Vector3.up;

        protected override void Start() {
            base.Start();
        }

        protected override void Update() {
            base.Update();
        }

        void Setup(ComputeBuffer buffer) {
            // GPUParticle[] particles = new GPUParticle[buffer.count];
            // buffer.GetData(particles);
            // var count = particles.Length;
        }

        public override void Init(ComputeBuffer buffer) {
            Clear();
        }

        public override void Dispatch(GPUComputeParticleSystem system) {
            shader.SetVector("_Force", force);
            shader.SetVector("_Seed", seed);
            shader.SetFloat("_DT", Time.deltaTime * lifetimeSpeed);
            shader.SetVector("_EmissionCenter", emissionArea.center);
            shader.SetVector("_EmissionSize", emissionArea.size);

            base.Dispatch(system);
        }

        void Clear () {
        }

        void OnEnable () {
            Clear();
        }

        void OnDisable () {
            Clear();
        }

    }

}


