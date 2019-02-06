Shader "Custom/WingsFire"
{
    Properties
    {
		_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
		_Speed ("Speed", Float) = 1
		_Origin ("Origin", Float) = 0
		_Length ("Length", Float) = .1
		_Intensity ("Intensity", FLoat) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }

		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD0;
				float4 objectPos : TEXCOORD1;
            };

			fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Speed;
			float _Origin;
			float _Length;
			float _Intensity;

			float _GlobalEquator;
			float _GlobalBottom;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.objectPos = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				// get uv from world pos 
				float2 uv = (float2(i.objectPos.x + i.objectPos.y, i.objectPos.z + _Time.x * _Speed) - _MainTex_ST.zw) / _MainTex_ST.xy;
                fixed4 col = tex2D(_MainTex, uv) * _Intensity * _Color;

				// vertical fall off
				col.a *= max(0, min(1, (i.objectPos.z - _Origin) / _Length));

				// Vertical fog
				col *= max(0, min(1, (i.worldPos.y - _GlobalBottom) / (_GlobalEquator - _GlobalBottom)));

                return col;
            }
            ENDCG
        }


		UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
    }
}
