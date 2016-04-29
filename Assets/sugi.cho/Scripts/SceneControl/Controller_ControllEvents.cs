using UnityEngine;
using System.Collections;
namespace sugi.cc
{
    public partial class Controller
    {

        public void OnTrack2Start()
        {
            Debug.Log("track2");
        }

        public void SetTriangleUpdateIdx(float val)
        {
            tUpdaterIdx = GetIndexOfArray(onUpdateTriangles.Length, val);
        }
        public void SetVertices0UpdateIdx(float val)
        {
            vUpdaterIdices[0] = GetIndexOfArray(onUpdateVertices.Length, val);
        }
        public void SetVertices1UpdateIdx(float val)
        {
            vUpdaterIdices[1] = GetIndexOfArray(onUpdateVertices.Length, val);
        }
        public void SetShape0Idx(float val)
        {
            shapeIdices[0] = GetIndexOfArray(shapeDataList.Count, val);
        }
        public void SetShape1Idx(float val)
        {
            shapeIdices[1] = GetIndexOfArray(shapeDataList.Count, val);
        }

        public void SetTimeScale(float val)
        {
            timeScale = 1f / (1f + 10f * val);
        }

        public void SetEmitPosIdx(float val)
        {
            emitUpdateIdx = GetIndexOfArray(posKernelNames.Length, val);
        }

        public void SetTargetPosIdx(float val)
        {
            targetUpdateIdx = GetIndexOfArray(posKernelNames.Length, val);
        }

        int GetIndexOfArray(int length, float val)
        {
            //val 0.0f...1.0f
            var i = Mathf.FloorToInt(length * val);
            return Mathf.Clamp(i, 0, length - 1);
        }
    }
}