Shader "LinHowe/SineWave"
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
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
			
				float cur = DecodeHeight(tex2D(_MainTex, i.uv));
				cur += _WaveParams.x * cos(dot(_WaveParams.yz,i.uv - _WaveOrigin) + _Timer * _WaveParams.w); 
				
				return EncodeHeight(cur);
			}
			ENDCG
		}
	}
}
