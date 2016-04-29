using UnityEngine;
using System.Collections;

public class DrawMesh : MonoBehaviour
{
    [SerializeField]
    Mesh mesh;
    [SerializeField]
    Material material;

    public void SetMesh(Mesh m) { mesh = m; }
    public void SetMaterial(Material m) { material = m; }

    // Update is called once per frame
    void Update()
    {
        if (mesh == null || material == null) return;
        Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, gameObject.layer);
    }
}
