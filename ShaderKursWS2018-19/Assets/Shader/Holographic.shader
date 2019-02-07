Shader "Custom/Holographic"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_BumpMap ("Bump Map", 2D) = "bump" {}
		_Color("Color", Color) = (1,0,0,1)
		//_Ambient("Ambient", Range(0, 1)) = 0.25
		_Emmision("Emmision", Range(0, 10)) = 6.5
			//_Rim("Rim Effect", Range(-1,5)) = 0.25
			_Speed("Speed", Range(0, 2)) = 0.1
			_Equator("Equator", Float) = -.1

			_IntersectRadius("Radius", Float) = .5
			_IntersectColor("Intersect Color", Color) = (1,1,1,1)
			_IntersectIntensity("Intensity", Float) = 1
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Blend SrcAlpha One
		//Cull off
		LOD 200

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
			//float _Ambient;
            float _Emmision;
            float _Speed;
            sampler2D _UndoEffectTexture; 
            float4 _UndoEffectTexture_ST;

			sampler2D _Pattern;
			float _Dissolve;
			float _DissolveEdge;
			fixed4 _DissolveGlow;
			float _DissolveIntensity;

			float _Equator;
			float _IntersectRadius;
			fixed4 _IntersectColor;
			float _IntersectIntensity;

			sampler2D _GlobalDissolveToBlackPattern;
			float4 _GlobalDissolveToBlackPatternST;
			float4 _GlobalDissolveToBlackVisualArea;

			float3 _PlayerPos;

			
			v2f vert (appdata v)
			{
				v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTexture);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			//float _Rim;

			fixed4 frag (v2f i) : SV_Target
			{
                // Get normalized world normal
                float3 normalDir = normalize(i.normalDir);

                // Get view direction in world space
                //float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);

				// get global uv
				float2 uv;
				if (i.worldPos.y > _Equator) {
					uv = i.worldPos.zx;
				}
				else {
					uv = float2(i.worldPos.x + i.worldPos.z, i.worldPos.y);
				}

				// Get the normals from the bump map
				float2 newUVBumpMap = TRANSFORM_TEX(uv, _BumpMap);
                float3 normals = UnpackNormal(tex2D(_BumpMap, newUVBumpMap));
                float2 offset = uv + _Time * _Speed * float2(1,1) + float2(normals.r, normals.g);
				float2 newUVmainTex = TRANSFORM_TEX(offset, _MainTex);
                float4 mainTex = tex2D(_MainTex, newUVmainTex);

                // Calculate final color
                float3 final = _Color.rgb * mainTex.xyz * _Emmision;

				// Holo rim effect
				//float border = 1 - (abs(dot(viewDirection, normalDir)));
				//float alpha = border * border * _Rim; //(border * (1.0f - _Rim) + _Rim);

				// Diffuse lightning
				//float nl = max(_Ambient, dot(normalDir, _WorldSpaceLightPos0.xyz));
				//final = nl * final * _LightColor0; 
				
				// Dissolve
				float pattern = tex2D(_Pattern, i.uv).r;
				clip(pattern - _Dissolve);
				if (pattern - _Dissolve < _DissolveEdge && pattern > _DissolveEdge)
				{
					final = _DissolveGlow * _DissolveIntensity;
				}

				// Intersect
				float dist = distance(i.worldPos, _PlayerPos);
				if (dist < _IntersectRadius) {
					final += _IntersectColor.xyz * _IntersectIntensity;
				}

				// Global Dissolve
				// get borders of visible area
				float alpha = 1;
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

                // Return final color
                return fixed4(final, mainTex.a * alpha);
			}

			ENDHLSL
		}

			UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
	}
	Fallback "Standard"
}
