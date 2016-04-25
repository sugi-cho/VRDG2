Shader "Unlit/visualizePointMesh"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Assets/CGINC/Quaternion.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float3 normal : TEXCOORD0;
				uint vid : TEXCOORD1;
				half3 bary : TEXCOORD2;
				float wireframe : TEXCOORD3;
				float4 color :TEXCOORD4;
				float4 vertex : SV_POSITION;
			};

			struct TriangleData {
				float3 position;
				float3 velocity;
				float4 rotation;
				float crossFade;
				float wireframe;
			};

			struct VertexData {
				float3 position;
				float3 normal;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			StructuredBuffer<TriangleData> _TData;
			StructuredBuffer<VertexData> _VData;

			v2f vert(appdata v, uint vid : SV_VertexID)
			{
				v2f o = (v2f)0;
				o.vertex = v.vertex;
				o.vid = vid;
				return o;
			}

			[maxvertexcount(18)]
			void geom(point v2f v[1], inout TriangleStream<v2f> triStream) {
				uint vid = v[0].vid;
				for (int num = 0; num < 6; num++) {
					uint tidx = vid * 6 + num;
					TriangleData tData = _TData[tidx];
					for (int i = 0; i < 3; i++) {
						uint idx = tidx * 3 + i;
						VertexData vData = _VData[idx];

						v2f o = (v2f)0;
						// o.vertex = half4(0,0,0,1);
						// o.vertex.z += 1.0;
						// o.vertex.xyz = rotateAngleAxis(o.vertex.xyz, float3(0,-1,0), i*UNITY_PI*2.0 / 3.0);
						// o.vertex.xyz = rotateWithQuaternion(o.vertex.xyz, tData.rotation);
						// o.vertex.xyz *= 0.1;
						// o.vertex.xyz += tData.position;
						// o.vertex.x += tidx;
						// o.vertex.y += num;
						o.vertex = half4(vData.position,1.0);
						o.vertex = mul(UNITY_MATRIX_MVP, o.vertex);
						o.normal = vData.normal;
						o.bary = half3(i == 0,i == 1,i == 2);
						o.wireframe = tData.wireframe;
						o.color = lerp(float4(1,0,0,1), float4(0,0.5,1,1), frac(idx / 622080.0));
						triStream.Append(o);
					}
					triStream.RestartStrip();
				}
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				half3 d = fwidth(i.bary);
				half3 a3 = smoothstep(half3(0,0,0), d*1.5, i.bary);
				half w = 1.0 - min(min(a3.x,a3.y),a3.z);

				return half4(i.normal,1) + w * i.wireframe;
			}
			ENDCG
		}
	}
}
