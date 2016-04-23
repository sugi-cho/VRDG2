using UnityEngine;
using System.Collections;
using System.Linq;
using sugi.cc;

[RequireComponent(typeof(Camera))]
public class CaptureColorDepth : MonoBehaviour
{
	Camera cam;
	[SerializeField]
	RenderTexture colorRt, depthRt;

	public string colorTexturePropertyName = "_MainTex";
	public string depthTexturePropertyName = "_DepthTex";
	public Renderer[] targetRenderers;

	void Start()
	{
		cam = GetComponent<Camera>();
		colorRt = Helper.CreateRenderTexture(cam.pixelWidth, cam.pixelHeight, null, RenderTextureFormat.ARGBFloat);
		depthRt = Helper.CreateRenderTexture(cam.pixelWidth, cam.pixelHeight, null, RenderTextureFormat.Depth);
		cam.SetTargetBuffers(colorRt.colorBuffer, depthRt.depthBuffer);
		foreach (var r in targetRenderers)
		{
			r.SetTexture(colorTexturePropertyName, colorRt);
			r.SetTexture(depthTexturePropertyName, depthRt);
		}
	}
}
