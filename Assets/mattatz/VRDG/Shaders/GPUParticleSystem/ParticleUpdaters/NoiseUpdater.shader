Shader "mattatz/GPUParticleSystem/NoiseUpdater" {

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
		#include "Assets/mattatz/Common/Shaders/Noise/SimplexNoise3D.cginc"

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

		float3 _NoiseScale;
		float3 _NoiseSpeed;
		float3 _NoiseIntensity;

		f2o wave(v2f IN) {
			f2o OUT;

			float4 pos = tex2D(_PosTex, IN.uv);
			float4 vel = tex2D(_VelTex, IN.uv);

			float3 force = float3(
				snoise(float3(_Time.x * _NoiseSpeed.x, pos.yz) * _NoiseScale) * _NoiseIntensity.x,
				snoise(float3(pos.x, _Time.x * _NoiseSpeed.y, pos.z) * _NoiseScale) * _NoiseIntensity.y,
				snoise(float3(pos.xy, _Time.x * _NoiseSpeed.z) * _NoiseScale) * _NoiseIntensity.z
			);
			vel.xyz += (force * unity_DeltaTime.x) * vel.w;

			OUT.color0 = pos;
			OUT.color1 = vel;
			OUT.color2 = tex2D(_RotationTex, IN.uv);

			return OUT;
		}

		ENDCG

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment wave
			ENDCG
		}

	}

}
