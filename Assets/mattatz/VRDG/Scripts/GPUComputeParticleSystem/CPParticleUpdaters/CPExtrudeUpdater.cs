using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using mattatz.Utils;

namespace mattatz {

    public class CPExtrudeUpdater : CPParticleUpdater, IBPMSynchronizable {

        public enum ExtrusionMode {
            Spread,
            Axis,
            Physics,
            Explosion,
        };

        public ExtrusionMode mode = ExtrusionMode.Spread;
        [Range(3, 7)] public int depth = 4;

        public Vector2 intensityRange = new Vector2(0.2f, 2.0f);
        public float intensity = 2f;

        public Vector2 speedRange = new Vector2(0.2f, 2f);
        public float speed = 1f;

        public float force = 1f;
        [Range(0f, 1f)] public float t = 1f;

        [SerializeField] List<CPBound> bounds;
        ComputeBuffer boundsBuffer;
        ComputeBuffer boundsReferencesBuffer;

        int axis = 0;

        void Bintree(Vector3 position, Vector3 size, int depth = 0) {

            if (depth <= 0) {
                var lp = position;
                var ls = size;
                var hls = ls * 0.5f;

                var epsilon = new Vector3(float.Epsilon, float.Epsilon, float.Epsilon);
                var max = lp + hls + epsilon;
                var min = lp - hls - epsilon;

                var offsets = new List<Vector3>();
                if(max.x >= 0.5f) offsets.Add(Vector3.right * ls.x);
                if(max.y >= 0.5f) offsets.Add(Vector3.up * ls.y);
                if(max.z >= 0.5f) offsets.Add(Vector3.back * ls.z);
                if(min.x <= -0.5f) offsets.Add(Vector3.left * ls.x);
                if(min.y <= -0.5f) offsets.Add(Vector3.down * ls.y);
                if(min.z <= -0.5f) offsets.Add(Vector3.forward * ls.z);

                if (offsets.Count <= 0) return;

                var c = offsets.Count;
                bounds.Add(new CPBound(position, size * 0.5f, offsets.ToArray()));

                return;
            }

            Vector3 boxSize = Vector3.zero;
            Vector3 offset = Vector3.zero;

            float rnd = Random.value;
            if(rnd < 0.333f) {
                boxSize = new Vector3(size.x * 0.5f, size.y, size.z);
                offset = new Vector3(boxSize.x * 0.5f, 0f, 0f);
            } else if(rnd < 0.666f) {
                boxSize = new Vector3(size.x, size.y * 0.5f, size.z);
                offset = new Vector3(0f, boxSize.y * 0.5f, 0f);
            } else {
                boxSize = new Vector3(size.x, size.y, size.z * 0.5f);
                offset = new Vector3(0f, 0f, boxSize.z * 0.5f);
            }

            Bintree(position - offset, boxSize, depth - 1);
            Bintree(position + offset, boxSize, depth - 1);
        }

        void Setup(GPUComputeParticleSystem system) {
            var buffer = system.ParticleBuffer;
            GPUParticle[] particles = new GPUParticle[buffer.count];
            buffer.GetData(particles);

            var count = particles.Length;

            bounds = new List<CPBound>();
            Bintree(Vector3.zero, Vector3.one, depth);
            boundsBuffer = new ComputeBuffer(bounds.Count, Marshal.SizeOf(typeof(CPBound_t)));
            boundsBuffer.SetData(bounds.Select(b => b.Structure()).ToArray());

            boundsReferencesBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(int)));

            var kernel = shader.FindKernel("Octree");
            shader.SetBuffer(kernel, "_Bounds", boundsBuffer);
            shader.SetInt("_BoundsCount", bounds.Count);
            shader.SetBuffer(kernel, "_BoundsReferences", boundsReferencesBuffer);
            Dispatch(kernel, system);

            shader.SetBuffer(0, "_BoundsReferences", boundsReferencesBuffer);
        }

        protected override void Update () {
            // UpdateBounds();
        }

        void UpdateBounds () {
            if (bounds.Count <= 0) return;

            switch(mode) {
                case ExtrusionMode.Spread:
                    bounds.ForEach(b => {
                        float n = Mathf.PerlinNoise(b.pos.x, b.pos.y + Time.timeSinceLevelLoad * speed);
                        b.Spread(intensity * n, t);
                    });
                    break;

                case ExtrusionMode.Axis:
                    bounds.ForEach(b => {
                        float n = Mathf.PerlinNoise(b.pos.x, b.pos.y + Time.timeSinceLevelLoad * speed);
                        b.MoveAlongAxis(axis, intensity * n, t);
                    });
                    break;

                case ExtrusionMode.Physics:
                    bounds.ForEach(b => {
                        b.AddForce(b.pos * force, Time.deltaTime);
                    });
                    break;

                case ExtrusionMode.Explosion:

                    if(Input.GetKeyDown(KeyCode.E)) {
                        var found = bounds.Find(b => !b.gravity);
                        if(found != null) found.gravity = true;
                    }

                    break;

            }

            boundsBuffer.SetData(bounds.Select(b => b.Structure()).ToArray());
        }

        public override void Dispatch(GPUComputeParticleSystem system) {

            if(boundsBuffer == null) {
                Setup(system);
            }

            UpdateBounds();

            shader.SetBuffer(dispatchID, "_Bounds", boundsBuffer);
            shader.SetFloat("_T", (mode == ExtrusionMode.Physics || mode == ExtrusionMode.Explosion) ? 1f : t);
            base.Dispatch(system);
        }

        public override void Init(ComputeBuffer buffer) {
            base.Init(buffer);

            if(bounds.Count > 0) {
                bounds.ForEach(b => {
                    b.Reset();
                });
            }
        }

        public void OnClick(int bpm, int samples) {
            axis++;

            float next = 60f / bpm / samples;
            float hn = next * 0.5f;
            StartCoroutine(Easing.Ease(hn, Easing.Exponential.Out, (float tt) => { t = tt; }, 0f, 1f, () => {
                StartCoroutine(Easing.Ease(hn, Easing.Quadratic.Out, (float tt) => { t = tt; }, 1f, 0f)); 
            }));
        }

        void Clear () {
            if(boundsBuffer != null) {
                boundsBuffer.Release();
                boundsBuffer = null;
            }
            if(boundsReferencesBuffer != null) {
                boundsReferencesBuffer.Release();
                boundsReferencesBuffer = null;
            }
        }

        void OnEnable () {
            Clear();
        }

        void OnDisable () {
            Clear();
        }

        public override void OnTrigger(OSCUnit unit) {
            if (unit.buttons[1]) {
                mode = ExtrusionMode.Spread;
            } else if (unit.buttons[2]) {
                mode = ExtrusionMode.Axis;
            } else if (unit.buttons[3]) {
                mode = ExtrusionMode.Physics;
            }
        }

        public override void OnControl(OSCUnit unit) {
            intensity = Mathf.Lerp(intensityRange.x, intensityRange.y, unit.sliders[0]);
            speed = Mathf.Lerp(speedRange.x, speedRange.y, unit.sliders[1]);
        }

    }

}
