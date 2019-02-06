Shader "Custom/GlobalGround"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Top", 2D) = "white" {}
		_MainTexSide("Side", 2D) = "white" {}
		_Equator("Equator", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
				float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _MainTexSide;
			float4 _MainTexSide_ST;
			float _Equator;
			float _GlobalEquator;
			float _GlobalBottom;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
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

					col = tex2D(_MainTex, uv);
				}
				else {
					uv.x = (i.worldPos.x + i.worldPos.z - _MainTexSide_ST.z) * _MainTexSide_ST.x;
					uv.y = (i.worldPos.y - _MainTexSide_ST.w) * _MainTexSide_ST.y;

					col = tex2D(_MainTexSide, uv);
				}

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
