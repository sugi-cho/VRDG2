using UnityEngine;
using System.Collections;

namespace mattatz {

    public class ParticleUpdater : MonoBehaviour {

        const string _PositionBufferKey = "_PosTex", _VelocityBufferKey = "_VelTex", _RotationBufferKey = "_RotationTex";

        [SerializeField] protected Material material;
        
        protected virtual void Start () {}
        protected virtual void Update () {}

        public virtual void Simulate(MRT read, MRT write) {
            var buffers = read.RenderTextures;
            material.SetTexture(_PositionBufferKey, buffers[0]);
            material.SetTexture(_VelocityBufferKey, buffers[1]);
            material.SetTexture(_RotationBufferKey, buffers[2]);
            write.Render(material);
        }

    }

}


