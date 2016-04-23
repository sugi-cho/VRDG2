Shader "Unlit/FullScreenZTest"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_DepthTex("depth", 2D) = "black"{}
		_NA("noise amount",Float) = 0
		_DA("depth amount",Float) = 0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" "RenderQueue" = "Transparent"}
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex,_DepthTex,_NoiseTex;
				float _NA,_DA;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = float4(v.vertex.xy*2.0, 1, 1);
					o.uv = v.uv;
					return o;
				}

				half4 frag(v2f i, out float depth : SV_Depth) : SV_Target
				{
					half4 n = tex2D(_NoiseTex, i.uv);
					float2 uv = i.uv + n.xy*(_ScreenParams.zw-1)*_NA;
					half4 col = tex2D(_MainTex, uv);
					depth = tex2D(_DepthTex,uv).r + (n.z - 0.5)*_ProjectionParams.w*_DA;
					return col;
				}
				ENDCG
			}
		}
}
