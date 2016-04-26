using UnityEngine;
using Random = UnityEngine.Random;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz {

    public class GPUParticleSystem : MonoBehaviour {

        public GameObject System { get { return system; } }
        public MRT ReadMRT { get { return mrts[readIndex]; } }
        public MRT WriteMRT { get { return mrts[writeIndex]; } }

        [SerializeField] protected int vertexCount = 65000;

        [SerializeField, Range(0f, 1f)] protected float emissionRate = 1f;
        [SerializeField] protected Vector3 emissionSphereSize = Vector3.one;

        [SerializeField] protected Material particleDisplayMat, particleUpdateMat;
        [SerializeField] protected List<ParticleUpdater> updaters;

        [SerializeField] protected Vector2 sizeRange = new Vector2(0.3f, 0.6f);
        [SerializeField, Range(0f, 1f)] protected float deceleration = 0.95f;
        [SerializeField, Range(0.1f, 2f)] protected float lifetimeSpeed = 0.5f;

        [SerializeField] protected MRT[] mrts;
        int readIndex = 0;
        int writeIndex = 1;

        protected GameObject system;

        protected const int VERTEXLIMIT = 65000;
        protected const string _PositionBufferKey = "_PosTex", _VelocityBufferKey = "_VelTex", _RotationBufferKey = "_RotationTex";

        protected virtual GameObject Build(int vertCount, out int bufSize) {
            System.Type[] objectType = new System.Type[2];

            objectType[0] = typeof(MeshFilter);
            objectType[1] = typeof(MeshRenderer);

            GameObject go = new GameObject("ParticleMesh", objectType);
            Mesh particleMesh = new Mesh();
            particleMesh.name = vertCount.ToString();

            int vc = Mathf.Min(VERTEXLIMIT, vertCount);
            bufSize = Mathf.CeilToInt(Mathf.Sqrt(vertCount * 1.0f));

            Vector3[] verts = new Vector3[vc];
            Vector3[] normals = new Vector3[vc];
            Vector2[] texcoords = new Vector2[vc];

            int[] indices = new int[vc];

            for (int i = 0; i < vc; i++) {
                int k = i;

                float tx = 1f * (k % bufSize) / bufSize;
                float ty = 1f * (k / bufSize) / bufSize;

                verts[i] = Random.insideUnitSphere;
                normals[i] = Random.insideUnitSphere;
                texcoords[i] = new Vector2(tx, ty);
                indices[i] = i;
            }

            particleMesh.vertices = verts;
            particleMesh.normals = normals;
            particleMesh.uv = texcoords;
            particleMesh.uv2 = texcoords;

            particleMesh.SetIndices(indices, MeshTopology.Points, 0);
            particleMesh.RecalculateBounds();
            var bounds = particleMesh.bounds;
            bounds.size = bounds.size * 300f;
            particleMesh.bounds = bounds;

            go.GetComponent<MeshRenderer>().material = particleDisplayMat;
            go.GetComponent<MeshFilter>().sharedMesh = particleMesh;

            return go;
        }

        protected virtual void Start() {
            int bufSize;
            system = Build(vertexCount, out bufSize);
            system.transform.parent = transform;
            system.transform.localPosition = Vector3.zero;
            system.transform.localScale = Vector3.one;
            system.transform.localRotation = Quaternion.identity;

            mrts = new MRT[2];

            for(int i = 0, n = mrts.Length; i < n; i++) {
                mrts[i] = new MRT(bufSize, bufSize);
            }

            particleUpdateMat.SetFloat("_SimulationTexSize", bufSize);
            particleUpdateMat.SetFloat("_SimulationTexDeltaSize", 1f / bufSize);
            particleUpdateMat.SetFloat("_SimulationParticleCount", bufSize * bufSize);
            SetProps();

            ReadMRT.Render(particleUpdateMat, 0); // init

            // set display mat
            particleDisplayMat.SetTexture(_PositionBufferKey, ReadMRT.RenderTextures[0]);
            particleDisplayMat.SetTexture(_VelocityBufferKey, ReadMRT.RenderTextures[1]);
            particleDisplayMat.SetTexture(_RotationBufferKey, ReadMRT.RenderTextures[2]);
        }

        protected void Update() {
            updaters.ForEach(updater => {
                updater.Simulate(ReadMRT, WriteMRT);
                Swap();
            });

            var buffers = ReadMRT.RenderTextures;
            SetProps();
            particleUpdateMat.SetTexture(_PositionBufferKey, buffers[0]);
            particleUpdateMat.SetTexture(_VelocityBufferKey, buffers[1]);
            particleUpdateMat.SetTexture(_RotationBufferKey, buffers[2]);
            WriteMRT.Render(particleUpdateMat, 1); // update
            Swap();

            particleDisplayMat.SetVector("_SizeRange", sizeRange);
        }

        protected void SetProps () {
            particleUpdateMat.SetFloat("_Deceleration", deceleration);
            particleUpdateMat.SetFloat("_LifetimeSpeed", lifetimeSpeed);
            particleUpdateMat.SetFloat("_EmissionRate", emissionRate);
            particleUpdateMat.SetVector("_EmissionSize", emissionSphereSize);
        }

        void Swap() {
            var tmp = readIndex;
            readIndex = writeIndex;
            writeIndex = tmp;
        }

        protected virtual void OnDestroy() {
            for(int i = 0, n = mrts.Length; i < n; i++) {
                mrts[i].Release();
            }
        }

        protected virtual void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, emissionSphereSize);
        }

    }

}


