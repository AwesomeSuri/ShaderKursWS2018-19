Shader "Custom/GrassGeometryShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

		_Cutoff("Cutoff", Range(0,1)) = 0.25

		_GrassHeight("Grass Height", float) = 0.25
		_GrassWidth("Grass Width", float) = 1.0
		_CameraOffsetX("Camera Offset X", float) = 0.0
		_CameraOffsetZ("Camera Offset Z", float) = 5.0
		_GrassOffset("Grass Offset", float) = -4.0

		_WindStrength("Wind Strength", float) = 0.2
		_WindSpeed("Wind Speed", float) = 10

		_Ambient ("Ambient", float) = 0.5
		_Radius ("Radius", float) = 1.0
		_PlayerPos ("_PlayerPos", Vector) = (0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

		Pass{
			
			Cull Off


			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			#pragma target 5.0

			sampler2D _MainTex;

			struct v2g
			{
				float4 pos : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float3 myColor : TEXCOORD1;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float3 faceNormal : TEXCOORD2;
				float2 uv : TEXCOORD0;
				float3 diffuseTerm : TEXCOORD1;
				
				
			};

			half _Glossiness;
			half _Metallic;
			half _GrassHeight;
			half _GrassWidth;
			half _Cutoff;
			float _WindStrength;
			float _WindSpeed;
			sampler2D _WindNormalMap;
			float4 _Color;

			float _CameraOffsetX;
			float _CameraOffsetZ;
			float _GrassOffset;

			float _Ambient;
			float _Radius;
			float3 _PlayerPos;

			v2g vert(appdata_full v)
			{
				v2g o;

				o.pos = v.vertex;
				o.normal = v.normal;
				o.uv = v.texcoord;
				o.myColor = v.color.rgb;// tex2Dlod(_MainTex, v.texcoord).rgb * v.color;

				return o;
			}

			void drawQuad(inout TriangleStream<g2f> tristream, g2f o, float3 v0, float3 v1, float3 perpendicularAngle, float3 faceNormal, float3 normal, float3 myColor){


				//first corner at (0,0)
				o.pos = UnityObjectToClipPos(v0 - perpendicularAngle * 0.5 * _GrassWidth);
				o.faceNormal = faceNormal;
				o.normal = normal;
				o.diffuseTerm = myColor;
				o.uv = float2(0, 0);
				tristream.Append(o);

				//second corner at (1,0)
				o.pos = UnityObjectToClipPos(v0 + perpendicularAngle * 0.5 * _GrassWidth);
				o.faceNormal = faceNormal;
				o.normal = normal;
				o.diffuseTerm = myColor;
				o.uv = float2(1, 0);
				tristream.Append(o);

				//third corner at (0,1)
				o.pos = UnityObjectToClipPos(v1 - perpendicularAngle * 0.5 * _GrassWidth);
				o.faceNormal = faceNormal;
				o.normal = normal;
				o.diffuseTerm = myColor;
				o.uv = float2(0, 1);
				tristream.Append(o);

				//fourth corner at (1,1)
				o.pos = UnityObjectToClipPos(v1 + perpendicularAngle * 0.5 * _GrassWidth);
				o.faceNormal = faceNormal;
				o.normal = normal;
				o.diffuseTerm = myColor;
				o.uv = float2(1, 1);
				tristream.Append(o);

				tristream.RestartStrip();
			}


			[maxvertexcount(12)]
			void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
			{
				float3 lightPosition = _WorldSpaceLightPos0;

				float3 v0 = IN[0].pos.xyz;

				//computing offset for v1
				float3 cameraWithOffset = float3(_WorldSpaceCameraPos.x + _CameraOffsetX, _WorldSpaceCameraPos.y, _WorldSpaceCameraPos.z + _CameraOffsetZ);
				float3 perpendicularAngle = normalize(cameraWithOffset + v0);//float3(0, 0, 1);

				//creates line from which actual corners are computed
				float3 v1 = float3(IN[0].pos.xyz + IN[0].normal.xyz * _GrassHeight);// + _GrassOffset * perpendicularAngle;
				float4 vertexWorld = mul(UNITY_MATRIX_M, v1);
				if(distance(_PlayerPos, vertexWorld) < _Radius)
				{
					//get distance to player
					float2 distance = min(_Radius / (pow(_PlayerPos.xz - vertexWorld.xz, 2) * 10), _Radius);
					float2 dir = normalize(_PlayerPos.xz - vertexWorld.xz);

					//dir sets direction
					v1.x -= distance.x * dir.x; //dir.x * 10;//(_PlayerPos.x - vertexWorld.x);
					v1.z -= distance.y * dir.y;
					
				}
				
				float3 myColor = (IN[0].myColor);

				//calculations for creating 3 quads instead of just one...
				float sin60 = 0.866;
				float sin120 = sin60;
				float cos60 = 0.5;
				float cos120 = -cos60;

				//reset perpendicularAngle so faceNormal will be rotated towards camera
				perpendicularAngle = float3(1, 0, 0);
				float3 perpendicularAngle2 = float3(sin60, 0, cos60);
				float3 perpendicularAngle3 = float3(sin120, 0, cos120);
				float3 faceNormal = cross(perpendicularAngle, IN[0].normal);
				float3 normal = IN[0].normal;

				//wind calculations
				float3 wind = float3(sin(_Time.x * _WindSpeed + v0.x) + sin(_Time.x * _WindSpeed + v0.z * 2 + cos(_Time.x * _WindSpeed + v0.x)), 
				 0,max(_CameraOffsetZ, cos(_Time.x * _WindSpeed + v0.x) + cos(_Time.x * _WindSpeed + v0.z)));
				v1 += wind * _WindStrength;


				//defining 3 quads which are rotated to each other
				g2f o;

				//void drawTriangle(TriangleStream<g2f> tristream, g2f o, float3 v0, float3 v1, float3 perpendicularAngle, float3 faceNormal, float3 color)
				//first quad  normalize(cross(perpendicularAngle, IN[0].normal))
				drawQuad(triStream, o, v0, v1, perpendicularAngle, faceNormal, normal, myColor);

				////second quad
				//drawQuad(triStream, o, v0, v1, perpendicularAngle2, faceNormal, normal, myColor);

				////third quad
				//drawQuad(triStream, o, v0, v1, perpendicularAngle3, faceNormal, normal, myColor);


			}

			half4 frag(g2f IN) : COLOR
			{
				fixed4 col = tex2D(_MainTex, IN.uv);
				float3 normal = normalize(IN.normal);
				float nl = max(_Ambient, dot(normal, _WorldSpaceLightPos0.xyz));
				clip(col.a - _Cutoff);
				col.rgb = col.rgb * _Color * IN.diffuseTerm * nl;//col.rgb * (IN.normal * 10) * _Color.rgb;
				return col;

			}

			ENDCG
		}
		
    }
	FallBack "Diffuse"
}

