using UnityEngine;
using UnityEditor;
using System.Collections;

public class VertexCache : MonoBehaviour
{
	public VertexCacheData data;
	public Material mat;

	// Use this for initialization
	void Start()
	{
		data.CreateBuffer();
		mat.SetBuffer("_Indices", data.indicesBuffer);
		mat.SetBuffer("_UV", data.uvBuffer);
		mat.SetBuffer("_VertexData", data.verticesBuffer);
		mat.SetBuffer("_NormalsData", data.normalsBuffer);
		mat.SetFloat("_VCount", data.vertexCount);
		mat.SetFloat("_Keyframes", data.keyFrames);
		mat.SetFloat("_AnimLength", data.animLength);
	}

	void OnDestroy()
	{
		data.ReleaseBuffers();
	}

	// Update is called once per frame
	void OnRenderObject()
	{
		mat.SetPass(0);
		Graphics.DrawProcedural(MeshTopology.Triangles, data.indices.Length);
	}
}
