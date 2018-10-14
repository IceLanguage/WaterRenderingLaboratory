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

#endif