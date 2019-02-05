Shader "Custom/Barrier"
{
    Properties
    {

		//can be set from another script: 1 is active, 0 is not active
		_BarrierActive ("Barrier is active", int) = 0

        //_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

		_BarrierTex ("Barrier Texture", 2D) = "white"{}
		_BarrierColor ("Barrier Color", Color) = (1, 0.2, 0.5, 0.5)
		_BarrierThickness ("Barrier Thickness", Range(0, 0.5)) = 0.05
		_BarrierAlpha ("Barrier Alpha", Range(0, 1)) = 0.25


		//Barrier pulse properties
		_PulseIntensity ("Pulse Intensity", float) = 4.0
		_PulseDistanceModifier ("Pulse Distance Modifier", float) = 20.0
		_PulseTextureModifier ("Pulse Distance Texture Modifier", float) = 0.5
		_PulseCycleModifier ("Pulse Cycle Modifier", float) = 3.0
		
		//Hex edge pulse properties
		_EdgePulseColor ("Edge Pulse Color", COLOR) = (0,0,0,0)
		_EdgePulseIntensity ("Edge Pulse Intensity", float) = 10.0
		_EdgeDistanceModifier ("Edge Pulse Distance Modifier", float) = 20.0
		_EdgeCycleModifier ("Edge Pulse Cycle Modifier", float) = 3.0
		_EdgeWidthModifier ("Edge Pulse Width Modifier", Range(0,0.99)) = 0.99
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Fade" }
        //LOD 200

		
		CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 5.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
			float3 worldNormal;
			float3 vertex;
			INTERNAL_DATA
        };

        half _Glossiness;
        half _Metallic;
        //fixed4 _Color;

		sampler2D _BarrierTex;
		fixed4 _BarrierColor;
		int _BarrierActive;
		float _BarrierThickness;
		float _BarrierAlpha;


		
			
			float _PulseIntensity;
			float _PulseDistanceModifier;
			float _PulseTextureModifier;
			float _PulseCycleModifier;

			float4 _EdgePulseColor;
			float _EdgePulseIntensity;
			float _EdgeDistanceModifier;
			float _EdgeCycleModifier;
			float _EdgeWidthModifier;

		//vertex Shader
		void vert(inout appdata_full v)
		{
			if(_BarrierActive == 1)
			{
				v.vertex.xyz += _BarrierThickness * v.normal;
			} else
			{
				v.vertex.xyz -= _BarrierThickness * v.normal;
			}
		}

        //// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        //// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        //// #pragma instancing_options assumeuniformscaling
        //UNITY_INSTANCING_BUFFER_START(Props)
        //    // put more per-instance properties here
        //UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			if(_BarrierActive == 1)
			{
				//Calculate helper variables
				float horizontalDist = abs(IN.vertex.x);
				float verticalDist = abs(IN.vertex.z);

				// Albedo comes from a texture tinted by color
				fixed4 barrierTex = tex2D (_BarrierTex, IN.uv_MainTex);

				//Calculate pulse term
				fixed4 pulseTex = barrierTex.g;
				fixed4 pulseTerm = abs(sin(horizontalDist * _PulseDistanceModifier - _Time.y * _PulseCycleModifier - barrierTex.r * _PulseTextureModifier)) * pulseTex * _BarrierColor * _PulseIntensity;

				//Calculate edge pulse term
				fixed4 edgePulseTex = barrierTex.r;
				fixed4 edgePulseTerm = max(sin((horizontalDist + verticalDist) * _EdgeDistanceModifier - _Time.y * _EdgeCycleModifier) - _EdgeWidthModifier, 0.0f) * edgePulseTex * _EdgePulseColor * _EdgePulseIntensity * (1 / (1-_EdgeWidthModifier));



				o.Albedo = _BarrierColor * (pulseTerm + edgePulseTerm).rgb;
				
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = _BarrierAlpha;
			} else
			{
				o.Alpha = 0;
			}
			
        }
        ENDCG
		

        
    }
    FallBack "Diffuse"
}
