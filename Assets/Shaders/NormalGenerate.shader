Shader "LinHowe/NormalGenerate"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "internal.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;//Vector4(1 / width, 1 / height, width, height)
			
			fixed4 frag (v2f i) : SV_Target
			{
				float lh = DecodeHeight(tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x, 0.0)));
				float rh = DecodeHeight(tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0.0)));
				float bh = DecodeHeight(tex2D(_MainTex, i.uv + float2(0.0, -_MainTex_TexelSize.y)));
				float th = DecodeHeight(tex2D(_MainTex, i.uv + float2(0.0, _MainTex_TexelSize.y)));
				float3 normal = normalize(float3(lh - rh, bh - th, 5.0*_MainTex_TexelSize.x));

				return float4(normal*0.5 + 0.5, 1.0);
			}
			ENDCG
		}
	}
}
