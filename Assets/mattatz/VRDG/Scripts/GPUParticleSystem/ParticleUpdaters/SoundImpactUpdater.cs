using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace mattatz {

    public class SoundImpactUpdater : ParticleUpdater {

        [System.Serializable]
        class Impact {
            public Impact_t structure;
            public float duration = 1f;
            public float ticker;
            public Impact(Vector3 p, float r, float s, float d = 1f) {
                structure = new Impact_t(p, r, s);
                duration = d;
                ticker = 0f;
            }
        }

        [System.Serializable]
        struct Impact_t {
            public Vector3 position;
            public float radius;
            public float intensity;
            public Impact_t (Vector3 p, float r, float s) {
                position = p;
                radius = r;
                intensity = s;
            }
        };

        [SerializeField] GPUParticleSystem system;
        [SerializeField] List<Impact> impacts;

        ComputeBuffer buffer;

        protected override void Start() {
            base.Start();
        }

        protected override void Update() {
            base.Update();

            if(buffer == null) {
                buffer = new ComputeBuffer(4096, Marshal.SizeOf(typeof(Impact_t)));
            }

            buffer.SetData(impacts.Select(im => im.structure).ToArray());
            material.SetBuffer("_Impacts", buffer);
            material.SetInt("_ImpactsCount", impacts.Count);

            for(int i = 0, n = impacts.Count; i < n; i++) {
                var im = impacts[i];
                im.ticker += Time.deltaTime;
                if(im.ticker > im.duration) {
                    impacts.RemoveAt(i);
                    n--;
                }
            }

            if(Input.GetKeyDown(KeyCode.S)) {
                AddImpact(new Vector3(0f, 10f, 10f), Random.Range(0.1f, 0.2f), Random.Range(3.5f, 3.7f), 0.1f);
            }

        }

        public void AddImpact (Vector3 world, float radius, float intensity, float duration) {
            var impact = new Impact(system.transform.worldToLocalMatrix * world, radius, intensity, duration);
            impacts.Add(impact);
        }

        void OnDisable() {
            if(buffer != null) {
                buffer.Release();
                buffer = null;
            }
        }

        void OnDrawGizmosSelected () {
            Gizmos.color = Color.green;
            Gizmos.matrix = system.transform.localToWorldMatrix;
            impacts.ForEach(im => {
                Gizmos.DrawWireSphere(im.structure.position, im.structure.radius);
            });
        }

    }

}


