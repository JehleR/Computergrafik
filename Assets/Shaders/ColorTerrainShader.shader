// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/ColorTerrainShader"
{
	Properties
	{
		// Lighting
		// Reflectance of the ambient light
		_Ka("Ambient Reflectance", Range(0, 1)) = 0.5
		// Reflectance of the diffuse light
		_Kd("Diffuse Reflectance", Range(0, 1)) = 0.5
		// Spekulare Reflektanz
		_Ks("Specular Reflectance", Range(0, 1)) = 0.5
		// Shininess
		_Shininess("Shininess", Range(0.1, 1000)) = 100


		// Land & contour lines
		// Colormap
		_ColorTex("Color Texture", 2D) = "white" {}
		// create checkbox for enabling contour lines
		// https://gist.github.com/smkplus/2a5899bf415e2b6bf6a59726bb1ae2ec
		[Toggle] 
		_UseContourLines("Show contour lines", Float) = 0
		// Contour lines intervall
		_ContourLinesIntervall("Contour Lines Intervall", Range(0, 10)) = 1
		// Contour Lines Fatness
		_ContourLinesFatness("Contour Lines Fatness", Range(0, 1)) = 0.03
		// Contour Lines color
		_ContourLinesColor("Contour Lines Color", Color) = (96, 46, 10, 1)


		// Water
		// Bump Map 1 for water
		_BumpMap("Water Map", 2D) = "bump" {}
		//Bump Map 2 for water
		_BumpMap2("Water Map 2", 2D) = "bump" {}
		// Water Color
		_WaterColor("Water Color", Color) = (123, 108, 221, 255)
		// Speed of Water Simulation
		_ScrollSpeedX("Speed X", Range(0, 1)) = 0.1
		_ScrollSpeedY("Speed Y", Range(0, 1)) = 0.3
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

			// struct for data from vertex to fragment shader
			struct v2f
			{
				// vertex positions in homogeneous coordinates
				float4 pos : SV_POSITION;
				// global vertex POSITION
				float3 worldPos : TEXCOORD0;

				// Weitergabe der Textur Koordinaten
				float2 uv : TEXCOORD1;
				half3 worldViewDir : TEXCOORD2;

				half3 normal : TEXCOORD3;

				// these three vectors will hold a 3x3 rotation matrix
				// that transforms from tangent to world space
				half3 tspace0 : TEXCOORD4;
				half3 tspace1 : TEXCOORD5;
				half3 tspace2  : TEXCOORD6;
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
			float _UseContourLines;
			float _ContourLinesIntervall;
			float _ContourLinesFatness;
			float4 _ContourLinesColor;

			// VERTEX SHADER
			v2f vert(appdata_full vertexIn)
			{
				v2f vertexOut;

				// transform vertices from object coordinates to clip coordinates
				vertexOut.pos = UnityObjectToClipPos(vertexIn.vertex);

				// calculate and normalize space vertex position towards the camera.
				vertexOut.worldViewDir = normalize(WorldSpaceViewDir(vertexIn.vertex));

				// Get world position of vertex
				vertexOut.worldPos = mul(unity_ObjectToWorld, vertexIn.vertex);

				// rotate with transform matrix
				half3 wTangent = UnityObjectToWorldDir(vertexIn.tangent);

				// transform normal vectors to world coordinates
				half3 worldNormal = UnityObjectToWorldNormal(vertexIn.normal);

				// compute bitangent from cross product of normal and tangent
				// bitanget vector is needed to convert the normal from the normal map
				// into world space
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
			float4 frag(v2f fragIn) : SV_Target {
				float4 color;
				half3 worldNormal;
				if (fragIn.worldPos.y > 0) {
					// set color based on height
					color = fragIn.worldPos.y < 9.9
						? tex2Dlod(_ColorTex, float4(0, fragIn.worldPos.y / 10, 0, 0))
						: tex2Dlod(_ColorTex, float4(0, 9.9 / 10, 0, 0));
					// transform normal vectors to world coordinates
					worldNormal = UnityObjectToWorldNormal(fragIn.normal);
				}
				else {
					color = _WaterColor;
					// Calc xOffset and yOffset for Water Simulation
					float xOffset = _ScrollSpeedX * _Time;
					float yOffset = _ScrollSpeedY * _Time;

					// sample the normal map, and decode from the Unity encoding
					half3 tnormal = UnpackNormal(tex2D(_BumpMap, fragIn.uv 
						+ float2(xOffset, yOffset * 0.5)));
					half3 tnormal2 = UnpackNormal(tex2D(_BumpMap2, fragIn.uv 
						+ float2(xOffset, yOffset)));

					// add both normal maps
					tnormal = (tnormal + tnormal2) / 2;

					// transform normal from tangent to world space
					worldNormal.x = dot(fragIn.tspace0, tnormal);
					worldNormal.y = dot(fragIn.tspace1, tnormal);
					worldNormal.z = dot(fragIn.tspace2, tnormal);
				}

				// calculate ambient light color
				float4 amb = float4(ShadeSH9(half4(worldNormal, 1)), 1);

				// get diffuse Light
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));

				// Diffuse component multiplied by light colour
				float4 diff = nl * _LightColor0;

				// get reflected light
				float3 worldSpaceReflection = reflect(normalize(-_WorldSpaceLightPos0.xyz),
					worldNormal);

				// calculate specular light
				half re = pow(max(dot(worldSpaceReflection, fragIn.worldViewDir), 0), _Shininess);
				float4 spec = re * _LightColor0;

				// set lighting
				// multiply base color with ambient and diffuse light
				color *= (_Ka * amb + _Kd * diff);
				if (fragIn.worldPos.y <= 0) {
					// add specular light
					color += _Ks * spec;
				}

				// set contour lines
				if (_UseContourLines > 0) {
					if (fragIn.worldPos.y > _ContourLinesFatness && 
						fragIn.worldPos.y % _ContourLinesIntervall < _ContourLinesFatness) {
						color = _ContourLinesColor;
					}
				}

				//clamps the value so that it is never larger than 1.0 and never smaller than 0.0
				return saturate(color);
			}
			ENDCG
		}
	}
}