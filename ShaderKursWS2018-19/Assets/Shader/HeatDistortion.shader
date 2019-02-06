﻿Shader "Custom/HeatDistortion"
{
	Properties
	{
		// Properties inspired by After Effects Heat Distortion Plug-in
		// Shader can only be used on overlay canvases or on worldspace meshes iff camera is set to orthographic

		_MainTex ("Noise Pattern", 2D) = "white" {}		// red channel for tint intensity
														// green channel for horizonal distortion, blue for vertical
														// needs to be seamless
														// use plasma.png for testing
		_Shape("Effect Shape", 2D) = "white" {}			// determines how the effect shape looks like
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
			};

			sampler2D _BackgroundTexture;

			sampler2D _MainTex;
			sampler2D _Shape;
			float _Amount;
			float _Size;
			float _SpeedX;
			float _SpeedY;
			fixed4 _Color;
			float _Intensity;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = ComputeGrabScreenPos(o.pos);

				// get current noise position
				o.noiseUV = o.uv;
				float2 move = float2(-_SpeedX, _SpeedY) * _Time.x;
				o.noiseUV += move;
				// prevent large numbers
				if (o.noiseUV.x > 1)
					o.noiseUV.x -= 1;
				if (o.noiseUV.y > 1)
					o.noiseUV.y -= 1;
				if (o.noiseUV.x < 0)
					o.noiseUV.x += 1;
				if (o.noiseUV.y < 0)
					o.noiseUV.y += 1;
				// scale uv to resize noise pattern
				o.noiseUV /= _Size;

				o.shapeUV = v.uv;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed3 noise = tex2D(_MainTex, i.noiseUV);

			// calculate uv offset
			float2 offset = float2(noise.g - .5, noise.b - .5) * _Amount;

			// get image pixel
			float2 uv = i.uv + offset;
			fixed3 background = tex2D(_BackgroundTexture, uv);

			// add tint
			fixed3 tint = _Color.rgb * _Intensity * noise.r;

			// effect shape
			float alpha = tex2D(_Shape, i.shapeUV).r;

			return float4(background + tint, alpha);
		}

		ENDHLSL
		}

		UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
	}

}