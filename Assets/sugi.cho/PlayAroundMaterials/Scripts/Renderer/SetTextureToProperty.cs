using UnityEngine;
using System.Collections;
namespace sugi.cc
{

    public class SetTextureToProperty : MonoBehaviour
    {
        public bool
            toGlobal;
        public Material[] targetMats;
        public Renderer[] targetRenderers;
        public string propertyName = "_Tex";
        public Texture texture;

        void Start() { if (texture != null) SetTexture(texture); }
        public void SetTexture(Texture tex)
        {
            tex.name += "/" + propertyName;
            if (toGlobal)
                Shader.SetGlobalTexture(propertyName, tex);
            foreach (var mat in targetMats)
                mat.SetTexture(propertyName, tex);
            foreach (var r in targetRenderers)
                r.SetTexture(propertyName, tex);
        }
    }
}