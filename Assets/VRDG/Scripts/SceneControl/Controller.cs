using UnityEngine;
using System.Collections;
using System.Linq;
using sugi.cc;

public class Controller : MonoBehaviour
{
	public ComputeShader compute;
	public VertexCacheData toriData;
	public Material drawer;

	public int numVertices = 622080;
	public int numTriangles = 207360;
	public int numTori;
	public int numIcosahedron;
	public int numOctahedron;
	public int numTetrahedron;

	ComputeBuffer vertexDataBuffer0;
	ComputeBuffer vertexDataBuffer1;
	ComputeBuffer vertexDataBuffer;

	ComputeBuffer triangleDataBuffer;

	ComputeBuffer toriIndicesBuffer;
	ComputeBuffer toriVerticesBuffer;
	ComputeBuffer toriNormalBuffer;


	// Use this for initialization
	void Start()
	{
		var toriTriangles = toriData.indices.Length / 3;
		numIcosahedron = numTriangles / 20;
		numOctahedron = numTriangles / 8;
		numTetrahedron = numTriangles / 4;
		numTori = numTriangles / toriTriangles;

		toriData.CreateBuffer();
		toriIndicesBuffer = toriData.indicesBuffer;
		toriVerticesBuffer = toriData.verticesBuffer;
		toriNormalBuffer = toriData.normalsBuffer;

		vertexDataBuffer0 = Helper.CreateComputeBuffer<VertexData>(numVertices);
		vertexDataBuffer1 = Helper.CreateComputeBuffer<VertexData>(numVertices);
		vertexDataBuffer = Helper.CreateComputeBuffer<VertexData>(numVertices);
		triangleDataBuffer = Helper.CreateComputeBuffer<TriangleData>(numTriangles);

		InitializeData();
	}

	void InitializeData()
	{
		var kernel = compute.FindKernel("InitializeTriangles");
		Debug.Log(kernel);
		compute.SetInt("_NumData", numTriangles);
		compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
		compute.Dispatch(kernel, numTriangles / 1024 + 1, 1, 1);

		kernel = compute.FindKernel("InitializeVertices");
		Debug.Log(kernel);
		compute.SetInt("_NumData", numVertices);
		compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
		compute.SetBuffer(kernel, "_OutVData", vertexDataBuffer);
		compute.Dispatch(kernel, numVertices / 1024 + 1, 1, 1);

		drawer.SetBuffer("_VData", vertexDataBuffer);
		drawer.SetBuffer("_TData", triangleDataBuffer);

		vertexDataBuffer.GetData(vds);
		Debug.Log(vds[5].normal);
	}
	VertexData[] vds = new VertexData[21];

	void OnDestroy()
	{
		new[] { vertexDataBuffer, vertexDataBuffer0, vertexDataBuffer1, triangleDataBuffer, toriIndicesBuffer, toriNormalBuffer, toriVerticesBuffer }.Where(b => b != null).ToList().ForEach(b =>
		{
			b.Release();
			b = null;
		});
	}

	void OnDrawGizmos()
	{
		for (var n = 0; n < vds.Length / 3; n++)
		{
			var p0 = vds[n * 3 + 0].position;
			var p1 = vds[n * 3 + 1].position;
			var p2 = vds[n * 3 + 2].position;
			Gizmos.DrawLine(p0, p1);
			Gizmos.DrawLine(p1, p2);
			Gizmos.DrawLine(p2, p0);
		}
	}

	// Update is called once per frame
	void Update()
	{

	}

	void UpdateVertexData()
	{
		var kernel = compute.FindKernel("LerpVertex");
		compute.SetBuffer(kernel, "_TData", triangleDataBuffer);
		compute.SetBuffer(kernel, "_VData0", vertexDataBuffer0);
		compute.SetBuffer(kernel, "_VData1", vertexDataBuffer1);
		compute.SetBuffer(kernel, "_VData", vertexDataBuffer);
		compute.Dispatch(kernel, numVertices / 1024 + 1, 1, 1);
		drawer.SetBuffer("_vData", vertexDataBuffer);
	}

	struct TriangleData
	{
		public Vector3 position;
		public Vector3 velocity;
		public Quaternion rotation;
		public float crossFade;
		public float wireframe;
	}

	struct VertexData
	{
		public Vector3 position;
		public Vector3 normal;
	}
}
