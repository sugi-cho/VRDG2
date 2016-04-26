using UnityEngine;
using System.Collections;

namespace mattatz {

    public class Rotater : MonoBehaviour {

        [SerializeField] float speed = 5f;

        void Start () {
        }
        
        void Update () {
        }

        void FixedUpdate() {
            transform.rotation *= Quaternion.AngleAxis(Time.fixedDeltaTime * speed, Vector3.up);
            transform.rotation *= Quaternion.AngleAxis(Time.fixedDeltaTime * speed, Vector3.right);
        }

    }

}


