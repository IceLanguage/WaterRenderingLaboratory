Shader "LinHowe/NoiseNoramlGenerate"
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

			#pragma multi_compile NoiseWave Other
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
			
			float4 frag (v2f i) : SV_Target
			{
				float2 speed = _Time.y * float2(0.05f, 0.05f);

				float2 uv = TRANSFORM_TEX(i.uv, _WaveMap);;
				float3 bump1 = UnpackNormal(tex2D(_WaveMap, uv + speed)).rgb;
				float3 bump2 = UnpackNormal(tex2D(_WaveMap, uv - speed)).rgb;
				float3 normal = normalize(bump1 + bump2);
				#if defined(UNITY_NO_DXT5nm)
				return float4(normal*0.5 + 0.5, 1.0);
#else
#if UNITY_VERSION > 2018
				return float4(normal.x*0.5 + 0.5, normal.y*0.5 + 0.5, 0, 1); //2018修改了法线压缩方式，增加了一种BC5压缩
#else
				return float4(0, normal.y*0.5 + 0.5, 0, normal.x*0.5 + 0.5);
#endif
#endif
			}
			ENDCG
		}
	}
}
