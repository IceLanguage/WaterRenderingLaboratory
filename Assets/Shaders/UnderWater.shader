Shader "LinHowe/UnderWater"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" { }
		_Gloss("Gloss", float) = 0
		_Specular("Specular", float) = 0
		_Diffuse("Diffuse", float) = 0
		_WaterColor("WaterColor", color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "internal.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 TW0 : TEXCOORD2;
				float4 TW1 : TEXCOORD3;
				float4 TW2 : TEXCOORD4;	
				float4 shadowProj : TEXCOORD5;
				float shadowDepth : TEXCOORD6;
			};

			float _Gloss;
			float _Specular;
			float _Diffuse;

			sampler2D _MainTex;
			sampler2D _BumpTex;

			float4 _WaterColor;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				

				UNITY_TRANSFER_FOG(o,o.vertex);
				

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				float3 worldTan = UnityObjectToWorldDir(v.tangent.xyz);
				float tanSign = v.tangent.w * unity_WorldTransformParams.w;
				float3 worldBinormal = cross(worldNormal, worldTan)*tanSign;

				o.TW0 = float4(worldTan.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.TW1 = float4(worldTan.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.TW2 = float4(worldTan.z, worldBinormal.z, worldNormal.z, worldPos.z);

				float4 cpos = mul(internalWorldLightMV, mul(unity_ObjectToWorld, v.vertex)); 
				o.shadowProj = mul(internalWorldLightVP, cpos); 
				float4 pj = o.shadowProj * 0.5f; 
				pj.xy = float2(pj.x, pj.y) + pj.w; 
				pj.zw = o.shadowProj.zw; 
				o.shadowProj = pj; 
				o.shadowDepth = -cpos.z * internalProjectionParams.w;

				
					
				return o;
			}
			
			

			float4 frag (v2f i) : SV_Target
			{
				float3 worldPos = float3(i.TW0.w, i.TW1.w, i.TW2.w);
				float3 normal = UnpackNormal(tex2D(_BumpTex, i.uv));
				float3 worldNormal = float3(dot(i.TW0.xyz, normal), dot(i.TW1.xyz, normal), dot(i.TW2.xyz, normal));
				
				float3 lightDir = normalize(internalWorldLightDir.xyz);
				float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				float4 col = tex2D(_MainTex, i.uv);

				//半兰伯特模型
				float3 diffuse = internalWorldLightColor.rgb * saturate(0.5 * dot(worldNormal, lightDir) + 0.5) * _Diffuse;

				//Blinn-Phong光照模型
				float3 halfdir = normalize(lightDir + viewDir);
				float ndh = saturate(dot(worldNormal, halfdir));
				float3 specular = internalWorldLightColor.rgb * pow(ndh, _Specular*128.0)*_Gloss;

				col.rgb *= diffuse + specular;

				float4 shadow = tex2Dproj(internalShadowMap, i.shadowProj); 
				float shadowdepth = DecodeFloatRGBA(shadow); 
				float shadowcol = step(i.shadowDepth - internalBias, shadowdepth)*0.7 + 0.3; 
				float2 uv = 0.5 - abs(i.shadowProj.xy / i.shadowProj.w - 0.5);
				float2 shadowatten = saturate(uv / (1 - internalWorldLightColor.a)); 
				float atten = shadowatten.x*shadowatten.y*shadowcol;
				col *= atten;

				//裁剪水底部分
				float clip = Clip(worldPos);
				if(clip == 1)
					col *= _WaterColor;

				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
