using UnityEngine;
using System.Collections;
using System.Linq;
using sugi.cc;

public class CreateBufferFromMesh : MonoBehaviour
{
	public Mesh mesh;
	ComputeBuffer buffer;
	public ComputeBufferEvent onCreate;
	public int vertsCount;

	public void CreateBuffer(Mesh mesh)
	{
		var dataArray = new meshData[0];
		for (var i = 0; i < mesh.subMeshCount; i++)
		{
			var indices = mesh.GetIndices(i);
			var newDataArray = indices.Select(idx => new meshData()
			{
				position = mesh.vertices[idx],
				normal = mesh.normals[idx],
			}).ToArray();
			dataArray = Helper.MargeArray(dataArray, newDataArray);
		}
		buffer = Helper.CreateComputeBuffer(dataArray, setData: true);
		onCreate.Invoke(buffer);
		vertsCount = dataArray.Length;
	}

	// Use this for initialization
	void Start()
	{
		if (mesh != null)
			CreateBuffer(mesh);
	}
	void OnDestroy()
	{
		if (buffer == null) return;
		buffer.Release();
		buffer = null;
	}

	struct meshData
	{
		public Vector3 position;
		public Vector3 normal;
	}
}
