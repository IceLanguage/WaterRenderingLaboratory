Shader "LinHowe/Water"
{
	Properties
	{
		_Gloss("Gloss", float) = 0
		_Specular("Specular", float) = 0
		_Height("Height", float) = 0
		_Range("Range", vector) = (0, 0, 0, 0)
		_BaseColor("BaseColor", color) = (1,1,1,1)
		_WaterColor("WaterColor", color) = (1,1,1,1)
		_Diffuse("diffuseTex",  color) = (1,1,1,1)
	}

	SubShader
	{
		//半透明着色器
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		//抓取屏幕到_GrabTexture
		GrabPass{}

		Pass
		{
			//不写入深度缓存
			zwrite off

			//透明度混合
			blend srcalpha oneminussrcalpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "internal.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 proj0 : TEXCOORD2;
				float4 proj1 : TEXCOORD3;
				
				float4 TW0 : TEXCOORD4;
				float4 TW1 : TEXCOORD5;
				float4 TW2 : TEXCOORD6;

				float4 vertex : SV_POSITION;
			};
			half _Gloss;
			half _Specular;
			half4 _BaseColor;
			half4 _WaterColor;
			half4 _Range;
			float _Height;
			half4 _Diffuse;
			
			sampler2D _GrabTexture;
			sampler2D_float _CameraDepthTexture;
			sampler2D _WaterHeightMap;
			sampler2D _WaterNormalMap;
			
			float3 GetLightDirection(float3 worldPos) {
				if (internalWorldLightPos.w == 0)
					return worldPos - internalWorldLightPos.xyz;
				else
					return internalWorldLightPos.xyz - worldPos;
			}

			v2f vert(appdata_full v)
			{
				v2f o;

				float4 projPos = UnityObjectToClipPos(v.vertex);
				
				//计算GrabPass纹理的纹理坐标
				o.proj0 = ComputeGrabScreenPos(projPos);
				
				//计算映射到屏幕的顶点坐标
				o.proj1 = ComputeScreenPos(projPos);

				//获取水面的高度
				float height = DecodeHeight(tex2Dlod(_WaterHeightMap, float4(v.texcoord.xy,0,0)));
				v.vertex.y += height * _Height;

				o.uv = v.texcoord;
				o.vertex =  UnityObjectToClipPos(v.vertex);
				//从顶点着色输出雾数据
				UNITY_TRANSFER_FOG(o,o.vertex);

				COMPUTE_EYEDEPTH(o.proj0.z);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				float3 worldTan = UnityObjectToWorldDir(v.tangent.xyz);
				float tanSign = v.tangent.w * unity_WorldTransformParams.w;
				float3 worldBinormal = cross(worldNormal, worldTan)*tanSign;
				o.TW0 = float4(worldTan.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.TW1 = float4(worldTan.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.TW2 = float4(worldTan.z, worldBinormal.z, worldNormal.z, worldPos.z);

				return o;
			}


			half4 frag(v2f i) : SV_Target
			{
				float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, i.proj1));
				float deltaDepth = depth - i.proj0.z;

				float3 normal = UnpackNormal(tex2D(_WaterNormalMap, i.uv));
				float3 worldNormal = float3(dot(i.TW0.xyz, normal), dot(i.TW1.xyz, normal), dot(i.TW2.xyz, normal));
				float3 worldPos = float3(i.TW0.w, i.TW1.w, i.TW2.w);
				float3 lightDir = normalize(GetLightDirection(worldPos.xyz));
				float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				float2 projUv = i.proj0.xy / i.proj0.w;
				half4 col = tex2D(_GrabTexture, projUv);

				float height = DecodeHeight(tex2D(_WaterHeightMap, i.uv));

				col.rgb = _BaseColor.rgb*col.rgb + pow(dot(worldNormal, lightDir) * 0.4 + 0.6, 80.0) * _WaterColor.rgb * 0.12;

				col.rgb += _WaterColor.rgb*col.rgb * (height*_Range.y);

				//Blinn-Phong光照模型
				float3 halfdir = normalize(lightDir + viewDir);
				float ndh = max(0, dot(worldNormal, halfdir));
				col.rgb += internalWorldLightColor.rgb * pow(ndh, _Specular*128.0)*_Gloss;

				//从顶点着色器中输出雾效数据，将第二个参数中的颜色值作为雾效的颜色值，且在正向附加渲染通道（forward-additive pass）中
				UNITY_APPLY_FOG(i.fogCoord, col); 

				col.a = 1.0;

				return col;
			}
			ENDCG
		}
	}
}
