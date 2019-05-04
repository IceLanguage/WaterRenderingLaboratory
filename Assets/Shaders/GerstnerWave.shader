Shader "LinHowe/GerstnerWave"
{
	Properties
	{
		_MainTex ("CurTex", 2D) = "white" {}
		_PreTex("PreTex", 2D) = "white" {}
		_WaveParams("WaveParams", vector) = (0,0,0,0)
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

			sampler2D _MainTex;
			sampler2D _PreTex;
			float4 _WaveParams;
			float4 _MainTex_TexelSize;
			float2 _WaveOrigin;
			float _Timer;
			float Qi;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float t = dot(_WaveParams.yz,i.uv - _WaveOrigin) + _Timer * _WaveParams.w;
			    Qi = 1 / _WaveParams.w / _WaveParams.x;
				#if GenerateGerstnerOffsetX
					float offset = DecodeHeight(tex2D(_WaterOffsetXMap, float4(i.uv,0,0)));
					offset += _WaveParams.x / _WaveParams.w * Qi * dot(_WaveParams.yz,float2((i.uv - _WaveOrigin).x,0)) * cos(t) ;
					return EncodeHeight(offset);
				#elif GenerateGerstnerOffsetZ
					float offset = DecodeHeight(tex2D(_WaterOffsetZMap, float4(i.uv,0,0)));
					offset  += _WaveParams.x / _WaveParams.w * Qi * dot(_WaveParams.yz,float2(0,(i.uv - _WaveOrigin).y)) * cos(t) ;
					return EncodeHeight(offset);
				#else
					float cur = DecodeHeight(tex2D(_MainTex, i.uv));
					cur += _WaveParams.x * sin(t); 
					return EncodeHeight(cur);
				#endif
				
			}
			ENDCG
		}
	}
}
