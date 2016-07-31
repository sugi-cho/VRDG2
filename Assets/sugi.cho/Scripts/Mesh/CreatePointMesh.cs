using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Linq;
using sugi.cc;

public class CreatePointMesh : MonoBehaviour
{
    public int numPoints = 65000;
    public MeshEvent onCreateMesh;
    public bool overrideBounds;
    public Bounds bounds;
    // Use this for initialization
    void Start()
    {
        var mesh = new Mesh();
        mesh.vertices = Enumerable.Repeat(Vector3.zero, numPoints).ToArray();
        mesh.uv = Enumerable.Range(0, numPoints).Select(i => new Vector2((float)i, (float)i / (float)numPoints)).ToArray();
        mesh.SetIndices(Enumerable.Range(0, numPoints).ToArray(), MeshTopology.Points, 0);
        if (overrideBounds)
            mesh.bounds = bounds;

        onCreateMesh.Invoke(mesh);
    }
}