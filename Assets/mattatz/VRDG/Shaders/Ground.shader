Shader "mattatz/Ground" {

	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Shininess ("Shininess", Float)	= 1.0
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGINCLUDE

		#include "Assets/mattatz/Common/Shaders/Noise/SimplexNoise3D.cginc"

		float3 _NoiseScale;
		float _NoiseSpeed;
		float _NoiseIntensity;

		float4 wave(float4 vertex, float3 normal) {
			float amount = snoise(
				(vertex + float3(0, _Time.x * _NoiseSpeed, 0)) * _NoiseScale
			) * _NoiseIntensity;
			vertex.xyz += normal * amount;
			return vertex;
		}

		ENDCG

		Pass {

			Tags { "LightMode"="ForwardBase" }
			Lighting On ZWrite On

			CGPROGRAM

			#pragma target 5.0
			#pragma multi_compile_fwdbase

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			#pragma vertex vert
			#pragma fragment frag

			fixed4 _Color;

			fixed _Shininess;

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				float3 normal	: NORMAL;
				float3 screenPos : TEXCOORD0;
				float2 uv       : TEXCOORD1;
				float3 lightDir : TEXCOORD2;
				LIGHTING_COORDS(4, 5)
			};

			v2f vert(appdata v) {

				v.vertex = wave(v.vertex, v.normal);

				v2f OUT;
				UNITY_INITIALIZE_OUTPUT(v2f, OUT);

				OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				OUT.screenPos = ComputeScreenPos(OUT.pos);
				OUT.uv = v.texcoord.xy;

				OUT.normal = v.normal;

				OUT.lightDir = ObjSpaceLightDir(v.vertex); // forwardだとこっち
				// OUT.lightDir = float3(0, -0.5, 1); // deferredだとこっち

				TRANSFER_VERTEX_TO_FRAGMENT(OUT);

				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target {
				float3 lightDir = normalize(IN.lightDir);

				float3 dx = ddx(IN.screenPos);
				float3 dy = ddy(IN.screenPos);
				float3 normal = normalize(cross(dx, dy));

				float nh = saturate(dot(normal, lightDir));
				float spec = pow(nh, _Shininess);

				fixed4 col = _Color;

				fixed atten = LIGHT_ATTENUATION(IN);
				col.rgb *= spec;
				col.rgb *= atten;

				return col;
			}

			ENDCG
		}

		Pass {

			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On ZTest LEqual

			CGPROGRAM

			#pragma target 3.0
			#pragma vertex shadow_vert
			#pragma fragment shadow_frag
			#pragma multi_compile_shadowcaster

			#include "UnityCG.cginc"
				
			struct shadow_v2f {
				V2F_SHADOW_CASTER;
			};

			shadow_v2f shadow_vert(appdata_full v) {
				shadow_v2f o;

				v.vertex = wave(v.vertex, v.normal);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

				return o;
			}

			fixed4 shadow_frag(shadow_v2f i) : SV_Target{
				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
		}

	}

}
