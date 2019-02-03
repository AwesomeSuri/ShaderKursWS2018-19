Shader "Custom/Portal"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,0,0,1)
		_Ambient ("Ambient", Range (0, 1)) = 0.25
		_Speed ("Speed", Range(0,1)) = 0.4
		_TwirlStrength ("Twirl Strength", Range(0,50)) = 10
		_CellDensity("Cell Density", Range(0,20)) = 5.0
		_DissolveAmount("Dissolve amount", Range(0,5)) = 2
		_Intensity ("Intensity", Range(0,10)) = 1
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Blend SrcAlpha One
		//Cull off
		LOD 200

		Pass
		{
			HLSLPROGRAM

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
				float3 worldNormal : TEXCOORD1;
				float2 uv : TEXCOORD0;
			};
				
			sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;
			float _Ambient;
			float _Speed;
			float _TwirlStrength;
			float _CellDensity;
			float _DissolveAmount;
			float _Intensity;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			inline float2 unity_voronoi_noise_randomVector (float2 UV, float offset)
			{
				float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
				UV = frac(sin(mul(UV, m)) * 46839.32);
				return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				fixed4 tex = tex2D(_MainTex, i.uv);
				float nl = max(_Ambient, dot(normal, _WorldSpaceLightPos0.xyz));
				float4 result = nl * _Color * tex * _LightColor0;

				// Add time
				float time = _Time.y;
				float timedOffset = _Speed * time;

				// Twirl Effect
				// Source: https://github.com/Unity-Technologies/ShaderGraph/wiki/Twirl-Node
				float2 center = float2(0.5f, 0.5f);
				float2 offset = timedOffset; //float2(0.0f, 0.0f);
				float2 delta = i.uv - center;
				float angle = _TwirlStrength * length(delta);
				float xTwirl = cos(angle) * delta.x - sin(angle) * delta.y;
				float yTwirl = sin(angle) * delta.x + cos(angle) * delta.y;
				float2 twirlOut = float2(xTwirl + center.x + offset.x, yTwirl + center.y + offset.y);

				// Voronoi Effect
				// Source: https://github.com/Unity-Technologies/ShaderGraph/wiki/Voronoi-Node
				float2 UV = twirlOut; //i.uv;
				float AngleOffset = 2.0f;

				float2 g = floor(UV * _CellDensity);
				float2 f = frac(UV * _CellDensity);
				float t = 8.0;
				float3 res = float3(8.0, 0.0, 0.0);

				float voronoiOut;
				float Cells;

				for(int y=-1; y<=1; y++)
				{
					for(int x=-1; x<=1; x++)
					{
						float2 lattice = float2(x,y);
						float2 offset = unity_voronoi_noise_randomVector(lattice + g, AngleOffset);
						float d = distance(lattice + offset, f);
						if(d < res.x)
						{
							res = float3(d, offset.x, offset.y);
							voronoiOut = res.x;
							Cells = res.y;
						}
					}
				}

				float voronoiOutPow = pow(voronoiOut, _DissolveAmount);

				return voronoiOutPow * tex * _Color * _Intensity;
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}