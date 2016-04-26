using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using mattatz.Utils;

namespace mattatz {

    public class Shell : MonoBehaviour, IBPMSynchronizable {

        enum ShellMode {
            Face,
            Local
        };

        public MRT ReadMRT { get { return mrts[readIndex]; } }
        public MRT WriteMRT { get { return mrts[writeIndex]; } }

        [System.Serializable]
        class NoiseData {
            public Vector3 scale = Vector3.one;
            public float intensity = 5f, speed = 1.8f;
        }

        [SerializeField] int subdivisions = 1;
        [SerializeField] float radius = 1f;

        [SerializeField] ShellMode mode = ShellMode.Local;

        [SerializeField] NoiseData clipData;
        [SerializeField, Range(0f, 1f)] float clipAmount = 0.5f;

        [SerializeField] NoiseData extrusionData;
        [SerializeField, Range(0f, 1f)] float extrusionAmount = 0.2f;

        [SerializeField] NoiseData scaleData;
        [SerializeField, Range(0f, 1f)] float scaleAmount = 0.2f;

        float timeOffset = 0.0f;

        [SerializeField] Material updateMaterial;

        [SerializeField] MRT[] mrts;
        int readIndex = 0;
        int writeIndex = 1;
        const string _PositionBufferKey = "_PosTex", _VelocityBufferKey = "_VelTex", _RotationBufferKey = "_RotationTex";

        Camera cam;
        Material material;

        void Start() {
            cam = Camera.main;
            material = GetComponent<Renderer>().material;
            timeOffset = Random.Range(0f, 100f);

            var mesh = OctahedronSphereCreator.Create(subdivisions, radius);
            mesh = Utils.MeshDisperser.Disperse(mesh);

            // set vertices center
            var tangents = new Vector4[mesh.vertexCount];
            for(int i = 0, n = tangents.Length; i < n; i += 3) {
                var v0 = mesh.vertices[i];
                var v1 = mesh.vertices[i + 1];
                var v2 = mesh.vertices[i + 2];
                tangents[i] = tangents[i + 1] = tangents[i + 2] = (v0 + v1 + v2) / 3f;
            }
            mesh.tangents = tangents;

            int count = mesh.triangles.Length / 3;

            mrts = new MRT[2];
            for(int i = 0, n = mrts.Length; i < n; i++) {
                mrts[i] = new MRT(count, 1);
            }
            ReadMRT.Render(updateMaterial, 0); // init

            material.SetTexture(_PositionBufferKey, ReadMRT.RenderTextures[0]);
            material.SetTexture(_VelocityBufferKey, ReadMRT.RenderTextures[1]);
            material.SetTexture(_RotationBufferKey, ReadMRT.RenderTextures[2]);

            GetComponent<MeshFilter>().mesh = mesh;
        }

        void Update() {
            var buffers = ReadMRT.RenderTextures;
            updateMaterial.SetTexture(_PositionBufferKey, buffers[0]);
            updateMaterial.SetTexture(_VelocityBufferKey, buffers[1]);
            updateMaterial.SetTexture(_RotationBufferKey, buffers[2]);
            WriteMRT.Render(updateMaterial, 1); // update
            Swap();

            material.SetInt("_Mode", (int)mode);

            SetNoiseData("C", material, clipData);
            SetNoiseData("E", material, extrusionData);
            SetNoiseData("S", material, scaleData);

            material.SetFloat("_T", timeOffset);
            material.SetFloat("_CT", clipAmount);
            material.SetFloat("_ET", extrusionAmount);
            material.SetFloat("_ST", scaleAmount);

            var vp = cam.WorldToViewportPoint(transform.position);
            vp.y = 1.0f - vp.y;
            material.SetVector("_SwirlCenter", vp);
        }

        void SetNoiseData(string prefix, Material mat, NoiseData data) {
            mat.SetVector("_" + prefix + "NoiseScale", data.scale);
            mat.SetFloat("_" + prefix + "NoiseIntensity", data.intensity);
            mat.SetFloat("_" + prefix + "NoiseSpeed", data.speed);
        }

        void OnDestroy() {
            Destroy(material);
        }

        void Swap() {
            var tmp = readIndex;
            readIndex = writeIndex;
            writeIndex = tmp;
        }

        RenderTexture CreateBuffer (int width, int height) {
            var buffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Repeat;
            buffer.Create();
            return buffer;
        }

        public void OnClick(int bpm, int samples) {
            float next = 60f / bpm / samples;

            // StartCoroutine(Shot(next * 0.75f));

            float duration0 = next * 0.25f;
            float duration1 = next * 0.4f;

            Easing.Ease(duration0, Easing.Exponential.Out, duration1, Easing.Quadratic.In, (float t) => {
                extrusionAmount = t;
            }, 0f, 1f);

        }

    }

}


