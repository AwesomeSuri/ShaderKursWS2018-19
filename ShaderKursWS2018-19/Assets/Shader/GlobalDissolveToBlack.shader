Shader "Custom/GlobalDissolveToBlack"
{
	// this shader is used as additional pass for other shaders that will also be globally dissolved to black
	// add at the end of the used shader: UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"

	SubShader{
		Tags { "RenderType" = "Transparent"}

		// not perfect behaviour for transparent objects but whatever...
		AlphaToMask On


		Pass
		{
			Name "DissolveToBlack"
			HLSLPROGRAM

			// not using surf shader because it's somehow still lit even though it's completely black
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			// controll these via script
			sampler2D _GlobalDissolveToBlackPattern;
			float4 _GlobalDissolveToBlackPatternST;
			float4 _GlobalDissolveToBlackVisualArea;
			float _GlobalDissolveToBlackGlowThickness;
			float _GlobalDissolveToBlackGlowIntensity;
			float4 _GlobalDissolveToBlackColor;

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

				// glow near border
				float3 result = (0, 0, 0);
				if (i.worldPos.x > borderLeft - _GlobalDissolveToBlackGlowThickness
					&& i.worldPos.x < borderRight - 1 + _GlobalDissolveToBlackGlowThickness
					&& i.worldPos.z > borderBottom - _GlobalDissolveToBlackGlowThickness
					&& i.worldPos.z < borderTop - 1 + _GlobalDissolveToBlackGlowThickness) {
					result = _GlobalDissolveToBlackColor.rgb * _GlobalDissolveToBlackGlowIntensity;
				}

				// draw only on non visible area
				float alpha = 1;
				if (i.worldPos.x >= borderLeft
					&& i.worldPos.x <= borderRight - 1
					&& i.worldPos.z >= borderBottom
					&& i.worldPos.z <= borderTop - 1) {
					alpha = 0;
				}

				return float4(result, alpha);
			}
				ENDHLSL
		}

	}
		Fallback "Standard"
}
