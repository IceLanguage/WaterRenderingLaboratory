

using UnityEngine;

namespace LinHowe
{
    class NoiseWave_Component : IWaveComponent
    {
        public NoiseWave_Component(WaterRender.WaterSurface water) : base(water)
        {

        }

        public override bool InitAndCheckWaveParams(float speed, float viscosity, float d)
        {
            return true;
        }

        public override void OnRenderImage(RenderTexture src, RenderTexture dst, Material waveEquationMat)
        {
            Graphics.Blit(src, dst, waveEquationMat);
        }

        public override void OnPostRender()
        {
        }
    }
}
