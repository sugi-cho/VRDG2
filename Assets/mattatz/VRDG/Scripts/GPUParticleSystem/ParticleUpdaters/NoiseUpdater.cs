using UnityEngine;
using System.Collections;

namespace mattatz
{

    public class NoiseUpdater : ParticleUpdater {

        [System.Serializable]
        public class NoiseData {
            public Vector3 scale = Vector3.one, speed = Vector3.one, intensity = Vector3.one;
        }

        public NoiseData noiseData;

        protected override void Update() {
            base.Update();

            material.SetVector("_NoiseScale", noiseData.scale);
            material.SetVector("_NoiseSpeed", noiseData.speed);
            material.SetVector("_NoiseIntensity", noiseData.intensity);
        }

    }

}


