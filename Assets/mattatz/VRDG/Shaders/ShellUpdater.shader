Shader "mattatz/Shell/Update" {

	Properties{
		_PosTex ("Position Texture (rgb = xyz, a = lifetime)", 2D) = "white" {}
		_VelTex ("Velocity Texture (rgb = vx, vy, vz, a = mass(size))", 2D) = "white" {}
		_RotationTex ("Rotation Texture (quaterion)", 2D) = "white" {}

		_EmissionRate ("Emission Rate", Range(0.0, 1.0)) = 1.0
		_EmissionSize ("Emission Size", Vector) = (1, 1, 1, -1)

		_Deceleration ("Deceleration", Range(0.0, 1.0)) = 0.95
		_LifetimeSpeed ("Lifetime Speed", Range(0, 1)) = 0.1
	}

	SubShader {

		Cull Off ZWrite Off ZTest Always

		CGINCLUDE

		#pragma target 5.0
		#include "UnityCG.cginc"
		#include "Assets/mattatz/Common/Shaders/Random.cginc"
		#include "Assets/mattatz/Common/Shaders/Math.cginc"

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

		fixed _EmissionRate;
		float3 _EmissionSize;

		float _Deceleration;

		float _LifetimeSpeed;

		float4 birth (float2 uv) {
			float2 seed = uv + _Time.x;
			float3 pos = random_point_on_sphere(seed);
			return float4(pos, 0.0);
		}

		float4 new_particle_rotation(float2 uv) {
			// Uniform random unit quaternion
			// http://www.realtimerendering.com/resources/GraphicsGems/gemsiii/urot.c
			float r = nrand(uv, 3);
			float r1 = sqrt(1.0 - r);
			float r2 = sqrt(r);
			float t1 = UNITY_PI * 2 * nrand(uv, 4);
			float t2 = UNITY_PI * 2 * nrand(uv, 5);
			return float4(sin(t1) * r1, cos(t1) * r1, sin(t2) * r2, cos(t2) * r2);
		}

		// Deterministic random rotation axis
		float3 get_rotation_axis(float2 uv) {
			// Uniformaly distributed points
			// http://mathworld.wolfram.com/SpherePointPicking.html
			float u = nrand(uv, 10) * 2 - 1;
			float theta = nrand(uv, 11) * UNITY_PI * 2;
			float u2 = sqrt(1 - u * u);
			return float3(u2 * cos(theta), u2 * sin(theta), u);
		}

		float4 update_rotation(float2 uv) {
			float4 r = tex2D(_RotationTex, uv);
			float3 v = tex2D(_VelTex, uv).xyz;

			float4 _SpinParams = float4(0.18, 0.02, 1, 1);

			float dt = 1.0;
			float theta = (_SpinParams.x + length(v) * _SpinParams.y) * dt;

			// Randomness
			// theta *= 1.0 - nrand(uv, 13) * _SpinParams.z;

			// Spin quaternion
			float4 dq = float4(get_rotation_axis(uv) * sin(theta), cos(theta));

			// Applying the quaternion and normalize the result.
			return normalize(qmul(dq, r));
		}

		f2o init(v2f IN) {
			f2o OUT;

			OUT.color0 = birth(IN.uv);
			OUT.color0.a = nrand(IN.uv);
			OUT.color1 = float4(0, 0, 0, nrand(IN.uv)); // a = mass, size
			OUT.color2 = new_particle_rotation(IN.uv);

			return OUT;
		}

		f2o update(v2f IN) {
			f2o OUT;

			float4 pos = tex2D(_PosTex, IN.uv);
			float4 vel = tex2D(_VelTex, IN.uv);

			vel.xyz += float3(0, 0.1, 0) * vel.w;

			pos.xyz += vel.xyz * unity_DeltaTime.x * (0.1 + vel.a);
			pos.a += unity_DeltaTime.x * _LifetimeSpeed * (0.5 + vel.a * 0.5);
			vel.xyz *= saturate(_Deceleration);

			if (pos.a > 1.0) {
				pos = birth(IN.uv);
				pos.a = 0.0;
				vel.xyz = 0.0;
				vel.a = nrand(float2(_Time.x + IN.uv.x, IN.uv.y));
			}

			OUT.color0 = pos;
			OUT.color1 = vel;
			OUT.color2 = update_rotation(IN.uv);

			return OUT;
		}

		ENDCG

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment init
			ENDCG
		}

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment update
			ENDCG
		}

	}

}
