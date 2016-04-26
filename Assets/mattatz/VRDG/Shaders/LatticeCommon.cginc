#ifndef _LATTICE_COMMON_INCLUDED
#define _LATTICE_COMMON_INCLUDED

#include "Assets/mattatz/Common/Shaders/Random.cginc"

float _T;

float _SideLength;

float _NoiseSpeed;
float3 _NoiseIntensity;
float3 _NoiseScale;

float3 wave(float3 vertex) {
	float t = _T * _NoiseSpeed;

	float3 offset = float3(
		snoise((vertex + float3(t, 0, 0)) * _NoiseScale),
		snoise((vertex + float3(0, t, 0)) * _NoiseScale),
		snoise((vertex + float3(0, 0, t)) * _NoiseScale)
	) * _NoiseIntensity;

	return offset;
}

float3 random_direction (float3 seed) {
	float r = nrand(seed);
	if (r < 0.166) {
		return float3(0, 0, 1);
	} else if (r < 0.332) {
		return float3(0, 0, -1);
	} else if (r < 0.498) {
		return float3(1, 0, 0);
	} else if (r < 0.664) {
		return float3(-1, 0, 0);
	} else if (r < 0.83) {
		return float3(0, 1, 0);
	}
	return float3(0, -1, 0);
}

float3 move(float3 vertex, float amount) {
	float3 dir = random_direction(vertex);
	return vertex + dir * _SideLength * amount;
}

#endif // _LATTICE_COMMON_INCLUDED