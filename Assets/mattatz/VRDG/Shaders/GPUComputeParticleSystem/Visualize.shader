Shader "mattatz/GPUBoxParticleSystem/Visualize" {

	Properties {
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_Ambient ("Ambient", Color) = (0.01, 0.01, 0.01, 1)
		_Color ("Color", Color) = (0, 0, 0, 1)
		_Size ("Size", Range(0.0001, 0.2)) = 0.05

		_Emission ("Emission", Float) = 1.0
		_Shininess ("Shininess", Float) = 2.0

		_ClipDistance ("Clip distance", Vector) = (0.0, 0.1, -1, -1)

		_Core ("Core distance", Vector) = (0, 0, 0, -1)
		_ScaleDistance ("Scale distance", Vector) = (0.3, 0.45, -1, -1)
	}

	SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGINCLUDE

			ENDCG

			Pass {
				Tags { "LightMode" = "ForwardBase" }
				Lighting On ZWrite On
			// ZWrite On ZTest Always

			CGPROGRAM

			#pragma multi_compile_fwdbase
			#include "./VisualizeCommon.cginc"
			#pragma vertex vert
			#pragma geometry geom_cube
			#pragma fragment frag

			fixed4 _Ambient;
			fixed4 _Color;

			fixed _Emission;
			fixed _Shininess;

			fixed4 frag(g2f IN) : SV_Target{

				// float dx = abs(IN.uv.x - 0.5);
				// float dy = abs(IN.uv.y - 0.5);
				// float s = smoothstep(0.3, 0.5, dx);
				// float s = smoothstep(0.3, 0.5, dx) * smoothstep(0.3, 0.5, dy);
				float s = 1.0;

				fixed4 col = IN.col * _Color * s;

				float3 normal = IN.normal;
				normal = normalize(normal);
				// normal = normalize(mul(_Object2World, float4(normal, 0.0)).xyz);

				float3 lightDir = normalize(IN.lightDir);
				float3 viewDir = normalize(IN.viewDir);
				float3 halfDir = normalize(lightDir + viewDir);

				float nh = dot(normal, halfDir);
				// float vh = dot(viewDir, halfDir);
				// float nv = dot(normal, viewDir);
				// float nl = dot(normal, lightDir);

				float alpha = acos(nh);
				fixed atten = LIGHT_ATTENUATION(IN);

				float spec = max(0.1, pow(saturate(nh), _Shininess));

				// float3 pl = point_lighting(IN.wpos, normal);
				// col.rgb += pl;

				// col.rgb *= exp(_Emission);
				col.rgb *= spec * atten;

				// return col;
				col.a = 0.0;
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
