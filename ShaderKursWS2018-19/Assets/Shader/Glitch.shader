Shader "Custom/Glitch"
{
	Properties
	{
		_Pattern("Noise Pattern", 2D) = "white" {}
		_MainTex("Effect Shape", 2D) = "white" {}
		_Effect("Effect", Vector) = (0,0,0,0)
		_Inversion("Inversion ST", Vector) = (1,1,0,0)
		_Horizontal("Horizontal ST", Vector) = (1,1,0,0)
		_Vertical("Vertical ST", Vector) = (1,1,0,0)
		_Void("Void ST", Vector) = (1,1,0,0)
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha

			GrabPass
			{
				"_BackgroundTexture"
			}

			Pass
			{
				HLSLPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float2 shapeUV : TEXCOORD1;
					float4 worldPos : TEXCOORD2;
				};

				sampler2D _BackgroundTexture;

				sampler2D _Pattern;
				float4 _Pattern_ST;
				sampler2D _MainTex;
				float4 _Effect;
				float4 _Inversion;
				float4 _Horizontal;
				float4 _Vertical;
				float4 _Void;

				sampler2D _GlobalDissolveToBlackPattern;
				float4 _GlobalDissolveToBlackPatternST;
				float4 _GlobalDissolveToBlackVisualArea;

				v2f vert(appdata v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = ComputeGrabScreenPos(o.pos);

					o.shapeUV = v.uv;

					o.worldPos = mul(unity_ObjectToWorld, v.vertex);;

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// calculate final uv
					float2 finalUV = i.uv;

				// get horizontal distortion
					float2 uv = (i.uv - _Horizontal.zw) * _Horizontal.xy;
				uv *= TRANSFORM_TEX(uv, _Pattern);
				fixed value = tex2D(_Pattern, uv).g;
				if (value > 0) {
					finalUV.x += _Effect.g;
				}

				// get vertical distortion
				uv = (i.uv - _Vertical.zw) * _Vertical.xy;
				uv *= TRANSFORM_TEX(uv, _Pattern);
				value = tex2D(_Pattern, uv).b;
				if (value > 0) {
					finalUV.y += _Effect.b;
				}

				// get albedo
				fixed3 albedo = tex2D(_BackgroundTexture, finalUV);

				// get inversion
				uv = (i.uv - _Inversion.zw) * _Inversion.xy;
				uv *= TRANSFORM_TEX(uv, _Pattern);
				value = tex2D(_Pattern, uv).r;
				if (value > 0 && _Effect.x > 0) {
					albedo = 1 - albedo;
				}

				// get void
				uv = (i.uv - _Void.zw) * _Void.xy;
				uv *= TRANSFORM_TEX(uv, _Pattern);
				value = tex2D(_Pattern, uv).a;
				if (value < 1 && _Effect.w > 0) {
					albedo = float3(0,0,0);
				}

				// effect shape
				float alpha = tex2D(_MainTex, i.shapeUV).a;


				// Global Dissolve
				// get borders of visible area
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

				return float4(albedo, alpha);
			}

			ENDHLSL
			}

			UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
		}

}
