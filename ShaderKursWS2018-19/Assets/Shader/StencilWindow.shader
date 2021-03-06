﻿Shader "Custom/StencilWindow"
{
	Properties {
        _Color ("Colour", Color) = (1,1,1,1)
		_Ambient ("Ambient", Range (0, 1)) = 0.25
        _RefNumber("Stencil number", Int) = 1

		_MainTex("Albedo (RGB)", 2D) = "white"{}
    }
    SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Opaque" "Queue"="Geometry-100"}
			ColorMask 0
			ZWrite off

			Stencil 
			{
				Ref [_RefNumber]
				Comp Always
				Pass replace
			}
 
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
		
			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD0;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return 0;
			}

			ENDHLSL
		}

		Pass
		{
			Tags { "RenderType"="Transparent" "Queue"="Transparent"}
			Blend SrcAlpha OneMinusSrcAlpha
 
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
				float3 worldNormal : TEXCOORD0;
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

				return _Color * nl;
			}

			ENDHLSL
		}

			UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
    }
}
