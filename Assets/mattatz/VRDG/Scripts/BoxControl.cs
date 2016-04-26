using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz {

    public class BoxControl : MonoBehaviour, IBPMSynchronizable {

        enum BoxAxis {
            X, Y, Z
        };

        public bool slice = false;
        public bool extrusion = false;
        [SerializeField, Range(1, 8)] int depth = 4;

        [SerializeField] List<Color> colors;
        [SerializeField] GameObject prefab;
        [SerializeField] List<Box> boxes;

        void Start() {
            Bintree(Vector3.zero, Vector3.one, depth);
            // Octree(Vector3.zero, Vector3.one, 2);
            boxes.ForEach(box => {
                box.Reduct(0.98f);
            });
        }

        void Update() {

            if(Input.GetKeyDown(KeyCode.B)) {
                boxes.ForEach(box => {
                    box.GoBackToOrigin(0.25f);
                });
            }

            if(Input.GetKeyDown(KeyCode.E)) {
                Extrude(0.2f);
            }

            if(Input.GetKeyDown(KeyCode.F)) { 
                AddForce();
            }

        }

        public void AddForce() {
            boxes.ForEach(box => {
                box.AddForce(Random.insideUnitCircle.normalized * Random.Range(30f, 40f));
            });
        }

        public void AddRandomSlice(Vector2 speedRange, Vector2 durationRange) {
            boxes.ForEach(box => {
                box.AddRandomSlice(Random.Range(speedRange.x, speedRange.y), Random.Range(durationRange.x, durationRange.y));
            });
        }

        public void AddRandomSlice(float speed, float duration) {
            boxes.ForEach(box => {
                box.AddRandomSlice(speed, duration);
            });
        }

        public void Extrude(float duration) {
            boxes.ForEach(box => {
                if(Random.value > 0.5f) {
                    box.Extrude(duration);
                }
            });
        }

        void Octree(Vector3 position, Vector3 size, int depth = 0) {

            if (depth <= 0) {
                boxes.Add(CreateBox(position, size));
                return;
            }

            var boxSize = size * 0.5f;
            var xoffset = new Vector3(boxSize.x * 0.5f, 0f, 0f);
            var yoffset = new Vector3(0f, boxSize.y * 0.5f, 0f);
            var zoffset = new Vector3(0f, 0f, boxSize.z * 0.5f);

            Octree(position + xoffset + yoffset + zoffset, boxSize, depth - 1);
            Octree(position + xoffset + yoffset - zoffset, boxSize, depth - 1);
            Octree(position + xoffset - yoffset + zoffset, boxSize, depth - 1);
            Octree(position + xoffset - yoffset - zoffset, boxSize, depth - 1);
            Octree(position - xoffset + yoffset + zoffset, boxSize, depth - 1);
            Octree(position - xoffset + yoffset - zoffset, boxSize, depth - 1);
            Octree(position - xoffset - yoffset + zoffset, boxSize, depth - 1);
            Octree(position - xoffset - yoffset - zoffset, boxSize, depth - 1);
        }

        void Bintree(Vector3 position, Vector3 size, int depth = 0) {

            if (depth <= 0) {
                boxes.Add(CreateBox(position, size));
                return;
            }

            var axis = RandomAxis();

            Vector3 boxSize = Vector3.zero;
            Vector3 offset = Vector3.zero;

            switch(axis) {

                case BoxAxis.X:
                    boxSize = new Vector3(size.x * 0.5f, size.y, size.z);
                    offset = new Vector3(boxSize.x * 0.5f, 0f, 0f);
                    break;

                case BoxAxis.Y:
                    boxSize = new Vector3(size.x, size.y * 0.5f, size.z);
                    offset = new Vector3(0f, boxSize.y * 0.5f, 0f);
                    break;

                case BoxAxis.Z:
                    boxSize = new Vector3(size.x, size.y, size.z * 0.5f);
                    offset = new Vector3(0f, 0f, boxSize.z * 0.5f);
                    break;

            }

            // var dec = offset * 0.1f;
            // boxes.Add(CreateBox(position - offset - dec * 0.5f, boxSize - dec));
            Bintree(position - offset, boxSize, depth - 1);
            Bintree(position + offset, boxSize, depth - 1);
        }

        Box CreateBox(Vector3 lp, Vector3 ls) {
            var go = Instantiate(prefab);
            go.transform.parent = transform;
            go.transform.localPosition = lp;
            go.transform.localScale = ls;
            var box = go.GetComponent<Box>();
            // box.color = colors[Random.Range(0, colors.Count)];
            return box;
        }

        BoxAxis RandomAxis () {
            var r = Random.value;
            if (r < 0.333f) return BoxAxis.X;
            else if (r < 0.666f) return BoxAxis.Y;
            return BoxAxis.Z;
        }

        void OnDrawGizmos () {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        public void OnClick(int bpm, int samples) {
            float next = 60f / bpm / samples;

            if(slice) AddRandomSlice(Random.Range(1f, 4f), Random.Range(next * 0.5f, next));
            if(extrusion) Extrude(Random.Range(next * 0.2f, next * 0.25f));
        }

    }

}


