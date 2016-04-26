Shader "mattatz/PostEffects/Lattice" {

	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Rows ("Rows", Float) = 15.0
		_Cols ("Cols", Float) = 10.0

		_Width ("Line Width", Range(0.0, 1.0)) = 0.1
		_Ratio ("Ratio", Float) = 1.0
	}

	SubShader {
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE

		#include "UnityCG.cginc"

		struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f {
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};

		sampler2D _MainTex;
		float4 _MainTex_TexelSize;

		fixed4 _Color;
		fixed _Rows;
		fixed _Cols;
		fixed _Width;
		fixed _Ratio;

		v2f vert (appdata v) {
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.uv;
			return o;
		}

		fixed4 lattice (sampler2D tex, float2 uv) {
			float dx = 1.0 / _Rows;
			float dy = 1.0 / _Cols;
			float x = fmod(uv.x, dx) / dx;
			float y = fmod(uv.y, dy) / dy;
			float hw = _Width * 0.5;
			float hh = hw * _Ratio;
			if(x < hw || y < hh || x > 1.0 - hw || y > 1.0 - hh) {
				return _Color;
			}
			return tex2D(tex, uv);
		}

		ENDCG

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			fixed4 frag (v2f i) : SV_Target {
				return lattice(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
