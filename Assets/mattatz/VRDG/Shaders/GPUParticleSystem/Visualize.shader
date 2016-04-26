Shader "mattatz/GPUParticleSystem/Visualize" {

	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Shininess ("Shininess", Float) = 32.0

		_PosTex ("Position Texture (rgb = xyz, a = lifetime)", 2D) = "white" {}
		_VelTex ("Velocity Texture (rgb = vx, vy, vz, a = mass(size))", 2D) = "white" {}
		_RotationTex ("Rotation Texture", 2D) = "white" {}

		_SimulationSize ("emission size", Vector) = (1, 1, 1, -1)
		_SimulationOffset ("emission offset", Vector) = (0, 0, 0, -1)

		_SizeRange ("Size range", Vector) = (0, 1, -1, -1)
	}

	SubShader {

		CGINCLUDE

		#include "Assets/mattatz/Common/Shaders/Math.cginc"

		sampler2D _PosTex;
		sampler2D _VelTex;
		sampler2D _RotationTex;

		float2 _SizeRange;

		float3 geom_triangle_common(float4 pos, float4 rotation, float size, out float4 v[3]) {

			float3 up = float3(0, 1, 0);
			float3 look = _WorldSpaceCameraPos - pos;
			look.y = 0;
			look = normalize(look);

			up = rotate_vector(up, rotation);
			look = rotate_vector(look, rotation);

			float3 right = cross(up, look);

			float halfS = 0.5f * lerp(_SizeRange.x, _SizeRange.y, size) + _SizeRange.x * size;

			// float4x4 vp = mul(UNITY_MATRIX_MVP, _World2Object);
			float4x4 m = _World2Object;

			v[0] = mul(m, float4(pos.xyz - halfS * up, 1.0f));
			v[1] = mul(m, float4(pos.xyz + halfS * right + halfS * up, 1.0f));
			v[2] = mul(m, float4(pos.xyz - halfS * right + halfS * up, 1.0f));

			return mul(m, float4(look, 0.0)).xyz;
			// return look;
		}

		float calculate_size(float4 pos, float4 vel) {
			float lifetime = pos.a;
			float size = vel.a; // 0.0 ~ 1.0
			const float duration = 0.1;
			size *= smoothstep(0.0, duration, lifetime) * smoothstep(1.0, 1.0 - duration, lifetime);
			return size;
		}

		ENDCG

		Pass {
			Tags { "LightMode"="ForwardBase" }
			Lighting On ZWrite On
			// ZTest Always
			Cull Off

			CGPROGRAM

			#pragma target 5.0
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			#pragma vertex vert
			#pragma geometry geom_triangle
			// #pragma geometry geom_billboard
			#pragma fragment frag

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct v2g {
				float4 pos : SV_POSITION;
				float4 rotation : NORMAL;
				float2 uv : TEXCOORD1;
				float size : TEXCOORD2;
			};

			struct g2f {
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 screenPos : TEXCOORD1;
				float3 lightDir : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
				LIGHTING_COORDS(5, 6)
			};

			v2g vert(appdata IN) {
				v2g OUT;

				float4 pos = tex2Dlod(_PosTex, float4(IN.texcoord.xy, 0, 0));
				float4 vel = tex2Dlod(_VelTex, float4(IN.texcoord.xy, 0, 0));
				float4 rot = tex2Dlod(_RotationTex, float4(IN.texcoord.xy, 0, 0));

				float4 vertex = float4(pos.xyz, 1.0);
				OUT.pos = mul(_Object2World, vertex);

				OUT.rotation = rot;
				OUT.uv = IN.texcoord;

				OUT.size = calculate_size(pos, vel);

				return OUT;
			}

			[maxvertexcount(3)]
			void geom_triangle (point v2g IN[1], inout TriangleStream<g2f> OUT) {
				float4 v[3];
				float3 normal = geom_triangle_common(IN[0].pos, IN[0].rotation, IN[0].size, v);

				g2f pIn;
				UNITY_INITIALIZE_OUTPUT(g2f, pIn);

				// pIn.normal = normal;
				pIn.normal = mul(_Object2World, float4(normal, 0.0)).xyz;

				pIn.pos = mul(UNITY_MATRIX_MVP, v[0]);
				pIn.screenPos = ComputeScreenPos(pIn.pos);
				pIn.uv = float2(1.0f, 0.0f);
				pIn.lightDir = WorldSpaceLightDir(v[0]);
				pIn.viewDir = WorldSpaceViewDir(v[0]);
				TRANSFER_VERTEX_TO_FRAGMENT(pIn);
				OUT.Append(pIn);

				pIn.pos = mul(UNITY_MATRIX_MVP, v[1]);
				pIn.screenPos = ComputeScreenPos(pIn.pos);
				pIn.uv = float2(1.0f, 1.0f);
				pIn.lightDir = WorldSpaceLightDir(v[1]);
				pIn.viewDir = WorldSpaceViewDir(v[1]);
				TRANSFER_VERTEX_TO_FRAGMENT(pIn);
				OUT.Append(pIn);

				pIn.pos = mul(UNITY_MATRIX_MVP, v[2]);
				pIn.screenPos = ComputeScreenPos(pIn.pos);
				pIn.uv = float2(0.0f, 0.0f);
				pIn.lightDir = WorldSpaceLightDir(v[2]);
				pIn.viewDir = WorldSpaceViewDir(v[2]);
				TRANSFER_VERTEX_TO_FRAGMENT(pIn);
				OUT.Append(pIn);
			}

			fixed4 _Color;
			float _Shininess;

			fixed4 frag(g2f IN) : SV_Target {

				// float3 dx = ddx(IN.screenPos);  // IN.pos is the screen space vertex position
				// float3 dy = ddy(IN.screenPos);
				// float3 normal = normalize(cross(dx, dy));
				float3 normal = normalize(IN.normal);

				float3 lightDir = normalize(IN.lightDir);
				float3 viewDir = normalize(IN.viewDir);

				normal = dot(viewDir, normal) > 0 ? normal : -normal;

				float nh = saturate(dot(normal, lightDir));
				float spec = pow(nh, _Shininess);

				fixed4 col = _Color * max(0.1, spec);
				return col;
			}

			ENDCG
		}

		Pass {

			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			ZWrite On ZTest LEqual

			CGPROGRAM

			#pragma target 5.0
			#pragma multi_compile_shadowcaster
			#pragma vertex shadow_vert
			#pragma geometry shadow_geom
			#pragma fragment shadow_frag

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct shadow_v2g {
				float4 rotation : NORMAL;
				float size : TANGENT;
				V2F_SHADOW_CASTER;
			};

			struct shadow_g2f {
				V2F_SHADOW_CASTER;
			};

			shadow_v2g shadow_vert (appdata IN) {
				shadow_v2g OUT = (shadow_v2g)0;

				float4 pos = tex2Dlod(_PosTex, float4(IN.texcoord.xy, 0, 0));
				float4 vel = tex2Dlod(_VelTex, float4(IN.texcoord.xy, 0, 0));
				float4 rot = tex2Dlod(_RotationTex, float4(IN.texcoord.xy, 0, 0));

				float4 vertex = float4(pos.xyz, 1.0);
				OUT.pos = mul(_Object2World, vertex);
				OUT.rotation = rot;
				OUT.size = calculate_size(pos, vel);

				return OUT;
			}

			[maxvertexcount(3)]
			void shadow_geom(point shadow_v2g IN[1], inout TriangleStream<shadow_g2f> OUT) {
				float4 p[3];
				geom_triangle_common(IN[0].pos, IN[0].rotation, IN[0].size, p);

				shadow_g2f pIn = UNITY_INITIALIZE_OUTPUT(shadow_g2f, pIn);

				pIn.pos = mul(UNITY_MATRIX_MVP, p[0]);
				OUT.Append(pIn);

				pIn.pos = mul(UNITY_MATRIX_MVP, p[1]);
				OUT.Append(pIn);

				pIn.pos = mul(UNITY_MATRIX_MVP, p[2]);
				OUT.Append(pIn);
			}

			float4 shadow_frag (shadow_g2f IN) : COLOR {
				SHADOW_CASTER_FRAGMENT(IN)
			}

			ENDCG
		}

	}

}
