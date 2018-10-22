#ifndef INTERNAL_CGINC
#define INTERNAL_CGINC

//SunLight.cs
uniform float4 internalWorldLightPos;
uniform float4 internalWorldLightColor;
float4x4 internalWorldLightMV;
float4x4 internalWorldLightVP;
float4 internalProjectionParams;
float internalBias;
sampler2D internalShadowMap;

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