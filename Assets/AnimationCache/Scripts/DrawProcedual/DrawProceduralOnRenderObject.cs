using UnityEngine;
using System.Collections;

public class DrawProceduralOnRenderObject : MonoBehaviour
{
    public Material mat;
    public int numIndices;
    public int numInstance = 1;

    public void SetIndicesCount(int num)
    {
        numIndices = num;
    }

    public void SetInstanceCount(int num)
    {
        numInstance = num;
    }

    void OnRenderObject()
    {
        mat.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Triangles, numIndices, numInstance);
    }
}
