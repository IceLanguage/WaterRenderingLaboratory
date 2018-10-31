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

//WaterBody.cs
float4 _BoundingBoxMin;
float4 _BoundingBoxMax;

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