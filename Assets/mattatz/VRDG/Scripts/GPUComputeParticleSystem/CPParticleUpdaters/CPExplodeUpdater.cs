using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using mattatz.Utils;

namespace mattatz {

    public class CPExplodeUpdater : CPParticleUpdater {

        [System.Serializable]
        struct Explosion {
            public Vector3 position;
            public float radius;
            public float intensity;
            public Explosion(Vector3 p, float r, float i) {
                position = p;
                radius = r;
                intensity = i;
            }
        };

        [SerializeField] List<Explosion> explosions;

        public Vector2 radiusRange = new Vector2(1f, 2f);
        public Vector2 sizeRange = new Vector2(0.2f, 0.5f);
        public Vector2 intensityRange = new Vector2(5f, 20f);

        [Range(0f, 1f)] public float radius = 1f;
        [Range(0f, 1f)] public float size = 0.5f;
        [Range(0f, 1f)] public float intensity = 0.5f;

        ComputeBuffer explosionBuffer;

        protected override void Update() {
            CheckInit();

            if(Input.GetKeyDown(KeyCode.E)) {
                // Explode(Random.insideUnitSphere, Random.Range(0.1f, 0.5f), 20f);
            }

            base.Update();
        }

        public override void Dispatch(GPUComputeParticleSystem system) {
            CheckInit();

            int count = explosions.Count;
            if(count > 0) { 
                explosionBuffer.SetData(explosions.ToArray());
                shader.SetBuffer(0, "_Explosions", explosionBuffer);
                shader.SetInt("_ExplosionsCount", count);
                base.Dispatch(system);

                explosions.Clear();
            }
        }

        public void Explode () {
            Explode(Random.insideUnitSphere * radius, size, intensity);
        }

        public void Explode(Vector3 p, float radius, float intensity) {
            var exp = new Explosion(p, radius, intensity);
            explosions.Add(exp);
        }

        void CheckInit () {
            if(explosionBuffer == null) {
                explosionBuffer = new ComputeBuffer(32, Marshal.SizeOf(typeof(Explosion)));
                explosionBuffer.SetData(explosions.ToArray());
            }
        }

        void OnDisable () {
            if(explosionBuffer != null) {
                explosionBuffer.Release();
                explosionBuffer = null;
            }
        }

        public override void OnTrigger(OSCUnit unit) {
            if(unit.buttons[1]) {
                Explode();
            }
        }

        public override void OnControl(OSCUnit unit) {
            radius = Mathf.Lerp(radiusRange.x, radiusRange.y, unit.sliders[0]);
            size = Mathf.Lerp(sizeRange.x, sizeRange.y, unit.sliders[1]);
            intensity = Mathf.Lerp(intensityRange.x, intensityRange.y, unit.sliders[2]);
        }

    }

}


