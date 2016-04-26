Shader "mattatz/GPUParticleSystem/BuoyancyUpdater" {

	Properties {
		_PosTex ("Position Texture (rgb = xyz, a = lifetime)", 2D) = "white" {}
		_VelTex ("Velocity Texture (rgb = vx, vy, vz, a = mass(size))", 2D) = "white" {}
		_RotationTex ("Rotation Texture (quaterion)", 2D) = "white" {}
		_Buoyancy ("Buoyancy", Float) = 1.0
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

		float _Buoyancy;

		f2o buoy(v2f IN) {
			f2o OUT;

			float4 vel = tex2D(_VelTex, IN.uv);
			vel.y += _Buoyancy * unity_DeltaTime.x;

			OUT.color0 = tex2D(_PosTex, IN.uv);
			OUT.color1 = vel;
			OUT.color2 = tex2D(_RotationTex, IN.uv);

			return OUT;
		}

		ENDCG

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment buoy
			ENDCG
		}

	}

}
