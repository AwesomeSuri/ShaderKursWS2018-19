Shader "Custom/Holographic"
{
	Properties
	{
		_MainTexture ("Main Texture", 2D) = "white" {}
		_BumpMap ("Bump Map", 2D) = "bump" {}
		_Color ("Color", Color) = (1,0,0,1)
		_Ambient ("Ambient", Range (0, 1)) = 0.25
        _Emmision ("Emmision", Range(0, 10)) = 6.5
		_Rim("Rim Effect", Range(-1,5)) = 0.25
		_Speed ("Speed", Range(0, 2)) = 0.1
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
				
			sampler2D _MainTexture;
            float4 _MainTexture_ST;
            sampler2D _BumpMap;
            float4 _BumpMap_ST;
			float4 _Color;
			float _Ambient;
            float _Emmision;
            float _Speed;
            sampler2D _UndoEffectTexture; 
            float4 _UndoEffectTexture_ST;

			sampler2D _Pattern;
			float _Dissolve;
			float _DissolveEdge;
			fixed4 _DissolveGlow;
			float _DissolveIntensity;

			
			v2f vert (appdata v)
			{
				v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTexture);
				o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			float _Rim;

			fixed4 frag (v2f i) : SV_Target
			{
                // Get normalized world normal
                float3 normalDir = normalize(i.normalDir);

                // Get view direction in world space
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);

				// Get the normals from the bump map
				float2 newUVBumpMap = TRANSFORM_TEX(i.uv, _BumpMap);
                float3 normals = UnpackNormal(tex2D(_BumpMap, newUVBumpMap));
                float2 offset = i.uv + _Time * _Speed + float2(normals.r, normals.g);
				float2 newUVmainTex = TRANSFORM_TEX(offset, _MainTexture);
                float4 mainTex = tex2D(_MainTexture, newUVmainTex);

                // Calculate final color
                float3 final = _Color.rgb * mainTex.xyz * _Emmision;

				// Holo rim effect
				float border = 1 - (abs(dot(viewDirection, normalDir)));
				float alpha = border * border * _Rim; //(border * (1.0f - _Rim) + _Rim);

				// Diffuse lightning
				float nl = max(_Ambient, dot(normalDir, _WorldSpaceLightPos0.xyz));
				final = nl * final * _LightColor0; 
				
				// Dissolve
				float pattern = tex2D(_Pattern, i.uv).r;
				clip(pattern - _Dissolve);
				if (pattern - _Dissolve < _DissolveEdge && pattern > _DissolveEdge)
				{
					final = _DissolveGlow * _DissolveIntensity;
				}

                // Return final color
                return fixed4(final, mainTex.a * alpha);
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
