Shader "Custom/Spotlight"
{
	Properties
	{
		_MainTexture ("Texture", 2D) = "white" {}
		_BumpMap ("Bump Map", 2D) = "bump" {}
		_Color ("Color", Color) = (1,0,0,1)
		_Ambient ("Ambient", Range (0, 1)) = 0.25
		_Radius ("Radius", Range(0, 15)) = 8.0
		_RingSize ("Ring size", Range(0, 5)) = 2.0
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
				// float4 color : COLOR;
				float3 worldPos : TEXCOORD2;
			};
				
			sampler2D _MainTexture;
            float4 _MainTexture_ST;
			sampler2D _BumpMap;
            float4 _BumpMap_ST;
			float4 _Color;
			float _Ambient;
			float _Radius;
			float _RingSize;

			float3 _PlayerPos;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.uv = TRANSFORM_TEX(v.uv, _MainTexture);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// Resulting color
				float4 result = _Color;

				// Calculate diffuse lighting
				float3 normal = normalize(i.worldNormal);
				float nl = max(_Ambient, dot(normal, _WorldSpaceLightPos0.xyz));
				float4 diffuseLight = nl * _LightColor0;

				// Get the normals from the bump map
				float2 newUVBumpMap = TRANSFORM_TEX(i.uv, _BumpMap);
                float3 normals = UnpackNormal(tex2D(_BumpMap, newUVBumpMap));
                float2 offset = i.uv + float2(normals.r, normals.g);
				float2 newUVmainTex = TRANSFORM_TEX(offset, _MainTexture);
                float4 mainTex = tex2D(_MainTexture, newUVmainTex);

				// Circle spotlight
				float dist = distance(i.worldPos, _PlayerPos.xyz);

				if(dist < _Radius)
				{
					result = mainTex * diffuseLight;
				}
				else if(dist > _Radius && dist < _Radius + _RingSize)
				{
					float blendStrength = dist - _Radius;
					result = lerp(mainTex, _Color, blendStrength / _RingSize);
					result *= diffuseLight;
				}
				else
				{
					result = float4(0,0,0,0);
				}

				

				return result;
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
