using UnityEngine;
using System.Collections;

namespace mattatz.PostEffects {

    public class Lattice : PostEffectBase {

        [SerializeField] Color color = Color.white;
        [SerializeField] float width = 0.05f;
        [SerializeField] int rows = 12;

        protected override void Update() {
            material.SetColor("_Color", color);
            material.SetFloat("_Width", width);
            material.SetFloat("_Rows", rows);

            var ratio = (float)Screen.width / Screen.height;

            var cols = 1f / ratio * rows;
            material.SetFloat("_Cols", cols);
            material.SetFloat("_Ratio", ratio);
        }

    }

}


