Shader "Custom/GlobalDissolveToBlack"
{
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_DissolveGlowColor("Dissolve glow colour", Color) = (0,1,1,1)
		_DissolveGlowOffset("Dissolve Glow Offset", Float) = .1
		_DissolveGlowIntensity("Dissolve Glow Intensity", Float) = 10
		_Dissolve("Dissolve", Vector) = (-5,5,-5,5)						// border of the visible area (left, right, bottom, top) on xz-plane
																		// controll this via script to change this globally
		_DissolveSize("Dissolve Size", Float) = 1
		_Pattern("Pattern", 2D) = "white" {}
	}

	SubShader{
		Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" }

		Pass
		{
			HLSLPROGRAM

			// not using surf shader because it's somehow still lit even though it's completely black
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"


			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _Ambient;

			sampler2D _Pattern;
			float4 _Pattern_ST;
			float4 _Dissolve;
			half _DissolveSize;
			half _DissolveGlowOffset;
			float _DissolveGlowIntensity;
			float4 _DissolveGlowColor;

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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// get borders of visible area
				float2 coord = i.worldPos.xz / _Pattern_ST.xy + _Pattern_ST.zw;
				if (coord.x > 1)
					coord.x - 1;
				if (coord.y > 1)
					coord.y - 1;
				if (coord.x < 0)
					coord.x + 1;
				if (coord.y < 0)
					coord.y + 1;
				float borderLeft = (_Dissolve.r + tex2D(_Pattern, coord));
				float borderRight = (_Dissolve.g + (1 - tex2D(_Pattern, coord)));
				float borderBottom = (_Dissolve.b + tex2D(_Pattern, coord));
				float borderTop = (_Dissolve.a + (1 - tex2D(_Pattern, coord)));

				// draw black if outside area
				int intensity = 0;
				if (i.worldPos.x >= borderLeft
					&& i.worldPos.x <= borderRight - 1
					&& i.worldPos.z >= borderBottom
					&& i.worldPos.z <= borderTop - 1) {
					intensity = 1;
				}

				// get albedo (mostly copied from course 1: Diffuse)
				float2 uv = i.uv / _MainTex_ST.xy + _MainTex_ST.zw;
				if (uv.x > 1)
					uv.x - 1;
				if (uv.y > 1)
					uv.y - 1;
				if (uv.x < 0)
					uv.x + 1;
				if (uv.y < 0)
					uv.y + 1;
				float3 normal = normalize(i.normalDir);
				float3 tex = tex2D(_MainTex, uv);
				//Basic n dot l lighting (Lambert), minimum is ensured to be at least _Ambient
				float nl = dot(normal, _WorldSpaceLightPos0.xyz);
				//Final color blending. Color blending is usually multiplicative!
				float3 result = nl * _Color * tex * _LightColor0 + unity_AmbientSky;


				// glow near border
				if (i.worldPos.x < borderLeft + _DissolveGlowOffset
					|| i.worldPos.x > borderRight - 1 - _DissolveGlowOffset
					|| i.worldPos.z < borderBottom + _DissolveGlowOffset
					|| i.worldPos.z > borderTop - 1 - _DissolveGlowOffset) {
					result = _DissolveGlowColor.rgb * _DissolveGlowIntensity;
				}

				return float4(result * intensity, 1);
			}
				ENDHLSL
		}

	}
		Fallback "Standard"
}
