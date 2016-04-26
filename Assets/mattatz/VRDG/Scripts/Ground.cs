using UnityEngine;
using System.Collections;

using mattatz.Utils;

namespace mattatz {

    public class Ground : MonoBehaviour {

        [System.Serializable]
        class NoiseData {
            public Vector3 scale = Vector3.one;
            public float speed = 1f, intensity = 1f;
        }

        [SerializeField] NoiseData noiseData;

        Material material;

        void Start () {
            material = GetComponent<Renderer>().material;
            var mesh = MeshDisperser.Disperse(GetComponent<MeshFilter>().mesh);
            GetComponent<MeshFilter>().mesh = mesh;
        }
        
        void Update () {
            material.SetVector("_NoiseScale", noiseData.scale);
            material.SetFloat("_NoiseSpeed", noiseData.speed);
            material.SetFloat("_NoiseIntensity", noiseData.intensity);
        }

    }

}


