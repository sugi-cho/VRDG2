using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace mattatz {

    public class MetaballEntity : MonoBehaviour {

        public MetaballRenderer m_renderer;
        public float radius = 0.25f;
        [Range(0.01f, 1.0f)] public float softness = 1.0f;
        public bool negative;
        MetaballRenderer.MetaballData data;

        Vector3 axis;
        public float axisScale = 5f;

        public float speedScale = 1f;
        public float speed = 0.5f;
        public float deceleration = 0.99f;

        float distance = -1f;
        float originRadius;
        float acceleration;
        float velocity;

        void Start () {
            axis = Random.insideUnitSphere.normalized;
            distance = (transform.parent.position - transform.position).magnitude;
            speed *= Random.Range(0.5f, 1.2f);
            originRadius = radius;
        }

        void Update() {
            if(m_renderer != null) {
                data.position = transform.position;
                data.radius = radius;
                data.softness = softness;
                data.negative = negative ? 1.0f : 0.0f;
                m_renderer.AddEntity(data);
            }
        }

        public void SetDistance (float scale) {
            if (distance < 0f) return;

            var len = distance * scale;
            var dir = (transform.position - transform.parent.position);
            transform.position = dir.normalized * len + transform.parent.position;
        }

        public void SetRadius (float scale) {
            radius = scale * originRadius;
        }

        public void AddForce (float acc) {
            acceleration += acc / (radius * 3f);
            axis = Quaternion.AngleAxis((Random.value - 0.5f) * axisScale, Vector3.up) * Quaternion.AngleAxis((Random.value - 0.5f) * axisScale, Vector3.right) * Quaternion.AngleAxis((Random.value - 0.5f) * axisScale, Vector3.forward) * axis;
        }

        void FixedUpdate() {
            var center = transform.parent.position;

            velocity += acceleration * Time.fixedDeltaTime;
            transform.RotateAround(center, axis, velocity * Time.fixedDeltaTime);

            velocity *= deceleration;
            acceleration = 0f;
        }

        void OnDrawGizmos() {
            if (!enabled) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, radius);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.parent.position);
        }
    }

}

