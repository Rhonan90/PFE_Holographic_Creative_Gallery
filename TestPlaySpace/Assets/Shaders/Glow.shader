Shader "HoloForge/Glow"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Multiplier("Intensity", Float) = 1	
		_Color("Glow Color", Color) = (1, 1, 1, 1)
		_Scale("Scale", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "DisableBatching" = "True"}
		LOD 100

		Pass
		{
			Blend One One
			ZWrite Off
			ZTest Always

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float4 vColor : COLOR;
				float2 customData : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;					
				float2 uv : TEXCOORD0;
				float3 viewPos : TEXCOORD3;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float _Multiplier;
			uniform float _Scale;
			uniform fixed4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.vertex = mul(UNITY_MATRIX_P,
					mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
					+ float4(v.vertex.x, v.vertex.y, 0.0, 0.0)
					* float4(_Scale, _Scale, 1.0, 1.0));

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				return o;
			}

			float Remap(float value, float from1, float to1, float from2, float to2) {
				return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);

				col *= _Color * _Multiplier;

				return col;

			}
			ENDCG
		}
	}
}