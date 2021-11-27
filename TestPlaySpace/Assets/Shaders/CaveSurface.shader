Shader "Monumenteque/CaveSurface"
{
    Properties
    {
		_Distance("Distance", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
			Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#include "UnityLightingCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 vertexLighting : TEXCOORD0;
            };

			float _Distance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

				float3 worldNor = normalize(mul(unity_ObjectToWorld, v.normal));
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);

				float4 lightPosition = float4(unity_4LightPosX0[0], unity_4LightPosY0[0], unity_4LightPosZ0[0], 1.0);

				float3 vertexToLightSource = lightPosition.xyz - worldPos.xyz;
				float3 lightDirection = normalize(vertexToLightSource);
				float distance = length(vertexToLightSource);
				float attenuation = 1 / distance * _Distance;
  //              attenuation = 1;
				float3 diffuseReflection = attenuation * unity_LightColor[0].rgb * max(0.0, dot(worldNor, lightDirection));
//                float3 diffuseReflection = attenuation * unity_LightColor[0].rgb;// *max(0.0, dot(worldNor, lightDirection));


				o.vertexLighting = diffuseReflection;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = float4(i.vertexLighting, 1);
                return col;
            }
            ENDCG
        }
    }
}