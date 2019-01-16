Shader "Custom/Hex"
{
	Properties
	{
		_Color ("Color Texture", Color) = (1, 1, 1, 1)
		_Ambient ("Ambient", Range(0,1)) = 0.5
		_Radius ("Radius", Range(0, 15)) = 8.0
		_RimValue ("Rim Value", Range(0, 1)) = 0.5
		_Glow ("Glow", Range(0, 50)) = 20
	}
	SubShader
	{
		Tags { "LightMode" = "ForwardBase" }

		Blend SrcAlpha One
		//Cull off

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
				float4 normal : NORMAL;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD1;
				float4 color : COLOR;
				float3 normalDir : TEXCOORD3;
                float4 worldPos : TEXCOORD2;
			};

			float4 _Color;
			float _Ambient;
			float _Radius;
			float4 _PlayerPos;
			float _RimValue;
			float _Glow;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.color = float4(0,0,0,1);
				o.pos = UnityObjectToClipPos(v.vertex);

				// float4 vertexWorld = mul(UNITY_MATRIX_M, v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				if(distance(_PlayerPos, o.worldPos/*vertexWorld*/) < _Radius)
				{
					o.color = float4(0,0,1,1);
					//vertexWorld.y = _PlayerPos.y - 1.0f;
					o.worldPos.y = _PlayerPos.y - 1.0f;
				}

				o.pos = mul(UNITY_MATRIX_VP, o.worldPos/*vertexWorld*/);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.normalDir = UnityObjectToWorldNormal(v.normal);

				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				// Diffuse lighting
				float3 normal = normalize(i.worldNormal);
				float nl = max(_Ambient, dot(normal, _WorldSpaceLightPos0.xyz));
				float4 diffuseTerm = nl * _LightColor0;

				// Get normalized world normal
                float3 normalDir = normalize(i.normalDir);
                // Get view direction in world space
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				// Holo rim effect
				float border = 1 - (abs(dot(viewDirection, normalDir)));
				float alpha = border * border * _RimValue;

				// Cirlce
				float dist = distance(i.worldPos, _PlayerPos.xyz);
				if(dist >= _Radius)
				{
					return fixed4(_Color.rgb, alpha);
				}
				else
				{
					return fixed4(_Glow * i.color.rgb * _Color.rgb * diffuseTerm.xyz, alpha);
				}
			}

			ENDHLSL
		}
	}
}
