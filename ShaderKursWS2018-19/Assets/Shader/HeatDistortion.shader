Shader "Custom/HeatDistortion"
{
	Properties
	{
		// Properties inspired by After Effects Heat Distortion Plug-in
		// Shader can only be used on overlay canvases or on worldspace meshes iff camera is set to orthographic

		_Pattern ("Noise Pattern", 2D) = "white" {}		// red channel for tint intensity
														// green channel for horizonal distortion, blue for vertical
														// needs to be seamless
														// use plasma.png for testing
		_MainTex ("Effect Shape", 2D) = "white" {}			// determines how the effect shape looks like
														// red/white for full opaque
														// use the unity standard particle for testing
		_Amount ("Distortion Amount", Float) = .02		// how much the image is distorted
		_Size ("Noise Size", Float) = 1					// size of the noise pattern
		_SpeedX ("Horizontal Wind Speed", Float) = 2	// movement of the noise pattern
		_SpeedY ("Horizontal Wind Speed", Float) = 10	// movement of the noise pattern
		_Color ("Tint", Color) = (1,0,0,0)				// color that will be added to the result
		_Intensity ("Intensity", Float) = 1				// strength of the tint
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
				float2 noiseUV : TEXCOORD1;
				float2 shapeUV : TEXCOORD2;
				float4 worldPos : TEXCOORD3;
			};

			sampler2D _BackgroundTexture;

			sampler2D _Pattern;
			sampler2D _MainTex;
			float _Amount;
			float _Size;
			float _SpeedX;
			float _SpeedY;
			fixed4 _Color;
			float _Intensity;

			sampler2D _GlobalDissolveToBlackPattern;
			float4 _GlobalDissolveToBlackPatternST;
			float4 _GlobalDissolveToBlackVisualArea;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = ComputeGrabScreenPos(o.pos);

				// get current noise position
				o.noiseUV = o.uv;
				float2 move = float2(-_SpeedX, _SpeedY) * _Time.x;
				o.noiseUV += move;
				// scale uv to resize noise pattern
				o.noiseUV /= _Size;

				o.shapeUV = v.uv;

				o.worldPos = mul(unity_ObjectToWorld, v.vertex);;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed3 noise = tex2D(_Pattern, i.noiseUV);

			// calculate uv offset
			float2 offset = float2(noise.g - .5, noise.b - .5) * _Amount;

			// get image pixel
			float2 uv = i.uv + offset;
			fixed3 background = tex2D(_BackgroundTexture, uv);

			// add tint
			fixed3 tint = _Color.rgb * _Intensity * noise.r;

			// effect shape
			float alpha = tex2D(_MainTex, i.shapeUV).a;


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

			return float4(background + tint, alpha);
		}

		ENDHLSL
		}

		UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
	}

}
