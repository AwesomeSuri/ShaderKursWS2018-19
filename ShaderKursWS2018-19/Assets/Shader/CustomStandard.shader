Shader "Custom/CustomStandard"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Top", 2D) = "white" {}
		_Emission("Emission", 2D) = "white" {}
		_EmissionColor("Emission Color", Color) = (1,1,1,1)
		_EmissionIntensity("Intensity", Float) = 0
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque"}
			LOD 100

			Pass
			{

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "UnityLightingCommon.cginc"
				#include "AutoLight.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float3 normal : TEXCOORD1;
					float4 worldPos : TEXCOORD2;
					LIGHTING_COORDS(3, 4)
				};

				fixed4 _Color;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _Emission;
				float4 _Emission_ST;
				fixed4 _EmissionColor;
				float _EmissionIntensity;

				float _GlobalEquator;
				float _GlobalBottom;

				v2f vert(appdata v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					o.normal = UnityObjectToWorldNormal(v.normal);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
					TRANSFER_VERTEX_TO_FRAGMENT(o);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// Albedo
					fixed4 col;
					float2 uv = (i.uv - _MainTex_ST.zw) / _MainTex_ST.xy;
					col = tex2D(_MainTex, uv) * _Color;

					// Diffuse lightning
					float atten = LIGHT_ATTENUATION(i);
					float nl = max(unity_AmbientSky, dot(normalize(i.normal), _WorldSpaceLightPos0.xyz));
					col = nl * col * _LightColor0 * atten;

					// Emission
					fixed4 emit;
					uv = (i.uv - _Emission_ST.zw) / _Emission_ST.xy;
					emit = tex2D(_Emission, uv) * _EmissionColor * _EmissionIntensity;
					col += emit;

					// Vertical fog
					col *= max(0, min(1, (i.worldPos.y - _GlobalBottom) / (_GlobalEquator - _GlobalBottom)));

					return col;
				}
				ENDCG
			}


			UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
		}
}
