Shader "Unlit/TestTransform"
{
	Properties
	{
		_X("right vector", Vector) = (1,0,0)
		_Y("up vector", Vector) = (0,1,0)
		_Z("forward vector", Vector) = (0,0,1)
		_Look("look", Vector) = (0,0,1)
		_Up("up",Vector) = (0,1,0)
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Assets/CGINC/Transform.cginc"
			#include "Assets/CGINC/Quaternion.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				half4 color : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float3 _X, _Y, _Z, _Look, _Up;

			v2f vert(appdata v)
			{
				_X = normalize(_X);
				_Y = normalize(_Y);
				_Z = normalize(_Z);
				half4 color = half4(v.vertex.xyz, 1);
				//float3x3 rot = getRotateMatrix(_X, _Y, _Z);
				float3x3 rot = getLookRotationMatrix(_Look, _Up);
				//float4 q = matrix_to_quaternion(rot);
				v.vertex.xyz = mul(rot, v.vertex.xyz);
				//v.vertex.xyz = rotateWithQuaternion(v.vertex.xyz, q);
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = i.color;

				return col;
			}
			ENDCG
		}
	}
}
