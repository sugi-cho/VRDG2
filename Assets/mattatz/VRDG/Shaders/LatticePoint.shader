Shader "mattatz/Lattice/Point" {

	Properties {
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_Color ("Color", Color) = (0, 0, 0, 1)
		_Size ("Size", Range(0.0001, 0.2)) = 0.05

		_Fineness ("Circle fineness", Range(4, 24)) = 10
		_FinenessScale ("Fineness scale", Float) = 0.1
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGINCLUDE

		#pragma target 5.0

		#include "UnityCG.cginc"

		#define PI 3.14159265359
		#define PI2 6.2831853072

		#include "Assets/mattatz/Common/Shaders/Noise/SimplexNoise3D.cginc"
		#include "./LatticeCommon.cginc"

		struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2g {
			float4 pos : POSITION;
			float2 uv : TEXCOORD0;
			float4 col : TEXCOORD1;
			float size : TEXCOORD2;
		};

		struct g2f {
			float4 pos : POSITION;
			float2 uv : TEXCOORD0;
			float4 col : TEXCOORD1;
		};

		sampler2D _MainTex;
		fixed4 _Color;

		fixed _Size;
		int _Fineness;
		float _FinenessScale;

		v2g vert(appdata v) {
			v2g OUT;

			v.vertex.xyz += wave(v.vertex.xyz);

			OUT.pos = mul(_Object2World, v.vertex);
			OUT.col = _Color;
			OUT.uv = v.uv;
			OUT.size = _Size;

			return OUT;
		}

		[maxvertexcount(4)]
		void geom_square(point v2g IN[1], inout TriangleStream<g2f> OUT) {
			float3 up = float3(0, 1, 0);
			float3 look = _WorldSpaceCameraPos - IN[0].pos;
			look.y = 0;
			look = normalize(look);
			float3 right = cross(up, look);

			float size = IN[0].size;
			float halfS = 0.5f * size;

			float4 v[4];
			v[0] = float4(IN[0].pos.xyz + halfS * right - halfS * up, 1.0f);
			v[1] = float4(IN[0].pos.xyz + halfS * right + halfS * up, 1.0f);
			v[2] = float4(IN[0].pos.xyz - halfS * right - halfS * up, 1.0f);
			v[3] = float4(IN[0].pos.xyz - halfS * right + halfS * up, 1.0f);

			float4x4 vp = mul(UNITY_MATRIX_MVP, _World2Object);

			g2f pIn;

			pIn.col = IN[0].col;
			pIn.pos = mul(vp, v[0]);
			pIn.uv = float2(1.0f, 0.0f);
			OUT.Append(pIn);

			pIn.pos = mul(vp, v[1]);
			pIn.uv = float2(1.0f, 1.0f);
			OUT.Append(pIn);

			pIn.pos = mul(vp, v[2]);
			pIn.uv = float2(0.0f, 0.0f);
			OUT.Append(pIn);

			pIn.pos = mul(vp, v[3]);
			pIn.uv = float2(0.0f, 1.0f);
			OUT.Append(pIn);
		};

		[maxvertexcount(84)]
		void geom_circle (point v2g IN[1], inout TriangleStream<g2f> OUT) {
			float3 up = float3(0, 1, 0);
			float3 look = _WorldSpaceCameraPos - IN[0].pos;
			look.y = 0;
			look = normalize(look);
			float3 right = cross(up, look);
			
			float size = IN[0].size;
			up *= size;
			right *= size;
			
			float3 center = IN[0].pos.xyz; 
					
			float4x4 vp = mul(UNITY_MATRIX_MVP, _World2Object);
			
			g2f pIn;
			pIn.col = IN[0].col;
			pIn.uv = float2(0.0f, 0.0f);
			
			for(int i = 0; i < _Fineness; i++) {
			
				float r = (float)i * _FinenessScale * PI2;
				float rn = (fmod(i + 1, _Fineness) * _FinenessScale) * PI2;
			
				float3 p = IN[0].pos.xyz;
				pIn.pos =  mul(vp, float4(p, 1));
				OUT.Append(pIn);
				
				p = IN[0].pos.xyz + cos(r) * right + sin(r) * up;
				pIn.pos =  mul(vp, float4(p, 1));
				OUT.Append(pIn);
				
				p = IN[0].pos.xyz + cos(rn) * right + sin(rn) * up;
				pIn.pos =  mul(vp, float4(p, 1));
				OUT.Append(pIn);
				
				OUT.RestartStrip();
			}
		}

	
		fixed4 frag(g2f IN) : SV_Target {
			// fixed4 col = tex2D(_MainTex, IN.uv);
			fixed4 col = IN.col;
			return col;
		}

		ENDCG

		Pass {
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

			// ZWrite On ZTest Always

			CGPROGRAM
			#pragma vertex vert
			// #pragma geometry geom_square
			#pragma geometry geom_circle
			// #pragma geometry geom_triangle
			#pragma fragment frag
			ENDCG
		}
	}
}
