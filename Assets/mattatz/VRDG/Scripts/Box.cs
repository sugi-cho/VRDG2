using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using mattatz.Utils;

namespace mattatz {

    public class Box : MonoBehaviour {

        [System.Serializable]
        struct SliceData_t {
            public Vector3 position; // (0.0 ~ 1.0)
            public Vector3 offset; // (0.0 ~ 1.0)
            public SliceData_t(Vector3 p, Vector3 o) {
                position = p;
                offset = o;
            }
        }

        [System.Serializable]
        class SliceData {
            public SliceData_t structure;
            public float ticker = 0f;
            public float speed = 1f;
            public float duration;

            public SliceData(Vector3 position, Vector3 offset, float s = 1f, float d = 1f) {
                structure = new SliceData_t(position, offset);
                speed = s;
                duration = d;
            }

            public virtual void Update(float dt) {
                ticker += dt;
                structure.position += structure.offset * dt * speed;
            }
        }

        [SerializeField] Rigidbody body;
        [SerializeField] Collider colr;

        public Color color = Color.white;
        Material material;

        List<SliceData> slices = new List<SliceData>();
        ComputeBuffer buffer;

        Vector3 origin;
        bool extruded;
        float reduction = 0.98f;

        void Start () {
            material = GetComponent<Renderer>().material;
            origin = transform.localPosition;
        }

        public void Reduct (float r = 0.98f) {
            reduction = r;
            transform.localScale *= reduction;
        }

        public void Extrude(float duration = 0.2f) {

            if(!extruded) {
                var lp = transform.localPosition;
                var ls = transform.localScale / reduction;
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

                StartCoroutine(ExtrudeAnim(duration, offsets[Random.Range(0, offsets.Count)]));
            } else {
                StartCoroutine(GoBack(duration));
            }

            extruded = !extruded;
        }

        IEnumerator ExtrudeAnim (float duration, Vector3 offset) {
            yield return 0;

            IgnoreSimulation(true);

            var from = transform.localPosition;
            var to = from + offset;

            StartCoroutine(Easing.Ease(duration, Easing.Exponential.In, (float t) => {
                transform.localPosition = Vector3.Lerp(from, to, t);
            }, 0f, 1f));
        }

        public void GoBackToOrigin (float duration = 1f) {
            StartCoroutine(GoBack(duration));
        }

        IEnumerator GoBack(float duration) {
            yield return 0;

            IgnoreSimulation(true);

            var from = transform.localPosition;
            var q = transform.localRotation;

            float time = 0f;
            while(time < duration) {
                yield return 0;

                time += Time.deltaTime;

                float t = time / duration;
                t = Easing.Quadratic.In(t);

                transform.localPosition = Vector3.Lerp(from, origin, t);
                transform.localRotation = Quaternion.Slerp(q, Quaternion.identity, t);
            }

            transform.localPosition = origin;
            transform.localRotation = Quaternion.identity;
        }

        public void IgnoreSimulation (bool use) {
            body.isKinematic = use;
            colr.enabled = !use;
        }

        public void AddForce (Vector3 f) {
            IgnoreSimulation(false);

            body.AddForce(f);
        }
        
        void Update () {
            material.SetColor("_Color", color);

            if(buffer == null) {
                buffer = new ComputeBuffer(4096, Marshal.SizeOf(typeof(SliceData_t)));
            }

            buffer.SetData(slices.Select(sl => sl.structure).ToArray());
            material.SetBuffer("_Slices", buffer);
            material.SetInt("_SlicesCount", slices.Count);

            for(int i = 0, n = slices.Count; i < n; i++) {
                var sl = slices[i];
                sl.Update(Time.deltaTime);
                if(sl.ticker > sl.duration) {
                    slices.RemoveAt(i);
                    n--;
                }
            }

            if(Input.GetKeyDown(KeyCode.D)) {
                StartCoroutine(AddRandomSliceDelay(Random.Range(0.1f, 0.5f)));
            }

        }

        public IEnumerator AddRandomSliceDelay(float delay = 0.2f, float speed = 3f, float duration = 0.5f) {
            yield return new WaitForSeconds(Mathf.Max(0f, delay));
            AddRandomSlice(speed, duration);
        }

        public void AddRandomSlice(float speed, float duration) {
            var axis = GetRandomAxis();
            var flag = Random.value > 0.5f ? true : false;
            AddSlice(
                (Vector3.one - (flag ? axis : Vector3.zero)),
                flag ? axis : - axis, speed, duration
            );
        }

        public void AddSlice(Vector3 position, Vector3 offset, float speed, float duration) {
            slices.Add(new SliceData(position, offset, speed, duration));
        }

        Vector3 GetRandomAxis() {
            var r = Random.value;
            if (r < 0.333f) return Vector3.right;
            else if (r < 0.666f) return Vector3.up;
            return Vector3.forward;
        }

        void OnDisable() {
            if(buffer != null) {
                buffer.Release();
                buffer = null;
            }
        }

    }

}


