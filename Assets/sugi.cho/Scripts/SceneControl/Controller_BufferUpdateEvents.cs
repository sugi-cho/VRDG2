using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace sugi.cc
{
    public partial class Controller
    {

        [SerializeField]
        int tUpdaterIdx = 0;
        [SerializeField]
        int targetUpdateIdx = 0;
        [SerializeField]
        int emitUpdateIdx = 0;
        [SerializeField]
        int[] vUpdaterIdices = new int[2];

        List<BufferAndCount> shapeDataList = new List<BufferAndCount>();
        [SerializeField]
        int[] shapeIdices = new int[2];


        string[] posKernelNames = new[] { "PositionToFloor", "PositionToCube", "PositionToSphere", "PositionToTarget" };

        public void SetShapeBuffer(ComputeBuffer buffer, int numIndices)
        {
            var bufferData = new BufferAndCount() { buffer = buffer, dataCount = numIndices };
            shapeDataList.Add(bufferData);
            shapeDataList = shapeDataList.OrderBy(b => b.dataCount).ToList();
        }

        #region TriangleUpdaters

        public void UpdateStop()
        {
            var kernel = compute.FindKernel("Stop");
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            compute.SetInt("_NumData", numTriangles);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);
        }
        public void UpdateFall()
        {
            var kernel = compute.FindKernel("GoUp");
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            compute.SetInt("_NumData", numTriangles);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);
        }

        public void UpdateTarget()
        {
            var kernel = compute.FindKernel("GoTarget");
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            compute.SetInt("_NumData", numTriangles);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);
        }

        public void UpdateToPos()
        {
            SetTargetPosBuffer();
            var kernel = compute.FindKernel("GotoPos");
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            compute.SetBuffer(kernel, "_Positions", targetPosBuffer);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);
        }

        void LifeUpdate()
        {
            SetEmitPosBuffer();
            var kernel = compute.FindKernel("LifeSpan");
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            compute.SetInt("_NumData", numTriangles);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.SetBuffer(kernel, "_Positions", emitPosBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);
        }

        void SetTargetPosBuffer()
        {
            var kernel = compute.FindKernel(posKernelNames[targetUpdateIdx]);
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            SetPosBuffer(kernel, targetPosBuffer);
        }

        void SetEmitPosBuffer()
        {
            var kernel = compute.FindKernel(posKernelNames[emitUpdateIdx]);
            SetPosBuffer(kernel, emitPosBuffer);
        }

        void SetPosBuffer(int kernel, ComputeBuffer targetBuffer)
        {
            compute.SetInt("_NumData", numTriangles);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.SetBuffer(kernel, "_Positions", targetBuffer);
            compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);
        }

        #endregion

        #region VertUpdaters
        public void ComputeVertexDataCommon(int kernel, ComputeBuffer targetBuffer)
        {
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.SetBuffer(kernel, "_VData0", outVertexDataBuffer);
            compute.SetBuffer(kernel, "_OutVData", targetBuffer);
            compute.Dispatch(kernel, numVertices / 1024, 1, 1);
        }
        public void UpdateToTriangle(int idx)
        {
            var kernel = compute.FindKernel("ToTriangle");
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            var targetBuffer = targetVertexBuffers[idx];
            ComputeVertexDataCommon(kernel, targetBuffer);

        }

        public void UpdateToShape(int idx)
        {
            var kernel = compute.FindKernel("ToShape");
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            var targetBuffer = targetVertexBuffers[idx];
            var shapeBuffer = shapeDataList[shapeIdices[idx]].buffer;
            var numObjIndices = shapeDataList[shapeIdices[idx]].dataCount;

            compute.SetInt("_NumData", numObjIndices);
            compute.SetBuffer(kernel, "_ObjData", shapeBuffer);
            ComputeVertexDataCommon(kernel, targetBuffer);

        }

        public void UpdateToTori(int idx)
        {

            var kernel = compute.FindKernel("ToTori");
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            var targetBuffer = targetVertexBuffers[idx];

            compute.SetInt("_NumData", toriData.indices.Length);
            compute.SetInt("_ToriVCount", toriData.vertexCount);
            compute.SetInt("_ToriFrameCount", toriFrames);
            compute.SetBuffer(kernel, "_ToriIndices", toriIndicesBuffer);
            compute.SetBuffer(kernel, "_ToriVertices", toriVerticesBuffer);
            compute.SetBuffer(kernel, "_ToriNormals", toriNormalBuffer);
            ComputeVertexDataCommon(kernel, targetBuffer);

        }

        #endregion

        [System.Serializable]
        public class BufferAndDataCountEvent : UnityEvent<ComputeBuffer, int> { }
        struct BufferAndCount
        {
            public ComputeBuffer buffer;
            public int dataCount;
        }
    }
}