// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/ColorTerrainShader"
{
	Properties
	{
		// Reflectance of the ambient light
		_Ka("Ambient Reflectance", Range(0, 1)) = 0.5

		// Reflectance of the diffuse light
		_Kd("Diffuse Reflectance", Range(0, 1)) = 0.5

		// Spekulare Reflektanz
		_Ks("Specular Reflectance", Range(0, 1)) = 0.5

		// Shininess
		_Shininess("Shininess", Range(0.1, 1000)) = 100

		// Colormap
		_ColorTex("Color Texture", 2D) = "white" {}

		_BumpMap("Bump Map", 2D) = "bump" {}
		_BumpMap2("Bump Map2", 2D) = "bump" {}
		
		_ScrollSpeedX("Speed X", Range(0, 1)) = 0.1

		_ScrollSpeedY("Speed Y", Range(0, 1)) = 0.3

		_WaterColor("Water Color", Color) = (123, 108, 221, 255)
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			// indicate that our pass is the "base" pass in forward
			// rendering pipeline. It gets ambient and main directional
			// light data set up; light direction in _WorldSpaceLightPos0
			// and color in _LightColor0
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
			// definition of used shaders and their names
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc" // for lightning

			// input struct of vertex shader
			/*struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float3 normal : NORMAL;
			};*/

			// struct for data from vertex to fragment shader
			struct v2f
			{
				// vertex positions in homogeneous coordinates
				float4 pos : SV_POSITION;
				// global vertex POSITION
				float3 worldPos : TEXCOORD0;
				// ambient light color
				float4 amb : COLOR1;
				// diffuse light color
				float4 diff : COLOR2;
				// Spekulare Licht Farbe
				float4 spec : COLOR3;

				// Weitergabe der Textur Koordinaten
				float2 uv : TEXCOORD5;

				float4 localVertex : COLOR4;
				half3 worldViewDir : TEXCOORD1;

				half3 normal : TEXCOORD6;

				// these three vectors will hold a 3x3 rotation matrix
				// that transforms from tangent to world space
				half3 tspace0 : TEXCOORD2;
				half3 tspace1 : TEXCOORD3;
				half3 tspace2  : TEXCOORD4;
			};

			fixed4 _Color;
			float _Ka, _Kd, _Ks;
			sampler2D _ColorTex;
			sampler2D _BumpMap;
			sampler2D _BumpMap2;
			float _Shininess;
			float _ScrollSpeedX;
			float _ScrollSpeedY;
			float4 _WaterColor;

			// VERTEX SHADER
			v2f vert(appdata_full vertexIn)
			{
				v2f vertexOut;

				vertexOut.localVertex = vertexIn.vertex;

				// transform vertices from object coordinates to clip coordinates
				vertexOut.pos = UnityObjectToClipPos(vertexIn.vertex);
		
				vertexOut.worldViewDir = normalize(WorldSpaceViewDir(vertexIn.vertex));


				// Get world position of vertex
				vertexOut.worldPos = mul(unity_ObjectToWorld, vertexIn.vertex);

				half3 wTangent = UnityObjectToWorldDir(vertexIn.tangent);

				// transform normal vectors to world coordinates
				half3 worldNormal = UnityObjectToWorldNormal(vertexIn.normal);
				// compute bitangent from cross product of normal and tangent
				// bitanget vector is needed to convert the normal from the normal map into world space
				// see: http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-13-normal-mapping/
				half tangentSign = vertexIn.tangent.w * unity_WorldTransformParams.w;
				half3 wBitangent = cross(worldNormal, wTangent) * tangentSign;

				// output the tangent space matrix
				vertexOut.tspace0 = half3(wTangent.x, wBitangent.x, worldNormal.x);
				vertexOut.tspace1 = half3(wTangent.y, wBitangent.y, worldNormal.y);
				vertexOut.tspace2 = half3(wTangent.z, wBitangent.z, worldNormal.z);

				vertexOut.uv = vertexIn.texcoord;
				vertexOut.normal = vertexIn.normal;

				return vertexOut;
			}

			// FRAGMENT SHADER
			float4 frag(v2f fragIn) : SV_Target{
				// set color based on height
				float4 color = fragIn.worldPos.y <= 10 ? tex2Dlod(_ColorTex, float4(0, fragIn.worldPos.y / 10, 0, 0)) : tex2Dlod(_ColorTex, float4(0, 0.99, 0, 0));

				float xOffset = _ScrollSpeedX * _Time;
				float yOffset = _ScrollSpeedY * _Time;

				// sample the normal map, and decode from the Unity encoding
				half3 tnormal = UnpackNormal(tex2D(_BumpMap, fragIn.uv + float2(xOffset, yOffset * 0.5)));
				half3 tnormal2 = UnpackNormal(tex2D(_BumpMap2, fragIn.uv + float2(xOffset , yOffset)));

				tnormal = (tnormal + tnormal2) / 2;

				// transform normal from tangent to world space
				half3 worldNormal;
				if (fragIn.worldPos.y <= 0) {
					worldNormal.x = dot(fragIn.tspace0, tnormal);
					worldNormal.y = dot(fragIn.tspace1, tnormal);
					worldNormal.z = dot(fragIn.tspace2, tnormal);
				}
				else {
					// transform normal vectors to world coordinates
					worldNormal = UnityObjectToWorldNormal(fragIn.normal);
				}

				float contourLineFatness = 0.03;
				float contourInterval = 1;

				float4 amb = float4(ShadeSH9(half4(worldNormal, 1)), 1);

				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				float4 diff = nl * _LightColor0;


				float3 worldSpaceReflection = reflect(normalize(-_WorldSpaceLightPos0.xyz), worldNormal);
				half re = pow(max(dot(worldSpaceReflection, fragIn.worldViewDir), 0), _Shininess);

				float4 spec = re * _LightColor0;

				if (fragIn.worldPos.y <= 0) {
					color = _WaterColor;
					color *= (_Ka * amb + _Kd * diff);
					color += _Ks * spec;
				} else if (fragIn.worldPos.y % contourInterval < contourLineFatness && fragIn.worldPos.y > contourLineFatness) {
					color.rgb = float3(0.545, 0.271, 0.075);
				}

				// multiply base color with ambient and diffuse light
				color *= (_Ka * amb + _Kd * diff);

				return saturate(color);
			}

			ENDCG
		}
	}
}