
using System.Collections.Generic;
using UnityEngine;

namespace LinHowe
{
    /// <summary>
    /// 波形组件枚举
    /// </summary>
    public enum WaveComponentEnum
    {
        WaveEquation,
        SineWave,
        SineWaveDeform,
        GerstnerWave
    }
    /// <summary>
    /// 水体波动组件
    /// </summary>
    public abstract class IWaveComponent
    {
        protected WaterRender.WaterSurface water;
        public IWaveComponent(WaterRender.WaterSurface water)
        {
            this.water = water;
        }
        /// <summary>
        /// 初始化检测
        /// </summary>
        /// <param name="speed">波速</param>
        /// <param name="viscosity">水体粘度系数</param>
        /// <param name="d">水体网格单元间隔</param>
        /// <returns></returns>
        public abstract bool InitAndCheckWaveParams(float speed, float viscosity,float d);

        public abstract void OnRenderImage(RenderTexture src, RenderTexture dst,Material waveEquationMat);

        public virtual void InitWaterCamera(int texSize,Material waveEquationMat) { }

        public virtual void OnDestroy() { }

        public virtual void OnPostRender() { }
    }

   
    
    
}
