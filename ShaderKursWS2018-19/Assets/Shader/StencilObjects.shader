Shader "Custom/StencilObjects"
{
	Properties
	{
		_Color ("Colour", COLOR) = (1,1,1,1)
		_Ambient ("Ambient", Range (0, 1)) = 0.25
        _StencilID("Stencil ID", Int) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
            Stencil {
                Ref [_StencilID]
                Comp Equal
				Pass keep
            }

			HLSLPROGRAM
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
				float3 worldNormal : TEXCOORD0;
			};

			float4 _Color;
			float _Ambient;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float nl = max(_Ambient, dot(normal, _WorldSpaceLightPos0.xyz));

				return _Color * nl;
			}
			ENDHLSL
		}
	}
}

