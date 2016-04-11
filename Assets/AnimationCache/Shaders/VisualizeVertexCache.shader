Shader "Unlit/VisualizeVertexCache"
{
	Properties
	{
		_VCount("vertexCount",Float) = 0
		_Keyframes("keyframeCount",Float) = 0
		_AnimLength("clip length",Float) = 0
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
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _VCount, _Keyframes, _AnimLength;

			StructuredBuffer<uint> _Indices;
			StructuredBuffer<float2> _UV;
			StructuredBuffer<float3> _VertexData;
			StructuredBuffer<float3> _NormalsData;

			v2f vert(uint vid : SV_VertexID)
			{
				float t = fmod(_Time.y * _Keyframes / _AnimLength,_Keyframes);
				float f0 = floor(t);
				float f1 = fmod(f0 + 1,_Keyframes);
				t = frac(t);

				uint idx = _Indices[vid];
				float2 uv = _UV[idx];
				float4 pos = float4(lerp(_VertexData[idx + _VCount*f0], _VertexData[idx + _VCount*f1], t), 1.0);
				float3 normal = lerp(_NormalsData[idx + _VCount*f0], _NormalsData[idx + _VCount*f1], t);

				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, pos);
				o.normal = normal;
				o.uv = uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				if (col.a < 0.5)discard;
				return col;
			}
			ENDCG
		}
	}
}
