Shader "mattatz/Lattice/Line" {

	Properties {
		_Color ("Color", Color) = (0, 0, 0, 1)
	}

	SubShader {

		CGINCLUDE

		#include "UnityCG.cginc"
		#include "Assets/mattatz/Common/Shaders/Random.cginc"
		#include "Assets/mattatz/Common/Shaders/Noise/SimplexNoise3D.cginc"
		#include "./LatticeCommon.cginc"

		struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			float4 color : COLOR;
		};

		fixed4 _Color;

		v2f vert(appdata v) {
			v2f o;

			v.vertex.xyz += wave(v.vertex.xyz);
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

			// float n = snoise(float3(v.uv * 2.0, _T * 0.5));
			// o.color = _Color * saturate(0.85 + n);
			o.color = _Color;
			o.uv = v.uv;

			return o;
		}

		fixed4 frag(v2f IN) : SV_Target {
			// fixed4 col = _Color;
			fixed4 col = IN.color;
			return col;
		}

		ENDCG

		Pass {
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
			LOD 200

			Blend SrcAlpha OneMinusSrcAlpha
			// ZWrite On 
			ZWrite On ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}
