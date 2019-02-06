Shader "Custom/CustomStandard"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Top", 2D) = "white" {}
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
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
					float3 normal : TEXCOORD1;
					float4 worldPos : TEXCOORD2;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _GlobalEquator;
				float _GlobalBottom;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					o.normal = UnityObjectToWorldNormal(v.normal);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// sample the texture
					fixed4 col;
					col = tex2D(_MainTex, i.uv);

					// Diffuse lightning
					float nl = max(unity_AmbientSky, dot(normalize(i.normal), _WorldSpaceLightPos0.xyz));
					col = nl * col * _LightColor0;

					// Vertical fog
					col *= min(max(0, (i.worldPos.y - _GlobalBottom) / (_GlobalEquator - _GlobalBottom)), 1);

					return col;
				}
				ENDCG
			}


			UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
		}
}
