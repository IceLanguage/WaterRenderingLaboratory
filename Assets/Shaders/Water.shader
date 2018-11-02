Shader "LinHowe/Water"
{
	Properties
	{
		_Gloss("Gloss", float) = 0
		_Specular("Specular", float) = 0
		_Diffuse("Diffuse", float) = 0
		_Height("Height", float) = 0
		_Range("Range", float) = 0
		_Refract("Refract", float) = 0
		_BaseColor("BaseColor", color) = (1,1,1,1)
		_WaterColor("WaterColor", color) = (1,1,1,1)
		_Fresnel("Fresnel x = bias,y = scale,z = power", vector) = (0, 0, 0, 0)
	}

	SubShader
	{
		//半透明着色器
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		//抓取屏幕内容放到_GrabTexture纹理中
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
				
				float4 TW0 : TEXCOORD3;
				float4 TW1 : TEXCOORD4;
				float4 TW2 : TEXCOORD5;

				float4 vertex : SV_POSITION;

			};
			float _Gloss;
			float _Specular;
			float _Diffuse;
			float _Range;
			float _Refract;
			float _Height;

			float4 _BaseColor;
			float4 _WaterColor;
			
			float4 _Fresnel;
			
			sampler2D _GrabTexture;
			sampler2D _CameraDepthTexture;
			sampler2D _WaterReflectTexture;
			
			float4 _GrabTexture_TexelSize;
			v2f vert(appdata_full v)
			{
				v2f o;

				float4 projPos = UnityObjectToClipPos(v.vertex);
				
				//计算GrabPass纹理的纹理坐标
				o.proj0 = ComputeGrabScreenPos(projPos);

				//获取水面的高度
				float height = DecodeHeight(tex2Dlod(_WaterHeightMap, float4(v.texcoord.xy,0,0)));
				v.vertex.y += height * _Height;

				o.uv = v.texcoord;
				//从顶点着色输出雾数据
				UNITY_TRANSFER_FOG(o,o.vertex);
				o.vertex =  UnityObjectToClipPos(v.vertex);

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


			float4 frag(v2f i) : SV_Target
			{
				float3 normal = UnpackNormal(tex2D(_WaterNormalMap, i.uv));
				float3 worldNormal = float3(dot(i.TW0.xyz, normal), dot(i.TW1.xyz, normal), dot(i.TW2.xyz, normal));
				float3 worldPos = float3(i.TW0.w, i.TW1.w, i.TW2.w);
				float3 lightDir = normalize(internalWorldLightDir.xyz);
				float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				float2 projUv = i.proj0.xy / i.proj0.w + normal.xy * _Refract;
				float4 col = tex2D(_GrabTexture,  projUv);

				float4 reflcol = tex2D(_WaterReflectTexture, projUv);
				col.rgb *= _BaseColor.rgb;
				float height = max(DecodeHeight(tex2D(_WaterHeightMap, i.uv)),0);

				//半兰伯特模型
				float3 diffuse = internalWorldLightColor.rgb * saturate(0.5 * dot(worldNormal, lightDir) + 0.5) * _Diffuse;

				

				//Blinn-Phong光照模型
				float3 halfdir = normalize(lightDir + viewDir);
				float ndh = saturate(dot(worldNormal, halfdir));
				float3 specular = internalWorldLightColor.rgb * pow(ndh, _Specular*128.0)*_Gloss;

				//菲涅尔效果
				float bias = _Fresnel.x,scale = _Fresnel.y,power =_Fresnel.z;
				float f = clamp(bias + pow(1 - saturate(dot(worldNormal, viewDir)),power) * scale, 0.0, 1.0);
				col.rgb = lerp(col.rgb,diffuse + reflcol.rgb, f);
				col.rgb += specular;


				//水波突出
				col.rgb += _WaterColor.rgb * (height*_Range);

				//从顶点着色器中输出雾效数据，将第二个参数中的颜色值作为雾效的颜色值，且在正向附加渲染通道（forward-additive pass）中
				UNITY_APPLY_FOG(i.fogCoord, col); 
				col.a = 1.0;

				return  col;
			}
			ENDCG
		}
	}
}
