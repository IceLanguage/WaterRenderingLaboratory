Shader "LinHowe/Force"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			cull front
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "internal.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float depth : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float internal_Force;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.depth = COMPUTE_DEPTH_01;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return EncodeHeight(i.depth*internal_Force);
			}
			ENDCG
		}
	}
}
