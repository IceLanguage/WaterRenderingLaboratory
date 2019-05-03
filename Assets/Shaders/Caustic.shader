Shader "LinHowe/Caustic"
{
	Properties
	{
		_Refract("Refract", float) = 0.1
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
				float4 vertex : SV_POSITION;
				float2 oldPos : TEXCOORD1;
				float2 newPos : TEXCOORD2;
			};

			half _Refract;
			
			v2f vert (appdata v)
			{
				v2f o;
				float3 normal = UnpackNormal(tex2Dlod(_WaterNormalMap, float4(v.texcoord.xy, 0, 0)));

				o.oldPos = v.vertex.xz;
				v.vertex.xz += normal.xy*_Refract;
				o.newPos = v.vertex.xz;

				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float oldArea = length(ddx(i.oldPos)) * length(ddy(i.oldPos));
				float newArea = length(ddx(i.newPos)) * length(ddy(i.newPos));

				float area = (oldArea / newArea) * 0.5;

				return float4(area, area, area, 1);
			}
			ENDCG
		}
	}
}
