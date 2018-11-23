using System.Collections.Generic;
using UnityEngine;

namespace LinHowe
{
    /// <summary>
    /// Gerstner几何波
    /// </summary>
    class GerstnerWave_Component : SineWave_Component
    {
        RenderTexture GerstnerOffsetXMap;
        RenderTexture GerstnerOffsetZMap;
        public GerstnerWave_Component(WaterRender.WaterSurface water) : base(water)
        {
            
        }

        public override void InitWaterCamera(int texSize, Material waveEquationMat)
        {
            GerstnerOffsetXMap = RenderTexture.GetTemporary(texSize, texSize, 16);
            GerstnerOffsetXMap.name = "[GerstnerOffsetXMap]";
            RenderTexture.active = GerstnerOffsetXMap;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            GerstnerOffsetZMap = RenderTexture.GetTemporary(texSize, texSize, 16);
            GerstnerOffsetZMap.name = "[GerstnerOffsetZMap]";
            RenderTexture.active = GerstnerOffsetZMap;
            GL.Clear(false, true, new Color(0, 0, 0, 0));

            waveEquationMat.SetFloat("Qi", 0.6f);
        }

        public override void OnDestroy()
        {
            if (GerstnerOffsetXMap)
                RenderTexture.ReleaseTemporary(GerstnerOffsetXMap);
            if (GerstnerOffsetZMap)
                RenderTexture.ReleaseTemporary(GerstnerOffsetZMap);
        }

        public override void OnRenderImage(RenderTexture src, RenderTexture dst, Material waveEquationMat)
        {
            int size = waves.Count;

            for (int i = 0; i < size; ++i)
            {
                timers[i] += Time.fixedDeltaTime;
                if (cycles[i] >= timers[i])
                {
                    timers[i] -= cycles[i];
                    Vector4 waveParams = waves[i];
                    Vector2 uv = origins[i];

                    SetWaveParams(waveEquationMat, waveParams, uv);
                    Shader.EnableKeyword("GenerateGerstnerOffsetX");
                    Graphics.Blit(src, GerstnerOffsetXMap, waveEquationMat);
                    Shader.DisableKeyword("GenerateGerstnerOffsetX");
                    Shader.EnableKeyword("GenerateGerstnerOffsetZ");
                    Graphics.Blit(src, GerstnerOffsetZMap, waveEquationMat);
                    Shader.DisableKeyword("GenerateGerstnerOffsetZ");
                    Graphics.Blit(src, dst, waveEquationMat); 
                    Graphics.Blit(dst, src);
                    
                }
            }
            MainTimer += Time.fixedDeltaTime;
        }

        public override void OnPostRender()
        {
            Shader.SetGlobalTexture("_WaterOffsetXMap", GerstnerOffsetXMap);
            Shader.SetGlobalTexture("_WaterOffsetZMap", GerstnerOffsetZMap);
        }

       
    }
}
