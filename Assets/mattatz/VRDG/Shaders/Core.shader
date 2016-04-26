Shader "mattatz/Core" {

	Properties {
		_Cube ("Reflection Map", Cube) = "" {}
		_Color ("Albedo", Color) = (0.75, 0.75, 0.8, 1.0)
		_SpecularColor ("Specular", Color) = (0.2, 0.2, 0.2, 1.0)
		_Shininess ("Shininess", Float) = 1.37
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		CGINCLUDE

		#include "UnityCG.cginc"
		#include "Assets/mattatz/Common/Shaders/Random.cginc"
		#include "Assets/mattatz/Common/Shaders/Noise/SimplexNoise3D.cginc"

		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float3 normal : NORMAL;
			float3 viewDir : TEXCOORD0;
			float3 lightDir : TEXCOORD1;
		};
	
		ENDCG

		Pass {
			Tags { "LightMode" = "ForwardBase" }
			Cull Back

			CGPROGRAM
			#pragma target 5.0
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag

			samplerCUBE _Cube;
			float _Shininess;

			float4 _Color;
			float4 _SpecularColor;

			v2f vert (appdata v) {
				v2f OUT;

				OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				OUT.normal = v.normal;
				OUT.viewDir = ObjSpaceViewDir(v.vertex);
				OUT.lightDir = ObjSpaceLightDir(v.vertex);

				return OUT;
			}

			// http://www.slis.tsukuba.ac.jp/~fujisawa.makoto.fu/cgi-bin/wiki/index.php?GLSL%A4%CB%A4%E8%A4%EB%A5%D5%A5%A9%A5%F3%A5%B7%A5%A7%A1%BC%A5%C7%A5%A3%A5%F3%A5%B0
			float4 frag(v2f IN) : SV_Target {
				float3 normal = normalize(IN.normal);
				float3 viewDir = normalize(IN.viewDir);
				float3 lightDir = normalize(IN.lightDir);

				float4 col = _Color;

				float spec = pow(max(0, dot(normalize(lightDir + viewDir), normal)), _Shininess);
				col += spec * _SpecularColor;

				float3 refl = reflect(viewDir, normal);
				col *= texCUBE(_Cube, refl);

				return col;
			}

			ENDCG
		}

	}
}
