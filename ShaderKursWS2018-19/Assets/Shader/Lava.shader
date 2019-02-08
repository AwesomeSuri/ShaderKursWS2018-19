Shader "Custom/Lava"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
        _BumpMap ("Bump Map", 2D) = "bump" {}
		_Color ("Color", Color) = (1,0,0,1)
		_Ambient ("Ambient", Range (0, 1)) = 0.25
        _Emmision ("Emmision", Range(5, 10)) = 6.5
        _Speed ("Speed", Range(0, 5)) = 2.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }

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
                float2 uv : TEXCOORD0;
				float3 normalDir : TEXCOORD1;
                float4 worldPos : TEXCOORD2;
			};
				
			sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _BumpMap;
            float4 _BumpMap_ST;
			float4 _Color;
			float _Ambient;
            float _Emmision;
            float _Speed;
            sampler2D _UndoEffectTexture; 
            float4 _UndoEffectTexture_ST;

			sampler2D _GlobalDissolveToBlackPattern;
			float4 _GlobalDissolveToBlackPatternST;
			float4 _GlobalDissolveToBlackVisualArea;
			
			v2f vert (appdata v)
			{
				v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
                // Get normalized world normal
                float3 normalDir = normalize(i.normalDir);

                // Get view direction in world space
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);

                // Get time
                float time = _Time.y;

                // Get the normals from the bump map and move them over time
                float2 flowUV = i.uv + time * _Speed * 0.01f;
                float2 newFlowUV = TRANSFORM_TEX(flowUV, _BumpMap);
                float3 normals = UnpackNormal(tex2D(_BumpMap, newFlowUV));

                // Add normals to texture
                float2 offset1 = i.uv +  time * _Speed * 0.01f + float2(normals.r, normals.g);
                float2 newUV1 = TRANSFORM_TEX(offset1, _MainTex);
                float4 mainTex1 = tex2D(_MainTex, newUV1);
                
                // Move the texture over time
                float2 offset2 = i.uv + time * _Speed * 0.01f;
                float2 newUV2 = TRANSFORM_TEX(offset2, _MainTex);
                float4 mainTex2 = tex2D(_MainTex, newUV2);
                
                // Calculate result color
                float3 result = _Color.rgb * mainTex1.xyz * mainTex2.xyz * _Emmision;

                // Dot product of normal direction and view direction
                float d = dot(normalDir, viewDirection);

                // Calculate final color
                float3 final = result * saturate(d);

				// Diffuse lightning
				float nl = max(_Ambient, dot(normalDir, _WorldSpaceLightPos0.xyz));
				final = nl * final * _LightColor0;

				float alpha = 1;
				// Global Dissolve
				// get borders of visible area
				float2 coord = i.worldPos.xz / _GlobalDissolveToBlackPatternST.xy + _GlobalDissolveToBlackPatternST.zw;
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

                // Return final color
                return fixed4(final * alpha, alpha);
			}

			ENDHLSL
		}

		UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
	}
	Fallback "Standard"
}
