using UnityEngine;
using System.Collections;

namespace mattatz.PostEffects {

    [RequireComponent(typeof(Camera))]
    public class PostEffectBase : MonoBehaviour {

        [SerializeField] protected Shader shader;
        [SerializeField] int pass = -1;
        [SerializeField] protected Material material;

        protected virtual void Start() {
        }

        protected virtual void Update() {
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst) {
            Graphics.Blit(src, dst, material, pass);
        }

        protected virtual void OnEnable() {
            if(material == null) {
                material = new Material(shader);
            }
        }

        protected virtual void OnDestroy () {
            if(material != null) {
                Destroy(material);
                material = null;
            }
        }

    }

}


