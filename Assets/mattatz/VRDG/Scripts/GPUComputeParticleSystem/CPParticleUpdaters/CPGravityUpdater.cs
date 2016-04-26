using UnityEngine;
using System.Collections;

namespace mattatz
{

    public class CPGravityUpdater : CPParticleUpdater {

        public Vector3 direction = Vector3.down;
        [Range(0f, 10f)] public float gravity = 9.81f;

        protected override void Update() {
            base.Update();

            // shader.SetVector("_GravityDirection", direction);

            // shader.SetVector("_GravityDirection", direction);
            // shader.SetFloat("_Gravity", gravity);
        }

        public override void Dispatch(GPUComputeParticleSystem system) {
            // shader.SetVector("_GravityDirection", system.transform.TransformDirection(direction));
            // Debug.Log(system.transform.TransformDirection(direction));
            // shader.SetFloat("_Gravity", gravity);
            // shader.SetVector("_GravityDirection", direction);

            // shader.SetVector("_GravityDirection", system.transform.TransformDirection(direction));
            shader.SetVector("_GravityDirection", system.transform.InverseTransformDirection(direction));
            shader.SetFloat("_Gravity", gravity);
            base.Dispatch(system);
        }

    }

}


