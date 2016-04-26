using UnityEngine;
using System.Collections;
using System.Linq;

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

        delegate void TBufferUpdater();
        delegate void VBufferUpdater(int idx);

        TBufferUpdater[] triUpdaters;
        VBufferUpdater[] vertUpdaters;
        [SerializeField]
        int tUpdaterIdx = 0;
        [SerializeField]
        int v0UpdaterIdx = 0;
        [SerializeField]
        int v1UpdaterIdx = 1;

        int toriFrames;

        ComputeBuffer vertexDataBuffer0;
        ComputeBuffer vertexDataBuffer1;
        ComputeBuffer vertexDataBuffer;
        ComputeBuffer[] targetVertexBuffers;

        ComputeBuffer triangleDataBuffer;

        ComputeBuffer toriIndicesBuffer;
        ComputeBuffer toriVerticesBuffer;
        ComputeBuffer toriNormalBuffer;
        float lifeTime = 10f;


        // Use this for initialization
        void Start()
        {
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

            vertexDataBuffer0 = Helper.CreateComputeBuffer<VertexData>(numVertices);
            vertexDataBuffer1 = Helper.CreateComputeBuffer<VertexData>(numVertices);
            vertexDataBuffer = Helper.CreateComputeBuffer<VertexData>(numVertices);
            triangleDataBuffer = Helper.CreateComputeBuffer<TriangleData>(numTriangles);

            targetVertexBuffers = new[] { vertexDataBuffer0, vertexDataBuffer1 };

            InitializeData();

            triUpdaters = new TBufferUpdater[1] { UpdateFall };
            vertUpdaters = new VBufferUpdater[3] { UpdateToTriangle, UpdateToShape, UpdateToTori };
        }

        void InitializeData()
        {
            var kernel = compute.FindKernel("InitializeTriangles");
            Debug.Log(kernel);
            compute.SetInt("_NumData", numTriangles);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);

            kernel = compute.FindKernel("InitializeVertices");
            Debug.Log(kernel);
            compute.SetInt("_NumData", numVertices);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.SetBuffer(kernel, "_OutVData", vertexDataBuffer);
            compute.Dispatch(kernel, numVertices / 1024 + 1, 1, 1);

            drawer.SetBuffer("_VData", vertexDataBuffer);
            drawer.SetBuffer("_TData", triangleDataBuffer);

        }

        void OnDestroy()
        {
            //Release All Buffers
            new[] { vertexDataBuffer, vertexDataBuffer0, vertexDataBuffer1, triangleDataBuffer, toriIndicesBuffer, toriNormalBuffer, toriVerticesBuffer }.Where(b => b != null).ToList().ForEach(b =>
            {
                b.Release();
                b = null;
            });
        }

        // Update is called once per frame
        void Update()
        {
            compute.SetVector("_Time", new Vector4(Time.timeSinceLevelLoad, Time.deltaTime, lifeTime)); //float4とかで、まとめて入れる

            triUpdaters[tUpdaterIdx]();
            vertUpdaters[v0UpdaterIdx](0);
            vertUpdaters[v1UpdaterIdx](1);
            LerpVertexData();
        }
        void UpdateTriangle(string kernelName)
        {
            var kernel = compute.FindKernel(kernelName);
            if (kernel == -1)
                Debug.LogError("InValid kernel name!! " + kernelName);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);
        }

        void LerpVertexData()
        {
            var kernel = compute.FindKernel("LerpVertex");
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.SetBuffer(kernel, "_VData0", vertexDataBuffer0);
            compute.SetBuffer(kernel, "_VData1", vertexDataBuffer1);
            compute.SetBuffer(kernel, "_OutVData", vertexDataBuffer);
            compute.Dispatch(kernel, numVertices / 1024 + 1, 1, 1);

            drawer.SetBuffer("_vData", vertexDataBuffer);
        }

        void SetTDataFromVData()
        {
            var kernel = compute.FindKernel("SetTDataFromVData");
            compute.SetBuffer(kernel, "_VData0", vertexDataBuffer);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);
        }

        struct TriangleData
        {
            public Vector3 position;
            public Vector3 velocity;
            public Quaternion rotation;
            public float crossFade;
            public float wireframe;
            public float life;
        }

        struct VertexData
        {
            public Vector3 position;
            public Vector3 normal;
        }
    }
}