using System.Collections.Generic;
using UnityEngine;

namespace LinHowe
{
    /// <summary>
    /// 正弦函数模拟水面波形
    /// </summary>
    class SineWave_Component : IWaveComponent
    {

        protected SineWaveMonoBehaviour sineWavesMonoBehaviour;
        protected List<Vector4> waves = new List<Vector4>();//传入shader中的波形参数
        protected List<Vector2> origins = new List<Vector2>();//传入shader中的波源uv参数
        protected List<float> timers = new List<float>();//计时器
        protected List<float> cycles = new List<float>();//周期
        protected float MainTimer = 0;//传入shader的时间参数

        public SineWave_Component(WaterRender.WaterSurface water) : base(water)
        {

        }
        public override bool InitAndCheckWaveParams(float speed, float viscosity, float d)
        {
            sineWavesMonoBehaviour = water.GetComponent<SineWaveMonoBehaviour>();
            if (sineWavesMonoBehaviour == null) return false;
            sineWavesMonoBehaviour.CaluateUV();
            int size = sineWavesMonoBehaviour.waves.Count;
            waves = new List<Vector4>(size);
            origins = new List<Vector2>(size);
            timers = new List<float>(size);
            cycles = new List<float>(size);
            for (int i = 0; i < size; ++i)
            {
                SineWave wave = sineWavesMonoBehaviour.waves[i];
                if (wave.T < 0.1f) wave.T = 0.1f;
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
                timers.Add(0);
                cycles.Add(wave.T);
            }
            return true;
        }

        public override void OnRenderImage(RenderTexture src, RenderTexture dst, Material waveEquationMat)
        {
            int size = waves.Count;
           
            for (int i = 0; i < size; ++i)
            {
                timers[i] += Time.fixedDeltaTime;
                if(cycles[i]>=timers[i])
                {
                    timers[i] -= cycles[i];
                    Vector4 waveParams = waves[i];
                    Vector2 uv = origins[i];

                    SetWaveParams(waveEquationMat, waveParams, uv);
                    Graphics.Blit(src, dst, waveEquationMat);
                    Graphics.Blit(dst, src);
                }
                
            }
            MainTimer += Time.fixedDeltaTime;


        }

        protected void SetWaveParams(Material waveEquationMat, Vector4 waveParams, Vector2 waveOrigin)
        {
            waveEquationMat.SetVector("_WaveOrigin", waveOrigin);
            waveEquationMat.SetVector("_WaveParams", waveParams);
            waveEquationMat.SetFloat("_Timer", MainTimer);
        }
    }
}
