﻿using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using mattatz.Utils;

namespace mattatz {

    public class CPConstructionUpdater : CPParticleUpdater {

        public float radius = 5f;
        public float duration = 3f;

        [SerializeField, Range(0f, 1f)] float t = 0f;
        [Range(3, 7)] public int depth = 4;

        ComputeBuffer delayBuffer;

        [SerializeField] List<CPBound> bounds;
        ComputeBuffer boundsBuffer;
        ComputeBuffer boundsReferencesBuffer;

        protected override void Start() {
            base.Start();
        }

        protected override void Update() {
            base.Update();
        }

        void Animate () {
            t = 0f;
            StartCoroutine(Easing.Ease(duration, Easing.Quadratic.Out, (float tt) => {
            // StartCoroutine(Easing.Ease(duration, Easing.Linear, (float tt) => {
                this.t = tt;
            }, 0f, 1f));
        }

        public override void Dispatch(GPUComputeParticleSystem system) {

            if(boundsBuffer == null) {
                Setup(system);
            }

            UpdateBounds();

            shader.SetFloat("_T", t);
            shader.SetBuffer(dispatchID, "_Bounds", boundsBuffer);
            base.Dispatch(system);
        }

        void UpdateBounds() {
            if (bounds.Count <= 0) return;
        }

        void Clear () {
            if(delayBuffer != null) {
                delayBuffer.Release();
                delayBuffer = null;
            }
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

                var bound = new CPBound(position, size * 0.5f, offsets.ToArray());

                bound.translation = Random.insideUnitSphere * radius;
                bound.rotation *= Quaternion.AngleAxis(Random.Range(0f, 360f), Random.insideUnitSphere.normalized);

                bounds.Add(bound);

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

    }

}



