Shader "mattatz/GPUParticleSystem/SoundImpactUpdater" {

	Properties {
		_PosTex ("Position Texture (rgb = xyz, a = lifetime)", 2D) = "white" {}
		_VelTex ("Velocity Texture (rgb = vx, vy, vz, a = mass(size))", 2D) = "white" {}
		_RotationTex ("Rotation Texture (quaterion)", 2D) = "white" {}
	}

	SubShader {

		Cull Off ZWrite Off ZTest Always

		CGINCLUDE

		#pragma target 5.0
		#include "UnityCG.cginc"

		struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		struct f2o {
			float4 color0 : COLOR0;
			float4 color1 : COLOR1;
			float4 color2 : COLOR2;
		};

		v2f vert(appdata IN) {
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
			o.uv = IN.uv;
			return o;
		}

		sampler2D _PosTex;
		sampler2D _VelTex;
		sampler2D _RotationTex;

		struct Impact_t {
			float3 position;
			float radius;
			float intensity;
		};

		StructuredBuffer<Impact_t> _Impacts;
		int _ImpactsCount;

		f2o impact (v2f IN) {
			f2o OUT;

			float4 pos = tex2D(_PosTex, IN.uv);
			float4 vel = tex2D(_VelTex, IN.uv);

			for (int i = 0; i < _ImpactsCount; i++) {
				Impact_t im = _Impacts[i];
				float3 dir = pos - im.position;
				float d = length(dir);

				/*
				if (d < im.radius) {
					vel.xyz += normalize(dir) * im.intensity;
				}
				*/

				vel.xyz += normalize(dir) * im.intensity / pow(d, 5);
			}

			OUT.color0 = pos;
			OUT.color1 = vel;
			OUT.color2 = tex2D(_RotationTex, IN.uv);

			return OUT;
		}

		ENDCG

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment impact
			ENDCG
		}

	}

}
