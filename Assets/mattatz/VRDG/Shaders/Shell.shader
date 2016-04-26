Shader "mattatz/Shell" {

	Properties {
		[MaterialEnum(Face, 0, Local, 1, Other, 2)] _Mode ("Mode", Int) = 0

		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Shininess ("Shininess", Float) = 5.0

		_PosTex ("Position Texture (rgb = xyz, a = lifetime)", 2D) = "white" {}
		_VelTex ("Velocity Texture (rgb = vx, vy, vz, a = mass(size))", 2D) = "white" {}
		_RotationTex ("Rotation Texture (quaterion)", 2D) = "white" {}

		_T ("Time offset", Float) = 0.0
		_CT  ("Clip Lerping", Range(0.0, 1.0)) = 0.0
		_ET ("Extrusion Lerping", Range(0.0, 1.0)) = 0.0
		_ST ("Scale Lerping", Range(0.0, 1.0)) = 0.0
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		CGINCLUDE

		#include "UnityCG.cginc"
		#include "AutoLight.cginc"

		#include "Assets/mattatz/Common/Shaders/Math.cginc"
		#include "Assets/mattatz/Common/Shaders/Random.cginc"
		#include "Assets/mattatz/Common/Shaders/Noise/SimplexNoise3D.cginc"

		#pragma target 3.0

		int _Mode;

		float4 _Color;

		float _Shininess;

		sampler2D _PosTex, _VelTex, _RotationTex;

		float3 _CNoiseScale;
		float _CNoiseSpeed, _CNoiseIntensity;

		float3 _ENoiseScale;
		float _ENoiseSpeed, _ENoiseIntensity;

		float3 _SNoiseScale;
		float _SNoiseSpeed, _SNoiseIntensity;

		fixed _T;
		fixed _CT;
		fixed _ET;
		fixed _ST;

		fixed2 _SwirlCenter;

		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			float2 uv : TEXCOORD0;
			float2 uv2 : TEXCOORD1;
		};

		float3 wave(float3 vertex) {
			float3 intensity = float3(0.1, 0.75, 0.75);
			float t = _T + _Time.x;
			float3 glitch = float3(
				snoise(vertex + float3(t, 0, 0)),
				snoise(vertex + float3(0, t, 0)),
				snoise(vertex + float3(0, 0, t))
			) * intensity;
			return lerp(vertex, vertex + glitch, _ET);
		}

		float3 extrude(float3 vertex, float3 normal) {
			float t = _Time.y + _T;
			float n = snoise((normal + float3(0, t * _ENoiseSpeed, 0)) * _ENoiseScale);
			n += 0.5;
			float s = _ENoiseIntensity * n;
			return lerp(vertex, vertex + normal * s, _ET);
		}

		float3 scale(float3 vertex, float3 center) {
			float t = _Time.y + _T;
			float n = snoise((center + float3(0, t * _SNoiseSpeed, 0)) * _SNoiseScale);
			return lerp(vertex, center, saturate(_ST * n));
		}

		float4 swirl(float4 p) {
			float2 center = _SwirlCenter;
			float radius = 1.0;

			float2 v = p.xy / p.w;
			float dist = length(center - v);

			float power = 3;

			if(dist < radius) {
				float percent = (radius - dist) / radius;
				float theta = percent * percent * power;
				float s = sin(theta);
				float c = cos(theta);
				v.xy = float2(dot(v.xy, float2(c, -s)), dot(v.xy, float2(s, c)));
				p.xy = v.xy * p.w;
			}

			return p;
		}

		float4 distort(float4 p) {
			float2 v = p.xy / p.w;
			float power = 0.5;

			// convert to polar coords:
			float radius = length(v);
			if (radius > 0) {
				float theta = atan2(v.y, v.x);

				// distortion
				radius = pow(radius, power);

				// convert back to Cartesian:
				v.x = radius * cos(theta);
				v.y = radius * sin(theta);
				p.xy = v.xy * p.w;
			}
			return p;
		}

		float clip_noise (float3 seed) {
			float t = _Time.y + _T;
			return (snoise((seed + float3(0, t * _CNoiseSpeed, 0)) * _CNoiseScale) + 1.0) * 0.5;
		}

		void clip_face(float3 local, float3 normal) {
			float n = clip_noise(normal);
			clip(n - _CT);
		}

		void clip_local(float3 local, float3 normal) {
			float n = clip_noise(local);
			clip(n - _CT);
		}

		void clip_common(float3 local, float3 normal) {
			if(_Mode == 0) clip_face(local, normal);
			else if(_Mode == 1) clip_local(local, normal);
		}

		ENDCG

		Pass {
			Tags { "LightMode"="ForwardBase" }
			Lighting On Cull Off ZWrite On

			CGPROGRAM

			#pragma multi_compile_fwdbase

			#pragma vertex vert_common
			#pragma fragment frag_clip

			struct v2f {
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float3 local : TANGENT;
				float2 uv : TEXCOORD0;
				float3 viewDir  : TEXCOORD1;
				float3 lightDir : TEXCOORD2;
				LIGHTING_COORDS(4, 5)
			};

			v2f vert_common (appdata v) {
				v2f OUT;
				UNITY_INITIALIZE_OUTPUT(v2f, OUT);

				// vertex.xyz = wave(vertex.xyz);
				v.vertex.xyz = scale(v.vertex.xyz, v.tangent.xyz);
				v.vertex.xyz = extrude(v.vertex.xyz, v.normal);

				float4 vertex = v.vertex;

				OUT.pos = mul(UNITY_MATRIX_MVP, vertex);
				OUT.local = vertex;
				OUT.uv = v.uv;
				OUT.normal = normalize(mul(_Object2World, float4(v.normal, 0)).xyz);
				OUT.viewDir = WorldSpaceViewDir(vertex);
				OUT.lightDir = WorldSpaceLightDir(vertex);

				TRANSFER_VERTEX_TO_FRAGMENT(OUT);

				// OUT.vertex = swirl(OUT.vertex);

				return OUT;
			}
			
			fixed4 frag_clip (v2f IN) : SV_Target {
				clip_common(IN.local, IN.normal);

				float3 viewDir = normalize(IN.viewDir);
				float3 lightDir = normalize(IN.lightDir);
				half3 h = normalize(lightDir + viewDir);

				float3 normal = IN.normal;
				normal = dot(IN.viewDir, IN.normal) > 0 ? normal : -normal;

				float spec = max(0.0, pow(saturate(dot(normal, h)), _Shininess));

				fixed atten = LIGHT_ATTENUATION(IN);

				fixed4 col = _Color;
				col.rgb *= max(0.1, spec);
				col.rgb *= atten;
				col.a = 0.0; // for mask bloom
				return col;
			}

			ENDCG
		}

		Pass {
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			ZTest LEqual

			CGPROGRAM

			#pragma vertex shadow_vert
			#pragma fragment shadow_frag
			#pragma multi_compile_shadowcaster

			struct shadow_v2f {
				V2F_SHADOW_CASTER;
				float3 local : TANGENT;
				float3 normal : NORMAL;
			};

			shadow_v2f shadow_vert(appdata_full v) {
				shadow_v2f OUT;
				UNITY_INITIALIZE_OUTPUT(shadow_v2f, OUT);

				// v.vertex.xyz = wave(v.vertex.xyz);
				v.vertex.xyz = scale(v.vertex.xyz, v.tangent.xyz);
				v.vertex.xyz = extrude(v.vertex.xyz, v.normal);

				TRANSFER_SHADOW_CASTER_NORMALOFFSET(OUT)

				OUT.local = v.vertex;
				OUT.normal = normalize(mul(_Object2World, float4(v.normal, 0)).xyz);

				// OUT.pos = swirl(OUT.pos);

				return OUT;
			}

			fixed4 shadow_frag(shadow_v2f IN) : SV_Target{
				clip_common(IN.local, IN.normal);

				SHADOW_CASTER_FRAGMENT(IN)
			}

			ENDCG
		}

	}
}
