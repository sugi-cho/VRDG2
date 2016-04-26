using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace mattatz {

    public class MetaballRenderer : MonoBehaviour {

        [System.Serializable]
        public struct MetaballData {
            public Vector3 position;
            public float radius;
            public float softness;
            public float negative;
        }

        public class SortByNegative : IComparer<MetaballData> {
            public int Compare(MetaballData a, MetaballData b) {
                return a.negative.CompareTo(b.negative);
            }
        }

        public int limit = 32;
        int count = 0;

        MetaballData[] entities;
        ComputeBuffer buffer;
        Material material;
        bool sort = false;

        public void AddEntity(MetaballData e) {
            InitializeMembers();
            int i = count++;
            if (i < limit) {
                entities[i] = e;
                sort = sort || e.negative != 0.0f;
            }
        }

        void InitializeMembers() {
            if (entities == null) {
                entities = new MetaballData[limit];
                buffer = new ComputeBuffer(limit, Marshal.SizeOf(typeof(MetaballData)));
                material = GetComponent<Renderer>().sharedMaterial;
            }
        }

        void OnDisable () {
            if (buffer != null) {
                buffer.Release();
                buffer = null;
            }
        }

        void LateUpdate() {
            InitializeMembers();

            if(sort) {
                System.Array.Sort(entities, 0, count, new SortByNegative());
                sort = false;
            }

            buffer.SetData(entities);
            material.SetBuffer("_Entities", buffer);
            material.SetInt("_NumEntities", Mathf.Min(count, limit));
            count = 0;
        }

    }

}

