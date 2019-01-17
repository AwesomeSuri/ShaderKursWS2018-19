Shader "Custom/DiffuseTexture"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Ambient ("Ambient", Range (0, 1)) = 0.25
	}
	SubShader
	{
		Tags { "LightMode" = "ForwardBase" }

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD1;
				float2 uv : TEXCOORD0;
			};
				
			sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;
			float _Ambient;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				fixed4 tex = tex2D(_MainTex, i.uv);
				float nl = max(_Ambient, dot(normal, _WorldSpaceLightPos0.xyz));
				float4 result = nl * _Color * tex * _LightColor0;
				return result;
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
