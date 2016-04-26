using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using mattatz.Utils;

namespace mattatz {

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Lattice : MonoBehaviour, IBPMSynchronizable {

        [System.Serializable]
        class NoiseData {
            public float speed;
            public Vector3 scale, intensity;
        };

        [SerializeField, Range(0.02f, 0.5f)] float spacing = 0.1f;

        [SerializeField] Shader lineShader;
        [SerializeField] Shader pointShader;

        [SerializeField] RenderTexture rt;
        [SerializeField] Texture2D cornerTex;
        [SerializeField] Color color = Color.black;
        [SerializeField] NoiseData waveNoiseData;

        List<Vector3> points;

        Material lineMaterial;
        Material pointMaterial;

        Mesh pointMesh;

        [SerializeField, Range(4, 24)] int fineness = 10;
        [SerializeField, Range(0.001f, 0.2f)] float size = 0.035f;
        [SerializeField] float speed = 1f;
        [SerializeField] bool stop = false;
        float ticker = 0f;

        void Start() {
            var mesh = Build();
            GetComponent<MeshFilter>().mesh = mesh;

            pointMesh = new Mesh();
            pointMesh.vertices = mesh.vertices;

            var indices = new int[mesh.vertexCount];
            for (int i = 0; i < mesh.vertexCount; i++) indices[i] = i;
            pointMesh.SetIndices(indices, MeshTopology.Points, 0);

            GetComponent<MeshRenderer>().material = lineMaterial = new Material(lineShader);
            pointMaterial = new Material(pointShader);

            var count = GetSideCount();
            float length = 1f / count;
            pointMaterial.SetFloat("_SideLength", length);
            lineMaterial.SetFloat("_SideLength", length);

            rt = new RenderTexture(count * count, count, 0, RenderTextureFormat.ARGBFloat);
            rt.filterMode = FilterMode.Point;
            rt.Create();
        }

        void Update() {
            if(!stop) ticker += Time.deltaTime * speed;

            pointMaterial.SetTexture("_MainTex", cornerTex);
            pointMaterial.SetFloat("_Size", size);
            pointMaterial.SetInt("_Fineness", fineness);
            pointMaterial.SetFloat("_FinenessScale", 1f / fineness);

            SetProps(lineMaterial);
            SetProps(pointMaterial);

            Graphics.DrawMesh(pointMesh, transform.localToWorldMatrix, pointMaterial, 0);
        }

        void SetProps(Material mat) {
            mat.SetColor("_Color", color);
            mat.SetFloat("_T", ticker);
            mat.SetFloat("_NoiseSpeed", waveNoiseData.speed);
            mat.SetVector("_NoiseScale", waveNoiseData.scale);
            mat.SetVector("_NoiseIntensity", waveNoiseData.intensity);
        }

        int GetSideCount() {
            return Mathf.FloorToInt(1f / spacing);
        }

        Mesh Build() {
            var mesh = new Mesh();

            int count = GetSideCount();
            int dcount = count * count;
            int tcount = count * count * count;

            var vertices = new Vector3[tcount];
            var uvs = new Vector2[tcount];
            var indices = new List<int>();

            Action<int> addForward = (int c) => {
                indices.Add(c);
                indices.Add(c + 1);
            };

            Action<int> addTop = (int c) => {
                indices.Add(c);
                indices.Add(c + count);
            };

            Action<int> addRight = (int c) => {
                indices.Add(c);
                indices.Add(c + dcount);
            };

            var scale = (1f / count);
            var dscale = scale * scale;
            var offset = - Vector3.one * 0.5f;

            for(int x = 0; x < count; x++) {
                bool xlast = x == count - 1;
                var xoffset = x * dcount;
                for(int y = 0; y < count; y++) {
                    bool ylast = y == count - 1;
                    var yoffset = y * count;
                    for(int z = 0; z < count; z++) {
                        bool zlast = z == count - 1;

                        var index = xoffset + yoffset + z;

                        vertices[index] = new Vector3(x, y, z) * scale + offset;
                        uvs[index] = new Vector2(x * scale + z * dscale, y * scale);

                        if (!xlast) addRight(index);
                        if (!ylast) addTop(index);
                        if (!zlast) addForward(index);
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);

            // points = vertices;
            Debug.Log("Lattice corners : " + tcount);

            return mesh;
        }

        void OnDrawGizmos() {
            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;

            if(points != null) {
                points.ForEach(p => {
                    Gizmos.DrawWireSphere(p, 0.01f);
                });
            }
        }

        public void OnClick(int bpm, int samples) {
            float next = 60f / bpm / samples;

            float duration0 = next * 0.4f;
            float duration1 = next * 0.2f;

            StartCoroutine(Easing.Ease(duration0, Easing.Quadratic.Out, duration1, Easing.Linear, (float t) => {
                speed = t;
            }, speed, speed * 3f));

            float intensity = waveNoiseData.intensity.x;

            StartCoroutine(Easing.Ease(duration0, Easing.Quadratic.Out, duration1, Easing.Linear, (float t) => {
                waveNoiseData.intensity = Vector3.one * t;
            }, intensity, intensity * 2f));

        }

    }

}


