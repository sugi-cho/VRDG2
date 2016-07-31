Shader "Unlit/Material00"
{
	Properties
	{
		_Size("particle size", Float) = 0.1
		_Speed("speed", Float) =  1.0
		_Base ("base color", Color) = (0,0.5,1,1)
		_Colorful("karafuru", Float) = 0.5
		_LifeTime ("life time", Float) = 5
		_LifeRand ("life random factor", Float) = 1
		_Yure ("yure guai", Float) = 1
		_Rate ("emit rate", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "AlphaTest+2" }
		LOD 100 Cull Off

		GrabPass{"_GrabTex"}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Assets/CGINC/Random.cginc"

			#define V_RIGHT normalize(UNITY_MATRIX_V[0].xyz)
			#define V_UP normalize(UNITY_MATRIX_V[1].xyz)
			#define V_FORWARD normalize(UNITY_MATRIX_V[2].xyz)

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 wPos : TEXCOORD1;
				float4 vCenter : TEXCOORD2;
				float4 gPos : TEXCOORD3;
				float4 color : TEXCOORD4;
				float size : TEXCOORD5;
				float4 vertex : SV_POSITION;
			};

			sampler3D _Noise3D;
			sampler2D _MainTex, _GrabTex;
			float4 _MainTex_ST,_Base;
			float _Size,_Speed, _Colorful,_LifeTime, _LifeRand,_Yure,_Rate;

			float3 getPos(uint id, float t) {
				float l = sqrt(id);
				float3 pos = 0;
				pos.x = sin(l*10)*l;
				pos.z = cos(l*10)*l;
				pos *= 0.1;
				pos = lerp(pos, pos + float3(0, 1, 0), pow(t,2));
				pos.x += tex3Dlod(_Noise3D, float4((pos + float3(0, _Time.x, 0))*0.1, 0))*_Yure;
				pos.y += tex3Dlod(_Noise3D, float4((pos + float3(0, 0, _Time.x))*0.1, 0))*_Yure;
				pos.z += tex3Dlod(_Noise3D, float4((pos + float3(_Time.x, 0, 0))*0.1, 0))*_Yure;
				return pos;
			}
			float4 getColor(uint id,float t) {
				float4 col = half4(rand3(float2(id, id*0.5)), 1);
				col = lerp(_Base, col, max(0,_Colorful));
				return col;
			}
			float getSize(uint id, float t) {
				float r = rand(float2(id, id*0.1));
				return _Size*saturate(t) * (r < _Rate);
			}
			
			v2f vert (appdata v, uint vid : SV_VertexID)
			{
				float t = fmod(_Time.y - vid*0.01 - rand(vid)*_LifeRand, _LifeTime + rand(vid)*_LifeRand)*_Speed;

				float4 center = float4(getPos(vid, t), 1);
				center = mul(_Object2World, center);
				v2f o = (v2f)0;
				o.vertex = center;
				o.wPos = center;
				o.color = getColor(vid, t);
				o.size = getSize(vid, t);
				return o;
			}
			[maxvertexcount(4)]
			void geom(point v2f v[1], inout TriangleStream<v2f> tStream) {
				v2f o = v[0];
				float4 wCenter = o.wPos;
				for (int x = 0; x < 2; x++)
				{
					for (int y = 0; y < 2; y++)
					{
						// ビルボード用の行列 
						float4x4 billboardMatrix = UNITY_MATRIX_V;
						billboardMatrix._m03 =
						billboardMatrix._m13 =
						billboardMatrix._m23 =
						billboardMatrix._m33 = 0;

						float2 uv = float2(x, y);
						o.uv = uv;

						o.vCenter = mul(UNITY_MATRIX_V, wCenter);
						o.wPos = wCenter + mul(float4((uv * 2 - float2(1, 1)) * o.size * saturate(abs(o.vCenter.z)-0.5), 0, 1), billboardMatrix);
						o.vertex = mul(UNITY_MATRIX_VP, o.wPos);
						o.gPos = ComputeGrabScreenPos(o.vertex);

						tStream.Append(o);
					}
				}

			}
			
			fixed4 frag (v2f i, out float depth:SV_Depth) : SV_Target
			{
				half3 vNormal;
				vNormal.xy = i.uv*2.0 - 1.0;
				half r2 = dot(vNormal.xy, vNormal.xy);
				if (r2 > 1.0)
					discard;
				vNormal.z = sqrt(1.0 - r2);

				half4 vPos = half4(i.vCenter.xyz + vNormal.xyz*_Size, 1.0);
				half4 cPos = mul(UNITY_MATRIX_P, vPos);
	#if defined(SHADER_API_D3D11)
				depth = cPos.z / cPos.w;
	#else
				depth = (cPos.z / cPos.w) * 0.5 + 0.5;
	#endif
				if (depth <= 0)
					discard;


				float f = pow(1 - dot(vNormal, float3(0, 0, 1)), 2) * 2;
				float3 normal = normalize(vNormal.x * V_RIGHT + vNormal.y*V_UP + vNormal.z*V_FORWARD);

				half3 h = normalize(float3(1, 0, 0) + V_FORWARD);
				float nh = max(0, dot(normal, h));
				float spec = pow(nh, 128);

				float2 gUV = i.gPos.xy / i.gPos.w;
				float4 g = tex2D(_GrabTex, gUV + vNormal.xy*(_ScreenParams.zw - 1) * 150 * f);

				float4 c = i.color = lerp(i.color*i.color, i.color, normal.y+0.5);
				c = c*(0.8 + f)+g*(0.5 + 0.5*c);
				c = lerp(g, c, f*0.5+0.25) + spec;
				float dist = abs(i.vCenter.z);
				c = lerp(c, g, saturate(dist*0.05));
				return c;
			}
			ENDCG
		}
	}
}
