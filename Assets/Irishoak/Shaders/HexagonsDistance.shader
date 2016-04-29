Shader "Unlit/HexagonsDistance"
{
	Properties
	{
		_MainTex          ("Texture",           2D           ) = "white" {}
		_ScreenResolution ("Screen Resolution", Vector       ) = (512,512,0,0)
		_Color            ("Color",             Color        ) = (1,1,1,1)
		_PulseRadius      ("Pulse Radius",      Float        ) = 0.0
		//_PulseWidth     ("Pulse Width",       Float        ) = 0.05
		_PulseFadeWidth   ("Pulse Fade Width",  Float        ) = 0.025
		_PulseRadiusNoise ("Pulse Width Noise", Float        ) = 0.01
		_InnerRadius      ("Inner Radius",      Range(0,0.5) ) = 0.0
		_OuterRadius      ("Outer Radius",      Range(0,0.5) ) = 0.5
		_OuterFadeWidth   ("Outer Fade Width",  Range(0,0.2)) = 0.2

		_EmissionIntensity ("Emission Intensity", Float) = 1.0
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Color.cginc"

	sampler2D _MainTex;
	float4 _MainTex_ST;

	float4 _ScreenResolution;

	float3 _Color;

	float  _PulseRadius;
	//float  _PulseWidth;
	float  _PulseFadeWidth;
	float  _PulseRadiusNoise;

	float  _InnerRadius;
	float  _OuterRadius;
	float  _OuterFadeWidth;

	float  _EmissionIntensity;

	// Created by inigo quilez - iq/2014
	// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

	// { 2d cell id, distance to border, distnace to center )
	float4 hexagon( float2 p ) 
	{
		float2 q = float2( p.x*2.0*0.5773503, p.y + p.x*0.5773503 );
		
		float2 pi = floor(q);
		float2 pf = frac(q);

		float v = fmod(pi.x + pi.y, 3.0);

		float ca = step(1.0,v);
		float cb = step(2.0,v);
		float2  ma = step(pf.xy,pf.yx);
		
	    // distance to borders
		float e = dot( ma, 1.0-pf.yx + ca*(pf.x+pf.y-1.0) + cb*(pf.yx-2.0*pf.xy) );

		// distance to center	
		p = float2( q.x + floor(0.5+p.y/1.5), 4.0*p.y/3.0 )*0.5 + 0.5;
		float f = length( (frac(p) - 0.5)*float2(1.0,0.85) );		
		
		return float4( pi + ca - cb*ma, e, f );
	}

	float hash1( float2  p ) { float n = dot(p,float2(127.1,311.7) ); return frac(sin(n)*43758.5453); }

	float noise( in float3 x )
	{
	    float3 p = floor(x);
	    float3 f = frac(x);
		f = f * f * (3.0 - 2.0 * f);
		float2 uv = (p.xy+float2(37.0,17.0)*p.z) + f.xy;
		float2 rg = tex2D( _MainTex, (uv+0.5)/256.0).yx;
		return lerp( rg.x, rg.y, f.z );
	}

	fixed4 frag (v2f_img i) : SV_Target
	{
		//float  time = _Time.y;
		float  time = 0.0;
		float2 fragCoord = i.uv.xy * 2.0 + 1.0;
		fixed4 fragColor = (fixed4)0;
		float2 uv = fragCoord.xy/_ScreenResolution.xy;
		float2 pos = (-_ScreenResolution.xy + 5.0 * fragCoord.xy)/_ScreenResolution.z;
		
	    // distort
		//pos *= 1.0 + 0.3*length(pos);
		
	    // gray
//		float4 h = hexagon(8.0*pos + 0.5*time);
//		float n = noise( float3(0.3*h.xy+time*0.1,time) );
//		float3 col = 0.15 + 0.15*hash1(h.xy+1.2)*float3(1.0, 1.0, 1.0);
//		col *= smoothstep( 0.10, 0.11, h.z );
//		col *= smoothstep( 0.10, 0.11, h.w );
//		col *= 1.0 + 0.15*sin(40.0*h.z);
//		col *= 0.75 + 0.5*h.z*n;

		float4 h   = 0.0;
		float  n   = 0.0;
		float3 col = (float3)0.0;

		float alpha = 1.0;

		// red
		h = hexagon(6.0 * pos + 0.6 * time);
		n = noise( float3(0.3*h.xy+time*0.1,time) );
		float3 colb = (_Color.rgb);// 0.9 + 0.8 * sin( hash1(h.xy)*1.5 + 2.0 + (1.0 - _Color));
		float  ha = hash1(h.xy).x;
		ha = fmod(ha, 1.0);
		colb *= smoothstep( 0.10, 0.60, h.z );
		colb *= 1.0 + 0.15*sin(40.0 * h.z);
		colb *= 0.75 + 0.5 * h.z * n;

		h = hexagon(6.0*(pos+0.1*float2(-1.3,1.0)) + 0.6*time);
	    //col *= 1.0-0.8*smoothstep(0.45,0.451,noise( float3(0.3*h.xy+time*0.1,time) ));

		col = lerp( col, colb, smoothstep(0.45,0.451,n) );

		//col *= pow( 16.0*uv.x*(1.0-uv.x)*uv.y*(1.0-uv.y), 0.1 );


		if ( ha.x < 0.1) {
			//col.rgb = 0.0;
			alpha   = 0.0;
		}

		float3 colHSV = rgb2hsv(col.rgb);
		//colHSV.g += (-0.5 + ha) * 1.25;

		col.rgb = hsv2rgb(colHSV);

		float dist = distance(i.uv.xy, float2(0.5, 0.5));
		//if (_PulseRadius < dist + ha * _PulseRadiusNoise) alpha *= 0.0;
		alpha = saturate(1.0-abs(((dist+ha*_PulseRadiusNoise) - _PulseRadius)/(_PulseFadeWidth)));

		if (_InnerRadius > dist) alpha *= 0.0;

		if (dist < _OuterRadius - _OuterFadeWidth) {
			alpha *= 1.0;
		} else if (dist > _OuterRadius - _OuterFadeWidth && dist <= _OuterRadius) {
			alpha *= saturate(1.0 - (dist - (_OuterRadius - _OuterFadeWidth)) / _OuterFadeWidth);
		} else {
			alpha *= 0.0;
		}

		fragColor = float4( col, alpha * length(col)) * _EmissionIntensity;

		return fragColor;
	}
	ENDCG

	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100

		Cull Off
		ZWrite Off
		//ZWrite On
		//ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment   frag
			ENDCG
		}
	}
}
