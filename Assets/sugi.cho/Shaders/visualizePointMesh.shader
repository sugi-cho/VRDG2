Shader "Unlit/visualizePointMesh"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_SC("scale", Range(0.0,1.0)) = 1
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
				float3 wPos : TEXCOORD5;
				float4 vertex : SV_POSITION;
			};

			struct TriangleData {
				float3 position;
				float3 velocity;
				float4 rotation;
				float crossFade;
				float2 life;
			};

			struct VertexData {
				float3 position;
				float3 normal;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			StructuredBuffer<TriangleData> _TData;
			StructuredBuffer<VertexData> _VData;
			StructuredBuffer<Float> _Dial;
			StructuredBuffer<Float> _Slider;
			uniform float3 _WOffset;
			uniform float _WScale;

			float3 _CamWorld_Pos;
			float3 _CamWorld_Dir;

			float4 _LightColor;
			float _LightIntensity;

			float _SC;

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

					float3 p0 = _VData[tidx * 3 + 0].position;
					float3 p1 = _VData[tidx * 3 + 1].position;
					float3 p2 = _VData[tidx * 3 + 2].position;
					float3 center = (p0 + p1 + p2) / 3.0;

					float3 normal = normalize(cross(p1 - p0, p2 - p0));

					for (int i = 0; i < 3; i++) {
						uint idx = tidx * 3 + i;
						VertexData vData = _VData[idx];

						v2f o = (v2f)0;
						o.normal = vData.normal;
						o.wPos = vData.position;
						o.wPos.xyz = center + saturate(0.4*tData.life.x * distance(tData.life.x, tData.life.y)) * (o.wPos.xyz - center);

						o.wPos *= _WScale;
						o.wPos += _WOffset;

						o.vertex = mul(UNITY_MATRIX_MVP, half4(o.wPos,1));
						o.bary = half3(i == 0,i == 1,i == 2);
						o.wireframe = 0.5;
						o.color = half4(tData.velocity + 0.01, 0);
						triStream.Append(o);
					}

					triStream.RestartStrip();
				}
			}

			fixed4 frag(v2f i) : SV_Target
			{
				half3 d = fwidth(i.bary);
				half3 a3 = smoothstep(half3(0,0,0), d*1.0, i.bary);
				half w = 1.0 - min(min(a3.x,a3.y),a3.z);

				half3 bl = normalize(i.wPos - _CamWorld_Pos.xyz);
				half fr = pow(1+dot(bl, normalize(i.normal)),4.0);

				float3 fl = -_CamWorld_Dir.xyz + float3(0, 0.5, 0);
				float3 col = dot(fl, i.normal) * _LightColor * _LightIntensity;
				col = col * (1-_Dial[6]) + fr;

				col += (normalize(i.color.rgb)+0.5)*_Dial[5];

				return half4(col,0.5) + w * i.wireframe * _Dial[7];
			}
			ENDCG
		}
	}
}
