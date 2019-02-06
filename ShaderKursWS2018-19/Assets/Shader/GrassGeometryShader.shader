﻿Shader "Custom/GrassGeometryShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

		_Cutoff("Cutoff", Range(0,1)) = 0.25

		_GrassHeight("Grass Height", Range(0, 3)) = 0.25
		_GrassWidth("Grass Width", Range(0, 3)) = 1.0

		_WindRange("Range of Grass affected by Wind", Range(0, 0.5)) = 0.2
		_WindSpeed("Wind Speed", float) = 5

		_Ambient ("Ambient", float) = 0.5

		_Radius ("Radius", float) = 1.0
		_PlayerPos ("Player Position", Vector) = (0, 0, 0)
		_PlayerOffset ("Player Position Offset", Vector) = (0, 0, 0.7)


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
				float4 myColor : TEXCOORD1;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float3 faceNormal : TEXCOORD2;
				float2 uv : TEXCOORD0;
				float4 diffuseTerm : TEXCOORD1;
				
				
			};

			
			half _GrassHeight;
			half _GrassWidth;
			half _Cutoff;
			float _WindSpeed;
			float _WindRange;
			sampler2D _WindNormalMap;
			float4 _Color;

			float _Ambient;
			float _Radius;
			float3 _PlayerPos;
			float3 _PlayerOffset;

			v2g vert(appdata_full v)
			{
				v2g o;

				o.pos = v.vertex;
				o.normal = v.normal;
				o.uv = v.texcoord;
				o.myColor = v.color;// tex2Dlod(_MainTex, v.texcoord).rgb * v.color;

				return o;
			}

			void drawQuad(inout TriangleStream<g2f> tristream, g2f o, float3 v0, float3 v1, float3 widthDirection, float3 faceNormal, float3 normal, float4 myColor){


				//first corner at (0,0)
				o.pos = UnityObjectToClipPos(v0 - widthDirection * 0.5 * _GrassWidth);
				o.faceNormal = faceNormal;
				o.normal = normal;
				o.diffuseTerm = myColor;
				o.uv = float2(0, 0);
				tristream.Append(o);

				//second corner at (1,0)
				o.pos = UnityObjectToClipPos(v0 + widthDirection * 0.5 * _GrassWidth);
				o.faceNormal = faceNormal;
				o.normal = normal;
				o.diffuseTerm = myColor;
				o.uv = float2(1, 0);
				tristream.Append(o);

				//third corner at (0,1)
				o.pos = UnityObjectToClipPos(v1 - widthDirection * 0.5 * _GrassWidth);
				o.faceNormal = faceNormal;
				o.normal = normal;
				o.diffuseTerm = myColor;
				o.uv = float2(0, 1);
				tristream.Append(o);

				//fourth corner at (1,1)
				o.pos = UnityObjectToClipPos(v1 + widthDirection * 0.5 * _GrassWidth);
				o.faceNormal = faceNormal;
				o.normal = normal;
				o.diffuseTerm = myColor;
				o.uv = float2(1, 1);
				tristream.Append(o);

				tristream.RestartStrip();
			}


			[maxvertexcount(4)]
			void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
			{
				
				float3 v0 = IN[0].pos.xyz;

				//computing offset for v1
				float3 cameraInObj = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
				float3 angleToCamera = normalize(cameraInObj - v0);
				//compute vector perpendicular to angleToCamera for the growing direction -> so quad will always have correct Z offset to camera
				float3 growDirection = cross(float3(1, 0, 0), angleToCamera);
				//creates line from which actual corners are computed
				float3 v1 = IN[0].pos.xyz + growDirection * _GrassHeight;
				float4 vertexWorld = mul(UNITY_MATRIX_M, v1);
				
				//calculates distance to Player and moves grass accordingly
				//_PlayerPos is set via script
				_PlayerPos += _PlayerOffset;
				if(distance((_PlayerPos), vertexWorld) < _Radius)
				{
					//get distance to player
					float2 distance = min(_GrassHeight / (pow(_PlayerPos.xz - vertexWorld.xz, 2) * 10), _GrassHeight); 
					//this line would stretch grass more so its more noticable but it makes grass longer than actual _GrassHeight 
					//float2 distance = min(_Radius / (pow(_PlayerPos.xz - vertexWorld.xz, 2) * 10), _Radius);
					float2 dir = normalize(_PlayerPos.xz - vertexWorld.xz);

					//dir sets direction
					v1.x -= distance.x * dir.x;
					//this could be used if camera was not fix
					//v1.z -= distance.y * dir.y;
					
				}

				
				
				float4 myColor = (IN[0].myColor);
				float3 faceNormal = angleToCamera;
				float3 normal = IN[0].normal;

				//wind calculations
				float3 wind = float3(sin(_Time.x * _WindSpeed + v0.x) + sin(_Time.x * _WindSpeed + v0.z * 2 + cos(_Time.x * _WindSpeed + v0.x)), 0, 0);
				v1 += wind * _WindRange;

				//should be just X axis -> otherwise grass will go in circles
				float3 widthDirAngle = float3(1, 0, 0);
				
				g2f o;
				//void drawQuad(inout TriangleStream<g2f> tristream, g2f o, float3 v0, float3 v1, float3 widthDirection, 
				//              float3 faceNormal, float3 normal, float4 myColor)
				drawQuad(triStream, o, v0, v1, widthDirAngle, faceNormal, normal, myColor);


			}

			half4 frag(g2f IN) : COLOR
			{
				fixed4 col = tex2D(_MainTex, IN.uv);
				float3 normal = normalize(IN.normal);
				float nl = max(_Ambient, dot(normal, _WorldSpaceLightPos0.xyz));
				
				col = col * _Color * IN.diffuseTerm * nl;
				clip(col.a - _Cutoff);
				return col;

			}

			ENDCG
		}

		UsePass "Custom/GlobalDissolveToBlack/DissolveToBlack"
		
    }
	FallBack "Diffuse"
}
