Shader "Custom/DiffuseColor"
{
	Properties
	{
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
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD1;
			};
				
			float4 _Color;
			float _Ambient;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float nl = max(_Ambient, dot(normal, _WorldSpaceLightPos0.xyz));
				float4 result = nl * _Color * _LightColor0;
				return result;
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
