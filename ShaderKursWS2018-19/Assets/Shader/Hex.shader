Shader "Custom/Hex"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color Texture", Color) = (1, 1, 1, 1)
		_Ambient ("Ambient", Range(0,1)) = 0.5
		_Radius ("Radius", Range(0, 15)) = 8.0
		_RimValue ("Rim Value", Range(0, 1)) = 0.5
		_Glow ("Glow", Range(0, 50)) = 20
		_FresnelPower ("Fresnel Power", Range(0,5)) = 2
	}
	SubShader
	{
		Tags { "LightMode" = "ForwardBase" }

		Cull Back
		ZWrite On
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
				// float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD1;
				// float4 color : COLOR;
				float3 normalDir : TEXCOORD3;
                float4 worldPos : TEXCOORD2;
			};

			float4 _Color;
			float _Ambient;
			float _Radius;
			float4 _PlayerPos;
			float _RimValue;
			float _Glow;
			float _FresnelPower;

			sampler2D _GlobalDissolveToBlackPattern;
			float4 _GlobalDissolveToBlackPatternST;
			float4 _GlobalDissolveToBlackVisualArea;
			
			v2f vert (appdata v)
			{
				v2f o;
				//o.color = float4(0,0,0,1);
				o.pos = UnityObjectToClipPos(v.vertex);

				// float4 vertexWorld = mul(UNITY_MATRIX_M, v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				if(distance(_PlayerPos, o.worldPos/*vertexWorld*/) < _Radius)
				{
					//o.color = float4(0,0,1,1);
					//vertexWorld.y = _PlayerPos.y - 1.0f;
					o.worldPos.y = _PlayerPos.y - 0.5f;
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

				float fresnelEffect = pow((1.0f - saturate(dot(normalize(normalDir), normalize(viewDirection)))), _FresnelPower);

				// Cirlce
				fixed4 final;
				float dist = distance(i.worldPos, _PlayerPos.xyz);
				if(dist >= _Radius)
				{
					final = fixed4(_Color.rgb * diffuseTerm, 1);
				}
				else
				{
					final = fixed4(_Glow * fresnelEffect * _Color.rgb * diffuseTerm, 1);
				}

				// Global Dissolve
				// get borders of visible area
				alpha = 1;
				float2 coord = i.worldPos.xz / _GlobalDissolveToBlackPatternST.xy + _GlobalDissolveToBlackPatternST.zw;
				if (coord.x > 1)
					coord.x - 1;
				if (coord.y > 1)
					coord.y - 1;
				if (coord.x < 0)
					coord.x + 1;
				if (coord.y < 0)
					coord.y + 1;
				float borderLeft = (_GlobalDissolveToBlackVisualArea.r + tex2D(_GlobalDissolveToBlackPattern, coord));
				float borderRight = (_GlobalDissolveToBlackVisualArea.g + (1 - tex2D(_GlobalDissolveToBlackPattern, coord)));
				float borderBottom = (_GlobalDissolveToBlackVisualArea.b + tex2D(_GlobalDissolveToBlackPattern, coord));
				float borderTop = (_GlobalDissolveToBlackVisualArea.a + (1 - tex2D(_GlobalDissolveToBlackPattern, coord)));
				if (i.worldPos.x < borderLeft
					|| i.worldPos.x > borderRight - 1
					|| i.worldPos.z < borderBottom
					|| i.worldPos.z > borderTop - 1) {
					alpha = 0;
				}

				return final * alpha;
			}

			ENDHLSL
		}

		UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
	}
}
