#ifndef INTERNAL_CGINC
#define INTERNAL_CGINC

//SunLight.cs
float4 internalWorldLightPos;
float4 internalWorldLightDir;
float4 internalWorldLightColor;
float4x4 internalWorldLightMV;
float4x4 internalWorldLightVP;
float4 internalProjectionParams;
float internalBias;
sampler2D internalShadowMap;

//WaterCamera.cs
sampler2D _WaterHeightMap;
sampler2D _WaterNormalMap;

//GerstnerWave_Component.cs
sampler2D _WaterOffsetXMap;
sampler2D _WaterOffsetZMap;

//WaterBody.cs
float4 _BoundingBoxMin;
float4 _BoundingBoxMax;

float Clip(float3 worldPos)
{
	if (worldPos.x < _BoundingBoxMin.x || worldPos.x > _BoundingBoxMax.x)
		return 0;
	if (worldPos.y < _BoundingBoxMin.y || worldPos.y > _BoundingBoxMax.y)
		return 0;
	if (worldPos.z < _BoundingBoxMin.z || worldPos.z > _BoundingBoxMax.z)
		return 0;
	return 1;
}
float DecodeHeight(float4 rgba) 
{
	float d1 = DecodeFloatRG(rgba.rg);
	float d2 = DecodeFloatRG(rgba.ba);

	if (d1 >= d2)
		return d1;
	else
		return -d2;
}

float4 EncodeHeight(float height) {
	float2 rg = EncodeFloatRG(height >= 0 ? height : 0);
	float2 ba = EncodeFloatRG(height < 0 ? -height : 0);

	return float4(rg, ba);
}

#endif