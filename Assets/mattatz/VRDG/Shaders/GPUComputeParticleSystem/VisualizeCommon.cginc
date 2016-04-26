
#pragma target 5.0

#include "UnityCG.cginc"

#ifndef _SHADOW_PARTICLE_
#include "AutoLight.cginc"
#endif

#include "Assets/mattatz/Common/Shaders/Random.cginc"
#include "Assets/mattatz/Common/Shaders/Math.cginc"
#include "Assets/mattatz/Common/Shaders/Noise/SimplexNoise3D.cginc"

#include "./GPUParticle.cginc"

struct appdata {
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

struct v2g {
	float4 vertex : POSITION;
	float4 rot : NORMAL;
	float2 uv : TEXCOORD0;
	float4 col : TEXCOORD1;
	float3 size : TEXCOORD2;
};

#define _g2f_common_ float4 wpos : TANGENT; float3 normal : NORMAL; float2 uv : TEXCOORD0; float4 col : TEXCOORD1; float3 lightDir : TEXCOORD2; float3 viewDir: TEXCOORD3;

struct g2f {
#ifdef _SHADOW_PARTICLE_
	V2F_SHADOW_CASTER;
	_g2f_common_
#else 
	float4 pos : POSITION;
	_g2f_common_
	LIGHTING_COORDS(5, 6)
#endif
};

sampler2D _MainTex;
fixed _Size;

StructuredBuffer<Particle> _Particles;

float3 _Core;

float4x4 _ModelMatrix;
float2 _ClipDistance;
float2 _ScaleDistance;

// Computes world space light direction
float3 world_space_light_dir(float4 v) {
	float3 worldPos = mul(_Object2World, v).xyz;
	// float3 worldPos = mul(_ModelMatrix, v).xyz;
#ifndef USING_LIGHT_MULTI_COMPILE
	return _WorldSpaceLightPos0.xyz - worldPos * _WorldSpaceLightPos0.w;
#else
#ifndef USING_DIRECTIONAL_LIGHT
	return _WorldSpaceLightPos0.xyz - worldPos;
#else
	return _WorldSpaceLightPos0.xyz;
#endif
#endif
}

// Computes world space view direction
float3 world_space_view_dir(in float4 v) {
	return _WorldSpaceCameraPos.xyz - mul(_Object2World, v).xyz;
	// return _WorldSpaceCameraPos.xyz - mul(_ModelMatrix, v).xyz;
}

float3 point_lighting(float3 wpos, float3 normal) {
	float3 lighting = float3(0.0, 0.0, 0.0);

	// #ifdef VERTEXLIGHT_ON
	for (int index = 0; index < 4; index++) {
		float3 lightPos = float3(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index]);
		float3 lightDir = normalize(lightPos - wpos);
		float sd = dot(lightDir, lightDir);
		float atten = 1.0 / (1.0 + unity_4LightAtten0[index] * sd);
		lighting += atten * unity_LightColor[index].rgb * max(0.0, dot(normal, lightDir));
	}
	// #endif

	return lighting;
}

v2g vert(appdata IN, uint id : SV_VertexID) {
	v2g OUT;

	Particle p = _Particles[id];

	float3 pos = p.pos;
	OUT.vertex = float4(pos, 1.0);
	OUT.rot = p.rot;
	OUT.col = p.col;
	OUT.uv = float2(0, 0);

	float lifetime = saturate(p.lifetime);

	float3 world = mul(_Object2World, OUT.vertex).xyz;
	float4 mvp = mul(UNITY_MATRIX_MVP, OUT.vertex);
	// float2 normalizedXY = (mvp.xy / mvp.w); // 0.0 ~ 1.0
	// float xy = smoothstep(1.0, 0.9, length(normalizedXY));
	float depth = smoothstep(_ClipDistance.x, _ClipDistance.y, pow(mvp.z / mvp.w, 1.0)); // for Camera moving
	float scale = smoothstep(_ScaleDistance.x, _ScaleDistance.y, length(_Core - world));  // for Core object appearance.
	float s = depth * scale;

	OUT.size = s * _Size * p.scale * smoothstep(0.0, 0.1, lifetime) * (1.0 - smoothstep(0.9, 1.0, lifetime));

	return OUT;
}

void add_face(v2g v, in g2f pIn, inout TriangleStream<g2f> OUT, float4 p[4]) {
	// float4x4 vp = mul(UNITY_MATRIX_VP, _ModelMatrix);
	float4x4 vp = mul(UNITY_MATRIX_VP, _Object2World);

	pIn.pos = mul(vp, p[0]);
	pIn.wpos = mul(_Object2World, p[0]);
	// pIn.lightDir = world_space_light_dir(p[0]);
	// pIn.viewDir = world_space_view_dir(p[0]);

	pIn.lightDir = ObjSpaceLightDir(p[0]);
	pIn.viewDir = ObjSpaceViewDir(p[0]);

	pIn.uv = float2(1.0f, 0.0f);

#ifdef _SHADOW_PARTICLE_
	v.vertex = p[0];
	TRANSFER_SHADOW_CASTER(pIn)
#else
	TRANSFER_VERTEX_TO_FRAGMENT(pIn)
	TRANSFER_SHADOW(pIn);
#endif

	OUT.Append(pIn);

	pIn.pos = mul(vp, p[1]);
	pIn.wpos = mul(_Object2World, p[1]);

	// pIn.lightDir = world_space_light_dir(p[1]);
	// pIn.viewDir = world_space_view_dir(p[1]);

	pIn.lightDir = ObjSpaceLightDir(p[1]);
	pIn.viewDir = ObjSpaceViewDir(p[1]);

	pIn.uv = float2(1.0f, 1.0f);

#ifdef _SHADOW_PARTICLE_
	v.vertex = p[1];
	TRANSFER_SHADOW_CASTER(pIn)
#else
	TRANSFER_VERTEX_TO_FRAGMENT(pIn)
	TRANSFER_SHADOW(pIn);
#endif

	OUT.Append(pIn);

	pIn.pos = mul(vp, p[2]);
	pIn.wpos = mul(_Object2World, p[2]);
	// pIn.lightDir = world_space_light_dir(p[2]);
	// pIn.viewDir = world_space_view_dir(p[2]);
	pIn.lightDir = ObjSpaceLightDir(p[2]);
	pIn.viewDir = ObjSpaceViewDir(p[2]);
	pIn.uv = float2(0.0f, 0.0f);

#ifdef _SHADOW_PARTICLE_
	v.vertex = p[2];
	TRANSFER_SHADOW_CASTER(pIn)
#else
	TRANSFER_VERTEX_TO_FRAGMENT(pIn)
	TRANSFER_SHADOW(pIn);
#endif

	OUT.Append(pIn);

	pIn.pos = mul(vp, p[3]);
	pIn.wpos = mul(_Object2World, p[3]);
	// pIn.lightDir = world_space_light_dir(p[3]);
	// pIn.viewDir = world_space_view_dir(p[3]);
	pIn.lightDir = ObjSpaceLightDir(p[3]);
	pIn.viewDir = ObjSpaceViewDir(p[3]);
	pIn.uv = float2(0.0f, 1.0f);

#ifdef _SHADOW_PARTICLE_
	v.vertex = p[3];
	TRANSFER_SHADOW_CASTER(pIn)
#else
	TRANSFER_VERTEX_TO_FRAGMENT(pIn)
	TRANSFER_SHADOW(pIn);
#endif

	OUT.Append(pIn);

	OUT.RestartStrip();
}

[maxvertexcount(24)]
void geom_cube(point v2g IN[1], inout TriangleStream<g2f> OUT) {

	float3 size = IN[0].size;
	float3 halfS = 0.5f * size;

	float3 pos = IN[0].vertex.xyz;
	float3 right = rotate_vector(float3(1, 0, 0), IN[0].rot) * halfS.x;
	float3 up = rotate_vector(float3(0, 1, 0), IN[0].rot) * halfS.y;
	float3 forward = rotate_vector(float3(0, 0, 1), IN[0].rot) * halfS.z;

	float4 v[4];

	g2f pIn;
	pIn.col = IN[0].col;

	// forward
	v[0] = float4(pos + forward + right - up, 1.0f);
	v[1] = float4(pos + forward + right + up, 1.0f);
	v[2] = float4(pos + forward - right - up, 1.0f);
	v[3] = float4(pos + forward - right + up, 1.0f);
	pIn.normal = normalize(forward);
	add_face(IN[0], pIn, OUT, v);

	// back
	v[0] = float4(pos - forward - right - up, 1.0f);
	v[1] = float4(pos - forward - right + up, 1.0f);
	v[2] = float4(pos - forward + right - up, 1.0f);
	v[3] = float4(pos - forward + right + up, 1.0f);
	pIn.normal = -normalize(forward);
	add_face(IN[0], pIn, OUT, v);

	// up
	v[0] = float4(pos - forward + right + up, 1.0f);
	v[1] = float4(pos - forward - right + up, 1.0f);
	v[2] = float4(pos + forward + right + up, 1.0f);
	v[3] = float4(pos + forward - right + up, 1.0f);
	pIn.normal = normalize(up);
	add_face(IN[0], pIn, OUT, v);

	// down
	v[0] = float4(pos + forward + right - up, 1.0f);
	v[1] = float4(pos + forward - right - up, 1.0f);
	v[2] = float4(pos - forward + right - up, 1.0f);
	v[3] = float4(pos - forward - right - up, 1.0f);
	pIn.normal = -normalize(up);
	add_face(IN[0], pIn, OUT, v);

	// left
	v[0] = float4(pos + forward - right - up, 1.0f);
	v[1] = float4(pos + forward - right + up, 1.0f);
	v[2] = float4(pos - forward - right - up, 1.0f);
	v[3] = float4(pos - forward - right + up, 1.0f);
	pIn.normal = -normalize(right);
	add_face(IN[0], pIn, OUT, v);

	// right
	v[0] = float4(pos - forward + right + up, 1.0f);
	v[1] = float4(pos + forward + right + up, 1.0f);
	v[2] = float4(pos - forward + right - up, 1.0f);
	v[3] = float4(pos + forward + right - up, 1.0f);
	pIn.normal = normalize(right);
	add_face(IN[0], pIn, OUT, v);

};
