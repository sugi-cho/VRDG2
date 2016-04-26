Shader "mattatz/Box"
{
	Properties {
		_Color ("Color", Color)	= (1, 1, 1, 1)
		_Shininess ("Shininess", Float) = 32.0
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		CGINCLUDE

		#include "UnityCG.cginc"
		#include "AutoLight.cginc"
		#include "Assets/mattatz/Common/Shaders/Noise/SimplexNoise3D.cginc"

		#pragma multi_compile_fwdbase

		struct SliceData_t {
			float3 position;
			float3 offset;
		};

		StructuredBuffer<SliceData_t> _Slices;
		int _SlicesCount;

		bool slice(float3 p) {
			p += 0.5;

			for (int i = 0; i < _SlicesCount; i++) {
				SliceData_t data = _Slices[i];
				if (
					abs(p.x - data.position.x) < abs(data.offset.x * 0.5) || 
					abs(p.y - data.position.y) < abs(data.offset.y * 0.5) || 
					abs(p.z - data.position.z) < abs(data.offset.z * 0.5)
					) {
					return true;
				}
			}
			return false;
		}

		float4 wave(float4 vertex) {
			/*
			float _NoiseSpeed = 5.0;
			float3 _NoiseScale = float3(5, 5, 5);
			float3 _NoiseIntensity = float3(0.1, 0.1, 0.1);
			vertex.xyz += float3(
				snoise((vertex + float3(_Time.x * _NoiseSpeed, vertex.yz)) * _NoiseScale),
				snoise((vertex + float3(vertex.x, _Time.x * _NoiseSpeed, vertex.z)) * _NoiseScale),
				snoise((vertex + float3(vertex.xy, _Time.x * _NoiseSpeed)) * _NoiseScale)
			) * _NoiseIntensity;
			*/
			return vertex;
		}

		float3 point_lighting (float3 world, float3 worldNormal) {
			float3 lighting = float3(0.0, 0.0, 0.0);
			for (int index = 0; index < 4; index++) {
				float4 lightPosition = float4(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index], 1.0);
				float3 distance = lightPosition.xyz - world;
				float3 lightDir = normalize(distance);
				float squaredDistance = dot(distance, distance);
				float attenuation = 1.0 / (1.0 + unity_4LightAtten0[index] * squaredDistance);
				float3 reflection = attenuation * unity_LightColor[index].rgb * max(0.0, dot(worldNormal, lightDir));
				lighting += reflection;
			}
			return lighting;
		}

		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float3 normal : NORMAL;
			float3 viewDir  : TEXCOORD0;
			float3 lightDir : TEXCOORD1;
			float3 local : TEXCOORD2;
			LIGHTING_COORDS(4, 5)
		};

		float4 _Color;
		float _Shininess;

		v2f vert_common (appdata IN) {
			v2f OUT;
			UNITY_INITIALIZE_OUTPUT(v2f, OUT);

			float4 vertex = wave(IN.vertex);

			OUT.pos = mul(UNITY_MATRIX_MVP, vertex);
			OUT.local = vertex;
			OUT.normal = normalize(mul(_Object2World, float4(IN.normal, 0)).xyz);

			OUT.viewDir = WorldSpaceViewDir(IN.vertex);
			OUT.lightDir = WorldSpaceLightDir(IN.vertex);

			TRANSFER_VERTEX_TO_FRAGMENT(OUT);

			return OUT;
		}

		v2f vert_cull_back (appdata IN) {
			v2f OUT = vert_common(IN);
			return OUT;
		}

		v2f vert_cull_front (appdata IN) {
			v2f OUT = vert_common(IN);
			OUT.normal *= -1;
			return OUT;
		}

		fixed4 frag (v2f IN) : SV_Target {
			if (slice(IN.local)) {
				clip(-1);
			} 

			float3 viewDir = normalize(IN.viewDir);
			float3 lightDir = normalize(IN.lightDir);
			half3 h = normalize(lightDir + viewDir);

			float3 normal = IN.normal;
			float spec = max(0.0, pow(saturate(dot(normal, h)), _Shininess));

			fixed atten = LIGHT_ATTENUATION(IN);

			fixed4 col = _Color;
			col.rgb *= max(0.1, spec);
			col.rgb *= atten;
			return col;
		}
	
		ENDCG

		Pass {
			Tags { "LightMode"="ForwardBase" }
			Lighting On Cull Off

			CGPROGRAM
			#pragma vertex vert_cull_back
			#pragma fragment frag
			ENDCG
		}

		/*
		Pass {
			// Tags { "LightMode"="ForwardAdd" }
			Lighting On Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert_cull_back
			#pragma fragment frag
			ENDCG
		}
		*/

		/*
		Pass {
			Tags { "LightMode"="ForwardBase" }
			Lighting On Cull Back

			CGPROGRAM

			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag

			ENDCG
		}
		*/

		Pass {

			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On 
			ZTest LEqual

			CGPROGRAM

			#pragma target 3.0
			#pragma vertex shadow_vert
			#pragma fragment shadow_frag
			#pragma multi_compile_shadowcaster
				
			struct shadow_v2f {
				V2F_SHADOW_CASTER;
				float3 local : NORMAL;
			};

			shadow_v2f shadow_vert(appdata_full v) {
				shadow_v2f OUT;
				v.vertex = wave(v.vertex);

				TRANSFER_SHADOW_CASTER_NORMALOFFSET(OUT)

				OUT.local = v.vertex;
				return OUT;
			}

			fixed4 shadow_frag(shadow_v2f IN) : SV_Target{
				if (slice(IN.local)) {
					clip(-1);
				}

				SHADOW_CASTER_FRAGMENT(IN)
			}

			ENDCG
		}

	}
}
