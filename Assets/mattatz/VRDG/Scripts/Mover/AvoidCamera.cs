using UnityEngine;
using System.Collections;

namespace mattatz {

    [RequireComponent (typeof(Rigidbody)) ]
    public class AvoidCamera : MonoBehaviour {
        Rigidbody rdbody;
        Camera cam;

        void Start () {
            cam = Camera.main;
            rdbody = GetComponent<Rigidbody>();
        }
        
        void Update () {
            var center = transform.parent.position;
            var dir = center - transform.position;
            rdbody.AddForce(dir);

            var repulsion = transform.position - cam.transform.position;
            var m = repulsion.magnitude;
            m = Mathf.Max(m, 0.5f);
            rdbody.AddForce(repulsion.normalized * (1f / m));
        }

    }

}


