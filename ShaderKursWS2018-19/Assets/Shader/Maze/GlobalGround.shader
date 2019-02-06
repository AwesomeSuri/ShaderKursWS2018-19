Shader "Custom/GlobalGround"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_Layer1("Layer 1", 2D) = "white" {}
		_Layer2("Layer 2", 2D) = "white" {}
		_Layer3("Layer 3", 2D) = "white" {}
		_Pattern("Pattern", 2D) = "black" {}
		_Threshold1("Threshold 1", Range (0, 1)) = 0
		_Threshold2("Threshold 2", Range(0, 1)) = 0
		_Threshold3("Threshold 3", Range(0, 1)) = 0
		_Blend("Blend", Range(0, 1)) = .1
		_Equator("Equator", Float) = 0
    }
    SubShader
    {
        Pass
        {
			Tags {"LightMode" = "ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Lighting.cginc"

			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
				float3 normal : TEXCOORD0;
				float4 worldPos : TEXCOORD1;
				SHADOW_COORDS(2)
            };

			fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _Layer1;
			float4 _Layer1_ST;
			sampler2D _Layer2;
			float4 _Layer2_ST;
			sampler2D _Layer3;
			float4 _Layer3_ST;
			sampler2D _Pattern;
			float4 _Pattern_ST;
			float _Threshold1;
			float _Threshold2;
			float _Threshold3;
			float _Blend;
			float _Equator;
			float _GlobalEquator;
			float _GlobalBottom;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				// use global pos for uv
				float2 uv;
				fixed4 col;
				if (i.worldPos.y >= _Equator) {
					uv.x = (i.worldPos.x - _MainTex_ST.z) * _MainTex_ST.x;
					uv.y = (i.worldPos.z - _MainTex_ST.w) * _MainTex_ST.y;
					fixed3 main = tex2D(_MainTex, uv);

					uv.x = (i.worldPos.x - _Layer1_ST.z) * _Layer1_ST.x;
					uv.y = (i.worldPos.z - _Layer1_ST.w) * _Layer1_ST.y;
					fixed3 layer1 = tex2D(_Layer1, uv);

					uv.x = (i.worldPos.x - _Layer2_ST.z) * _Layer2_ST.x;
					uv.y = (i.worldPos.z - _Layer2_ST.w) * _Layer2_ST.y;
					fixed3 layer2 = tex2D(_Layer2, uv);

					uv.x = (i.worldPos.x - _Layer3_ST.z) * _Layer3_ST.x;
					uv.y = (i.worldPos.z - _Layer3_ST.w) * _Layer3_ST.y;
					fixed3 layer3 = tex2D(_Layer3, uv);

					uv.x = (i.worldPos.x - _Pattern_ST.z) * _Pattern_ST.x;
					uv.y = (i.worldPos.z - _Pattern_ST.w) * _Pattern_ST.y;
					fixed3 pattern = tex2D(_Pattern, uv);

					float alpha1 = min(1, max(0, pattern.r - _Threshold1)/_Blend);
					float alpha2 = min(1, max(0, pattern.g - _Threshold2)/_Blend);
					float alpha3 = min(1, max(0, pattern.b - _Threshold3)/_Blend);

					col = fixed4(alpha3 * layer3 +
						(1 - alpha3) * (alpha2 * layer2 +
						(1 - alpha2) * (alpha1 * layer1 +
						(1 - alpha1) * main)),
						1);
				}
				else {
					uv.x = (i.worldPos.x + i.worldPos.z - _MainTex_ST.z) * _MainTex_ST.x;
					uv.y = (i.worldPos.y - _MainTex_ST.w) * _MainTex_ST.y;

					col = tex2D(_MainTex, uv);
				}

				// Diffuse lightning
				float atten = SHADOW_ATTENUATION(i);
				float nl = dot(normalize(i.normal), _WorldSpaceLightPos0.xyz);
				float intensity = nl * atten + unity_AmbientSky;
				col = intensity * col * _LightColor0;

				// Vertical fog
				col *= min(max(0, (i.worldPos.y - _GlobalBottom) / (_GlobalEquator - _GlobalBottom)), 1);

                return col * _Color;
            }
            ENDCG
        }


		UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"

		// shadow casting support
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
	Fallback "Diffuse"
}
