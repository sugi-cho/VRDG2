using UnityEngine;
using System.Collections;
using sugi.cc;

namespace NMS.Passage
{
    public class RenderTexture3D : MonoBehaviour
    {
        public int size = 256;
        public RenderTextureFormat format = RenderTextureFormat.ARGBFloat;
        public ComputeShader compute;
        public TextureEvent onCreateTexture;
        public int repeat = 4;
        public string kernelName = "CSMain";

        [SerializeField]
        RenderTexture rt3d;

        // Use this for initialization
        void Start()
        {
            rt3d = new RenderTexture(size, size, 0, format);
            rt3d.isVolume = true;
            rt3d.volumeDepth = size;
            rt3d.enableRandomWrite = true;
            rt3d.wrapMode = TextureWrapMode.Repeat;
            rt3d.Create();
            rt3d.name = this.ToString() + ".rt3d";

            onCreateTexture.Invoke(rt3d);
            Render();
        }

        void OnDestroy()
        {
            if (rt3d == null) return;
            rt3d.Release();
            rt3d = null;
        }

        public void Render()
        {
            var kernel = compute.FindKernel(kernelName);
            compute.SetVector("_TexelSize", new Vector2(1f / size, size));
            compute.SetInt("_Repeat", repeat);
            compute.SetTexture(kernel, "_Rt3d", rt3d);
            compute.Dispatch(kernel, size / 8, size / 8, size / 8);
        }

    }
}