#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced '_WorldSpaceCameraPos.w' with '1.0'

Shader "Ist/ProceduralModeling/Metaball" {

	Properties {
		_Cube ("Reflection Map", Cube) = "" {}
		_Color ("Albedo", Color) = (0.75, 0.75, 0.8, 1.0)
		_SpecularColor ("Specular", Color) = (0.2, 0.2, 0.2, 1.0)

		_Shininess ("Shininess", Float) = 1.37

		_Clipping ("Clipping", Int) = 2
		_OffsetPosition ("OffsetPosition", Vector) = (0, 0, 0, 0)
		_Scale ("Scale", Vector) = (1, 1, 1, 0)
		_CutoutDistance ("Cutout Distance", Float) = 0.01

		_ZTest ("ZTest", Int) = 4

		[Toggle(ENABLE_DEPTH_OUTPUT)] _DepthOutput ("Depth Output", Float) = 0
	}

	CGINCLUDE

	struct Metaball {
		float3 position;
		float radius;
		float softness;
		float negative;
	};

	int _NumEntities;
	StructuredBuffer<Metaball> _Entities;

	#define MAX_MARCH_STEPS 24

	#include "ProceduralModeling.cginc"

	void initialize(inout raymarch_data R) {
	}

	float map(float3 pg)
	{
		float d = dot(_Scale, 1.0);
		for (int i = 0; i < _NumEntities; ++i) {
			Metaball mb = _Entities[i];
			if (mb.negative) {
				d = soft_max(d, -sdSphere(pg - mb.position, mb.radius), mb.softness);
			}
			else {
				d = soft_min(d, sdSphere(pg - mb.position, mb.radius), mb.softness);
			}
		}
		d = max(d, sdBox(localize(pg), _Scale*0.5));
		return d;
	}

	void posteffect(inout gbuffer_out O, vs_out I, raymarch_data R)
	{
	}

	#include "Framework.cginc"
	ENDCG

	SubShader{
		Tags{ "RenderType" = "Opaque" "DisableBatching" = "True" "Queue" = "Geometry+10" }

		Pass{
			Tags { "LightMode" = "ShadowCaster" }
			Cull Front
			ColorMask 0
			CGPROGRAM
			#pragma vertex vert_shadow
			#pragma fragment frag_shadow
			ENDCG
		}

		Pass {
			Tags { "LightMode" = "ForwardBase" }
			ZTest[_ZTest]
			Cull Back
			CGPROGRAM
			#pragma target 5.0
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag_forward
			#pragma multi_compile ___ UNITY_HDR_ON
			#pragma multi_compile ___ ENABLE_DEPTH_OUTPUT

			samplerCUBE _Cube;
			float _Shininess;

			// http://www.slis.tsukuba.ac.jp/~fujisawa.makoto.fu/cgi-bin/wiki/index.php?GLSL%A4%CB%A4%E8%A4%EB%A5%D5%A5%A9%A5%F3%A5%B7%A5%A7%A1%BC%A5%C7%A5%A3%A5%F3%A5%B0
			float4 frag_forward(vs_out IN, out float depth : SV_Depth) : SV_Target {
				float2 coord = IN.screen_pos.xy / IN.screen_pos.w;
				float3 world_pos = IN.world_pos.xyz;

				raymarch_data rmd;
				UNITY_INITIALIZE_OUTPUT(raymarch_data, rmd);
				rmd.ray_pos = world_pos;

				initialize(rmd);
				raymarching(rmd);

				float3 normal = IN.world_normal;

				//float3 d1 = ddx(world_pos);
				//float3 d2 = ddy(world_pos);
				//normal = normalize(cross(d2, d1));

				if (rmd.total_distance > 0.0) {
					normal = guess_normal(rmd.ray_pos);
				}

				float4 local_pos = mul(_World2Object, float4(rmd.ray_pos, 1.0));
				float3 viewDir = normalize(WorldSpaceViewDir(local_pos));
				float3 lightDir = normalize(WorldSpaceLightDir(local_pos));

				float4 col = _Color;
				// col.rgb *= max(0.1, dot(normalize(lightDir + viewDir), normal));

				// float spec = pow(max(0, dot(normalize(lightDir + viewDir), normal)), _Shininess);
				// col += spec * _SpecularColor;

				// float3 refl = reflect(viewDir, normal);
				// col *= texCUBE(_Cube, refl);

				float4 projected = mul(UNITY_MATRIX_VP, float4(rmd.ray_pos.xyz, 1.0));
				depth = projected.z / projected.w;
				clip(depth);

				return col;
			}

			ENDCG
		}

		Pass {
			Tags { "LightMode" = "Deferred" }
			Stencil {
				Comp Always
				Pass Replace
				Ref 128
			}
			ZTest [_ZTest]
			Cull Back
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag_gbuffer
			#pragma multi_compile ___ UNITY_HDR_ON
			#pragma multi_compile ___ ENABLE_DEPTH_OUTPUT
			ENDCG
		}
	}

	Fallback Off
}
