using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz {

    public class SoundController : MonoBehaviour, IBPMSynchronizable {

        public bool ripple = false;
        public bool click = false;

        [SerializeField] RippleEffect rippleEffect;
        [SerializeField] List<Vector3> positions;
        [SerializeField] GameObject prefab;

        Camera cam;
        List<GameObject> objects;

        void Start () {
            cam = rippleEffect.GetComponent<Camera>();

            objects = positions.Select(p => {
                var go = Instantiate(prefab);
                go.transform.localScale *= 0.5f;
                go.transform.position = p;
                go.transform.parent = transform;
                return go;
            }).ToList();
        }
        
        void Update () {
        }

        public void OnClick(int bpm, int samples) {

            if(click) {
                objects.ForEach(obj => {
                    obj.GetComponent<IBPMSynchronizable>().OnClick(bpm, samples);
                });
            }

            if(ripple) {
                for(int i = 0, n = objects.Count; i < n; i++) {
                    var obj = objects[i];
                    var pos = cam.WorldToViewportPoint(obj.transform.position);
                    if(pos.x >= 0f && pos.x <= 1f && pos.y >= 0f && pos.y <= 1f && pos.z > 0f) {
                        pos.y = 1f - pos.y; // flip y
                        rippleEffect.Emit(pos);
                    }
                }
            }

        }

        void OnDrawGizmos() {
            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            positions.ForEach(p => {
                Gizmos.DrawWireSphere(p, 0.2f);
            });
        }

    }

}


