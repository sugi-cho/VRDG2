using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace sugi.cc
{
    public partial class Controller
    {
        List<BufferAndCount> shapeDataList = new List<BufferAndCount>();
        int[] shapeIdxs = new int[2];
        public void SetShapeBuffer(ComputeBuffer buffer, int numIndices)
        {
            var bufferData = new BufferAndCount() { buffer = buffer, dataCount = numIndices };
            shapeDataList.Add(bufferData);
            shapeDataList.OrderBy(b => b.dataCount).ToList();
        }

        #region TriangleUpdaters
        void UpdateFall()
        {
            var kernel = compute.FindKernel("GoFall");
            compute.SetInt("_NumData", numTriangles);
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);

        }
        #endregion

        #region VertUpdaters
        void ComputeVertexDataCommon(int kernel, ComputeBuffer targetBuffer)
        {
            compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
            compute.SetBuffer(kernel, "_VData0", vertexDataBuffer);
            compute.SetBuffer(kernel, "_OutVData", targetBuffer);
            compute.Dispatch(kernel, numVertices / 1024, 1, 1);
        }
        void UpdateToTriangle(int idx)
        {
            var kernel = compute.FindKernel("ToTriangle");
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            var targetBuffer = targetVertexBuffers[idx];
            ComputeVertexDataCommon(kernel, targetBuffer);
        }

        void UpdateToShape(int idx)
        {
            var kernel = compute.FindKernel("ToShape");
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            var targetBuffer = targetVertexBuffers[idx];
            var shapeBuffer = shapeDataList[shapeIdxs[idx]].buffer;
            var numObjIndices = shapeDataList[shapeIdxs[idx]].dataCount;

            compute.SetInt("_NumData", numObjIndices);
            compute.SetBuffer(kernel, "_ObjectData", shapeBuffer);
            ComputeVertexDataCommon(kernel, targetBuffer);
        }

        void UpdateToTori(int idx)
        {
            var kernel = compute.FindKernel("ToTori");
            if (kernel == -1)
                Debug.LogError("InValid Kernel name");
            var targetBuffer = targetVertexBuffers[idx];

            compute.SetInt("_NumData", toriData.indices.Length);
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