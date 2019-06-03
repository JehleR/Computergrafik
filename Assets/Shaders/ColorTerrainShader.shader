// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/ColorTerrainShader"
{
	Properties
	{
		// Reflectance of the ambient light
		_Ka("Ambient Reflectance", Range(0, 1)) = 0.5

		// Reflectance of the diffuse light
		_Kd("Diffuse Reflectance", Range(0, 1)) = 0.5
	}

	SubShader
	{
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
		};

		float _Ka, _Kd;

		// VERTEX SHADER
		v2f vert(appdata_full vertexIn)
		{
			v2f vertexOut;
			// transform vertices from object coordinates to clip coordinates
			vertexOut.pos = UnityObjectToClipPos(vertexIn.vertex);

			// Get world position of vertex
			vertexOut.worldPos = mul(unity_ObjectToWorld, vertexIn.vertex);

			// transform normal vectors to world coordinates
			half3 worldNormal = UnityObjectToWorldNormal(vertexIn.normal);

			// calculate ambient light color
			vertexOut.amb = float4(ShadeSH9(half4(worldNormal,1)),1);

			// calculate diffuse light color
			half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
			vertexOut.diff = nl * _LightColor0;

			return vertexOut;
		}

		// FRAGMENT SHADER
		float4 frag(v2f fragIn) : SV_Target{
			// set color based on height
			float4 color = float4(0, fragIn.worldPos.y, 0, 0) / 10;
			float contourLineFatness = 0.03;

			if (fragIn.worldPos.y <= 0) {
				color.rgb = float3(0, 0, 139);
			} else if (fragIn.worldPos.y % 1 < contourLineFatness && fragIn.worldPos.y > contourLineFatness) {
				color.rgb = float3(255, 0, 0);
			}

			// multiply base color with ambient and diffuse light
			color *= (_Ka * fragIn.amb + _Kd * fragIn.diff);

			return color;
		}

			ENDCG
		}
	}
}