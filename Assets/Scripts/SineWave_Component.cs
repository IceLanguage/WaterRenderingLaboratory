using System.Collections.Generic;
using UnityEngine;

namespace LinHowe
{
    /// <summary>
    /// 正弦函数叠加组件
    /// </summary>
    class SineWave_Component : IWaveComponent
    {

        private SineWaveMonoBehaviour sineWavesMonoBehaviour;
        private List<Vector4> waves = new List<Vector4>();
        private List<Vector2> origins = new List<Vector2>();
        private float timer = 0;

        public SineWave_Component(WaterRender.WaterSurface water) : base(water)
        {

        }
        public override bool InitAndCheckWaveParams(float speed, float viscosity, float d)
        {
            sineWavesMonoBehaviour = water.GetComponent<SineWaveMonoBehaviour>();
            if (sineWavesMonoBehaviour == null) return false;
            sineWavesMonoBehaviour.CaluateUV();
            int size = sineWavesMonoBehaviour.waves.Count;
            for (int i = 0; i < size; ++i)
            {
                SineWave wave = sineWavesMonoBehaviour.waves[i];
                //wave.S = speed;
                Vector2 direction = wave.D;
                wave.D = direction.normalized;
                Vector4 WaveParams = Vector4.zero;
                float AngularFrequency = 2 * Mathf.PI / wave.L;
                float PhaseConstant = AngularFrequency * wave.S;
                WaveParams.x = wave.A * PhaseConstant;
                WaveParams.y = wave.D.x * AngularFrequency;
                WaveParams.z = wave.D.y * AngularFrequency;
                WaveParams.w = PhaseConstant;
                waves.Add(WaveParams);
                origins.Add(wave.pos);
            }
            return true;
        }

        public override void InitWaterCamera(int texSize, Material waveEquationMat)
        {

        }

        public override void OnDestroy()
        {

        }

        public override void OnRenderImage(RenderTexture src, RenderTexture dst, Material waveEquationMat)
        {
            int size = waves.Count;
           
            for (int i = 0; i < size; ++i)
            {
                Vector4 waveParams = waves[i];
                Vector2 uv = origins[i];
                
                SetWaveParams(waveEquationMat, waveParams, uv);
                Graphics.Blit(src, dst, waveEquationMat);
            }
            timer += Time.fixedDeltaTime;


        }

        protected void SetWaveParams(Material waveEquationMat, Vector4 waveParams, Vector2 waveOrigin)
        {
            waveEquationMat.SetVector("_WaveOrigin", waveOrigin);
            waveEquationMat.SetVector("_WaveParams", waveParams);
            waveEquationMat.SetFloat("_Timer", timer);
        }
    }
}
