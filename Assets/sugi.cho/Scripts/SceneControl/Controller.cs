using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Events;

namespace sugi.cc
{
    public partial class Controller : MonoBehaviour
    {
        public ComputeShader compute;
        public VertexCacheData toriData;
        public Material drawer;

        public int numVertices = 622080;
        public int numTriangles = 207360;
        public int numTori;
        public int numIcosahedron;
        public int numOctahedron;
        public int numTetrahedron;

        public UnityEvent[] onUpdateTriangles;
        public IntEvent[] onUpdateVertices;

        int toriFrames;

        ComputeBuffer outVertexDataBuffer;
        ComputeBuffer[] targetVertexBuffers;

        ComputeBuffer triangleDataBuffer;
        ComputeBuffer feedbackTriangleDataBuffer;

        ComputeBuffer toriIndicesBuffer;
        ComputeBuffer toriVerticesBuffer;
        ComputeBuffer toriNormalBuffer;

        ComputeBuffer emitPosBuffer;
        ComputeBuffer targetPosBuffer;

        float lifeTime = 10f;
        float timeScale = 1f;
        float time = 0f;

        Vector3 targetPos;

        // Use this for initialization
        void Start()
        {
            Cursor.visible = false;
            var toriTriangles = toriData.indices.Length / 3;
            numIcosahedron = numTriangles / 20;
            numOctahedron = numTriangles / 8;
            numTetrahedron = numTriangles / 4;
            numTori = numTriangles / toriTriangles;

            toriFrames = toriData.keyFrames;

            toriData.CreateBuffer();
            toriIndicesBuffer = toriData.indicesBuffer;
            toriVerticesBuffer = toriData.verticesBuffer;
            toriNormalBuffer = toriData.normalsBuffer;

            targetVertexBuffers = new[] {
                Helper.CreateComputeBuffer<VertexData>(numVertices),
                Helper.CreateComputeBuffer<VertexData>(numVertices)
            };
            outVertexDataBuffer = Helper.CreateComputeBuffer<VertexData>(numVertices);
            triangleDataBuffer = Helper.CreateComputeBuffer<TriangleData>(numTriangles);
            feedbackTriangleDataBuffer = Helper.CreateComputeBuffer<TriangleData>(numTriangles);

            emitPosBuffer = Helper.CreateComputeBuffer<Vector3>(numTriangles);
            targetPosBuffer = Helper.CreateComputeBuffer<Vector3>(numTriangles);

            InitializeData();
        }

        void InitializeData()
        {
            var kernel = compute.FindKernel("InitializeTriangles");
            compute.SetInt("_NumData", numTriangles);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);

            kernel = compute.FindKernel("ToTriangle");
            compute.SetInt("_NumData", numVertices);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.SetBuffer(kernel, "_OutVData", outVertexDataBuffer);
            compute.Dispatch(kernel, numVertices / 1024 + 1, 1, 1);

            drawer.SetBuffer("_VData", outVertexDataBuffer);
            drawer.SetBuffer("_TData", triangleDataBuffer);

        }

        void OnDestroy()
        {
            //Release All Buffers
            new[] { outVertexDataBuffer, targetVertexBuffers[0], targetVertexBuffers[1], triangleDataBuffer, feedbackTriangleDataBuffer, toriIndicesBuffer, toriNormalBuffer, toriVerticesBuffer }.Where(b => b != null).ToList().ForEach(b =>
             {
                 b.Release();
                 b = null;
             });
            toriData.ReleaseBuffers();
        }

        // Update is called once per frame
        void Update()
        {
            var dt = timeScale * Time.deltaTime;
            time += dt;

            compute.SetVector("_Time", new Vector4(time, dt, lifeTime)); //float4とかで、まとめて入れる -> SetFloats使うといい。ちゃんとマニュアル見ないとはまる

            SetTarget(time);
            onUpdateTriangles[tUpdaterIdx].Invoke();
            LifeUpdate();

            onUpdateVertices[vUpdaterIdices[0]].Invoke(0);
            onUpdateVertices[vUpdaterIdices[1]].Invoke(1);
            LerpVertexData();
        }

        void SetTarget(float t)
        {
            t *= 0.5f;
            targetPos.x = Mathf.Sin(t + Mathf.Cos(t));
            targetPos.y = 0.5f + (1f + Mathf.Sin(t * 2.6f + Mathf.Sin(t))) * 0.1f;
            targetPos.z = Mathf.Cos(t * 0.95f);
            targetPos *= 30f + 10f * Mathf.Sin(t * 0.5f);

            compute.SetVector("_TargetPos", targetPos);
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawSphere(targetPos, 1f);
        }

        void LerpVertexData()
        {
            var kernel = compute.FindKernel("LerpVertex");
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.SetBuffer(kernel, "_VData0", targetVertexBuffers[0]);
            compute.SetBuffer(kernel, "_VData1", targetVertexBuffers[1]);
            compute.SetBuffer(kernel, "_OutVData", outVertexDataBuffer);
            compute.Dispatch(kernel, numVertices / 1024 + 1, 1, 1);

            kernel = compute.FindKernel("SetTDataFromVData");
            compute.SetBuffer(kernel, "_VData0", outVertexDataBuffer);
            compute.SetBuffer(kernel, "_TData", feedbackTriangleDataBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);

            drawer.SetBuffer("_VData", outVertexDataBuffer);
            drawer.SetBuffer("_TData", triangleDataBuffer);
        }

        void SetTDataFromVData()
        {
            var kernel = compute.FindKernel("SetTDataFromVData");
            compute.SetBuffer(kernel, "_VData0", outVertexDataBuffer);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);
        }

        struct TriangleData
        {
            public Vector3 position;
            public Vector3 velocity;
            public Quaternion rotation;
            public float crossFade;
            public Vector2 life;
        }

        struct VertexData
        {
            public Vector3 position;
            public Vector3 normal;
        }
    }
}