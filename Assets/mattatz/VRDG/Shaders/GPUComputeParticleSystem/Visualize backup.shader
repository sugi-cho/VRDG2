Shader "mattatz/GPUBoxParticleSystem/Visualize Backup" {

	Properties {
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_Color("Color", Color) = (0, 0, 0, 1)
		_Size("Size", Range(0.0001, 0.2)) = 0.05

		_Shininess("Shininess", Float) = 2.0
	}

	SubShader {
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass {
			Tags { "LightMode" = "ForwardBase" "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
			Lighting On ZWrite On
			// ZWrite On ZTest Always

			CGPROGRAM

			#include "./VisualizeCommon.cginc"
			#pragma vertex vert
			// #pragma geometry geom_square
			#pragma geometry geom_cube
			#pragma fragment frag

			fixed _Shininess;

			fixed4 frag(g2f IN) : SV_Target {
				fixed4 col = IN.col;

				float3 normal = IN.normal;
				normal = normalize(mul(_ModelMatrix, float4(normal, 0.0)).xyz);

				float3 lightDir = normalize(IN.lightDir);
				float3 viewDir = normalize(IN.viewDir);
				float3 halfDir = normalize(lightDir + viewDir);

				float nh = dot(normal, halfDir);
				float vh = dot(viewDir, halfDir);
				float nv = dot(normal, viewDir);
				float nl = dot(normal, lightDir);

				float alpha = acos(nh);
				fixed atten = LIGHT_ATTENUATION(IN);

				float spec = max(0.0, pow(saturate(nh), _Shininess));
				col.rgb = float3(spec, spec, spec);
				col.rgb *= atten;

				return col;
			}

			ENDCG
		}

		Pass {

			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			ZWrite On ZTest LEqual

			CGPROGRAM

			#define _SHADOW_PARTICLE_ 1
			#pragma multi_compile_shadowcaster
			#include "VisualizeCommon.cginc"
			#pragma vertex vert
			#pragma geometry geom_cube
			#pragma fragment shadow_frag

			float4 shadow_frag (g2f IN) : COLOR {
				SHADOW_CASTER_FRAGMENT(IN)
			}

			ENDCG
		}

	}
}
