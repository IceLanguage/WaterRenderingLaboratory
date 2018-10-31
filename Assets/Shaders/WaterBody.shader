Shader "LinHowe/WaterBody"
{
	Properties
	{
		_RayStep("RayStep",float) = 64
		_Height("Height", float) = 3.67
		_MainColor ("MainColor", color) = (1,1,1,1)
	}
	SubShader
	{ 
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "true" }

		Pass
		{
			//不写入深度缓存
			zwrite off

			//透明度混合
			blend srcalpha one
			colormask rgb
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			#include "internal.cginc"

			struct v2f
			{
				float3 worldPos : TEXCOORD0;
				float4 vertex : SV_POSITION;
				UNITY_FOG_COORDS(1)
			};

			float _RayStep;
			float _Height;
			float4 _MainColor;
			
			float4 _WaterPlane;
			float4 _BoundingBoxSize;
			
			v2f vert (appdata_full v)
			{
				v2f o;

				float height = DecodeHeight(tex2Dlod(_WaterHeightMap, float4(v.texcoord.xy,0,0)));
				v.vertex.y += height * _Height;

				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			float ClipInBoundingBox(float3 worldPos) {
				if (worldPos.x < _BoundingBoxMin.x || worldPos.x > _BoundingBoxMax.x)
					return 0;
				if (worldPos.y < _BoundingBoxMin.y )
					return 0;
				if (worldPos.z < _BoundingBoxMin.z || worldPos.z > _BoundingBoxMax.z)
					return 0;
				return 1;
			}
			float4 frag (v2f i) : SV_Target
			{
				float4 lightDeltaColor =1.0 /_RayStep ;//* internalWorldLightColor;

				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));

				float4 col = float4(0, 0, 0, 0);
				
				float delta = max(max(_BoundingBoxSize.x,_BoundingBoxSize.y),_BoundingBoxSize.z)/_RayStep;
				for (float k = 0 ; k <_RayStep; k += 1)
				{
					
					float3 p = i.worldPos - viewDir * k * delta;
					float3 lightDir =  normalize(internalWorldLightPos.xyz - p);
					float3 hitPos = p + lightDir * (_WaterPlane.w - dot(p, _WaterPlane.xyz) / dot(lightDir, _WaterPlane.xyz));
					float2 uv = (hitPos.xz - _BoundingBoxMin.xz) / _BoundingBoxSize.xz;
					float3 normal = UnpackNormal(tex2D(_WaterNormalMap, uv));
					float diffuse = saturate(dot( -lightDir ,normal ) ) ;
					float isClip = ClipInBoundingBox(p);
					
					col += _MainColor * lightDeltaColor * isClip * diffuse ;
				}
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col ;
			}

			
			ENDCG
		}
	}
}
