Shader "mattatz/GPUBoxParticleSystem/Visualize" {

	Properties {
		_MainTex ("Base (RGB)", 2D) = "black" {}
		_Color ("Color", Color) = (0, 0, 0, 1)
		_Size ("Size", Range(0.0001, 0.2)) = 0.05

		_Shininess ("Shininess", Float) = 2.0

		_CameraDistance ("Camera distance", Vector) = (0.0, 0.1, -1, -1)
		_ClipDistance ("Camera clip distance", Vector) = (0.5, 0.95, -1, -1)
		_Core ("Core world position", Vector) = (0, 0, 0, -1)
		_CoreDistance ("Core clip distance", Vector) = (0.3, 0.45, -1, -1)
	}

	SubShader {
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

			fixed4 _Color;

			fixed _Shininess;

			// OSC
			uniform float4 _LightColor;
			uniform float _LightIntensity;

			float3 GetCameraPosition()  { return _WorldSpaceCameraPos; }
			float3 GetCameraForward()   { return -UNITY_MATRIX_V[2].xyz; }

			fixed4 frag(g2f IN) : SV_Target{
				fixed4 col = IN.col * _Color;

				float3 normal = IN.normal;
				normal = normalize(normal);

				// normal : world space normal
				half3 bl = normalize(IN.wpos - GetCameraPosition());
				half fr = pow(max(0.1, 1.0 + dot(bl, normal)), _Shininess);
				// half fr = pow(1.0 + dot(bl, normal), 4.0);
				float3 fl = - GetCameraForward() + float3(0, 0.5, 0);
				col.rgb = dot(fl, normal) * _LightColor * _LightIntensity;
				col.rgb += fr;
				// col = col * (1 - _Dial[6]) + fr;

				/*
				float3 lightDir = normalize(IN.lightDir);
				float3 viewDir = normalize(IN.viewDir);
				float3 halfDir = normalize(lightDir + viewDir);

				float nh = dot(normal, halfDir);

				float3 light = _LightIntensity * _LightColor.rgb;
				float3 spec = light * max(0.1, pow(saturate(nh), _Shininess));
				col.rgb += spec;
				*/

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
