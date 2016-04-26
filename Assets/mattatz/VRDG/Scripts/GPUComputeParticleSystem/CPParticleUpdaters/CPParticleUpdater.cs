using UnityEngine;
using System.Collections;
using System;

namespace mattatz {

    /*
     * ParticleUpdater for GPUComputeParticleSystem
     * CP means ComPute
     */
    public class CPParticleUpdater : MonoBehaviour, IOSCUnitResponsible {

        protected int dispatchID = 0;
        [SerializeField] protected ComputeShader shader;

        protected const int _Thread = 8;
        protected const string _BufferKey = "_Particles";

        protected virtual void Start () {}
        protected virtual void Update () {}
        public virtual void Init (ComputeBuffer buffer) {}

        public virtual void Dispatch (GPUComputeParticleSystem system) {
            Dispatch(dispatchID, system);
        }

        protected void Dispatch (int id, GPUComputeParticleSystem system) {
            shader.SetBuffer(id, _BufferKey, system.ParticleBuffer);
            shader.Dispatch(id, system.ParticleBuffer.count / _Thread + 1, 1, 1);
        }

        public virtual void OnTrigger(OSCUnit unit) {}
        public virtual void OnControl(OSCUnit unit) {}

    }

}


