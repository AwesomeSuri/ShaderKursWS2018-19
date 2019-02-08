Shader "Custom/Flag"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,0,0,1)
		_Ambient ("Ambient", Range (0, 1)) = 0.25
        _Speed ("Sin Speed", Range(0, 10)) = 4
        _Frequency ("Sin Frequency", Range(0, 10)) = 1
        _Amplitude ("Sin Amplitude", Range(0, 1)) = 0.5
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
				float4 worldPos : TEXCOORD2;
			};
				
			sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;
			float _Ambient;

            // Sin values
            float _Speed;
            float _Frequency;
            float _Amplitude;

			sampler2D _GlobalDissolveToBlackPattern;
			float4 _GlobalDissolveToBlackPatternST;
			float4 _GlobalDissolveToBlackVisualArea;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                //o.vertex.y += sin(worldPos.x + _Time.w);
                o.vertex.y += sin((v.vertex.x + _Time.y * _Speed) * _Frequency) * _Amplitude * v.vertex.x;


				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				fixed4 tex = tex2D(_MainTex, i.uv);
				float nl = max(_Ambient, dot(normal, _WorldSpaceLightPos0.xyz));
				float4 result = nl * _Color * tex * _LightColor0;


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

				return result * alpha;
			}

			ENDHLSL
		}

		UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
	}
	Fallback "Standard"
}
